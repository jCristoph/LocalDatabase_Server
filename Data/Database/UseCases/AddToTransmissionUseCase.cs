using System;
using System.Data.SqlClient;
using System.IO;

namespace LocalDatabase_Server.Database
{
    public static class AddToTransmissionUseCase
    {
        public static void invoke(string token, DateTime transmissionDate, long fileSize, TransmissionType transmissionType, SqlConnection connectionString)
        {
            SqlCommand query = new SqlCommand
            {
                Connection = connectionString,
                CommandText = $@"INSERT INTO [Transaction]([transactionDate],[fileSize],[userToken],[transactionType]) VALUES ('{transmissionDate.ToString("MM/dd/yyyy HH:mm:ss")}', '{fileSize}', '{token}' ,'{(int)transmissionType}')"
            };
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }
        public static void invoke(Transmission t, SqlConnection connectionString)
        {
            SqlCommand query = new SqlCommand
            {
                Connection = connectionString,
                CommandText = $@"INSERT INTO [Transaction]([transactionDate],[fileSize],[userToken],[transactionType]) VALUES ('{t.date.ToString("MM/dd/yyyy HH:mm:ss")}', '{t.fileSize}', '{t.userToken}' ,'{(int)t.transmissionType}')"
            };
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }
    }
}
