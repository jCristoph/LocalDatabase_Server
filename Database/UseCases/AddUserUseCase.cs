using System.Data.SqlClient;
using System.IO;

namespace LocalDatabase_Server.Database
{
    public static class AddUserUseCase
    {
        public static void invoke(string surname, string name, SqlConnection connectionString)
        {

            SqlCommand query = new SqlCommand();
            string token = Generator.GenerateToken();
            string login = Generator.GenerateLogin(surname, name);
            string password = Generator.GenerateRandomString();
            query.CommandText = "INSERT INTO [User]([Name],[Surname],[Login],[Password],[Token])";
            query.CommandText += $"VALUES ('{surname}', '{name}', '{login}', '{password}', '{token}')";
            query.Connection = connectionString;
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();

            string pathString = Path.Combine(@"C:\Directory_test", token);
            Directory.CreateDirectory(pathString);
        }
    }
}
