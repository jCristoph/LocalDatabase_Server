using System.Windows;

namespace LocalDatabase_Server
{
    /// <summary>
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class Start : Window
    {
        public Start()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void settingsButton_Click(object sender, RoutedEventArgs e)
        {
            Panels.SettingsPanel.SettingsPanelOverview settingsOverview = new Panels.SettingsPanel.SettingsPanelOverview
            {
                Owner = this
            };
            settingsOverview.Show();
            this.Hide();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            ServerStarter.Stop();
            this.Close();   
        }

        private void startServerButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainPanel = new MainWindow();
            mainPanel.Owner = this;
            mainPanel.Show();
            this.Hide();
        }

        private void setIp_Click(object sender, RoutedEventArgs e)
        {
            Panels.SettingsPanel.SettingsPanelOverview settingsOverview = new Panels.SettingsPanel.SettingsPanelOverview
            {
                Owner = this
            };
            settingsOverview.Show();
            this.Hide();
        }
    }
}
