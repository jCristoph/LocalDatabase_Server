using System.Data.SqlClient;
using System.IO;

namespace LocalDatabase_Server.Database
{
    public static class ChangePasswordUseCase
    {
        public static void invoke(string token, string newPassword, SqlConnection connectionString)
        {

            SqlCommand query = new SqlCommand
            {
                CommandText = $@"UPDATE [User] SET password = '{newPassword}' WHERE token = '{token }'",
                Connection = connectionString
            };
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }
    }
}
