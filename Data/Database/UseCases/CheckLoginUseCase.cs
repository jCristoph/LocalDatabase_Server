using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Server.Data.Database.UseCases
{
    public static class CheckLoginUseCase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="connectionString"></param>
        /// <returns>array [0] -> token
        /// [1] -> limit</returns>
        public static string[] invoke(string login, string password, SqlConnection connectionString)
        {
            SqlCommand query = new SqlCommand
            {
                Connection = connectionString,
                CommandText = $"SELECT * FROM [User] WHERE Login = '{login}' and Password = '{password}'"
            };

            SqlDataAdapter adapter = new SqlDataAdapter(query);
            DataTable table = new DataTable();
            adapter.Fill(table);

            if (table.Rows.Count == 1)
            {
                double temp = (double)(Int64.Parse(table.Rows[0].ItemArray.GetValue(6).ToString()));
                return new string[] { table.Rows[0].ItemArray.GetValue(5).ToString(), temp.ToString() };
            }
            else
            {
                return new string[] { "ERROR", "0" };
            }
        }
    }
}