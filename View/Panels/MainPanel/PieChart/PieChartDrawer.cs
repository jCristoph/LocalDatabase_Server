﻿using System;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Diagnostics;
using LocalDatabase_Server.Directory;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace LocalDatabase_Server.View.Panels.MainPanel.PieChartDrawer
{
    public class PieChartDrawer
    {
        private List<PieCategory> Categories { get; set; }

        float pieWidth = 0;
        float pieHeight = 0;
        float centerX = 0;
        float centerY = 0;
        float radius = 0;
        Canvas canvas;
        ItemsControl itemsControl;
        double folderSize;

        public PieChartDrawer(Canvas canvas, ItemsControl itemsControl, double folderSize) {
            pieWidth = 200;
            pieHeight = 200;
            centerX = pieWidth / 2;
            centerY = pieHeight / 2;
            radius = pieWidth / 2;
            this.canvas = canvas;
            this.itemsControl = itemsControl;
            this.folderSize = folderSize;
        }

        /// <summary>
        /// method that creates a pie chart from paths
        /// </summary>
        public void DrawPieChart()
        {
            canvas.Width = pieWidth;
            canvas.Height = pieHeight;

            long availableSpace = SettingsManager.Instance.GetAvailableSpace();
            double p1 = Math.Round((double)(folderSize / availableSpace), 2) * 100.0;
            double p2 = Math.Round((double)(availableSpace - folderSize) / availableSpace, 2) * 100.0;

            Categories = new List<PieCategory>() {
                new PieCategory
                {
                    Title = "Allocated",
                    Percentage = p1,
                    ColorBrush = Brushes.Red,
                },
                new PieCategory
                {
                    Title = "Free",
                    Percentage = p2,
                    ColorBrush = Brushes.Aqua,
                },
            };

            itemsControl.ItemsSource = Categories;
            DrawPie();
        }

        private void DrawPie()
        {
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
                LineSegment line2Segment = new LineSegment(new Point(centerX, centerY), false);

                PathFigure pathFigure = new PathFigure(
                    new Point(centerX, centerY),
                    new List<PathSegment>()
                    {
                        line1Segment,
                        arcSegment,
                        line2Segment,
                    },
                    true);

                List<PathFigure> pathFigures = new List<PathFigure>() { pathFigure, };
                PathGeometry pathGeometry = new PathGeometry(pathFigures);
                Path path = new Path()
                {
                    Fill = category.ColorBrush,
                    Data = pathGeometry,
                };
                canvas.Children.Add(path);

                prevAngle = angle;

                Line outline1 = DrawPieChartOutline(pointX: line1Segment.Point.X, line1Segment.Point.Y);
                Line outline2 = DrawPieChartOutline(arcSegment.Point.X, arcSegment.Point.Y);

                canvas.Children.Add(outline1);
                canvas.Children.Add(outline2);
            }
        }

        private Line DrawPieChartOutline(double pointX, double pointY)
        {
            Line outline = new Line()
            {
                X1 = centerX,
                Y1 = centerY,
                X2 = pointX,
                Y2 = pointY,
                Stroke = Brushes.Transparent,
                StrokeThickness = 5,
            };

            return outline;
        }
    }
}
