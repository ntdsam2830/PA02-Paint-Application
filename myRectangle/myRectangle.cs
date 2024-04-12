﻿using myShape;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace myRectangle
{
    public class myRectangle : IShape
    {
        private Point startPoint;
        private Point endPoint;

        public string shapeName => "Rectangle";
        public string shapeImage => "images/shapeRectangle.png";

        public void addStartPoint(Point point) { startPoint = point; }
        public void addEndPoint(Point point) { endPoint = point; }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement convertShapeType()
        {

            var start = startPoint;
            var end = endPoint;

            var left = Math.Min(start.X, end.X);
            var right = Math.Max(start.X, end.X);

            var top = Math.Min(start.Y, end.Y);
            var bottom = Math.Max(start.Y, end.Y);

            var width = right - left;
            var height = bottom - top;

            var element = new Rectangle()
            {
                Fill = Brushes.AliceBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(element, left);
            Canvas.SetTop(element, top);

            return element;
        }
    }
}