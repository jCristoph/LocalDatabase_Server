using System.Windows;

namespace LocalDatabase_Server.MessagePanel
{
    public partial class MessagePanel : Window
    {
        /// <summary>
        /// It could be message box with decision (two buttons - yes or not) or just informative
        /// </summary>
        /// <param name="content"></param>
        /// <param name="isPanelDecisive">If true, shows yes and no buttons. If false, shows OK button</param>
        public MessagePanel(string content, bool isPanelDecisive)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
           
            if (!isPanelDecisive)
            {
                yesButton.Visibility = Visibility.Hidden;
                yesButton.Visibility = Visibility.Collapsed;
                okNoButton.Content = "OK";
            }
            DataContext = this;
            message.Text = content;
        }

        private void onCloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
