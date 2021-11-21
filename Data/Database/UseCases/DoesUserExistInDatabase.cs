
using LocalDatabase_Server.Database;
using System;
using System.Data;
using System.Data.SqlClient;

namespace LocalDatabase_Server.Data.Database.UseCases
{
    class DoesUserExistInDatabase
    {
        /// <summary>
        /// Checks if user exist
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <param name="connectionString"></param>
        /// <returns>true if user exist, false when it doesn't</returns>
        public static bool invoke(string login, string password, SqlConnection connectionString)
        {
            User user = GetUserByLoginAndPasswordUseCase.invoke(login, password, connectionString);
            return user != null;
        }
    }
}