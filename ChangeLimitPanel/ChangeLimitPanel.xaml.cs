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
    /// <summary>
    /// Logika interakcji dla klasy ChangeLimitPanel.xaml
    /// </summary>
    public partial class ChangeLimitPanel : Window
    {
        public long newlimit { set; get; }
        public ChangeLimitPanel()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }
        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            string input = textBox.Text.Replace(".", ",");
            Regex rx = new Regex(@"\D+");
            if(!rx.IsMatch(input))
            {
                newlimit = (long)(Convert.ToDouble(input) * 1000000000);
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
