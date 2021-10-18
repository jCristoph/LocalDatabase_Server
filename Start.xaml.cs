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
            Panels.SettingsPanel.SettingsPanel settingsPanel = new Panels.SettingsPanel.SettingsPanel();
            settingsPanel.Show();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void startServerButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainPanel = new MainWindow();
            mainPanel.Show();
            this.Close();
        }
    }
}
