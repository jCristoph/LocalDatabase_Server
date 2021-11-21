using LocalDatabase_Server.Data.Database.UseCases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;

namespace LocalDatabase_Server.Database
{
    public sealed class DatabaseManager
    {
        private static readonly Lazy<DatabaseManager> lazy = new Lazy<DatabaseManager>(() => new DatabaseManager());
        public static DatabaseManager Instance { get { return lazy.Value; } }

        private readonly SqlConnection connectionString;
        private static ObservableCollection<User> users;
        private static ObservableCollection<Transmission> transmissions;

        private DatabaseManager()
        {
            connectionString = new ConnectionString().GetConnectionString();
            transmissions = GetTransmissions();
            users = GetUsers();
        }

        public ObservableCollection<User> GetUsers()
        {
            LoadUsers();
            return users;
        }

        public ObservableCollection<Transmission> GetTransmissions()
        {
            LoadTransmissions();
            return transmissions;
        }

        #region Database getters
        //method that adds new user into db by two parameters - name and surname. The rest of parameters are created here by random character string or string buliding.
        //N in query means that we can use polish characters
        public void AddUser(string surname, string name)
        {
            bool isAdded = AddUserUseCase.invoke(surname, name, password, connectionString);
            if (isAdded)
            {
                // success 
            }
            else
            {
                // handle error
            }
        }

        public void DeleteUser(string token)
        {
            DeleteUserUseCase.invoke(token, connectionString);
        }

        public void ChangeLimit(long newLimit, string token)
        {
            ChangeLimitUseCase.invoke(token, newLimit, connectionString);
        }

        public void ChangePassword(string newPassword, string token)
        {
            ChangePasswordUseCase.invoke(token, newPassword, connectionString);
        }
        
        public void AddToTransmission(string userToken, DateTime transmissionDate, long fileSize, TransmissionType transmissionType)
        {
            AddToTransmissionUseCase.invoke(token: userToken, transmissionDate, fileSize, transmissionType, connectionString);
        }
        #endregion

        #region Database setters
        /// <summary>
        /// Checks if user exists in database.
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns> If correct returns string[2] where [0] is token and [1] is limit.
        /// If incorrect returns error message</returns>
        public string[] Login(string login, string password)
        {
            User user = LoginUseCase.invoke(login, password, connectionString);
            if (user == null) return new string[] { "ERROR", "0" };
            return new string[] { user.token, user.limit.ToString() };
        }

        public User FindUserByToken(string token)
        {
            User matchingUser = GetUserByTokenUseCase.invoke(token, connectionString);
            return matchingUser;
        }

        private void LoadUsers()
        {
            List<User> userList = GetUsersUseCase.invoke(connectionString);
            users = new ObservableCollection<User>(userList);
        }

        private void LoadTransmissions()
        {
            if (transmissions != null)
            {
                transmissions.Clear();
            }
            List<Transmission> transmissionList = GetTransmissionsUseCase.invoke(connectionString);
            transmissions = new ObservableCollection<Transmission>(transmissionList);
        }
        #endregion
    }
}
