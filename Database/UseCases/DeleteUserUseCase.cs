using System.Data.SqlClient;
using System.IO;

namespace LocalDatabase_Server.Database
{
    public static class DeleteUserUseCase
    {
        public static void invoke(string token, SqlConnection connectionString)
        {

            SqlCommand query = new SqlCommand
            {
                CommandText = $@"DELETE FROM [User] WHERE token = '{token}'",
                Connection = connectionString
            };
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }
    }
}
