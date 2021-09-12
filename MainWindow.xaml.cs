using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace LocalDatabase_Server
{
    public partial class MainWindow : Window
    {
        //list of active users and transmissions has to be here because gui use them
        //Observable Collection is a special container where things in gui and things in container are allways the same - automatic refresh
        ObservableCollection<Database.User> activeUsers;
        ObservableCollection<Database.Transmission> transmissions;

        private List<Category> Categories { get; set; }

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen; //app is always in center of screen
            InitializeComponent(); //runs gui
            pieChart(); //creates pie chart
            activeUsers = new ObservableCollection<Database.User>();
            transmissions = new ObservableCollection<Database.Transmission>();
            transmissionsList.ItemsSource = transmissions;
            activeUsersList.ItemsSource = activeUsers;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(transmissionsList.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("date", ListSortDirection.Descending));
            //transmissions.Add(new Database.Transmission(1, DateTime.Now, 0, "ABC", 0));
            Task serverThread = new Task(() => newThread());
            serverThread.Start();
        }

        //method that starts server - it has to be in other thread because meanwhile the gui has to run
        private void newThread()
        {
            ServerStarter ss = new ServerStarter("127.0.0.1", 25000, activeUsers, transmissions);
        }
        #region button events

        //shows registration panel -> lets go to Panels/Registration/Registration.xaml.cs
        private void registrationButton_Click(object sender, RoutedEventArgs e)
        {
            Registration.Registration r = new Registration.Registration();
            r.Show();
        }

        //shows users panel and hides main window -> lets go to Panels/Users/Users.xaml.cs
        private void allUsersButton_Click(object sender, RoutedEventArgs e)
        {
            Users.Users usersPanel = new Users.Users();
            usersPanel.Owner = this; //its needed to show again main window in users panel
            usersPanel.Show();
            this.Hide();
        }

        //ends app
        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion


        /// <summary>
        /// method that creates a pie chart from paths
        /// </summary>
        private void pieChart()
        {
            ///parameters for pie chart
            float pieWidth = 200;
            float pieHeight = 200;
            float centerX = pieWidth / 2;
            float centerY = pieHeight / 2;
            float radius = pieWidth / 2;
            canv.Width = pieWidth;
            canv.Height = pieHeight;

            ///data is loaded - free space on disk and size of app folder
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

        //methods that counts a folder size - it has to sum every file in folder and subfolders
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
    }
    /// <summary>
    /// a class which is needed for pie chart
    /// these parameters set a pie chart
    /// </summary>
    public class Category
    {
        public double Percentage { get; set; }
        public string Title { get; set; }
        public Brush ColorBrush { get; set; }
    }
}
