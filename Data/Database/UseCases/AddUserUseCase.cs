using LocalDatabase_Server.Data.Database.UseCases;
using LocalDatabase_Server.Directory;
using System.Data.SqlClient;
using System.IO;

namespace LocalDatabase_Server.Database
{
    public static class AddUserUseCase
    {
        public static bool invoke(string surname, string name, SqlConnection connectionString)
        {

            SqlCommand query = new SqlCommand();
            string token = Generator.GenerateToken();
            string login = Generator.GenerateLogin(surname, name);
            // TODO: for now it's random, method is moved to client side
            string password = Generator.GenerateRandomString();

            bool doesUserExist = DoesUserExistInDatabase.invoke(login, password, connectionString);
            if (doesUserExist) return false;

            query.CommandText = "INSERT INTO [User]([Name],[Surname],[Login],[Password],[Token])";
            query.CommandText += $"VALUES ('{surname}', '{name}', '{login}', '{password}', '{token}')";
            query.Connection = connectionString;
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();

            string pathString = Path.Combine(SettingsManager.Instance.GetSavePath(), login);
            System.IO.Directory.CreateDirectory(pathString);

            return true;
        }
    }
}
