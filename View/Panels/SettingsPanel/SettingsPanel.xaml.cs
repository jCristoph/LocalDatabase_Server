using LocalDatabase_Server.Directory;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;

namespace LocalDatabase_Server.Panels.SettingsPanel
{
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
                ShowMessagePanel("Value can not be empty");
                return;
            }

            if (int.TryParse(idleTimeText, out int IdleTimeInt))
            {
                SettingsManager.Instance.SetIdleTime(IdleTimeInt);
                ShowMessagePanel();
            }
            else
            {
                ShowMessagePanel("Value is not a number");
            }
        }

        private void changeSystemFolderSizeButton_Click(object sender, RoutedEventArgs e)
        {
            string systemFolderSizeText = systemFolderSize.Text;
            if (string.IsNullOrEmpty(systemFolderSizeText))
            {
                ShowMessagePanel("Value can not be empty");
                return;
            }

            if (int.TryParse(systemFolderSizeText, out int SystemFolderSizeInt))
            {
                SettingsManager.Instance.SetAvailableSpace(SystemFolderSizeInt);
                ShowMessagePanel();
            }
            else
            {
                ShowMessagePanel("Value is not a number");
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

        private void changeServerIpButton_Click(object sender, RoutedEventArgs e)
        {
            string serverIp = serverIpText.Text;
            if (string.IsNullOrEmpty(serverIp))
            {
                ShowMessagePanel("Value can not be empty");
                return;
            }

            if (!IsValidateIP(serverIp))
            {
                ShowMessagePanel("Ip address is not in correct format");
                return;
            }

            SettingsManager.Instance.SetServerIp(serverIp);
            ShowMessagePanel();
        }

        private bool IsValidateIP(string Address)
        {
            string Pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";
            Regex check = new Regex(Pattern);

            return check.IsMatch(Address, 0);
        }
    }
}