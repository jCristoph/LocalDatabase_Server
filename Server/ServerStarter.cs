using LocalDatabase_Server.Database;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
        public ServerStarter(string ip, int port, ObservableCollection<Database.User> activeUsers, ObservableCollection<Database.Transmission> transmissions)
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
                Task sessionWatcherTask = new Task(() =>
                {
                    //server after session time clear active user container. then all users have to log in another time. 
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();
                    while (true)
                    {
                        stopWatch.Restart();
                        Application.Current.Dispatcher.Invoke(new Action(() => { activeUsers.Clear(); }));
                        Thread.Sleep(15 * 60 * 1000); // setting session time at 15 minutes right now
                    }
                });
                sessionWatcherTask.Start();
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Task handleDeviceTask = new Task(() => 
                    {
                        isConnected = true;
                        while (isConnected)
                        {
                            //the only thing that server have to do with client is listen to request. Then eventually answer.
                            try
                            {
                                readMessage(client);
                            }
                            catch (Exception e)
                            {
                                isConnected = false;
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

        //method that recognie messages from xml language
        public void recognizeMessage(string data, TcpClient client)
        {
            int taskIndexHome = data.IndexOf("<Task=") + "<Task=".Length;
            int taskIndexEnd = data.IndexOf(">");
            string task = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
            string token = "";
            User u = null;
            if(data.Contains("<Token>"))
            {
                taskIndexHome = data.IndexOf("<Token>") + "<Token>".Length;
                taskIndexEnd = data.IndexOf("</Token>");
                token = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
            }
            string destinationPath = "";
            Database.DatabaseManager databaseManager = new Database.DatabaseManager();
            string path = "";
            //after translate system choose which method run 
            switch (task)
            {
                case "Login":
                    string[] temp = ServerCom.LoginRecognizer(data);
                    u = new User(temp[0]);
                    if (!activeUsers.Contains(u)) //user can be logged in only on one device in the same time. It could be a problem if device or program stopped running unexpectedly
                    {
                        User loggedUser = databaseManager.FindUserByToken(temp[0]);
                        if (loggedUser != null)
                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                activeUsers.Add(loggedUser);
                            }));
                        sendMessage(ServerCom.CheckLoginMessage(temp), client);
                    }
                    else
                    {
                        sendMessage("<Task=CheckLogin><isLogged>ERROR1</isLogged><Limit></Limit><Login>", client);
                    }
                    break;
                case "ChngPass":
                    u = new User(token);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        sendMessage(ServerCom.responseMessage(ServerCom.ChangePasswordRecognizer(data)), client);
                    }
                    else
                    {
                        sendMessage("<Task=SessionExpired></SessionExpired>", client);
                    }
                    break;
                case "ReadOrder": //when client sends download file request
                    u = new User(token);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        sendMessage(ServerCom.responseMessage("Pobieram..."), client);
                        string[] arr = ServerCom.DownloadRecognizer(data);
                        Thread.Sleep(1000);
                        downloadFile(client, arr[0]);

                        databaseManager.AddToTransmission(token, DateTime.Now, new FileInfo((arr[0] + "\\" + arr[1]).Replace("Main_Folder", @"C:\Directory_test")).Length, 1);
                        Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
                    }
                    else
                    {
                        sendMessage("<Task=SessionExpired></SessionExpired>", client);
                    }
                    break;
                case "Send": //when client sends upload file request
                    u = new User(token);
                    // TODO: Client and server has to be prepared to wait for message 
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        path = ServerCom.SendRecognizer(data);
                        sendFile(client, path);
                        databaseManager.AddToTransmission(token, DateTime.Now, new FileInfo(path.Replace("Main_Folder", @"C:\Directory_test")).Length, 0);
                        Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
                    }
                    else
                    {
                        sendMessage("<Task=SessionExpired></SessionExpired>", client);
                    }
                    break;
                case "SendDir": //when client sends send my directory request
                    u = new User(token);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        dm = new DirectoryManager(@"C:\Directory_test\" + token + "\\");
                        foreach (var mess in ServerCom.SendDirectoryMessage(dm.directoryElements))
                            sendMessage(mess, client);
                    }
                    else
                    {
                        sendMessage("<Task=SessionExpired></SessionExpired>", client);
                    }
                    break;
                case "CreateFolder":
                    u = new User(token);
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        sendMessage(ServerCom.responseMessage("Stworzono nowy folder"), client);
                        destinationPath = ServerCom.DownloadRecognizer(data)[0];
                        dm.CreateFolder(destinationPath);
                    }
                    else
                    {
                        sendMessage("<Task=SessionExpired></SessionExpired>", client);
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
                        sendMessage(ServerCom.responseMessage(dm.DeleteElement(path, isFolder)), client);
                        databaseManager.AddToTransmission(token, DateTime.Now, deletedFileSize, 2);
                        Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
                    }
                    else
                    {
                        sendMessage("<Task=SessionExpired></SessionExpired>", client);
                    }
                    break;
                case "Logout":
                    u = new User(ServerCom.LogOutRecognizer(data));
                    if (activeUsers.Contains(u)) //if user isnt in active users container he has to log in one more time - session is limited
                    {
                        client.Close();
                        isConnected = false;
                        Application.Current.Dispatcher.Invoke(new Action(() => { activeUsers.Remove(u); }));
                    }
                    break;
                case "Response": // is universal request for sending messages
                    MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel(ServerCom.responseRecognizer(data), false);
                    mp.ShowDialog();
                    break;
            }
        }
        //tcp/ip read message method. Reads bytes and translate it to string  - it will be changed for ssl connection
        private void readMessage(TcpClient client)
        {
            var stream = client.GetStream();
            Byte[] bytes = new Byte[1024];
            int i;
            string data = "";
            do
            {
                i = stream.Read(bytes, 0, bytes.Length);
                data += Encoding.UTF8.GetString(bytes, 0, i);
                if (!stream.DataAvailable)
                    Thread.Sleep(1);
            } while (stream.DataAvailable);
            recognizeMessage(data, client);
        }
        //tcp/ip send message method. translate string to bytes and send it to client by stream  - it will be changed for ssl connection
        private void sendMessage(string str, TcpClient client)
        {
            try
            {
                var stream = client.GetStream();
                Byte[] reply = System.Text.Encoding.UTF8.GetBytes(str);
                stream.Write(reply, 0, reply.Length);
            }
            catch (Exception e)
            {

            }
        }
        //tcp/ip download method. gets bytes and sum it to create a file. From bytes read a name of file. Saves it in path chosed by user.
        private void downloadFile(TcpClient client, string destinationPath)
        {
            try
            {
                client.GetStream().Flush();
                Socket handlerSocket = client.Client;
                if (handlerSocket.Connected)
                {
                    string fileName = string.Empty;
                    NetworkStream networkStream = new NetworkStream(handlerSocket);
                    int thisRead = 0;
                    int blockSize = 1024;
                    Byte[] dataByte = new Byte[blockSize];
                    lock (this)
                    {
                        string folderPath = destinationPath.Replace("Main_Folder", @"C:\Directory_test") + @"\";
                        handlerSocket.Receive(dataByte);
                        int fileNameLen = BitConverter.ToInt32(dataByte, 0);
                        fileName = Encoding.UTF8.GetString(dataByte, 4, fileNameLen);
                        Stream fileStream = File.OpenWrite(folderPath + fileName);
                        fileStream.Write(dataByte, 4 + fileNameLen, (1024 - (4 + fileNameLen)));
                        while (networkStream.DataAvailable)
                        {
                            thisRead = networkStream.Read(dataByte, 0, blockSize);
                            fileStream.Write(dataByte, 0, thisRead);
                            if (!networkStream.DataAvailable)
                                Thread.Sleep(10);
                        } 
                        fileStream.Close();
                    }
                    handlerSocket = null;
                }
            }
            catch (Exception e)
            {

            }
        }
        //tcp/ip send file method. Read the entire file to program then bytes sends by stream.
        private void sendFile(TcpClient client, string path)
        {
            int IndexHome = path.LastIndexOf("\\") + "\\".Length;
            int IndexEnd = path.Length;
            string shortFileName = path.Substring(IndexHome, IndexEnd - IndexHome);
            path = path.Replace("Main_Folder", @"C:\Directory_test");
            string longFileName = path;
            try
            {
                byte[] fileNameByte = Encoding.UTF8.GetBytes(shortFileName);
                byte[] fileData = File.ReadAllBytes(longFileName);
                byte[] clientData = new byte[4 + fileNameByte.Length + fileData.Length];
                byte[] fileNameLen = BitConverter.GetBytes(fileNameByte.Length);
                fileNameLen.CopyTo(clientData, 0);
                fileNameByte.CopyTo(clientData, 4);
                fileData.CopyTo(clientData, 4 + fileNameByte.Length);
                NetworkStream networkStream = new NetworkStream(client.Client);
                networkStream.Write(clientData, 0, clientData.GetLength(0));
                networkStream.Close();
            }
            catch (Exception e)
            {

            }
        }
    }
}
