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
                CommandText = $@"INSERT INTO [Transaction]([transactionDate],[fileSize],[userToken],[transactionType]) VALUES ('{transmissionDate}', '{fileSize}', '{token}' ,'{(int)transmissionType}')"
            };
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }
    }
}
