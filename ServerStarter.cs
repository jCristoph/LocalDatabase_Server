using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LocalDatabase_Server
{
    class ServerStarter
    {
        TcpListener server = null;
        DirectoryManager dm = null;
        ObservableCollection<Database.User> activeUsers;
        List<string> activeUserTokens;
        ObservableCollection<Database.Transmission> transmissions;

        public ServerStarter(string ip, int port, ObservableCollection<Database.User> activeUsers, ObservableCollection<Database.Transmission> transmissions)
        {
            this.activeUsers = activeUsers;
            this.transmissions = transmissions;
            Database.DatabaseManager databaseManager = new Database.DatabaseManager();
            Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
            activeUserTokens = new List<string>();
            IPAddress localAddr = IPAddress.Parse(ip);
            server = new TcpListener(localAddr, port);
            server.Start();
            StartListener();
        }
        public void StartListener()
        {
            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    Task t1 = new Task(() => HandleDeivce(client));
                    t1.Start();
                }
            }
            catch (SocketException e)
            {
                server.Stop();
            }
        }
        public void HandleDeivce(Object obj)
        {
            TcpClient client = (TcpClient)obj;
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while (true) 
            {
                readMessage(client);
                if(stopWatch.ElapsedMilliseconds > 21 * 1000)
                {
                    stopWatch.Restart();
                    activeUserTokens.Clear();
                    Application.Current.Dispatcher.Invoke(new Action(() => { activeUsers.Clear(); }));
                }
            }
        }

        public void recognizeMessage(string data, TcpClient client)
        {
            int taskIndexHome = data.IndexOf("<Task=") + "<Task=".Length;
            int taskIndexEnd = data.IndexOf(">");
            string task = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
            string token = "";
            if(data.Contains("<Token>"))
            {
                taskIndexHome = data.IndexOf("<Token>") + "<Token>".Length;
                taskIndexEnd = data.IndexOf("</Token>");
                token = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
            }
            string destinationPath = "";
            Database.DatabaseManager databaseManager = new Database.DatabaseManager();
            string path = "";
            switch (task)
            {
                case "Login":
                    sendMessage(ServerCom.CheckLoginMessage(ServerCom.LoginRecognizer(data)), client);
                    break;
                case "ChngPass":
                    sendMessage(ServerCom.responseMessage(ServerCom.ChangePasswordRecognizer(data)), client);
                    break;
                case "ReadOrder": //kiedy wysylane jest zadanie pobrania pliku
                    sendMessage(ServerCom.responseMessage("Pobieram..."), client);
                    string[] arr = ServerCom.DownloadRecognizer(data);
                    Thread.Sleep(1000);
                    downloadFile(client, arr[0]);

                    databaseManager.AddToTransmission(token, DateTime.Now, new FileInfo((arr[0] + "\\" + arr[1]).Replace("Main_Folder", @"C:\Directory_test")).Length, 1);
                    Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
                    break;
                case "Send": ////kiedy wysylane jest zadanie wyslania pliku
                    path = ServerCom.SendRecognizer(data);
                    sendFile(client, path);
                    databaseManager.AddToTransmission(token, DateTime.Now, new FileInfo(path.Replace("Main_Folder", @"C:\Directory_test")).Length, 0);
                    Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
                    break;
                case "Share":
                    string[] values = ServerCom.ShareRecognizer(data);
                    values[2] = values[2].Replace("Main_Folder", @"C:\Directory_test");
                    databaseManager.AddToSharedFile(values[0], values[1], values[2], values[3]);
                    databaseManager.AddToTransmission(values[0], DateTime.Now, new FileInfo(values[2]).Length, 3);
                    Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
                    break;
                case "SendUsers":
                    token = ServerCom.SendUsersOrderRecognizer(data);
                    foreach (var mess in ServerCom.SendUsersMessage(token))
                        sendMessage(mess, client);
                    break;
                case "UsersSharing":
                    path = ServerCom.UsersSharingRecognizer(data);
                    var usersSharing = ServerCom.SendUsersSharingMessage(databaseManager.FindUserByToken(databaseManager.FindInSharedFile(path)));
                    if(usersSharing.Length > 0)
                    {
                        foreach (var mess in usersSharing)
                            sendMessage(mess, client);
                    }
                    else
                        sendMessage(ServerCom.responseMessage("EMPTY"), client);
                    break;
                case "SendDir": //kiedy wysylane jest zadanie wyslania biblioteki
                    token = ServerCom.SendDirectoryOrderRecognizer(data);
                    dm = new DirectoryManager(@"C:\Directory_test\" + token + "\\");
                    if (!activeUserTokens.Contains(token))
                    {
                        activeUserTokens.Add(token);
                        Application.Current.Dispatcher.Invoke(new Action(() => { activeUsers.Add(databaseManager.FindUserByToken(token)); })); 
                    }
                    foreach (var mess in ServerCom.SendDirectoryMessage(dm.directoryElements))
                        sendMessage(mess, client);
                    foreach (var mess in ServerCom.SendDirectoryMessage(dm.SharedFilesList(token)))
                        sendMessage(mess, client);
                    break;
                case "CreateFolder":
                    sendMessage(ServerCom.responseMessage("Stworzono nowy folder"), client);
                    destinationPath = ServerCom.DownloadRecognizer(data)[0];
                    dm.CreateFolder(destinationPath);
                    break;
                case "Delete":
                    path = ServerCom.DeleteRecognizer(data)[0];
                    long deletedFileSize = new FileInfo(path.Replace("Main_Folder", @"C:\Directory_test")).Length;
                    string isFolder = ServerCom.DeleteRecognizer(data)[1];
                    sendMessage(ServerCom.responseMessage(dm.DeleteElement(path, isFolder)), client);
                    databaseManager.AddToTransmission(token, DateTime.Now, deletedFileSize, 2);
                    Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
                    break;
                case "Response":
                    MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel(ServerCom.responseRecognizer(data), false);
                    mp.ShowDialog();
                    break;
            }
        }
        private void readMessage(TcpClient client)
        {
            try
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
            catch(Exception e)
            {

            }
        }
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
