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

namespace LocalDatabase_Server
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>ty
    public partial class MainWindow : Window
    {
        private List<Category> Categories { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            pieChart();
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
        private void pieChart()
        {
            float pieWidth = 100;
            float pieHeight = 100;
            float centerX = pieWidth / 2;
            float centerY = pieHeight / 2;
            float radius = pieWidth / 2;
            canv.Width = pieWidth;
            canv.Height = pieHeight;
            Categories = new List<Category>() {
                new Category
                {
                    Title = "Zajęte",
                    Percentage = 10,
                    ColorBrush = Brushes.Red,
                },

                new Category
                {
                    Title = "Wolne",
                    Percentage = 90,
                    ColorBrush = Brushes.Aqua,
                },
            };

            detailsItemsControl.ItemsSource = Categories;

            // draw pie
            float angle = 0, prevAngle = 0;
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
                var path = new Path()
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
                    Stroke = Brushes.White,
                    StrokeThickness = 5,
                };
                var outline2 = new Line()
                {
                    X1 = centerX,
                    Y1 = centerY,
                    X2 = arcSegment.Point.X,
                    Y2 = arcSegment.Point.Y,
                    Stroke = Brushes.White,
                    StrokeThickness = 5,
                };

                canv.Children.Add(outline1);
                canv.Children.Add(outline2);
            }
        }
    }
    public class Category
    {
        public float Percentage { get; set; }
        public string Title { get; set; }
        public Brush ColorBrush { get; set; }
    }
}
