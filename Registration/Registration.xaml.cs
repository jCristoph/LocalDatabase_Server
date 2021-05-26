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
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }
        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            Database.DatabaseManager dm = new Database.DatabaseManager();
            dm.AddUser(surnameTextBox.Text, nameTextBox.Text);

            MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Dodano użytkownika", false);
            mp.ShowDialog();
            this.Close();
        }



        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
