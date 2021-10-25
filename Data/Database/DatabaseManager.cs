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
        private static List<User> users;

        private DatabaseManager()
        {
            connectionString = new ConnectionString().GetConnectionString();
            users = new List<User>();
            LoadUsers();
        }

        public List<User> GetUsers()
        {
            LoadUsers();
            return users;
        }

        #region Database getters
        //method that adds new user into db by two parameters - name and surname. The rest of parameters are created here by random character string or string buliding.
        //N in query means that we can use polish characters
        public void AddUser(string surname, string name)
        {
            AddUserUseCase.invoke(surname, name, connectionString);
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
        /// Checks if password is correct.
        /// If incorrect then 
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns> If correct returns string[2] where [0] is token and [1] is space to use.
        /// If incorrect returns error message</returns>
        public string[] CheckLogin(string login, string password)
        {
            string [] result = CheckLoginUseCase.invoke(login, password, connectionString);
            return result;
        }

        //method with simple sql query - load all users from database to app
        private void LoadUsers()
        {
            SqlCommand query = new SqlCommand
            {
                Connection = connectionString,
                CommandText = "SELECT * FROM [User]"
            };

            SqlDataAdapter adapter = new SqlDataAdapter(query);
            DataTable table = new DataTable();
            adapter.Fill(table);
            List<User> usersList = new List<User>();

            for(int i = 0; i < table.Rows.Count; i++)
            {
                //when the data is loaded then program has to clean it all. From each row it has to load a correct data and cast it to right data type.
                //Check Users table in database to see what every column contains.
                User u = new User(Int32.Parse(table.Rows[i].ItemArray.GetValue(0).ToString()), table.Rows[i].ItemArray.GetValue(1).ToString(), table.Rows[i].ItemArray.GetValue(2).ToString(), table.Rows[i].ItemArray.GetValue(3).ToString(), table.Rows[i].ItemArray.GetValue(4).ToString(), table.Rows[i].ItemArray.GetValue(5).ToString(), Int64.Parse(table.Rows[i].ItemArray.GetValue(6).ToString()));
                usersList.Add(u);
            }

            users = usersList;
        }

        //method that load all users and then from container looking for users witch tokens is the same with parameter container. Returns all users data.
        //this method could be improve by looking for in database - without loading all users to container - memory save
        public User FindUserByToken(string token)
        {
            User matchingUser = GetUserByTokenUseCase.invoke(token, connectionString);
            return matchingUser;
        }

        //a method that is needed to load transmissions to app. Then they could be shown in gui. To keep one transmissions container it is transferred by reference.
        public void LoadTransmissions(ObservableCollection<Transmission> transmissions)
        {
            transmissions.Clear();
            SqlCommand query = new SqlCommand
            {
                Connection = connectionString,
                CommandText = "SELECT * FROM [Transaction]"
            };
            SqlDataAdapter adapter = new SqlDataAdapter(query);
            DataTable table = new DataTable();
            adapter.Fill(table);
            for (int i = 0; i < table.Rows.Count; i++)
            {
              
                //when the data is loaded then program has to clean it all. From each row it has to load a correct data and cast it to right data type.
                //Check Transmissions table in database to see what every column contains.
                int id = Int32.Parse(table.Rows[i].ItemArray.GetValue(0).ToString());
                DateTime date = (DateTime)table.Rows[i].ItemArray.GetValue(1);
                long fileSize = Int64.Parse(table.Rows[i].ItemArray.GetValue(2).ToString());
                string userToken = table.Rows[i].ItemArray.GetValue(3).ToString();
                TransmissionType transmissionType = (TransmissionType)Int32.Parse(table.Rows[i].ItemArray.GetValue(4).ToString());

                Transmission t = new Transmission(id, date, fileSize, userToken, transmissionType);
                transmissions.Add(t);
            }
        }
        #endregion
    }
}
