using LocalDatabase_Server.Data;
using LocalDatabase_Server.Database;
using LocalDatabase_Server.Directory;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;

namespace LocalDatabase_Server.Server
{
    public class FileTransporter
    {
        private string ip;
        private int port;
        private string fileName;
        private FileInfo file;
        static int BUFFER_SIZE = 4096;
        Socket socket;
        string token;
        int serverPortNumber;
        int clientPortNumber;

        public FileTransporter(string ip, string fileName, int port)
        {
            this.ip = ip;
            this.port = port;
            file = new FileInfo(fileName);
            this.fileName = fileName;
            this.serverPortNumber = ServerStarter.GetServerPortNumber();
            this.clientPortNumber = serverPortNumber + 1;
        }

        public void connectAsServer()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
            socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipe);
            socket.Listen(10);
            socket = socket.Accept();
        }

        public void connectAsClient()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), port);
            socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipe);
        }

        #region recieve File Asynchronous
        public void recieveFile()
        {
            var recieveFile_bg = new BackgroundWorker();
            recieveFile_bg.DoWork += new DoWorkEventHandler(recieveFile_bg_DoWork);
            recieveFile_bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(recieveFile_bg_RunWorkerCompleted);
            recieveFile_bg.ProgressChanged += recieveFile_bg_ProgressChanged;
            recieveFile_bg.WorkerSupportsCancellation = true;
            recieveFile_bg.WorkerReportsProgress = true;
            recieveFile_bg.RunWorkerAsync();
        }

        private void recieveFile_bg_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker helperBW = sender as BackgroundWorker;
            helperBW.ReportProgress(0);
            var readed = -1;
            var buffer = new Byte[BUFFER_SIZE];
            int i = 0;
            using (var fileStream = file.OpenWrite())
            using (var networkStream = new NetworkStream(socket, false))
            {
                networkStream.ReadTimeout = 10000;
                do
                {
                    try
                    {
                        readed = networkStream.Read(buffer, 0, buffer.Length);
                        fileStream.Write(buffer, 0, readed);
                        helperBW.ReportProgress(i++);
                    }
                    catch (Exception ex)
                    {
                        ExceptionCatcher.addExceptionToFile(ex.ToString());
                        readed = 0;
                    }
                    //If you test it on loopback better uncomment line below. Buffer is slower than loopback transfer
                    Thread.Sleep(1);
                } while (readed > (BUFFER_SIZE - 1));
                networkStream.Close();
            }
        }
        private void recieveFile_bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Console.WriteLine(e.ProgressPercentage.ToString());
        }
        private void recieveFile_bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DatabaseManager.Instance.AddToTransmission(token, DateTime.Now, new FileInfo(fileName).Length, TransmissionType.Upload);
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Thread.Sleep(1000);
        }
        #endregion

        #region Send File Asynchronous
        public void sendFile()
        {
            var sendFile_bg = new BackgroundWorker();
            sendFile_bg.DoWork += new DoWorkEventHandler(sendFile_bg_DoWork);
            sendFile_bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(sendFile_bg_RunWorkerCompleted);
            sendFile_bg.ProgressChanged += sendFile_bg_ProgressChanged;
            sendFile_bg.WorkerSupportsCancellation = true;
            sendFile_bg.WorkerReportsProgress = true;
            sendFile_bg.RunWorkerAsync();
        }

        private void sendFile_bg_DoWork(object sender, DoWorkEventArgs e)
        {
            var path = fileName.Replace("Main_Folder", SettingsManager.Instance.GetSavePath());
            file = new FileInfo(path);
            BackgroundWorker helperBW = sender as BackgroundWorker;
            helperBW.ReportProgress(0);
            var readed = -1;
            int i = 0;
            var buffer = new Byte[BUFFER_SIZE];
            using (var networkStream = new BufferedStream(new NetworkStream(socket, false)))
            using (var fileStream = file.OpenRead())
            {
                while (readed != 0)
                {
                    readed = fileStream.Read(buffer, 0, buffer.Length);
                    if (readed != 0)
                    {
                        try
                        {
                            networkStream.Write(buffer, 0, readed);
                            helperBW.ReportProgress(i++);
                            //If you test it on loopback better uncomment line below. Buffer is slower than loopback transfer
                            Thread.Sleep(1);
                        }
                        catch (Exception ex)
                        {
                            ExceptionCatcher.addExceptionToFile(ex.ToString());
                            readed = 0;
                        }
                    }
                }
                buffer = new byte[BUFFER_SIZE];
                networkStream.Close();
            }
        }
        private void sendFile_bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //Console.WriteLine(e.ProgressPercentage.ToString());
        }
        private void sendFile_bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            Thread.Sleep(1000);
        }

        internal void setContainers(string token)
        {
            this.token = token;
        }
        #endregion

    }
}
