﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

namespace LocalDatabase_Server
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>ty
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Task t = new Task(() => newThread());
            t.Start();
        } 

        private void newThread()
        {
            ServerStarter ss = new ServerStarter("192.168.1.204", 25000);
        }

        private void registrationButton_Click(object sender, RoutedEventArgs e)
        {
            Registration.Registration r = new Registration.Registration();
            r.Show();
        }

        private void allUsersButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
