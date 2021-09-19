using System.Windows;

namespace LocalDatabase_Server.MessagePanel
{
    public partial class MessagePanel : Window
    {
        public bool isAnswered = false;

        //this panel is used for two application. It could be message box with decision (two buttons - yes or not) or just informative (one button - ok)
        //it is controlled by parameter yesNoOK
        public MessagePanel(string content, bool isDecisive)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
            //if isDecisive is false then a panel will be just informative. Then one button has to be hidden and other has to have other text
            if (!isDecisive)
            {
                yesButton.Visibility = Visibility.Hidden;
                yesButton.Visibility = Visibility.Collapsed;
                okNoButton.Content = "OK";
            }
            DataContext = this;
            message.Text = content;
        }

        //if user clicks yes button then the answer is send to decision part of program and next panel close.
        private void yesButton_Click(object sender, RoutedEventArgs e)
        {
            isAnswered = true;
            this.Close();
        }

        //if user clicks no or ok button then panel just close.
        private void okNoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
