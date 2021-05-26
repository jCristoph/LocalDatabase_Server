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
        public string CheckLogin(string login, string password)
        {
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;
            zapytanie.CommandText = "SELECT * FROM [User] WHERE Login = '" + login + "' and Password = '" + password + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
            DataTable tabela = new DataTable();
            adapter.Fill(tabela);
            if (tabela.Rows.Count == 1)
                return tabela.Rows[0].ItemArray.GetValue(5).ToString();
            else
                return "ERROR";
        }
        public void AddUser(string surname, string name)
        {
            SqlCommand zapytanie = new SqlCommand();
            string token = generateRandomString();
            zapytanie.CommandText = @"INSERT INTO [User]([Name],[Surname],[Login],[Password],[Token]) VALUES ('" + surname + "', '" + name + "', '" + generateLogin(surname, name) + "', '" + token + "', '" + token + "')";
            zapytanie.Connection = polaczenie;
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();

            string pathString = System.IO.Path.Combine(@"C:\Directory_test", token);
            System.IO.Directory.CreateDirectory(pathString);
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
                User u = new User(Int32.Parse(tabela.Rows[i].ItemArray.GetValue(0).ToString()), tabela.Rows[i].ItemArray.GetValue(1).ToString(), tabela.Rows[i].ItemArray.GetValue(2).ToString(), tabela.Rows[i].ItemArray.GetValue(3).ToString(), tabela.Rows[i].ItemArray.GetValue(4).ToString(), tabela.Rows[i].ItemArray.GetValue(5).ToString());
                users.Add(u);
            }
            return users;
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
