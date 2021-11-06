using System.Data.SqlClient;
using System.IO;

namespace LocalDatabase_Server.Database
{
    public static class ChangeLimitUseCase
    {
        public static void invoke(string token, long newLimit, SqlConnection connectionString)
        {

            SqlCommand query = new SqlCommand
            {
                CommandText = $@"UPDATE [User]SET limit = '{newLimit}' WHERE token = '{token}'",
                Connection = connectionString
            };
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }
    }
}
