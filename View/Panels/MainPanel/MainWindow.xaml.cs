using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using LocalDatabase_Server.Database;
using LocalDatabase_Server.Directory;
using LocalDatabase_Server.View.Panels.MainPanel.PieChartDrawer;
using System.Windows.Media;
using LocalDatabase_Server.Data.Utils;

namespace LocalDatabase_Server
{
    public partial class MainWindow : Window
    {
        //list of active users and transmissions has to be here because gui use them
        //Observable Collection is a special container where things in gui and things in container are allways the same - automatic refresh
        ObservableCollection<User> activeUsers;
        ObservableCollection<Transmission> transmissions;
        PieChartDrawer pieChartDrawer;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent(); //runs gui
            pieChartDrawer = new PieChartDrawer(canvas: canv, itemsControl: detailsItemsControl);
            pieChartDrawer.DrawPieChart();

            activeUsers = new ObservableCollection<User>();
            transmissions = DatabaseManager.Instance.GetTransmissions();
            transmissionsList.ItemsSource = transmissions;
            activeUsersList.ItemsSource = activeUsers;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(transmissionsList.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("date", ListSortDirection.Descending));


            Task serverThread = new Task(() => newThread());
            serverThread.Start();
        }

        //method that starts server - it has to be in other thread because meanwhile the gui has to run
        private void newThread()
        {
            ServerStarter.Init(activeUsers: activeUsers, transmissions: transmissions, pieChartDrawer: pieChartDrawer);
        }
        #region button events

        //shows registration panel -> lets go to Panels/Registration/Registration.xaml.cs
        private void registrationButton_Click(object sender, RoutedEventArgs e)
        {
            //Registration.Registration r = new Registration.Registration();
            //r.Show();
        }

        //shows users panel and hides main window -> lets go to Panels/Users/Users.xaml.cs
        private void allUsersButton_Click(object sender, RoutedEventArgs e)
        {
            Users.Users usersPanel = new Users.Users();
            usersPanel.Owner = this; //its needed to show again main window in users panel
            usersPanel.Show();
            this.Hide();
        }

        //ends app
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            ServerStarter.Stop();
            this.Owner.Close();
            Environment.Exit(0);
            Close();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            this.Owner.Show();
            ServerStarter.Stop();
            this.Close();
        }

        private void logUserOutButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = ((Button)sender);
            var temp = (User)btn.DataContext;
            activeUsers.Remove(temp);
        }
        #endregion
    }

    class PieCategory
    {
        public double Percentage { get; set; }
        public string Title { get; set; }
        public Brush ColorBrush { get; set; }
    }
}
