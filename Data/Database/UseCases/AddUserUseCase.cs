using LocalDatabase_Server.Directory;
using System.Data.SqlClient;
using System.IO;

namespace LocalDatabase_Server.Database
{
    public static class AddUserUseCase
    {
        public static void invoke(string surname, string name, string password, SqlConnection connectionString)
        {

            SqlCommand query = new SqlCommand();
            string token = Generator.GenerateToken();
            string login = Generator.GenerateLogin(surname, name);
           
            query.CommandText = "INSERT INTO [User]([Name],[Surname],[Login],[Password],[Token])";
            query.CommandText += $"VALUES ('{surname}', '{name}', '{login}', '{password}', '{token}')";
            query.Connection = connectionString;
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();

            string pathString = Path.Combine(SettingsManager.Instance.GetSavePath(), login);
            System.IO.Directory.CreateDirectory(pathString);
        }
    }
}