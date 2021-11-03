using LocalDatabase_Server.Database;
using System.Data.SqlClient;

namespace LocalDatabase_Server.Data.Database.UseCases
{
    public static class GetUserByTokenUseCase
    {
        public static User invoke(string token, SqlConnection connectionString)
        {

            SqlCommand query = new SqlCommand
            {
                CommandText = $@"SELECT * FROM [User] WHERE token = '{token}'",
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
                string login = dr["Login"].ToString();
                string password = dr["Password"].ToString();
                long limit = (long)dr["Limit"];
                user = new User(id: userID, surname, name, login, password, token, limit);
            }

            connectionString.Close();
            return user;
        }
    }
}
