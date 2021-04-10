using System;
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
        static TextBlock text = null;
        TcpListener server = null;
        int connectedDevices = 0;
        public ServerStarter(TextBlock txt, string ip, int port)
        {
            text = txt;
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
                    // Console.WriteLine("Waiting for a connection...");
                    Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Waiting for a connection..., Connected: " + connectedDevices; }));
                    TcpClient client = server.AcceptTcpClient();
                    //Console.WriteLine("Connected!");
                    Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Connected!"; }));
                    connectedDevices++;
                    Task t1 = new Task(() => HandleDeivce(client));
                    t1.Start();
                }
            }
            catch (SocketException e)
            {
                //Console.WriteLine("SocketException: {0}", e);
                Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "SocketException: " + e; }));
                server.Stop();
            }
        }
        public void HandleDeivce(Object obj)
        {
            TcpClient client = (TcpClient)obj;
            //funkcja sprawdza czy klient jest polaczony
            while (true) 
            {
                readMessage(client);
                //sprawdza czy połączenie jest aktywne
                if (((client.Client.Poll(1000, SelectMode.SelectRead) && (client.Client.Available == 0)) || !client.Client.Connected))
                    break;
            }
            connectedDevices--;
            client.Close();
        }

        public void recognizeMessage(string data, TcpClient client)
        {
            int taskIndexHome = data.IndexOf("<Task=") + "<Task=".Length;
            int taskIndexEnd = data.IndexOf(">");
            string task = data.Substring(taskIndexHome, taskIndexEnd - taskIndexHome);
            DirectoryManager dm = new DirectoryManager(@"C:\Directory_test");
            Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = task; }));
            switch (task)
            {
                case "Login":
                    sendMessage(ServerCom.CheckLoginMessage(ServerCom.LoginRecognizer(data)), client);
                    break;
                case "ReadOrder": //kiedy wysylane jest zadanie pobrania pliku
                    sendMessage(ServerCom.responseMessage("OK"), client);
                    Thread.Sleep(1000);
                    downloadFile(client);
                    break;
                case "Send": ////kiedy wysylane jest zadanie wyslania pliku
                    sendFile(client, ServerCom.SendRecognizer(data));
                    break;
                case "SendDir": //kiedy wysylane jest zadanie wyslania biblioteki
                    foreach(var mess in ServerCom.SendDirectoryMessage(dm.directoryElements))
                        sendMessage(mess, client);
                    break;
                case "Delete":
                    string path = ServerCom.DeleteRecognizer(data);
                    sendMessage(ServerCom.responseMessage(dm.DeleteElement(path)), client);
                    break;
                case "Response":
                    MessageBox.Show(ServerCom.responseRecognizer(data));
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
                MessageBox.Show(e.ToString());
            }
        }
        private void sendMessage(string str, TcpClient client)
        {
            var stream = client.GetStream();
            Byte[] reply = System.Text.Encoding.UTF8.GetBytes(str);
            stream.Write(reply, 0, reply.Length);
        }
        private void downloadFile(TcpClient client)
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
                        Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Downloading!!!"; }));
                        string folderPath = @"c:\Directory_test\";
                        handlerSocket.Receive(dataByte);
                        int fileNameLen = BitConverter.ToInt32(dataByte, 0);
                        fileName = Encoding.ASCII.GetString(dataByte, 4, fileNameLen);
                        Stream fileStream = File.OpenWrite(folderPath + fileName);
                        fileStream.Write(dataByte, 4 + fileNameLen, (1024 - (4 + fileNameLen)));
                        do
                        {
                            thisRead = networkStream.Read(dataByte, 0, blockSize);
                            fileStream.Write(dataByte, 0, thisRead);
                            if (!networkStream.DataAvailable)
                                Thread.Sleep(10);
                        } while (networkStream.DataAvailable);
                        fileStream.Close();
                    }
                    Application.Current.Dispatcher.Invoke(new Action(() => { text.Text = "Downloaded!!!"; }));
                    handlerSocket = null;
                }
            }
            catch
            {

            }
        }
        private void sendFile(TcpClient client, string path)
        {
            int IndexHome = path.LastIndexOf("\\") + "\\".Length;
            int IndexEnd = path.Length;
            string shortFileName = path.Substring(IndexHome, IndexEnd - IndexHome);
            path = path.Replace("Main_Folder", @"C:\Directory_test");
            string longFileName = path;// @"C:\Directory_test\plik1.txt";
            try
            {
                byte[] fileNameByte = Encoding.ASCII.GetBytes(shortFileName);
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
            catch
            {

            }
        }
    }
}
