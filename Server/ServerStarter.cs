using LocalDatabase_Server.Data;
using LocalDatabase_Server.Database;
using LocalDatabase_Server.Directory;
using LocalDatabase_Server.Server;
using LocalDatabase_Server.View.Panels.MainPanel.PieChartDrawer;
using System;
using System.Collections.ObjectModel;
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
    public static class ServerStarter
    {

        private static TcpListener server = null;
        private static DirectoryManager dm = null;
        private static ObservableCollection<User> ActiveUsers;
        private static ObservableCollection<Transmission> Transmissions;
        private static bool isConnected;
        private static SslStream sslStream;
        private static SslCertificate sslCertificate;
        private static int portNumber;
        private static string serverIp;
        private static PieChartDrawer pieChart;
        static private PortAssigner portAssigner;
       
        public static void Init(ObservableCollection<User> activeUsers, ObservableCollection<Transmission> transmissions, PieChartDrawer pieChartDrawer, int port = 25000)
        {
            ActiveUsers = activeUsers;
            Transmissions = transmissions;
            pieChart = pieChartDrawer;
            portNumber = port;
            serverIp = SettingsManager.Instance.GetServerIp();
            IPAddress localAddr = IPAddress.Parse(serverIp);
            server = new TcpListener(localAddr, port);
            server.Start();
            portAssigner = new PortAssigner();
            StartListener();
        }

        public static int GetServerPortNumber()
        {
            return portNumber;
        }

        public static void Stop()
        {
            if (sslStream != null)
            {
                sslStream.Close();
            }
            isConnected = false;
            if (server != null)
            {
                server.Stop();
            }
        }

        //start server method
        private static void StartListener()
        {
            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Task handleDeviceTask = new Task(() =>
                    {
                        sslCertificate = new SslCertificate();
                        X509Certificate serverCertificate = sslCertificate.GetCertificate();
                        sslStream = new SslStream(client.GetStream(), false, sslCertificate.IsCertificateValid);
                        sslStream.AuthenticateAsServer(serverCertificate, true, SslProtocols.Tls12, false);
                        sslStream.ReadTimeout = SettingsManager.Instance.GetIdleTime() * 60000;
                        isConnected = true;
                        string token = readMessage(sslStream);
                        while (isConnected)
                        {
                            //the only thing that server have to do with client is listen to request. Then eventually answer.
                            try
                            {
                                readMessage(sslStream);
                            }
                            catch (Exception e)
                            {
                                ExceptionCatcher.addExceptionToFile(e.ToString());
                                User u = new User(token);                   
                                Application.Current.Dispatcher.Invoke(new Action(() => { ActiveUsers.Remove(u); }));
                                isConnected = false;
                            }
                        }
                    }); //when new client wants to connect, the new thread is created
                    handleDeviceTask.Start();
                }
            }
            catch (SocketException e)
            {
                ExceptionCatcher.addExceptionToFile(e.ToString());
                Stop();
            }
        }

        //method that recognie messages from xml language
        public static string recognizeMessage(string data, SslStream sslStream)
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
            
            string path = "";
            //after translation system choose which method run 
            switch (task)
            {
                case "Login":
                    string[] temp = ServerCom.LoginRecognizer(data);
                    u = new User(temp[0]); //temp[0] - token
                    if (!ActiveUsers.Contains(u)) //user can be logged in only on one device in the same time. It could be a problem if device or program stopped running unexpectedly
                    {
                        User loggedUser = DatabaseManager.Instance.FindUserByToken(temp[0]);
                        if (loggedUser != null)
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                ActiveUsers.Add(loggedUser);
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
                    token = Generator.GenerateToken();
                    u = new User(token);
                    if (surname.Length > 2 && name.Length > 2)
                    {
                        var cs = new ConnectionString();
                        bool doesUserExist = AddUserUseCase.invoke(name, surname, password, token, cs.GetConnectionString());
                        if (doesUserExist)
                        {
                            sendMessage(ServerCom.responseMessage("User already exists"), sslStream);
                        }
                        else
                        {
                            sendMessage(ServerCom.responseMessage("New token " + token.ToString()), sslStream);
                        }
                    }
                    else
                    {
                        sendMessage(ServerCom.responseMessage("Data is not valid"), sslStream);
                    }

                    if (!ActiveUsers.Contains(u)) //user can be logged in only on one device in the same time. It could be a problem if device or program stopped running unexpectedly
                    {
                        User loggedUser = DatabaseManager.Instance.FindUserByToken(temp[0]);
                        if (loggedUser != null)
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                ActiveUsers.Add(loggedUser);
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
                    if (ActiveUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
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
                    if (ActiveUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        u = DatabaseManager.Instance.FindUserByToken(token);
                        dm = new DirectoryManager(SettingsManager.Instance.GetSavePath() + token + "\\");

                        if((dm.usedSpace() * 1000000000) < u.limit) //dm.usedspace returns space in gigabytes and u.limit in bytes so we have to convert it
                        {
                            sendMessage(ServerCom.acceptTransferMessage(portAssigner.GetPort()), sslStream);
                            string[] arr = ServerCom.DownloadRecognizer(data);
                            Thread.Sleep(1000);
                            var fileName = (arr[0] + "\\" + arr[1]).Replace("Main_Folder", SettingsManager.Instance.GetSavePath());
                            FileTransporter fileTransporter = new FileTransporter(serverIp, fileName, portAssigner.GetPort());
                            fileTransporter.connectAsServer();
                            fileTransporter.recieveFile();
                            fileTransporter.setContainers(token, AddTransmission);
                        }
                        else
                        {
                            sendMessage(ServerCom.responseMessage("Oooppsss! You don't have enough space. Contact your admin."), sslStream);
                        }
                        portAssigner.AssignPort();
                    }
                    else
                    {
                        sendMessage(ServerCom.sessionExpired(), sslStream);
                    }
                    break;
                case "Send": //when client sends upload file request
                    u = new User(token);
                    // TODO: Client and server has to be prepared to wait for message 
                    sendMessage(ServerCom.acceptTransferMessage(portAssigner.GetPort()), sslStream);
                    if (ActiveUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        path = ServerCom.SendRecognizer(data);
                        Thread.Sleep(10);

                        FileTransporter fileTransporter = new FileTransporter(serverIp, path, portAssigner.GetPort());
                        fileTransporter.connectAsServer();
                        fileTransporter.sendFile();

                        var fileSize = new FileInfo(path.Replace("Main_Folder", SettingsManager.Instance.GetSavePath())).Length;
                        AddTransmission(fileSize, token, TransmissionType.Download);
                    }
                    else
                    {
                        sendMessage(ServerCom.sessionExpired(), sslStream);
                    }
                    portAssigner.AssignPort();
                    break;
                case "SendDir": //when client sends send my directory request
                    u = new User(token);
                    if (ActiveUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        dm = new DirectoryManager(SettingsManager.Instance.GetSavePath() + token + "\\");
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
                    if (ActiveUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
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
                    if (ActiveUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        path = ServerCom.DeleteRecognizer(data)[0];
                        string isFolder = ServerCom.DeleteRecognizer(data)[1];
                        long deletedFileSize;
                        if (isFolder.Equals("False"))
                            deletedFileSize = new FileInfo(path.Replace("Main_Folder", SettingsManager.Instance.GetSavePath())).Length;
                        else
                            deletedFileSize = 0;
                        sendMessage(ServerCom.responseMessage(dm.DeleteElement(path, isFolder)), sslStream);

                        AddTransmission(deletedFileSize, token, TransmissionType.Delete);
                    }
                    else
                    {
                        sendMessage(ServerCom.sessionExpired(), sslStream);
                    }
                    break;
                case "Logout":
                    u = new User(ServerCom.LogOutRecognizer(data));
                    if (ActiveUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        sslStream.Close();
                        isConnected = false;
                        Application.Current.Dispatcher.Invoke(new Action(() => { ActiveUsers.Remove(u); }));
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
        public static string readMessage(SslStream sslStream)
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
        private static string sendMessage(string outputMessage, SslStream sslStream)
        {
            sslStream.Flush();
            var outputBuffer = Encoding.UTF8.GetBytes(outputMessage);
            sslStream.Write(outputBuffer);
            sslStream.Flush();
            return outputMessage;
        }

        private static void AddTransmission(long fileSize, string token, TransmissionType transmissionType)
        {
            Transmission t = new Transmission(0, DateTime.Now, fileSize, token, transmissionType);
            DatabaseManager.Instance.AddToTransmission(t);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Transmissions.Add(t);
                pieChart.DrawPieChart();
            }));
            
        }
    }
}