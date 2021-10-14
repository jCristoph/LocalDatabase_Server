using LocalDatabase_Server.Database;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace LocalDatabase_Server.Server
{
    public class FileTransporter
    {
        private string ip;
        private string fileName;
        private FileInfo file;
        static int BUFFER_SIZE = 4096;
        Socket socket;

        DatabaseManager databaseManager;
        ObservableCollection<Transmission> transmissions;
        string token;

        public FileTransporter(string ip, string fileName)
        {
            this.ip = ip;
            file = new FileInfo(fileName);
            this.fileName = fileName;
        }

        public void connectAsServer()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), 25001);
            socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipe);
            socket.Listen(10);
            socket = socket.Accept();
        }

        public void connectAsClient()
        {
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ip), 25001);
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
                    catch
                    {
                        readed = 0;
                    }
                    //If you test it on loopback better uncomment line below. Buffer is slower than loopback transfer
                    //Thread.Sleep(1);
                } while (readed > (BUFFER_SIZE - 1));
                networkStream.Close();
            }
            e.Result = "zwracany typ";
        }
        private void recieveFile_bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage.ToString());
        }
        private void recieveFile_bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            databaseManager.AddToTransmission(token, DateTime.Now, new FileInfo(fileName).Length, TransmissionType.Upload);
            Application.Current.Dispatcher.Invoke(new Action(() => { databaseManager.LoadTransmissions(transmissions); }));
            if (e.Cancelled)
                Console.WriteLine("Stopped by button");
            else
                Console.WriteLine("Stopped by the end of operation");
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
            var path = fileName.Replace("Main_Folder", @"C:\Directory_test");
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
                            //Thread.Sleep(1);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }

                    }
                }
                buffer = new byte[BUFFER_SIZE];
                networkStream.Close();
            }
            e.Result = "zwracany typ";
        }
        private void sendFile_bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage.ToString());
        }
        private void sendFile_bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Console.WriteLine(e.Error.ToString());
            }
            if (e.Cancelled)
                Console.WriteLine("Stopped by button");
            else
                Console.WriteLine("Stopped by the end of operation");
        }

        internal void setContainers(DatabaseManager databaseManager, ObservableCollection<Transmission> transmissions, string token)
        {
            this.transmissions = transmissions;
            this.databaseManager = databaseManager;
            this.token = token;
        }
        #endregion

    }
}
