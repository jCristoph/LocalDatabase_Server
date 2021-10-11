using System;

namespace LocalDatabase_Server.Database
{
    public enum TransmissionType : Int16
    {
        Pobieranie = 0,
        Wysyłanie = 1,
        Usuwanie = 2
    }

    class Transmission
    {
        public int id { set; get; }
        public DateTime date { set; get; }
        public long fileSize { set; get; }
        public string userToken { set; get; }
        public TransmissionType transmissionType { get; set; }

        public Transmission(int id, DateTime date, long fileSize, string userToken, TransmissionType transmissionType)
        {
            this.id = id;
            this.date = date;
            this.fileSize = fileSize;
            this.userToken = userToken;
            this.transmissionType = transmissionType;
        }
    }
}
