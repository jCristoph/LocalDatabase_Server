using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;


namespace LocalDatabase_Server.Users
{
    /// <summary>
    /// Logika interakcji dla klasy Users.xaml
    /// </summary>
    public partial class Users : Window
    {
        private ObservableCollection<Database.User> users;
        readonly Database.DatabaseManager databaseManager;
        //constructor. When user open this panel, system load all users and list them.
        public Users()
        {
            databaseManager = new Database.DatabaseManager();
            users = databaseManager.LoadUsers();
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //app is always in center of screen
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
