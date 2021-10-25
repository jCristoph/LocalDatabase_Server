
using System.Windows;

namespace LocalDatabase_Server.Panels.SettingsPanel
{
    /// <summary>
    /// Interaction logic for SettingsPanelOverview.xaml
    /// </summary>
    public partial class SettingsPanelOverview : Window
    {
        public SettingsPanelOverview()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            idleSessionTimeText.Text = Directory.SettingsManager.Instance.GetIdleTime().ToString();
            systemfolderSizeText.Text = (Directory.SettingsManager.Instance.GetAvailableSpace() / 1000000000.0).ToString();
            mainFolderPathText.Text = Directory.SettingsManager.Instance.GetSavePath();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Start start = new Start();
            start.Show();
            
            this.Close();
        }

        private void changeSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPanel settingsPanel = new SettingsPanel();
            settingsPanel.Show();
            this.Close();
        }
    }
}
