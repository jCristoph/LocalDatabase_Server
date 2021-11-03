using LocalDatabase_Server.Database;
using LocalDatabase_Server.Server;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace LocalDatabase_Server
{
    class ServerStarter
    {
        TcpListener server = null;
        DirectoryManager dm = null;
        ObservableCollection<Database.User> activeUsers;
        ObservableCollection<Database.Transmission> transmissions;
        bool isConnected;

        //constructor
        public ServerStarter(ObservableCollection<Database.User> activeUsers, ObservableCollection<Database.Transmission> transmissions, string ip = "127.0.0.1", int port = 25000)
        {
            this.activeUsers = activeUsers;
            this.transmissions = transmissions;
            Database.DatabaseManager databaseManager = new Database.DatabaseManager();
            Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
            server.Start();
            StartListener();
        }

        //start server method
        public void StartListener()
        {
            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Task handleDeviceTask = new Task(() =>
                    {
                        var serverCertificate = getServerCert();
                        var sslStream = new SslStream(client.GetStream(), false, ValidateCertificate);
                        sslStream.AuthenticateAsServer(serverCertificate, true, SslProtocols.Tls12, false);
                        sslStream.ReadTimeout = 15 * 60 * 1000; //after 15 minutes afk user is logged out
                        isConnected = true;
                        var token = readMessage(sslStream);
                        while (isConnected)
                        {
                            //the only thing that server have to do with client is listen to request. Then eventually answer.
                            try
                            {
                                readMessage(sslStream);
                            }
                            catch (Exception e)
                            {
                                User u = new User(token);
                                sslStream.Close();
                                isConnected = false;
                                Application.Current.Dispatcher.Invoke(new Action(() => { activeUsers.Remove(u); }));
                            }
                        }
                    }); //when new client wants to connect, the new thread is created
                    handleDeviceTask.Start();
                }
            }
            catch (SocketException e)
            {
                server.Stop();
            }
        }

        #region ssl_methods
        static bool ValidateCertificate(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            // we don't have a proper certificate tree
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
                return true;
            return false;
        }
        private static X509Certificate getServerCert()
        {
            X509Store store = new X509Store(StoreName.My,
               StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate2 foundCertificate = null;
            foreach (X509Certificate2 currentCertificate
               in store.Certificates)
            {
                if (currentCertificate.IssuerName.Name
                   != null && currentCertificate.IssuerName.
                   Name.Equals("CN=MySslSocketCertificate"))
                {
                    foundCertificate = currentCertificate;
                    break;
                }
            }
            return foundCertificate;
        }
        #endregion

        //method that recognie messages from xml language
        public string recognizeMessage(string data, SslStream sslStream)
        {
            int taskIndexHome = data.IndexOf("<Task=") + "<Task=".Length;
            int taskIndexEnd = data.IndexOf(">");
            string task = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
            string token = "";
            User u = null;
            if (data.Contains("<Token>"))
            {
                taskIndexHome = data.IndexOf("<Token>") + "<Token>".Length;
                taskIndexEnd = data.IndexOf("</Token>");
                token = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
            }
            string destinationPath = "";
            Database.DatabaseManager databaseManager = new Database.DatabaseManager();
            string path = "";
            //after translation system choose which method run 
            switch (task)
            {
                case "Login":
                    string[] temp = ServerCom.LoginRecognizer(data);
                    u = new User(temp[0]); //temp[0] - token
                    if (!activeUsers.Contains(u)) //user can be logged in only on one device in the same time. It could be a problem if device or program stopped running unexpectedly
                    {
                        User loggedUser = databaseManager.FindUserByToken(temp[0]);
                        if (loggedUser != null)
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                activeUsers.Add(loggedUser);
                            }));
                        sendMessage(ServerCom.CheckLoginMessage(temp), sslStream);
                    }
                    else
                    {
                        sendMessage("<Task=CheckLogin><isLogged>ERROR1</isLogged><Limit></Limit><Login>", sslStream);
                    }
                    return temp[0];

                case "Registration":
                    temp = ServerCom.RegistrationRecognizer(data);
                    string surname = temp[0], name = temp[1], password = temp[2];
                    u = new User(temp[0]); //temp[0] - token
                    if (surname.Length > 2 && name.Length > 2)
                    {
                        var cs = new ConnectionString();
                        AddUserUseCase.invoke(surname, name, password, cs.GetConnectionString());
                        sendMessage(ServerCom.responseMessage("Registration success"), sslStream);
                    }
                    else
                    {
                        sendMessage(ServerCom.responseMessage("Data is not valid"), sslStream);
                    }



                    if (!activeUsers.Contains(u)) //user can be logged in only on one device in the same time. It could be a problem if device or program stopped running unexpectedly
                    {
                        User loggedUser = databaseManager.FindUserByToken(temp[0]);
                        if (loggedUser != null)
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                activeUsers.Add(loggedUser);
                            }));
                        sendMessage(ServerCom.CheckLoginMessage(temp), sslStream);
                    }
                    else
                    {
                        sendMessage("<Task=CheckLogin><isLogged>ERROR1</isLogged><Limit></Limit><Login>", sslStream);
                    }
                    return temp[0];


                case "ChngPass":
                    u = new User(token);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        sendMessage(ServerCom.responseMessage(ServerCom.ChangePasswordRecognizer(data)), sslStream);
                    }
                    else
                    {
                        sendMessage(ServerCom.sessionExpired(), sslStream);
                    }
                    break;
                case "ReadOrder": //when client sends download file request
                    u = new User(token);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        u = databaseManager.FindUserByToken(token);
                        dm = new DirectoryManager(@"C:\Directory_test\" + token + "\\");

                        if((dm.usedSpace() * 1000000000) < u.limit) //dm.usedspace returns space in gigabytes and u.limit in bytes so we have to convert it
                        {
                            sendMessage(ServerCom.responseMessage("It's ok"), sslStream);
                            string[] arr = ServerCom.DownloadRecognizer(data);
                            Thread.Sleep(1000);
                            FileTransporter fileTransporter = new FileTransporter("127.0.0.1", (arr[0] + "\\" + arr[1]).Replace("Main_Folder", @"C:\Directory_test"));
                            fileTransporter.connectAsServer();
                            fileTransporter.recieveFile();
                            fileTransporter.setContainers(databaseManager, transmissions, token);
                        }
                        else
                        {
                            sendMessage(ServerCom.responseMessage("Oooppsss! You don't have enough space. Contact your admin."), sslStream);
                        }
                    }
                    else
                    {
                        sendMessage(ServerCom.sessionExpired(), sslStream);
                    }
                    break;
                case "Send": //when client sends upload file request
                    u = new User(token);
                    // TODO: Client and server has to be prepared to wait for message 
                    sendMessage(ServerCom.responseMessage("OK"), sslStream);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        path = ServerCom.SendRecognizer(data);
                        Thread.Sleep(10);

                        FileTransporter fileTransporter = new FileTransporter("127.0.0.1", path);
                        fileTransporter.connectAsServer();
                        fileTransporter.sendFile();

                        databaseManager.AddToTransmission(token, DateTime.Now, new FileInfo(path.Replace("Main_Folder", @"C:\Directory_test")).Length, 0);
                        Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
                    }
                    else
                    {
                        sendMessage(ServerCom.sessionExpired(), sslStream);
                    }
                    break;
                case "SendDir": //when client sends send my directory request
                    u = new User(token);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        dm = new DirectoryManager(@"C:\Directory_test\" + token + "\\");
                        string message = "";
                        foreach (var mess in ServerCom.SendDirectoryMessage(dm.directoryElements))
                            message += mess;
                        sendMessage(message, sslStream);
                    }
                    else
                    {
                        sendMessage(ServerCom.sessionExpired(), sslStream);
                    }
                    break;
                case "CreateFolder":
                    u = new User(token);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        sendMessage(ServerCom.responseMessage("Stworzono nowy folder"), sslStream);
                        destinationPath = ServerCom.DownloadRecognizer(data)[0];
                        dm.CreateFolder(destinationPath);
                    }
                    else
                    {
                        sendMessage(ServerCom.sessionExpired(), sslStream);
                    }
                    break;
                case "Delete":
                    u = new User(token);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        path = ServerCom.DeleteRecognizer(data)[0];
                        string isFolder = ServerCom.DeleteRecognizer(data)[1];
                        long deletedFileSize;
                        if (isFolder.Equals("False"))
                            deletedFileSize = new FileInfo(path.Replace("Main_Folder", @"C:\Directory_test")).Length;
                        else
                            deletedFileSize = 0;
                        sendMessage(ServerCom.responseMessage(dm.DeleteElement(path, isFolder)), sslStream);
                        databaseManager.AddToTransmission(token, DateTime.Now, deletedFileSize, TransmissionType.Delete);
                        Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
                    }
                    else
                    {
                        sendMessage(ServerCom.sessionExpired(), sslStream);
                    }
                    break;
                case "Logout":
                    u = new User(ServerCom.LogOutRecognizer(data));
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        sslStream.Close();
                        isConnected = false;
                        Application.Current.Dispatcher.Invoke(new Action(() => { activeUsers.Remove(u); }));
                    }
                    break;
                case "Response": // is universal request for sending messages
                    MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel(ServerCom.responseRecognizer(data), false);
                    mp.ShowDialog();
                    break;
            }
            return "";
        }
        //tcp/ip read message method. Reads bytes and translate it to string  - it will be changed for ssl connection
        public string readMessage(SslStream sslStream)
        {
            var inputBuffer = new byte[4096];
            StringBuilder messageData = new StringBuilder();
            var inputBytes = 0;
            do
            {
                inputBytes = sslStream.Read(inputBuffer, 0, inputBuffer.Length);
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(inputBuffer, 0, inputBytes)];
                decoder.GetChars(inputBuffer, 0, inputBytes, chars, 0);
                messageData.Append(chars);
                if (messageData.ToString().IndexOf("<EOM>") != -1)
                {
                    break;
                }
            } while (inputBytes != 0);
            sslStream.Flush();
            return recognizeMessage(messageData.ToString(), sslStream);
        }
        //tcp/ip send message method. translate string to bytes and send it to client by stream  - it will be changed for ssl connection
        private string sendMessage(string outputMessage, SslStream sslStream)
        {
            sslStream.Flush();
            var outputBuffer = Encoding.UTF8.GetBytes(outputMessage);
            sslStream.Write(outputBuffer);
            sslStream.Flush();
            return outputMessage;
        }
    }
}