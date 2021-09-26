using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LocalDatabase_Server.Server
{
    public class FileTransporter
    {
        private string ip;
        private FileInfo file;
        Socket socket;

        public FileTransporter(string ip, string fileName)
        {
            this.ip = ip;
            file = new FileInfo(fileName);
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

        public async Task sendFile()
        {
            var readed = -1;
            var buffer = new Byte[4096];
            using (var networkStream = new BufferedStream(new NetworkStream(socket, false)))
            using (var fileStream = file.OpenRead())
            {
                while (readed != 0)
                {
                    readed = fileStream.Read(buffer, 0, buffer.Length);
                    if (readed != 0)
                    {
                        networkStream.Write(buffer, 0, buffer.Length);
                    }
                }
                await networkStream.FlushAsync();
            }
        }

        public async Task recieveFile()
        {
            var readed = -1;
            var buffer = new Byte[4096];
            using (var fileStream = file.OpenWrite())
            using (var networkStream = new NetworkStream(socket, false))
            {
                //networkStream.ReadTimeout = 1000;
                while (readed != 0)
                {
                    readed = networkStream.Read(buffer, 0, buffer.Length);
                    fileStream.Write(buffer, 0, readed);
                }
            }
        }
    }
}
