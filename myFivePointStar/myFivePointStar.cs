﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using myColor;
using myShape;
using myStroke;
using myWidthness;

namespace myFivePointStar
{
    public class myFivePointStar : IShape
    {
        private Point startPoint;
        private Point endPoint;
        private IWidthness widthness;
        private IStroke strokeStyle;
        private IColor colorValue;
        private bool isFill;
        private bool isEdit;

        private Grid EditGrid;
        private Button LeftTopButton;
        private Button RightTopButton;
        private Button LeftBottomButton;
        private Button RightBottomButton;
        private Button LeftCenterButton;
        private Button RightCenterButton;
        private Button TopCenterButton;
        private Button BottomCenterButton;
        private Button RotateButton;

        public string shapeName => "FivePointStar";
        public string shapeImage => "images/shape5Star.png";

        public void addStartPoint(Point point) { startPoint = point; }
        public void addEndPoint(Point point) { endPoint = point; }
        public void addWidthness(IWidthness width) 
        {
            widthness = width;
        }
        public void addStrokeStyle(IStroke stroke)
        {
            strokeStyle = stroke;
        }
        public void addColor(IColor color)
        {
            colorValue = color;
        }
        public void addPointList(List<Point> pointList) { }
        public void addFontSize(int fontSize) { }
        public void addFontFamily(string fontFamily) { }
        public TextBox getTextBox() { return null; }
        public void setTextString(string text) { }
        public void setFocus(bool focus) { }
        public void setBold(bool bold) { }
        public void setItalic(bool italic) { }
        public void setBackground(byte r, byte g, byte b) { }
        public Point getStartPoint()
        {
            return startPoint;
        }
        public Point getEndPoint()
        {
            return endPoint;
        }
        public Point getCenterPoint()
        {
            return new Point((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);
        }
        public void setShapeFill(bool isShapeFill)
        {
            isFill = isShapeFill;
        }
        public void setEdit(bool edit)
        {
            isEdit = edit;
        }
        public Grid getEditGrid()
        {
            return EditGrid;
        }
        public Button getStartButton()
        {
            return null;
        }
        public Button getEndButton()
        {
            return null;
        }
        public Button getLeftTopButton()
        {
            return LeftTopButton;
        }
        public Button getRightTopButton()
        {
            return RightTopButton;
        }
        public Button getLeftBottomButton()
        {
            return LeftBottomButton;
        }
        public Button getRightBottomButton()
        {
            return RightBottomButton;
        }
        public Button getLeftCenterButton()
        {
            return LeftCenterButton;
        }
        public Button getRightCenterButton()
        {
            return RightCenterButton;
        }
        public Button getTopCenterButton()
        {
            return TopCenterButton;
        }
        public Button getBottomCenterButton()
        {
            return BottomCenterButton;
        }
        public Button getRotateButton()
        {
            return RotateButton;
        }
        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement convertShapeType() {

            Point center = new Point((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);

            var left = Math.Min(startPoint.X, endPoint.X);
            var right = Math.Max(startPoint.X, endPoint.X);

            var top = Math.Min(startPoint.Y, endPoint.Y);
            var bottom = Math.Max(startPoint.Y, endPoint.Y);

            var width = right - left;
            var height = bottom - top;

            string status = "";

            if (startPoint.X < endPoint.X && startPoint.Y < endPoint.Y)
            {
                status = "normal";
            }
            else if (startPoint.X < endPoint.X && startPoint.Y > endPoint.Y)
            {
                status = "upside";
            }
            else if (startPoint.X > endPoint.X && startPoint.Y < endPoint.Y)
            {
                status = "reverse";
            }
            else if (startPoint.X > endPoint.X && startPoint.Y > endPoint.Y)
            {
                status = "upside-reverse";
            }

            Path element;

            if (isFill)
            {
                element = new Path
                {
                    StrokeThickness = widthness.widthnessValue,
                    StrokeDashArray = strokeStyle.strokeValue,
                    Stroke = colorValue.colorValue,
                    Fill = colorValue.colorValue,
                    Data = CreateFivePointStarGeometry(center, width, height, status)
                };
            } else
            {
                element = new Path
                {
                    StrokeThickness = widthness.widthnessValue,
                    StrokeDashArray = strokeStyle.strokeValue,
                    Stroke = colorValue.colorValue,
                    Data = CreateFivePointStarGeometry(center, width, height, status)
                };
            }

            if (isEdit)
            {
                Canvas canvas = new Canvas();

                EditGrid = new Grid()
                {
                    Width = width,
                    Height = height,
                    Background = Brushes.Transparent,
                };

                Canvas.SetLeft(EditGrid, left);
                Canvas.SetTop(EditGrid, top);

                Rectangle rectangle = new Rectangle()
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    StrokeDashArray = new DoubleCollection() { 10, 2 },
                    Width = width,
                    Height = height,
                };

                Canvas.SetLeft(rectangle, left);
                Canvas.SetTop(rectangle, top);

                LeftTopButton = new Button();
                LeftTopButton.Width = 10;
                LeftTopButton.Height = 10;
                LeftTopButton.Background = Brushes.White;
                Canvas.SetLeft(LeftTopButton, left - 5);
                Canvas.SetTop(LeftTopButton, top - 5);

                RightTopButton = new Button();
                RightTopButton.Width = 10;
                RightTopButton.Height = 10;
                RightTopButton.Background = Brushes.White;
                Canvas.SetLeft(RightTopButton, right - 5);
                Canvas.SetTop(RightTopButton, top - 5);

                LeftBottomButton = new Button();
                LeftBottomButton.Width = 10;
                LeftBottomButton.Height = 10;
                LeftBottomButton.Background = Brushes.White;
                Canvas.SetLeft(LeftBottomButton, left - 5);
                Canvas.SetTop(LeftBottomButton, bottom - 5);

                RightBottomButton = new Button();
                RightBottomButton.Width = 10;
                RightBottomButton.Height = 10;
                RightBottomButton.Background = Brushes.White;
                Canvas.SetLeft(RightBottomButton, right - 5);
                Canvas.SetTop(RightBottomButton, bottom - 5);

                LeftCenterButton = new Button();
                LeftCenterButton.Width = 10;
                LeftCenterButton.Height = 10;
                LeftCenterButton.Background = Brushes.White;
                Canvas.SetLeft(LeftCenterButton, left - 5);
                Canvas.SetTop(LeftCenterButton, top + (height / 2) - 5);

                RightCenterButton = new Button();
                RightCenterButton.Width = 10;
                RightCenterButton.Height = 10;
                RightCenterButton.Background = Brushes.White;
                Canvas.SetLeft(RightCenterButton, right - 5);
                Canvas.SetTop(RightCenterButton, top + (height / 2) - 5);

                TopCenterButton = new Button();
                TopCenterButton.Width = 10;
                TopCenterButton.Height = 10;
                TopCenterButton.Background = Brushes.White;
                Canvas.SetLeft(TopCenterButton, left + (width / 2) - 5);
                Canvas.SetTop(TopCenterButton, top - 5);

                BottomCenterButton = new Button();
                BottomCenterButton.Width = 10;
                BottomCenterButton.Height = 10;
                BottomCenterButton.Background = Brushes.White;
                Canvas.SetLeft(BottomCenterButton, left + (width / 2) - 5);
                Canvas.SetTop(BottomCenterButton, bottom - 5);

                RotateButton = new Button();
                RotateButton.Width = 20;
                RotateButton.Height = 20;
                RotateButton.Background = Brushes.White;
                Canvas.SetLeft(RotateButton, left + (width / 2) - 10);
                Canvas.SetTop(RotateButton, top - 40);

                canvas.Children.Add(rectangle);
                canvas.Children.Add(element);
                canvas.Children.Add(EditGrid);

                canvas.Children.Add(LeftTopButton);
                canvas.Children.Add(RightTopButton);
                canvas.Children.Add(LeftBottomButton);
                canvas.Children.Add(RightBottomButton);

                canvas.Children.Add(LeftCenterButton);
                canvas.Children.Add(RightCenterButton);
                canvas.Children.Add(TopCenterButton);
                canvas.Children.Add(BottomCenterButton);

                canvas.Children.Add(RotateButton);

                return canvas;
            }

            return element;
        }

        private Geometry CreateFivePointStarGeometry(Point center, double width, double height, string status)
        {
            var geometry = new PathGeometry();
            var figure = new PathFigure();

            if (status == "normal")
            {
                figure.StartPoint = new Point(center.X, startPoint.Y);
                figure.IsClosed = true;

                figure.Segments.Add(new LineSegment(new Point(center.X + width / 8, center.Y - height / 8), true));
                figure.Segments.Add(new LineSegment(new Point(endPoint.X, center.Y - height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X + width / 6, center.Y + height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X + width / 3, endPoint.Y), true));
                figure.Segments.Add(new LineSegment(new Point(center.X , center.Y + height / 4), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 3, endPoint.Y), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 6, center.Y + height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(startPoint.X, center.Y - height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 8, center.Y - height / 8), true));
                figure.Segments.Add(new LineSegment(new Point(center.X, startPoint.Y), true));
            }
            else if (status == "upside")
            {
                figure.StartPoint = new Point(center.X, endPoint.Y);
                figure.IsClosed = true;

                figure.Segments.Add(new LineSegment(new Point(center.X + width / 8, center.Y - height / 8), true));
                figure.Segments.Add(new LineSegment(new Point(endPoint.X, center.Y - height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X + width / 6, center.Y + height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X + width / 3, startPoint.Y), true));
                figure.Segments.Add(new LineSegment(new Point(center.X, center.Y + height / 4), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 3, startPoint.Y), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 6, center.Y + height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(startPoint.X, center.Y - height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 8, center.Y - height / 8), true));
                figure.Segments.Add(new LineSegment(new Point(center.X, endPoint.Y), true));
            }
            else if (status == "reverse")
            {
                figure.StartPoint = new Point(center.X, startPoint.Y);
                figure.IsClosed = true;

                figure.Segments.Add(new LineSegment(new Point(center.X + width / 8, center.Y - height / 8), true));
                figure.Segments.Add(new LineSegment(new Point(startPoint.X, center.Y - height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X + width / 6, center.Y + height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X + width / 3, endPoint.Y), true));
                figure.Segments.Add(new LineSegment(new Point(center.X, center.Y + height / 4), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 3, endPoint.Y), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 6, center.Y + height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(endPoint.X, center.Y - height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 8, center.Y - height / 8), true));
                figure.Segments.Add(new LineSegment(new Point(center.X, startPoint.Y), true));
            }
            else if (status == "upside-reverse")
            {
                figure.StartPoint = new Point(center.X, endPoint.Y);
                figure.IsClosed = true;

                figure.Segments.Add(new LineSegment(new Point(center.X + width / 8, center.Y - height / 8), true));
                figure.Segments.Add(new LineSegment(new Point(startPoint.X, center.Y - height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X + width / 6, center.Y + height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X + width / 3, startPoint.Y), true));
                figure.Segments.Add(new LineSegment(new Point(center.X, center.Y + height / 4), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 3, startPoint.Y), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 6, center.Y + height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(endPoint.X, center.Y - height / 12), true));
                figure.Segments.Add(new LineSegment(new Point(center.X - width / 8, center.Y - height / 8), true));
                figure.Segments.Add(new LineSegment(new Point(center.X, endPoint.Y), true));
            }

            geometry.Figures.Add(figure);
            return geometry;
        }
    }
}
