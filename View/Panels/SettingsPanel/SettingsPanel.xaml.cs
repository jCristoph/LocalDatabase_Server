using LocalDatabase_Server.Directory;
using System.Windows;
using System.Windows.Forms;

namespace LocalDatabase_Server.Panels.SettingsPanel
{
    /// <summary>
    /// Interaction logic for SettingsPanel.xaml
    /// </summary>
    public partial class SettingsPanel : Window
    {
        public SettingsPanel()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Start start = new Start();
            start.Show();
            Close();
        }

        private void changeSessionTimeButton_Click(object sender, RoutedEventArgs e)
        {
            string idleTimeText = iddleSessionTime.Text;
            if (string.IsNullOrEmpty(idleTimeText))
            {
                return;
            }

            if (int.TryParse(idleTimeText, out int IdleTimeInt))
            {
                SettingsManager.Instance.SetIdleTime(IdleTimeInt);
                ShowMessagePanel();
            }
            else
            {
                ShowMessagePanel("Values are not numbers");
            }
        }

        private void changeSystemFolderSizeButton_Click(object sender, RoutedEventArgs e)
        {
            string systemFolderSizeText = systemFolderSize.Text;
            if (string.IsNullOrEmpty(systemFolderSizeText))
            {
                return;
            }

            if (int.TryParse(systemFolderSizeText, out int SystemFolderSizeInt))
            {
                SettingsManager.Instance.SetAvailableSpace(SystemFolderSizeInt);
                ShowMessagePanel();
            }
            else
            {
                ShowMessagePanel("Values are not numbers");
            }
        }

        private void ShowMessagePanel(string message = "Changes applied")
        {
            MessagePanel.MessagePanel messagePanel = new MessagePanel.MessagePanel(message, false);
            messagePanel.ShowDialog();
        }

        private void changeFolderPathButton_Click(object sender, RoutedEventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    SettingsManager.Instance.SetSavePath(folderBrowserDialog.SelectedPath);
                    ShowMessagePanel("Changed default path to: " + folderBrowserDialog.SelectedPath);
                }
            }
        }
    }
}
