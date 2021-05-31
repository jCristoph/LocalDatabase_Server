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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;

namespace LocalDatabase_Server
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>ty
    public partial class MainWindow : Window
    {
        ObservableCollection<Database.User> activeUsers;
        ObservableCollection<Database.Transmission> transmissions;

        private List<Category> Categories { get; set; }
        public MainWindow()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            pieChart();
            activeUsers = new ObservableCollection<Database.User>();
            transmissions = new ObservableCollection<Database.Transmission>();
            transmissionsList.ItemsSource = transmissions;
            activeUsersList.ItemsSource = activeUsers;
            Task t = new Task(() => newThread());
            t.Start();
        }

        private void newThread()
        {
            ServerStarter ss = new ServerStarter("127.0.0.1", 25000, activeUsers, transmissions);
        }

        private void registrationButton_Click(object sender, RoutedEventArgs e)
        {
            Registration.Registration r = new Registration.Registration();
            r.Show();
        }

        private void allUsersButton_Click(object sender, RoutedEventArgs e)
        {
            Users.Users usersPanel = new Users.Users();
            usersPanel.Owner = this;
            usersPanel.Show();
            this.Hide();
        }
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static long GetFileSizeSumFromDirectory(string searchDirectory)
        {
            var files = Directory.EnumerateFiles(searchDirectory);

            // get the sizeof all files in the current directory
            var currentSize = (from file in files let fileInfo = new FileInfo(file) select fileInfo.Length).Sum();

            var directories = Directory.EnumerateDirectories(searchDirectory);

            // get the size of all files in all subdirectories
            var subDirSize = (from directory in directories select GetFileSizeSumFromDirectory(directory)).Sum();

            return currentSize + subDirSize;
        }

        private void pieChart()
        {
            float pieWidth = 100;
            float pieHeight = 100;
            float centerX = pieWidth / 2;
            float centerY = pieHeight / 2;
            float radius = pieWidth / 2;
            canv.Width = pieWidth;
            canv.Height = pieHeight;
            DriveInfo dDrive = new DriveInfo("C");
            var folderSize = (double)GetFileSizeSumFromDirectory(@"C:\Directory_test\");
            var availableSpace = dDrive.AvailableFreeSpace;
            double p1 = Math.Round((folderSize / availableSpace),2) * 100.0f;
            double p2 = Math.Round(((float)(dDrive.AvailableFreeSpace - GetFileSizeSumFromDirectory(@"C:\Directory_test\")) / dDrive.AvailableFreeSpace),2) * 100.0f;

            Categories = new List<Category>() {
                new Category
                {
                    Title = "Zajęte",
                    Percentage = p1,
                    ColorBrush = Brushes.Red,
                },
                new Category
                {
                    Title = "Wolne",
                    Percentage = p2,
                    ColorBrush = Brushes.Aqua,
                },
            };

            detailsItemsControl.ItemsSource = Categories;

            // draw pie
            double angle = 0, prevAngle = 0;
            foreach (var category in Categories)
            {
                double line1X = (radius * Math.Cos(angle * Math.PI / 180)) + centerX;
                double line1Y = (radius * Math.Sin(angle * Math.PI / 180)) + centerY;

                angle = category.Percentage * (float)360 / 100 + prevAngle;
                Debug.WriteLine(angle);

                double arcX = (radius * Math.Cos(angle * Math.PI / 180)) + centerX;
                double arcY = (radius * Math.Sin(angle * Math.PI / 180)) + centerY;

                var line1Segment = new LineSegment(new Point(line1X, line1Y), false);
                double arcWidth = radius, arcHeight = radius;
                bool isLargeArc = category.Percentage > 50;
                var arcSegment = new ArcSegment()
                {
                    Size = new Size(arcWidth, arcHeight),
                    Point = new Point(arcX, arcY),
                    SweepDirection = SweepDirection.Clockwise,
                    IsLargeArc = isLargeArc,
                };
                var line2Segment = new LineSegment(new Point(centerX, centerY), false);

                var pathFigure = new PathFigure(
                    new Point(centerX, centerY),
                    new List<PathSegment>()
                    {
                        line1Segment,
                        arcSegment,
                        line2Segment,
                    },
                    true);

                var pathFigures = new List<PathFigure>() { pathFigure, };
                var pathGeometry = new PathGeometry(pathFigures);
                var path = new System.Windows.Shapes.Path()
                {
                    Fill = category.ColorBrush,
                    Data = pathGeometry,
                };
                canv.Children.Add(path);

                prevAngle = angle;


                // draw outlines
                var outline1 = new Line()
                {
                    X1 = centerX,
                    Y1 = centerY,
                    X2 = line1Segment.Point.X,
                    Y2 = line1Segment.Point.Y,
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 5,
                };
                var outline2 = new Line()
                {
                    X1 = centerX,
                    Y1 = centerY,
                    X2 = arcSegment.Point.X,
                    Y2 = arcSegment.Point.Y,
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 5,
                };

                canv.Children.Add(outline1);
                canv.Children.Add(outline2);
            }
        }
    }
    public class Category
    {
        public double Percentage { get; set; }
        public string Title { get; set; }
        public Brush ColorBrush { get; set; }
    }
}
