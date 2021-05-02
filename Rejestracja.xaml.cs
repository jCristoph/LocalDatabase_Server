using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;

namespace PZ_Panel_Logowania
{
    /// <summary>
    /// Logika interakcji dla klasy Rejestracja.xaml
    /// </summary>
    public partial class Rejestracja : UserControl
    {
        public Rejestracja()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Txt_haslo.Text != Txt_haslo2.Text)
                MessageBox.Show("Podano różne hasła");
            else
            {
                SqlConnection polaczenie = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\krzem\source\repos\PZ_Panel_Logowania\PZ_Panel_Logowania\Baza_Danych\PZ_BD.mdf;Integrated Security=True;Connect Timeout=30");
                SqlCommand zapytanie = new SqlCommand();
                zapytanie.CommandText = "SELECT * FROM [dbo].[User]";
                zapytanie.Connection = polaczenie;
                SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
                DataTable tabela = new DataTable();
                adapter.Fill(tabela);
                int i = tabela.Rows.Count + 1;
                
                
                zapytanie.CommandText = @"INSERT INTO [User]([Name],[Surname],[Login],[Password],[Token]) VALUES ('" + Txt_imie.Text.Trim() + "', '" + Txt_nazwisko.Text.Trim() + "', '" + Txt_login.Text.Trim() + "', '" + Txt_haslo.Text.Trim() + "', '"+ i +"')";
                polaczenie.Open();
                zapytanie.ExecuteNonQuery();
                polaczenie.Close();

                MessageBox.Show("Dodano użytkownika");

                /*
                SqlDataAdapter adapter = new SqlDataAdapter(zapytanie);
                DataTable tabela = new DataTable();

                adapter.Fill(tabela);*/
            }

        }

        private void TextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void Txt_nazwisko_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Txt_login_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Txt_haslo_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Txt_haslo2_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
