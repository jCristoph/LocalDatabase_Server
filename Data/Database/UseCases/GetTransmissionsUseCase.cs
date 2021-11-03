using LocalDatabase_Server.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace LocalDatabase_Server.Data.Database.UseCases
{
    public static class GetTransmissionsUseCase
    {
        public static List<Transmission> invoke(SqlConnection connectionString)
        {
            SqlCommand query = new SqlCommand
            {
                Connection = connectionString,
                CommandText = "SELECT * FROM [Transaction]"
            };

            SqlDataAdapter adapter = new SqlDataAdapter(query);
            DataTable table = new DataTable();
            adapter.Fill(table);

            List<Transmission> transmissionList = new List<Transmission>();

            for (int i = 0; i < table.Rows.Count; i++)
            {

                //when the data is loaded then program has to clean it all. From each row it has to load a correct data and cast it to right data type.
                //Check Transmissions table in database to see what every column contains.
                int id = Int32.Parse(table.Rows[i].ItemArray.GetValue(0).ToString());
                DateTime date = (DateTime)table.Rows[i].ItemArray.GetValue(1);
                long fileSize = Int64.Parse(table.Rows[i].ItemArray.GetValue(2).ToString());
                string userToken = table.Rows[i].ItemArray.GetValue(3).ToString();
                TransmissionType transmissionType = (TransmissionType)Int32.Parse(table.Rows[i].ItemArray.GetValue(4).ToString());

                Transmission t = new Transmission(id, date, fileSize, userToken, transmissionType);
                transmissionList.Add(t);
            }
            return transmissionList;
        }
    }
}
