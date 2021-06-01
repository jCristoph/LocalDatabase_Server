using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Server.Database
{
    class Transmission
    {
        public int id { set; get; }
        public DateTime date { set; get; }
        public long fileSize { set; get; }
        public string userToken { set; get; }
        public string transmissionType { set; get; } // 0 - download, 1 - upload, 2 - delete, 3 - share

        public Transmission(int id, DateTime date, long fileSize, string userToken, int transmissionType)
        {
            this.id = id;
            this.date = date;
            this.fileSize = fileSize;
            this.userToken = userToken;
            switch (transmissionType)
            {
                case 0:
                    this.transmissionType = "pobieranie";
                    break;
                case 1:
                    this.transmissionType = "wysyłanie";
                    break;
                case 2:
                    this.transmissionType = "usuwanie";
                    break;
                case 3:
                    this.transmissionType = "udostępnianie";
                    break;
            }
        }
    }
}
