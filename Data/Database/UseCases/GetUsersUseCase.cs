using LocalDatabase_Server.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace LocalDatabase_Server.Data.Database.UseCases
{
    public static class GetUsersUseCase
    {
        public static List<User> invoke(SqlConnection connectionString)
        {
            SqlCommand query = new SqlCommand
            {
                Connection = connectionString,
                CommandText = "SELECT * FROM [User]"
            };

            SqlDataAdapter adapter = new SqlDataAdapter(query);
            DataTable table = new DataTable();
            adapter.Fill(table);
            List<User> usersList = new List<User>();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                //when the data is loaded then program has to clean it all. From each row it has to load a correct data and cast it to right data type.
                //Check Users table in database to see what every column contains.
                User u = new User(Int32.Parse(table.Rows[i].ItemArray.GetValue(0).ToString()), table.Rows[i].ItemArray.GetValue(1).ToString(), table.Rows[i].ItemArray.GetValue(2).ToString(), table.Rows[i].ItemArray.GetValue(3).ToString(), table.Rows[i].ItemArray.GetValue(4).ToString(), table.Rows[i].ItemArray.GetValue(5).ToString(), Int64.Parse(table.Rows[i].ItemArray.GetValue(6).ToString()));
                usersList.Add(u);
            }
            return usersList;
        }
    }
}
