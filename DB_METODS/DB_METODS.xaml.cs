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

namespace LocalDatabase_Server.DB_METODS
{
    /// <summary>
    /// Logika interakcji dla klasy DB_METODS.xaml
    /// </summary>
    public partial class DB_METODS : Page
    {
        public DB_METODS()
        {
            InitializeComponent();
        }



        private void sharedfile_Click(object sender, RoutedEventArgs e)
        {
            string recipientToken = recipientTokenText.Text;
            string senderToken = senderTokenText.Text;
            string path = pathText.Text;
            int permissions = int.Parse(permissionsText.Text);

            SqlConnection polaczenie = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\krzem\source\repos\PZ_Panel_Logowania\PZ_Panel_Logowania\Baza_Danych\PZ_BD.mdf;Integrated Security=True;Connect Timeout=30");
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;

            zapytanie.CommandText = @"INSERT INTO [SharedFile]([path],[recipientToken],[senderToken],[permissions]) VALUES ('" + path + "', '" + recipientToken + "', '" + senderToken + "', '" + permissions + "')";
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();

            MessageBox.Show("Dodano do tabeli");
        }

        private void transmission_Click(object sender, RoutedEventArgs e)
        {
            string userToken = UserTokenText.Text;
            string TransmissionDate = transmissionDateText.Text;
            float fileSize = float.Parse(fileSizeText.Text);
            int transmissionType = int.Parse(transmissionTypeText.Text);

            SqlConnection polaczenie = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\krzem\source\repos\PZ_Panel_Logowania\PZ_Panel_Logowania\Baza_Danych\PZ_BD.mdf;Integrated Security=True;Connect Timeout=30");
            SqlCommand zapytanie = new SqlCommand();
            zapytanie.Connection = polaczenie;

            zapytanie.CommandText = @"INSERT INTO [Transaction]([transactionDate],[fileSize],[userToken],[transactionType]) VALUES ('" + TransmissionDate + "', '" + fileSize + "', '" + userToken + "', '" + transmissionType + "')";
            polaczenie.Open();
            zapytanie.ExecuteNonQuery();
            polaczenie.Close();

            MessageBox.Show("Dodano do tabeli");
        }
    }
}
