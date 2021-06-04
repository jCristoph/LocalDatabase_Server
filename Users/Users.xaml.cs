using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace LocalDatabase_Server.Users
{
    /// <summary>
    /// Logika interakcji dla klasy Users.xaml
    /// </summary>
    public partial class Users : Window
    {
        private ObservableCollection<Database.User> users;
        Database.DatabaseManager dm;
        public Users()
        {
            dm = new Database.DatabaseManager();
            users = dm.LoadUsers();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            listView.ItemsSource = users;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Owner.Show();
            this.Close();
        }

        private void deleteUserButton(object sender, RoutedEventArgs e)
        {
            Database.User u = (Database.User)(((Button)sender).DataContext);
            dm.DeleteUser(u.token);
            string path = @"C:\Directory_test\" + u.token;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].token.Equals(u.token))
                {
                    users.Remove(users[i]);
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }
            }
        }
        private void changeLimitButton(object sender, RoutedEventArgs e)
        {
            Database.User u = (Database.User)(((Button)sender).DataContext);
            ChangeLimitPanel.ChangeLimitPanel clp = new ChangeLimitPanel.ChangeLimitPanel();
            clp.ShowDialog();
            dm.ChangeLimit(clp.newlimit, u.token);
        }
    }
}
