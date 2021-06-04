using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalDatabase_Server.Database
{
    class DatabaseManager
    {
        private ObservableCollection<User> users;
        SqlConnection polaczenie = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\thekr\source\repos\LocalDatabase_Server\PZ_BD.mdf;Integrated Security=True;Connect Timeout=30");

        public DatabaseManager()
        {
            users = new ObservableCollection<User>();
        }
        public string[] CheckLogin(string login, string password)
        {
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;
            zapytanie.CommandText = "SELECT * FROM [User] WHERE Login = '" + login + "' and Password = '" + password + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
            DataTable tabela = new DataTable();
            adapter.Fill(tabela);
            if (tabela.Rows.Count == 1)
            {
                double temp = (double)(Int64.Parse(tabela.Rows[0].ItemArray.GetValue(6).ToString()) / 1000000000.0);
                return new string[] { tabela.Rows[0].ItemArray.GetValue(5).ToString(), temp.ToString() };
            }
            else
                return new string[] { "ERROR", "0" }; 
        }
        public void AddUser(string surname, string name)
        {
            SqlCommand zapytanie = new SqlCommand();
            string token = generateRandomString();
            zapytanie.CommandText = @"INSERT INTO [User]([Name],[Surname],[Login],[Password],[Token]) VALUES (N'" + surname + "', N'" + name + "', '" + generateLogin(surname, name) + "', '" + token + "', '" + token + "')";
            zapytanie.Connection = polaczenie;
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();

            string pathString = System.IO.Path.Combine(@"C:\Directory_test", token);
            System.IO.Directory.CreateDirectory(pathString);
        }

        public void DeleteUser(string token)
        {
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.CommandText = @"DELETE FROM [User] WHERE token = '" + token + "'";
            zapytanie.Connection = polaczenie;
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();
        }

        public void ChangeLimit(long newLimit, string token)
        {
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.CommandText = @"UPDATE [User]
                                      SET limit = '" + newLimit + "'" +
                                      "WHERE token = '" + token + "'";
            zapytanie.Connection = polaczenie;
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();
        }

        public void ChangePassword(string newPassword, string token)
        {
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.CommandText = @"UPDATE [User]
                                      SET password = '" + newPassword + "'" +
                                      "WHERE token = '" + token + "'";
            zapytanie.Connection = polaczenie;
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();
        }
        public ObservableCollection<User> LoadUsers()
        {
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;
            zapytanie.CommandText = "SELECT * FROM [User]";
            SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
            DataTable tabela = new DataTable();
            adapter.Fill(tabela);
            for(int i = 0; i < tabela.Rows.Count; i++)
            {
                User u = new User(Int32.Parse(tabela.Rows[i].ItemArray.GetValue(0).ToString()), tabela.Rows[i].ItemArray.GetValue(1).ToString(), tabela.Rows[i].ItemArray.GetValue(2).ToString(), tabela.Rows[i].ItemArray.GetValue(3).ToString(), tabela.Rows[i].ItemArray.GetValue(4).ToString(), tabela.Rows[i].ItemArray.GetValue(5).ToString(), Int64.Parse(tabela.Rows[i].ItemArray.GetValue(6).ToString()));
                users.Add(u);
            }
            return users;
        }

        public void AddToSharedFile(string senderToken, string recipientToken, string path, string permissions)
        {
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;

            zapytanie.CommandText = @"INSERT INTO [SharedFile]([path],[recipientToken],[senderToken],[permissions]) VALUES (N'" + path + "', '" + recipientToken + "', '" + senderToken + "', '" + Convert.ToInt32(permissions) + "')";
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();
        }
        
        public ObservableCollection<string> FindInSharedFile(string data)
        {
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;
            //if an input string is a path
            if(data.Length > 10)
            {
                zapytanie.CommandText = "SELECT * FROM [SharedFile] WHERE Path = '" + data + "'";
                SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
                DataTable tabela = new DataTable();
                adapter.Fill(tabela);
                ObservableCollection<string> userTokens = new ObservableCollection<string>();
                for (int i = 0; i < tabela.Rows.Count; i++)
                {
                    userTokens.Add(tabela.Rows[i].ItemArray.GetValue(2).ToString());
                }
                return userTokens;
            }
            //if an input string is a path
            else
            {
                zapytanie.CommandText = "SELECT * FROM [SharedFile] WHERE recipientToken = '" + data + "'";
                SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
                DataTable tabela = new DataTable();
                adapter.Fill(tabela);
                ObservableCollection<string> paths = new ObservableCollection<string>();
                for (int i = 0; i < tabela.Rows.Count; i++)
                {
                    paths.Add(tabela.Rows[i].ItemArray.GetValue(1).ToString());
                }
                return paths;
            }
        }

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

        public void AddToTransmission(string userToken, DateTime TransmissionDate, long fileSize, int transmissionType)
        {
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;

            zapytanie.CommandText = @"INSERT INTO [Transaction]([transactionDate],[fileSize],[userToken],[transactionType]) VALUES ('" + TransmissionDate + "', '" + fileSize + "', '" + userToken + "', '" + transmissionType + "')";
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();
        }

        public void LoadTransmissions(ObservableCollection<Transmission> transmissions)
        {
            //ObservableCollection<Transmission> transmissons = new ObservableCollection<Transmission>();
            transmissions.Clear();
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;
            zapytanie.CommandText = "SELECT * FROM [Transaction]";
            SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
            DataTable tabela = new DataTable();
            adapter.Fill(tabela);
            for (int i = 0; i < tabela.Rows.Count; i++)
            {
                Transmission t = new Transmission(Int32.Parse(tabela.Rows[i].ItemArray.GetValue(0).ToString()), (DateTime)tabela.Rows[i].ItemArray.GetValue(1), Int64.Parse(tabela.Rows[i].ItemArray.GetValue(2).ToString()), tabela.Rows[i].ItemArray.GetValue(3).ToString(), Int32.Parse(tabela.Rows[i].ItemArray.GetValue(4).ToString()));
                transmissions.Add(t);
            }
           // return transmissons;
        }

        private string generateRandomString()
        {
            //unsafe
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[10];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }
        private string generateLogin(string surname, string name)
        {
            return surname + '.' + name;
        }
    }
}
