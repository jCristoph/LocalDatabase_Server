using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            string input = textBox.Text.Replace(".", ","); //protection if user uses dot insted of comma
            Regex rx = new Regex(@"\D+"); //protection if user type something different than numbers
            if(!rx.IsMatch(input))
            {
                newlimit = (long)(Convert.ToDouble(input) * 1000000000); //user enter value in GB but in system uses just Bytes
                this.Close();
            }
            else
            {
                MessagePanel.MessagePanel mp = new MessagePanel.MessagePanel("Zła wartość",false);
                mp.ShowDialog();
            }
        }
    }
}
