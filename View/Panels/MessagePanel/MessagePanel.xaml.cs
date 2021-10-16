using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LocalDatabase_Server.MessagePanel
{
    public partial class MessagePanel : Window
    {
        public bool answear = false;

        //this panel is used for two application. It could be message box with decision (two buttons - yes or not) or just informative (one button - ok)
        //it is controlled by parameter yesNoOK
        public MessagePanel(string content, bool yesNoOK)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
            //if yesNoOK is false then a panel will be just informative. Then one button has to be hidden and other has to have other text
            if (!yesNoOK)
            {
                yesButton.Visibility = Visibility.Hidden;
                yesButton.Visibility = Visibility.Collapsed;
                okNoButton.Content = "OK";
            }
            DataContext = this;
            message.Text = content;
        }

        //if user clicks yes button then the answear is send to decision part of program and next panel close.
        private void yesButton_Click(object sender, RoutedEventArgs e)
        {
            answear = true;
            this.Close();
        }

        //if user clicks no or ok button then panel just close.
        private void okNoButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
