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
        //constructor. When user open this panel, system load all users and list them.
        public Users()
        {
            dm = new Database.DatabaseManager();
            users = dm.LoadUsers();
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
            listView.ItemsSource = users;
        }

        //if user clicks back button then panel just close and main window shows again.
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            Owner.Show();
            this.Close();
        }

        //this button deletes permamently user i db and also his data (files and folders)
        private void deleteUserButton(object sender, RoutedEventArgs e)
        {
            Database.User u = (Database.User)(((Button)sender).DataContext); //checking who was choosed.
            dm.DeleteUser(u.token); //deleted from db
            string path = @"C:\Directory_test\" + u.token;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].token.Equals(u.token))
                {
                    users.Remove(users[i]); //deleted from container
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true); //deleted his data
                    }
                }
            }
        }

        //this button is used for changing limit of data for user. 
        private void changeLimitButton(object sender, RoutedEventArgs e)
        {
            Database.User u = (Database.User)(((Button)sender).DataContext); //checking who was choosed.
            ChangeLimitPanel.ChangeLimitPanel clp = new ChangeLimitPanel.ChangeLimitPanel(); //new panel opens. Look at Panels/ChangeLimitPanel
            clp.ShowDialog();
            dm.ChangeLimit(clp.newlimit, u.token); //limit is also saved in container
        }
    }
}
