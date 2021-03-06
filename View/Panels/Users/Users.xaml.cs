using LocalDatabase_Server.Directory;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using LocalDatabase_Server.Data.Utils;

namespace LocalDatabase_Server.Users
{
    /// <summary>
    /// Logika interakcji dla klasy Users.xaml
    /// </summary>
    public partial class Users : Window
    {
        private ObservableCollection<Database.User> users;
        
        public Users()
        {
            users = Database.DatabaseManager.Instance.GetUsers();
            foreach(var u in users)
            {
                u.limit = (long)UnitsConverter.ConvertBytesToGigabytes(u.limit);
            }
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
            Database.DatabaseManager.Instance.DeleteUser(u.token); //deleted from db
            string path = SettingsManager.Instance.GetSavePath() + u.token;
            for (int i = 0; i < users.Count; i++)
            {
                if (users[i].token.Equals(u.token))
                {
                    users.Remove(users[i]); //deleted from container
                    if (System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.Delete(path, true); //deleted his data
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
            Database.DatabaseManager.Instance.ChangeLimit(clp.newlimit, u.token); //limit is also saved in container
            users.Remove(u);
            u.limit = (long)UnitsConverter.ConvertBytesToGigabytes(clp.newlimit);
            users.Add(u);
        }
    }
}
