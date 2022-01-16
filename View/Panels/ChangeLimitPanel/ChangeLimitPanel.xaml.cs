using System;
using System.Text.RegularExpressions;
using System.Windows;
using LocalDatabase_Server.Data.Utils;

namespace LocalDatabase_Server.ChangeLimitPanel
{
    public partial class ChangeLimitPanel : Window
    {
        public long newlimit { set; get; }
        public ChangeLimitPanel()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent();
        }

        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            string input = textBox.Text; //protection if user uses dot insted of comma
            Regex rx = new Regex(@"\D+"); //protection if user type something different than numbers
            if(!rx.IsMatch(input) && !input.Equals(""))
            {
                newlimit = UnitsConverter.ConvertGigabytesToBytes(Convert.ToInt64(input)); //user enter value in GB but in system uses just Bytes
                this.Close();
            }
            else
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Wrong value", false);
                mp.ShowDialog();
            }
        }
    }
}
