using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LocalDatabase_Server.Registration
{
    /// <summary>
    /// Logika interakcji dla klasy Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection polaczenie = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\krzem\source\repos\PZ_Panel_Logowania\PZ_Panel_Logowania\Baza_Danych\PZ_BD.mdf;Integrated Security=True;Connect Timeout=30");
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.CommandText = "SELECT * FROM [dbo].[User]";
            zapytanie.Connection = polaczenie;
            SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
            DataTable tabela = new DataTable();
            adapter.Fill(tabela);

            string token = generateRandomString();
            zapytanie.CommandText = @"INSERT INTO [User]([Name],[Surname],[Login],[Password],[Token]) VALUES ('" + SurnameText.Text + "', '" + NameText.Text + "', '" + generateLogin() + "', '" + token + "', '" + token + "')";
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();

            string pathString = System.IO.Path.Combine(@"C:\Directory_test", token);
            System.IO.Directory.CreateDirectory(pathString);

            MessageBox.Show("Dodano użytkownika");

            /*
            SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
            DataTable tabela = new DataTable();
            adapter.Fill(tabela);*/

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
        private string generateLogin()
        {
            return SurnameText.Text + '.' + NameText.Text;
        }
    }
}
