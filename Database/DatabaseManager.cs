using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace LocalDatabase_Server.Database
{
    class DatabaseManager
    {
        //container with users 

        private readonly ObservableCollection<User> users;
        readonly string dbFilePath;
        readonly SqlConnection connectionString;

        public DatabaseManager()
        {
            users = new ObservableCollection<User>();
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;

            dbFilePath = $"{projectDirectory}\\Database\\PZ_BD.mdf";
            connectionString = new SqlConnection($@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={dbFilePath};Integrated Security=True;Connect Timeout=30");
        }

        #region Database getters
        //method that adds new user into db by two parameters - name and surname. The rest of parameters are created here by random character string or string buliding.
        //N in query means that we can use polish characters
        public void AddUser(string surname, string name)
        {

            SqlCommand query = new SqlCommand();
            string token = generateRandomString(); //token has to be unique so better to check if it isnt duplicated
            query.CommandText = "INSERT INTO [User]([Name],[Surname],[Login],[Password],[Token])";
            query.CommandText += $"VALUES ('{name}', '{surname}', '{generateLogin(surname, name)}', '{token}', '{token}')";
            query.Connection = connectionString;
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();

            string pathString = Path.Combine(@"C:\Directory_test", token);
            Directory.CreateDirectory(pathString);
        }

        //method that edit a row in user table in db. simple update query.
        public void DeleteUser(string token)
        {
            SqlCommand query = new SqlCommand();
            query.CommandText = @"DELETE FROM [User] WHERE token = '" + token + "'";
            query.Connection = connectionString;
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }

        //method that edit a row in user table in db. simple update query.
        public void ChangeLimit(long newLimit, string token)
        {
            SqlCommand query = new SqlCommand();
            query.CommandText = @"UPDATE [User]
                                      SET limit = '" + newLimit + "'" +
                                      "WHERE token = '" + token + "'";
            query.Connection = connectionString;
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }

        //method that edit a row in user table in db. simple update query.
        public void ChangePassword(string newPassword, string token)
        {
            SqlCommand query = new SqlCommand();
            query.CommandText = @"UPDATE [User]
                                      SET password = '" + newPassword + "'" +
                                      "WHERE token = '" + token + "'";
            query.Connection = connectionString;
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }

        //method that add new transmission to transmission table in db. simple insert into query.
        public void AddToTransmission(string userToken, DateTime TransmissionDate, long fileSize, int transmissionType)
        {
            SqlCommand query = new SqlCommand();
            query.Connection = connectionString;

            query.CommandText = @"INSERT INTO [Transaction]([transactionDate],[fileSize],[userToken],[transactionType]) VALUES ('" + TransmissionDate + "', '" + fileSize + "', '" + userToken + "', '" + transmissionType + "')";
            connectionString.Open();
            query.ExecuteNonQuery();
            connectionString.Close();
        }
        #endregion

        #region Database setters
        //a method that checks if password is correct. If the password is correct it returns token and how many space user has to use.
        //If password is incorrect then returns "error message"
        public string[] CheckLogin(string login, string password)
        {
            SqlCommand query = new SqlCommand();
            query.Connection = connectionString;
            query.CommandText = "SELECT * FROM [User] WHERE Login = '" + login + "' and Password = '" + password + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(query);
            DataTable table = new DataTable();
            adapter.Fill(table);
            if (table.Rows.Count == 1)
            {
                double temp = (double)(Int64.Parse(table.Rows[0].ItemArray.GetValue(6).ToString()) / 1000000000.0);
                return new string[] { table.Rows[0].ItemArray.GetValue(5).ToString(), temp.ToString() }; //array contains token [0] and limit [1]
            }
            else
                return new string[] { "ERROR", "0" };
        }

        //method with simple sql query - load all users from database to app
        public ObservableCollection<User> LoadUsers()
        {
            SqlCommand query = new SqlCommand();
            query.Connection = connectionString;
            query.CommandText = "SELECT * FROM [User]";
            SqlDataAdapter adapter = new SqlDataAdapter(query);
            DataTable table = new DataTable();
            adapter.Fill(table);
            for(int i = 0; i < table.Rows.Count; i++)
            {
                //when the data is loaded then program has to clean it all. From each row it has to load a correct data and cast it to right data type.
                //Check Users table in database to see what every column contains.
                User u = new User(Int32.Parse(table.Rows[i].ItemArray.GetValue(0).ToString()), table.Rows[i].ItemArray.GetValue(1).ToString(), table.Rows[i].ItemArray.GetValue(2).ToString(), table.Rows[i].ItemArray.GetValue(3).ToString(), table.Rows[i].ItemArray.GetValue(4).ToString(), table.Rows[i].ItemArray.GetValue(5).ToString(), Int64.Parse(table.Rows[i].ItemArray.GetValue(6).ToString()));
                users.Add(u);
            }
            return users;
        }

        //method that load all users and then from container looking for users witch tokens is the same with parameter container. Returns all users data.
        //this method could be improve by looking for in database - without loading all users to container - memory save
        public ObservableCollection<User> FindUserByToken(ObservableCollection<string> tokens)
        {
            ObservableCollection<User> matchingUsers = new ObservableCollection<User>();
            LoadUsers();
            for(int i = 0; i < tokens.Count; i++)
            {
                for(int j = 0; j < users.Count; j++)
                {
                    if (tokens[i].Equals(users[j].token))
                    {
                        matchingUsers.Add(users[j]);
                    }
                }
            }
            return matchingUsers;
        }

        //method that load all users and then from container looking for user witch token is the same with parameter. Returns all user data. The only difference is that here we have only one token - not container of tokens.
        //this method could be improve by looking for in database - without loading all users to container - memory save
        public User FindUserByToken(string token)
        {
            LoadUsers();
            for (int j = 0; j < users.Count; j++)
            {
                if (token.Equals(users[j].token))
                {
                    return users[j];
                }
            }
            return null;
        }

        //a method that is needed to load transmissions to app. Then they could be shown in gui. To keep one transmissions container it is transferred by reference.
        public void LoadTransmissions(ObservableCollection<Transmission> transmissions)
        {
            transmissions.Clear();
            SqlCommand query = new SqlCommand();
            query.Connection = connectionString;
            query.CommandText = "SELECT * FROM [Transaction]";
            SqlDataAdapter adapter = new SqlDataAdapter(query);
            DataTable table = new DataTable();
            adapter.Fill(table);
            for (int i = 0; i < table.Rows.Count; i++)
            {
                //when the data is loaded then program has to clean it all. From each row it has to load a correct data and cast it to right data type.
                //Check Transmissions table in database to see what every column contains.
                Transmission t = new Transmission(Int32.Parse(table.Rows[i].ItemArray.GetValue(0).ToString()), (DateTime)table.Rows[i].ItemArray.GetValue(1), Int64.Parse(table.Rows[i].ItemArray.GetValue(2).ToString()), table.Rows[i].ItemArray.GetValue(3).ToString(), Int32.Parse(table.Rows[i].ItemArray.GetValue(4).ToString()));
                transmissions.Add(t);
            }
        }
        #endregion

        /*
         * WATCHOUT THIS METHOD IS UNSAFE - IT HAS TO BE CHANGED IN FUTURE
         */
        private string generateRandomString()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[10];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        //method that creates a login from surname and name
        private string generateLogin(string surname, string name)
        {
            return surname + '.' + name;
        }
    }
}
