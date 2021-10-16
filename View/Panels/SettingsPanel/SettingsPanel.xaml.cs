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
            this.Close();
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
                ShowSuccessMessagePanel();
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
                ShowSuccessMessagePanel();
            }
        }

        private void ShowSuccessMessagePanel()
        {
            MessagePanel.MessagePanel messagePanel = new MessagePanel.MessagePanel("Changes applied", false);
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
                    System.Windows.Forms.MessageBox.Show("Changed default path to: " + folderBrowserDialog.SelectedPath);
                }
            }
        }
    }
}
