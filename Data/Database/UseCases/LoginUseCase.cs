using LocalDatabase_Server.Database;
using System.Data.SqlClient;

namespace LocalDatabase_Server.Data.Database.UseCases
{
    public static class LoginUseCase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="connectionString"></param>
        /// <returns><returns>User if user exists in database or null if it doesn't/returns></returns>
        public static User invoke(string login, string password, SqlConnection connectionString)
        {
            User user = GetUserByLoginAndPasswordUseCase.invoke(login, password, connectionString);
            return user;
        }
    }
}