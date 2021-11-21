using LocalDatabase_Server.Database;
using System.Data.SqlClient;

namespace LocalDatabase_Server.Data.Database.UseCases
{
    public static class GetUserByLoginAndPasswordUseCase
    {
        public static User invoke(string login, string password, SqlConnection connectionString)
        {
            SqlCommand query = new SqlCommand
            {
                CommandText = $"SELECT * FROM [User] WHERE Login = '{login}' and Password = '{password}'",
                Connection = connectionString
            };
            connectionString.Open();

            var dr = query.ExecuteReader();
            User user = null;
            if (dr.Read())
            {
                int userID = (int)dr["userID"];
                string name = dr["Name"].ToString();
                string surname = dr["Surname"].ToString();
                string loginStr = dr["Login"].ToString();
                string passwordStr = dr["Password"].ToString();
                string token = dr["Token"].ToString();
                long limit = (long)dr["Limit"];
                user = new User(id: userID, surname, name, loginStr, passwordStr, token, limit);
            }

            connectionString.Close();
            return user;
        }
    }
}
