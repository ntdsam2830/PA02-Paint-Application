﻿using myColor;
using myShape;
using myStroke;
using myWidthness;
using System.Data;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using TextBox = System.Windows.Controls.TextBox;

namespace Paint_Application
{
    public class Font()
    {
        public string fontName { get; set; }
    }

    public class Layer()
    {
        public List<IShape> drawSurface = new List<IShape>();
        public string layerName { get; set; }
    }

    public partial class MainWindow : Window
    {
        //Các bước boolean kiểm tra tình trạng đóng/mở của các combobox hoặc các function
        private bool isSelectionOpen = false;
        private bool isTextBold = false;
        private bool isTextItalic = false;
        private bool isTextBackgroundFill = false;
        private bool isTextFontFamilyOpen = false;
        private bool isTextFontSizeOpen = false;
        private bool isFunctionSelected = false;
        private bool isStyleWidthOpen = false;
        private bool isStyleStrokeOpen = false;
        private bool isToolEraseOpen = false;
        private bool isDrawing = false;
        private bool isShiftDown = false;
        private bool isShapeFill = false;
        private bool isTextOpen = false;
        private bool isEditting = false;
        private bool isSelecting = false;

        //Lưu giữ điểm bắt đầu và kết thúc của nét vẽ
        Point startPoint;
        Point endPoint;

        //Lưu giữ các biến tương tác đặc biệt của chương trình
        IColor customColor;
        IShape freeLine;
        IShape text;
        IShape selection;
        IShape newShape;
        List<IShape> listNewShape;

        //List border giúp xác định các border khi người dùng chọn vào các function
        private List<Border> function = new List<Border>();

        //List giúp lưu các biến để edit hình ảnh
        Grid EditGrid;
        Button StartButton;
        Button EndButton;
        Button LeftTopButton;
        Button RightTopButton;
        Button LeftBottomButton;
        Button RightBottomButton;
        Button LeftCenterButton;
        Button RightCenterButton;
        Button TopCenterButton;
        Button BottomCenterButton;
        Button RotateButton;

        //List fonts lưu giữ các kiểu fonts
        private List<Font> fonts = new List<Font>();

        //Các biến global lưu giữ các thông số của ứng dụng
        private string globalFontFamily;
        private int globalFontSize = 12;

        private byte backgroundRed = 255;
        private byte backgroundGreen = 255;
        private byte backgroundBlue = 255;

        private IShape selectedShape = null;
        private IColor selectedColor = null;

        //Các biến phục vụ cho chức năng edit hình ảnh
        private int editShapeIndex;
        private string editType;
        private Point movingPoint;
        private double movingStartX;
        private double movingStartY;
        private double movingEndX;
        private double movingEndY;

        //List lưu giữ tất cả các loại hình vẽ được load từ file dll (bao gồm các hình vẽ + phiên bản ấn shift của chúng)
        private List<IShape> allShapeList = new List<IShape>();

        //Các list lưu trữ các data được load từ file dll
        private List<IShape> shapeList = new List<IShape>();
        private List<IWidthness> widthnessList = new List<IWidthness>();
        private List<IStroke> strokeList = new List<IStroke>();
        private List<IColor> colorList = new List<IColor>();

        //List drawSurface giúp lưu trữ các nét vẽ trên 1 bề mặt
        private List<Layer> layerList = new List<Layer>();
        private int currentLayerIndex;
        private List<IShape> drawSurface = new List<IShape>();

        //List recoverList giúp lưu trữ lại các nét vẽ đã bị xóa hoặc undo
        private List<IShape> recoverList = new List<IShape>();

        //List eraseList giúp lưu giữ các hình vẽ bị xóa
        private List<Point> eraseList = new List<Point>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var fontList = Fonts.SystemFontFamilies;
            for (int i = 0; i < fontList.Count; i++)
            {
                Font tempFont = new Font {fontName = fontList.ElementAt(i).Source};
                fonts.Add(tempFont);
            }
            textFontCombobox.ItemsSource = fonts;

            selectionCombobox.SelectedIndex = 0;
            textFontCombobox.SelectedIndex = 0;
            styleWidthCombobox.SelectedIndex = 0;
            styleStrokeCombobox.SelectedIndex = 0;

            textFontCombobox.MaxDropDownHeight = 160;

            function.Add(selectionBorder);
            function.Add(textBorder);
            function.Add(toolEraseBorder);
            function.Add(toolMouseBorder);

            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var fis = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach (var fi in fis)
            {
                var assembly = Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if ((type.IsClass) && (typeof(IShape).IsAssignableFrom(type)))
                    {
                        if (!type.Name.Contains("Shift") && !type.Name.Equals("myFreeLine") && !type.Name.Equals("myText") && !type.Name.Equals("myRectangleSelection"))
                        {
                            shapeList.Add((IShape)Activator.CreateInstance(type)!);
                        }

                        if (type.Name.Equals("myFreeLine"))
                        {
                            freeLine = (IShape)Activator.CreateInstance(type)!;
                        }

                        if (type.Name.Equals("myText"))
                        {
                            text = (IShape)Activator.CreateInstance(type)!;
                        }

                        if (type.Name.Equals("myRectangleSelection"))
                        {
                            selection = (IShape)Activator.CreateInstance(type)!;
                        }

                        allShapeList.Add((IShape)Activator.CreateInstance(type)!);
                    }

                    if ((type.IsClass) && (typeof(IWidthness).IsAssignableFrom(type)))
                    {
                        widthnessList.Add((IWidthness)Activator.CreateInstance(type)!);
                    }

                    if ((type.IsClass) && (typeof(IStroke).IsAssignableFrom(type)))
                    {
                        strokeList.Add((IStroke)Activator.CreateInstance(type)!);
                    }

                    if ((type.IsClass) && (typeof(IColor).IsAssignableFrom(type)))
                    {
                        if (!type.Name.Equals("myCustomColor"))
                        {
                            colorList.Add((IColor)Activator.CreateInstance(type)!);
                        } else
                        {
                            customColor = (IColor)Activator.CreateInstance(type)!;
                        }
                    }
                }
            }

            shapeListview.ItemsSource = shapeList;
            styleWidthCombobox.ItemsSource = widthnessList;
            styleStrokeCombobox.ItemsSource = strokeList;
            colorListview.ItemsSource = colorList;

            layerList.Add(new Layer() { layerName = "Layer 1", drawSurface = new List<IShape>()});
            currentLayerIndex = 0;
            layerListView.ItemsSource = layerList;

            selectedColor = colorList[0];
            colorListview.SelectedIndex = 0;
            layerListView.SelectedIndex = 0;
        }

        private void minimizeButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void closeButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void functionSelected(int index)
        {
            shapeListview.SelectedItem = null;
            selectedShape = null;
            function[index].Opacity = 1;

            for (int i = 0; i < function.Count; i++)
            {
                if (i != index && function[i].Opacity != 0)
                {
                    function[i].Opacity = 0;
                }
            }

            if (index == 0)
            {
                isSelecting = true;
            } else {
                isSelecting = false;
            }

            if (index == 1)
            {
                isTextOpen = true;
            } else
            {
                isTextOpen = false;
            }

            if (index == 2)
            {
                isToolEraseOpen = true;
            } else
            {
                isToolEraseOpen = false;
            }

            if (index == 3)
            {
                isFunctionSelected = false;
            } else
            {
                isFunctionSelected = true;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void selectionButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isSelectionOpen)
            {
                selectionCombobox.IsDropDownOpen = true;
                selectionButtonContent.Source = new BitmapImage(new Uri("images/arrow-up.png", UriKind.Relative));
                isSelectionOpen = true;

                textFontCombobox.IsDropDownOpen = false;
                isTextFontFamilyOpen = false;

                fontSizeStackpanel.Visibility = Visibility.Collapsed;
                isTextFontSizeOpen = false;

                styleWidthCombobox.IsDropDownOpen = false;
                isStyleWidthOpen = false;

                styleStrokeCombobox.IsDropDownOpen = false;
                isStyleStrokeOpen = false;
            } else
            {
                selectionCombobox.IsDropDownOpen = false;
                selectionButtonContent.Source = new BitmapImage(new Uri("images/arrow-down.png", UriKind.Relative));
                isSelectionOpen = false;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void selecionStackpanelMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            functionSelected(0);
        }

        private void selectionComboboxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectionButtonContent.Source = new BitmapImage(new Uri("images/arrow-down.png", UriKind.Relative));
            isSelectionOpen = false;
            functionSelected(0);
        }

        private void selectionComboboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = selectionCombobox.SelectedIndex;
            switch (index)
            {
                case 0:
                    selectionImage.Source = new BitmapImage(new Uri("images/rec-selection.png", UriKind.Relative));
                    break;
                case 1:
                    selectionImage.Source = new BitmapImage(new Uri("images/free-selection.png", UriKind.Relative));
                    break;
                case 2:
                    selectionImage.Source = new BitmapImage(new Uri("images/all-selection.png", UriKind.Relative));
                    break;
                default: break;
            }

            selectionButtonContent.Source = new BitmapImage(new Uri("images/arrow-down.png", UriKind.Relative));
        }

        private void textButtonClick(object sender, RoutedEventArgs e)
        {
            functionSelected(1);
        }

        private void textBoldButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isTextBold)
            {
                textBoldBorder.Opacity = 1;
                isTextBold = true;
            } else
            {
                textBoldBorder.Opacity = 0;
                isTextBold = false;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void textItalicButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isTextItalic)
            {
                textItalicBorder.Opacity = 1;
                isTextItalic = true;
            } else
            {
                textItalicBorder.Opacity = 0;
                isTextItalic= false;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void textBackgroundButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isTextBackgroundFill)
            {
                textBackgroundImage.Source = new BitmapImage(new Uri("images/textBackgroundEffect.png", UriKind.Relative));
                isTextBackgroundFill = true;
                textBackgroundCustom.Visibility = Visibility.Visible;
            } else
            {
                textBackgroundImage.Source = new BitmapImage(new Uri("images/textBackground.png", UriKind.Relative));
                isTextBackgroundFill = false;
                textBackgroundCustom.Visibility = Visibility.Hidden;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void textFontButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isTextFontFamilyOpen)
            {
                textFontCombobox.IsDropDownOpen = true;
                isTextFontFamilyOpen = true;

                selectionCombobox.IsDropDownOpen = false;
                selectionButtonContent.Source = new BitmapImage(new Uri("images/arrow-down.png", UriKind.Relative));
                isSelectionOpen = false;

                fontSizeStackpanel.Visibility = Visibility.Collapsed;
                isTextFontSizeOpen = false;

                styleWidthCombobox.IsDropDownOpen = false;
                isStyleWidthOpen = false;

                styleStrokeCombobox.IsDropDownOpen = false;
                isStyleStrokeOpen = false;
            } else
            {
                textFontCombobox.IsDropDownOpen = false;
                isTextFontFamilyOpen = false;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void textFontComboboxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isTextFontFamilyOpen = false;
        }

        private void textFontComboboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic item = textFontCombobox.SelectedItem as dynamic;
            if (item != null)
            {
                globalFontFamily = item.fontName;
            }
        }

        private void fontSizeButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isTextFontSizeOpen)
            {
                string text = fontSizeTextbox.Text;
                int number;

                if (int.TryParse(text, out number))
                {
                    if (number >= 1 && number <= 72)
                    {
                        globalFontSize = number;
                        fontSizeTextbox.Text = number.ToString();
                    }
                    else
                    {
                        fontSizeTextbox.Text = globalFontSize.ToString();
                    }
                }
                else
                {
                    fontSizeTextbox.Text = globalFontSize.ToString();
                }

                fontSizeStackpanel.Visibility = Visibility.Visible;
                isTextFontSizeOpen = true;

                selectionCombobox.IsDropDownOpen = false;
                selectionButtonContent.Source = new BitmapImage(new Uri("images/arrow-down.png", UriKind.Relative));
                isSelectionOpen = false;

                textFontCombobox.IsDropDownOpen = false;
                isTextFontFamilyOpen = false;

                styleWidthCombobox.IsDropDownOpen = false;
                isStyleWidthOpen = false;

                styleStrokeCombobox.IsDropDownOpen = false;
                isStyleStrokeOpen = false;
            } else
            {
                fontSizeStackpanel.Visibility = Visibility.Collapsed;
                isTextFontSizeOpen = false;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void increaseFontSizeClick(object sender, RoutedEventArgs e)
        {
            string curSizeString = fontSizeTextbox.Text;
            if (int.TryParse(curSizeString, out _))
            {
                int curSizeInt = Int32.Parse(curSizeString);
                globalFontSize = Math.Min(curSizeInt + 2, 72);
                fontSizeTextbox.Text = globalFontSize.ToString();
            } else
            {
                fontSizeTextbox.Text = globalFontSize.ToString();
            }
        }

        private void decreaseFontSizeClick(object sender, RoutedEventArgs e)
        {
            string curSizeString = fontSizeTextbox.Text;
            if (int.TryParse(curSizeString, out _))
            {
                int curSizeInt = Int32.Parse(curSizeString);
                globalFontSize = Math.Max(curSizeInt - 2, 1);
                fontSizeTextbox.Text = globalFontSize.ToString();
            } else
            {
                fontSizeTextbox.Text = globalFontSize.ToString();
            }
        }

        private void fontSizeTextboxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string text = fontSizeTextbox.Text;
                int number;

                if (int.TryParse(text, out number))
                {
                    if (number >= 1 && number <= 72)
                    {
                        globalFontSize = number;
                        fontSizeTextbox.Text = number.ToString();
                    } else
                    {
                        fontSizeTextbox.Text = globalFontSize.ToString();
                    }
                } else
                {
                    fontSizeTextbox.Text = globalFontSize.ToString();
                }

                fontSizeStackpanel.Visibility = Visibility.Collapsed;
                isTextFontSizeOpen = false;
            }
        }

        private void styleWidthButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isStyleWidthOpen)
            {
                styleWidthCombobox.IsDropDownOpen = true;
                isStyleWidthOpen = true;

                selectionCombobox.IsDropDownOpen = false;
                selectionButtonContent.Source = new BitmapImage(new Uri("images/arrow-down.png", UriKind.Relative));
                isSelectionOpen = false;

                textFontCombobox.IsDropDownOpen = false;
                isTextFontFamilyOpen = false;

                fontSizeStackpanel.Visibility = Visibility.Collapsed;
                isTextFontSizeOpen = false;

                styleStrokeCombobox.IsDropDownOpen = false;
                isStyleStrokeOpen = false;
            } else
            {
                styleWidthCombobox.IsDropDownOpen = false;
                isStyleWidthOpen = false;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void styleWidthComboboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IWidthness selectedWidthness = (IWidthness)styleWidthCombobox.SelectedItem;
            styleWidthImage.Source = new BitmapImage(new Uri(selectedWidthness.widthnessImage, UriKind.Relative));
        }

        private void styleWidthComboboxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isStyleWidthOpen = false;
        }

        private void styleStrokeButtonClick(object sender, RoutedEventArgs e)
        {
            if (!isStyleStrokeOpen)
            {
                styleStrokeCombobox.IsDropDownOpen = true;
                isStyleStrokeOpen = true;

                selectionCombobox.IsDropDownOpen = false;
                selectionButtonContent.Source = new BitmapImage(new Uri("images/arrow-down.png", UriKind.Relative));
                isSelectionOpen = false;

                textFontCombobox.IsDropDownOpen = false;
                isTextFontFamilyOpen = false;

                fontSizeStackpanel.Visibility = Visibility.Collapsed;
                isTextFontSizeOpen = false;

                styleWidthCombobox.IsDropDownOpen = false;
                isStyleWidthOpen = false;
            } else
            {
                styleStrokeCombobox.IsDropDownOpen = false;
                isStyleStrokeOpen = false;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void styleStrokeComboboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IStroke selectedStroke = (IStroke)styleStrokeCombobox.SelectedItem;
            styleStrokeImage.Source = new BitmapImage(new Uri(selectedStroke.strokeImage, UriKind.Relative));
        }

        private void styleStrokeComboboxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isStyleStrokeOpen = false;
        }

        private void toolEraseButtonClick(object sender, RoutedEventArgs e)
        {
            functionSelected(2);
        }

        private void toolMouseButtonClick(object sender, RoutedEventArgs e)
        {
            functionSelected(3);
        }

        private void removeAllFunctionSelectedToSelectShape()
        {
            for (int i = 0; i < function.Count; i++)
            {
                if (function[i].Opacity != 0)
                {
                    function[i].Opacity = 0;
                }
            }
        }

        private void shapeListviewPreviewMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            removeAllFunctionSelectedToSelectShape();
            isFunctionSelected = true;
            isToolEraseOpen = false;
            ListViewItem selectedItem = (ListViewItem)sender;
            shapeListview.SelectedItem = selectedItem;

            selectedShape = (IShape)selectedItem.Content;

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void drawAreaMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDrawing = false;

            if (selectedShape != null)
            {
                if (!selectedShape.shapeName.Equals("Rectangle Selection"))
                {
                    layerList[currentLayerIndex].drawSurface.Add((IShape)selectedShape.Clone());
                    drawSurface.Add((IShape)selectedShape.Clone());
                    toolUndoButton.Opacity = 1;
                    toolRedoButton.Opacity = 0.3;

                    toolFileExportButton.Opacity = 1;
                    recoverList.Clear();
                }
            }

            if (selectedShape == null && isToolEraseOpen)
            {
                selectedShape = freeLine;
                IShape newFreeLine = (IShape)selectedShape.Clone();
                newFreeLine.addPointList(eraseList);

                if (!checkIfDrawSurfaceEmpty(layerList[currentLayerIndex].drawSurface))
                {
                    layerList[currentLayerIndex].drawSurface.Add(newFreeLine);
                    drawSurface.Add(newFreeLine);

                    toolUndoButton.Opacity = 1;
                    toolRedoButton.Opacity = 0.3;
                    recoverList.Clear();
                } else
                {
                    toolUndoButton.Opacity = 0.3;
                    toolRedoButton.Opacity = 0.3;
                    recoverList.Clear();
                }

                eraseList.Clear();
                selectedShape = null;
            }
        }

        private void drawAreaMouseMove(object sender, MouseEventArgs e)
        {
            if (isFunctionSelected && !isToolEraseOpen)
            {
                drawBackGround.Cursor = Cursors.Cross;
            }
            else if (isFunctionSelected && isToolEraseOpen)
            {
                drawBackGround.Cursor = new Cursor(new MemoryStream(Properties.Resources.toolErase));
            }
            else
            {
                drawBackGround.Cursor = null;
            }

            if (isDrawing && selectedShape != null)
            {
                endPoint = e.GetPosition(drawArea);
                drawArea.Children.Clear();

                foreach (var item in drawSurface)
                {
                    drawArea.Children.Add(item.convertShapeType());
                }

                if (isShiftDown)
                {
                    if (!selectedShape.shapeName.Contains("Shift"))
                    {
                        for (int i = 0; i < allShapeList.Count; i++)
                        {
                            string shiftShapeName = "Shift" + selectedShape.shapeName;
                            if (allShapeList[i].shapeName.Equals(shiftShapeName))
                            {
                                selectedShape = allShapeList[i];
                                break;
                            }
                        }
                    }
                } else
                {
                    if (selectedShape.shapeName.Contains("Shift"))
                    {
                        for (int i = 0; i < allShapeList.Count; i++)
                        {
                            string[] shapeNameSplit = selectedShape.shapeName.Split("Shift");
                            string shapeName = shapeNameSplit[1];
                            if (allShapeList[i].shapeName.Equals(shapeName))
                            {
                                selectedShape = allShapeList[i];
                                break;
                            }
                        }
                    }
                }

                selectedShape.addStartPoint(startPoint);
                selectedShape.addEndPoint(endPoint);
                selectedShape.addWidthness((IWidthness)styleWidthCombobox.SelectedItem);
                selectedShape.addStrokeStyle((IStroke)styleStrokeCombobox.SelectedItem);
                selectedShape.addColor(selectedColor);
                selectedShape.setShapeFill(isShapeFill);

                drawArea.Children.Add(selectedShape.convertShapeType());
            }

            if (isDrawing && selectedShape == null && isToolEraseOpen) 
            {
                Point point = e.GetPosition(drawArea);
                eraseList.Add(point);
                
                 Ellipse dot = new Ellipse();
                 dot.Fill = Brushes.White;
                 dot.Width = dot.Height = 20;
                 Canvas.SetLeft(dot, point.X);
                if (point.Y >= 20) 
                {
                    Canvas.SetTop(dot, point.Y - 20);
                }

                 drawArea.Children.Add(dot);
            }

            if (isEditting)
            {
                if(!string.IsNullOrEmpty(editType) && editShapeIndex != -1)
                {
                    Point point = e.GetPosition(drawArea);

                    bool outRangeX = false;
                    bool outRangeY = false;

                    double oldX;
                    double oldY;
                    double newX;
                    double newY;

                    switch (editType)
                    {
                        case "Left Top Editting":
                            oldX = drawSurface[editShapeIndex].getEndPoint().X;
                            oldY = drawSurface[editShapeIndex].getEndPoint().Y;

                            newX = oldX - 20;
                            newY = oldY - 20;

                            if (point.X + 20 > oldX) { outRangeX = true;}
                            if (point.Y + 20 > oldY) { outRangeY = true;}

                            if (!outRangeX && !outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(point);
                            } else if (outRangeX && !outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(newX, point.Y));
                            } else if (!outRangeX && outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(point.X, newY));
                            } else
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(newX, newY));
                            }
                            break;
                        case "Right Bottom Editting":
                            oldX = drawSurface[editShapeIndex].getStartPoint().X;
                            oldY = drawSurface[editShapeIndex].getStartPoint().Y;

                            newX = oldX + 20;
                            newY = oldY + 20;

                            if (point.X - 20 < oldX) { outRangeX = true; }
                            if (point.Y - 20 < oldY) { outRangeY = true; }

                            if (!outRangeX && !outRangeY)
                            {
                                drawSurface[editShapeIndex].addEndPoint(point);
                            }
                            else if (outRangeX && !outRangeY)
                            {
                                drawSurface[editShapeIndex].addEndPoint(new Point(newX, point.Y));
                            }
                            else if (!outRangeX && outRangeY)
                            {
                                drawSurface[editShapeIndex].addEndPoint(new Point(point.X, newY));
                            }
                            else
                            {
                                drawSurface[editShapeIndex].addEndPoint(new Point(newX, newY));
                            }
                            break;
                        case "Right Top Editting":
                            oldX = drawSurface[editShapeIndex].getStartPoint().X;
                            oldY = drawSurface[editShapeIndex].getEndPoint().Y;

                            newX = oldX + 20;
                            newY = oldY - 20;

                            if (point.X - 20 < oldX) { outRangeX = true; }
                            if (point.Y + 20 > oldY) { outRangeY = true; }

                            if (!outRangeX && !outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(drawSurface[editShapeIndex].getStartPoint().X, point.Y));
                                drawSurface[editShapeIndex].addEndPoint(new Point(point.X, drawSurface[editShapeIndex].getEndPoint().Y));
                            }
                            else if (outRangeX && !outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(drawSurface[editShapeIndex].getStartPoint().X, point.Y));
                                drawSurface[editShapeIndex].addEndPoint(new Point(newX, drawSurface[editShapeIndex].getEndPoint().Y));
                            }
                            else if (!outRangeX && outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(drawSurface[editShapeIndex].getStartPoint().X, newY));
                                drawSurface[editShapeIndex].addEndPoint(new Point(point.X, drawSurface[editShapeIndex].getEndPoint().Y));
                            }
                            else
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(drawSurface[editShapeIndex].getStartPoint().X, newY));
                                drawSurface[editShapeIndex].addEndPoint(new Point(newX, drawSurface[editShapeIndex].getEndPoint().Y));
                            }
                            break;
                        case "Left Bottom Editting":
                            oldX = drawSurface[editShapeIndex].getEndPoint().X;
                            oldY = drawSurface[editShapeIndex].getStartPoint().Y;

                            newX = oldX - 20;
                            newY = oldY + 20;

                            if (point.X + 20 > oldX) { outRangeX = true; }
                            if (point.Y - 20 < oldY) { outRangeY = true; }

                            if (!outRangeX && !outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(point.X, drawSurface[editShapeIndex].getStartPoint().Y));
                                drawSurface[editShapeIndex].addEndPoint(new Point(drawSurface[editShapeIndex].getEndPoint().X, point.Y));
                            }
                            else if (outRangeX && !outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(newX, drawSurface[editShapeIndex].getStartPoint().Y));
                                drawSurface[editShapeIndex].addEndPoint(new Point(drawSurface[editShapeIndex].getEndPoint().X, point.Y));
                            }
                            else if (!outRangeX && outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(point.X, drawSurface[editShapeIndex].getStartPoint().Y));
                                drawSurface[editShapeIndex].addEndPoint(new Point(drawSurface[editShapeIndex].getEndPoint().X, newY));
                            }
                            else
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(newX, drawSurface[editShapeIndex].getStartPoint().Y));
                                drawSurface[editShapeIndex].addEndPoint(new Point(drawSurface[editShapeIndex].getEndPoint().X, newY));
                            }
                            break;
                        case "Left Center Editting":
                            oldX = drawSurface[editShapeIndex].getEndPoint().X;
                            newX = oldX - 20;

                            if (point.X + 20 > oldX) { outRangeX = true; }

                            if (!outRangeX)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(point.X, drawSurface[editShapeIndex].getStartPoint().Y));
                            } else
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(newX, drawSurface[editShapeIndex].getStartPoint().Y));
                            }
                            break;
                        case "Right Center Editting":
                            oldX = drawSurface[editShapeIndex].getStartPoint().X;
                            newX = oldX + 20;

                            if (point.X - 20 < oldX) { outRangeX = true; }

                            if (!outRangeX)
                            {
                                drawSurface[editShapeIndex].addEndPoint(new Point(point.X, drawSurface[editShapeIndex].getEndPoint().Y));
                            }
                            else
                            {
                                drawSurface[editShapeIndex].addEndPoint(new Point(newX, drawSurface[editShapeIndex].getEndPoint().Y));
                            }
                            break;
                        case "Top Center Editting":
                            oldY = drawSurface[editShapeIndex].getEndPoint().Y;
                            newY = oldY - 20;

                            if (point.Y + 20 > oldY) { outRangeY = true; }

                            if (!outRangeY)
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(drawSurface[editShapeIndex].getStartPoint().X, point.Y));
                            }
                            else
                            {
                                drawSurface[editShapeIndex].addStartPoint(new Point(drawSurface[editShapeIndex].getStartPoint().X, newY));
                            }
                            break;
                        case "Bottom Center Editting":
                            oldY = drawSurface[editShapeIndex].getStartPoint().Y;
                            newY = oldY + 20;

                            if (point.Y - 20 < oldY) { outRangeY = true; }

                            if (!outRangeY)
                            {
                                drawSurface[editShapeIndex].addEndPoint(new Point(drawSurface[editShapeIndex].getEndPoint().X, point.Y));
                            }
                            else
                            {
                                drawSurface[editShapeIndex].addEndPoint(new Point(drawSurface[editShapeIndex].getEndPoint().X, newY));
                            }
                            break;
                        case "Move Editting":
                            double newStartX = point.X - movingStartX;
                            double newStartY = point.Y - movingStartY;

                            double newEndX = point.X - movingEndX;
                            double newEndY = point.Y - movingEndY;

                            drawSurface[editShapeIndex].addStartPoint(new Point(newStartX, newStartY));
                            drawSurface[editShapeIndex].addEndPoint(new Point(newEndX, newEndY));
                            break;
                        case "Start Point Editting":
                            drawSurface[editShapeIndex].addStartPoint(point);
                            break;
                        case "End Point Editting":
                            drawSurface[editShapeIndex].addEndPoint(point);
                            break;
                    }

                    redrawCanvasAfterEdit();
                }
            }
        }

        private void drawAreaMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectionCombobox.IsDropDownOpen = false;
            selectionButtonContent.Source = new BitmapImage(new Uri("images/arrow-down.png", UriKind.Relative));
            isSelectionOpen = false;

            textFontCombobox.IsDropDownOpen = false;
            isTextFontFamilyOpen = false;

            fontSizeStackpanel.Visibility = Visibility.Collapsed;
            isTextFontSizeOpen = false;

            styleWidthCombobox.IsDropDownOpen = false;
            isStyleWidthOpen = false;

            styleStrokeCombobox.IsDropDownOpen = false;
            isStyleStrokeOpen = false;

            layerListView.Visibility = Visibility.Collapsed;
            newLayerButton.Visibility = Visibility.Collapsed;
            layerInformStackPanel.Visibility = Visibility.Collapsed;

            if (isFunctionSelected)
            {
                isDrawing = true;
                startPoint = e.GetPosition(drawArea);

                if (selectedShape == null && isToolEraseOpen)
                {
                    eraseList.Add(startPoint);

                    Ellipse dot = new Ellipse();
                    dot.Fill = Brushes.White;
                    dot.Width = dot.Height = 20;
                    Canvas.SetLeft(dot, startPoint.X);
                    if (startPoint.Y >= 20)
                    {
                        Canvas.SetTop(dot, startPoint.Y - 20);
                    }

                    drawArea.Children.Add(dot);
                }

                if (selectedShape == null && isTextOpen)
                {
                    isDrawing = false;

                    selectedShape = text;
                    IShape newText = (IShape)selectedShape.Clone();

                    newText.addFontSize(globalFontSize);
                    newText.addFontFamily(globalFontFamily);
                    newText.addColor(selectedColor);

                    newText.addStartPoint(startPoint);
                    newText.addEndPoint(new Point(startPoint.X + globalFontSize * 10, startPoint.Y + globalFontSize * 2));
                    newText.setFocus(true);
                    newText.setBold(isTextBold);
                    newText.setItalic(isTextItalic);

                    if (isTextBackgroundFill)
                    {
                        newText.setShapeFill(true);
                        newText.setBackground(backgroundRed, backgroundGreen, backgroundBlue);
                    }
                    else
                    {
                        newText.setShapeFill(false);
                    }

                    drawArea.Children.Add(newText.convertShapeType());
                    TextBox newTextBox = newText.getTextBox();
                    newTextBox.Focus();
                    newTextBox.TextChanged += newTextBoxTextChanged;
                    newTextBox.LostFocus += newTextBoxLostFocus;

                    layerList[currentLayerIndex].drawSurface.Add(newText);
                    drawSurface.Add(newText);

                    selectedShape = null;
                }

                if (isSelecting)
                {
                    selectedShape = selection;
                }
            }
            else
            {
                Point point = e.GetPosition(drawArea);
                editShapeIndex = getEditShape(point);

                if (editShapeIndex != -1)
                {
                    toolHorizontalButton.Opacity = 1;
                    toolVerticalButton.Opacity = 1;

                    redrawCanvasAfterEdit();
                } else
                {
                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();

                    editShapeIndex = -1;
                    isEditting = false;

                    for (int i = 0; i < drawSurface.Count; i++)
                    {
                        drawSurface[i].setEdit(false);
                        drawArea.Children.Add(drawSurface[i].convertShapeType());
                    }

                    toolHorizontalButton.Opacity = 0.3;
                    toolVerticalButton.Opacity = 0.3;
                }
            }
        }

        private void EdittingMouseUp(object sender, MouseButtonEventArgs e)
        {
            isEditting = false;
            editType = "";
        }
        private void EndButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "End Point Editting";
        }
        private void StartButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Start Point Editting";
        }

        private void EditGridPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Move Editting";
            movingPoint = e.GetPosition(drawArea);

            movingStartX = movingPoint.X - drawSurface[editShapeIndex].getStartPoint().X;
            movingStartY = movingPoint.Y - drawSurface[editShapeIndex].getStartPoint().Y;

            movingEndX = movingPoint.X - drawSurface[editShapeIndex].getEndPoint().X;
            movingEndY = movingPoint.Y - drawSurface[editShapeIndex].getEndPoint().Y;
        }
        private void BottomCenterButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Bottom Center Editting";
        }

        private void TopCenterButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Top Center Editting";
        }

        private void RightCenterButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Right Center Editting";
        }

        private void LeftCenterButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Left Center Editting";
        }

        private void RightBottomButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Right Bottom Editting";
        }

        private void LeftBottomButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Left Bottom Editting";
        }

        private void RightTopButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Right Top Editting";
        }

        private void LeftTopButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            editType = "Left Top Editting";
        }

        private void RotateButtonPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isEditting = true;
            double rotateAngle = layerList[currentLayerIndex].drawSurface[editShapeIndex].getAngle();
            switch (rotateAngle)
            {
                case 0:
                    layerList[currentLayerIndex].drawSurface[editShapeIndex].setAngle(90);
                    break;
                case 90:
                    layerList[currentLayerIndex].drawSurface[editShapeIndex].setAngle(180);
                    break;
                case 180:
                    layerList[currentLayerIndex].drawSurface[editShapeIndex].setAngle(270);
                    break;
                case 270:
                    layerList[currentLayerIndex].drawSurface[editShapeIndex].setAngle(0);
                    break;
                default: break;
            }
            redrawCanvasAfterEdit();
        }

        private void newTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            IShape text = layerList[currentLayerIndex].drawSurface[layerList[currentLayerIndex].drawSurface.Count - 1];
            layerList[currentLayerIndex].drawSurface.RemoveAt(layerList[currentLayerIndex].drawSurface.Count - 1);

            if (textBox.Text.Length > 15)
            {
                string textResult = "";
                string[] newLineSplit = textBox.Text.Split("\r\n");
                if (newLineSplit.Length > 1)
                {
                    int lineMultiple = (textBox.Text.Length / 15) + 2 + newLineSplit.Length - 1;
                    text.addEndPoint(new Point(startPoint.X + globalFontSize * 10, startPoint.Y + globalFontSize * lineMultiple));

                    for (int i = 0; i < newLineSplit.Length; i++)
                    {
                        if (newLineSplit[i].Length > 15)
                        {
                            string[] textSplit = newLineSplit[i].Split();
                            string tempText = "";
                            bool isLongString = false;

                            if (textSplit.Length > 1)
                            {
                                for (int j = 0; j < textSplit.Length; j++)
                                {
                                    tempText += " " + textSplit[j] + " ";
                                    tempText = tempText.Trim();

                                    if (tempText.Length > 15)
                                    {
                                        isLongString = true;
                                        j--;
                                        string[] tempSplitText = tempText.Split();
                                        for (int k = 0; k < tempSplitText.Length - 1; k++)
                                        {
                                            textResult += " " + tempSplitText[k] + " ";
                                            textResult = textResult.Trim();
                                            textResult = Regex.Replace(textResult, @"\s+", " ");
                                        }
                                        textResult += Environment.NewLine;
                                        tempText = "";
                                    }
                                }

                                if (isLongString)
                                {
                                    textResult += tempText;
                                }
                                else
                                {
                                    textResult = tempText;
                                }
                            } else
                            {
                                for (int j = 0; j < newLineSplit[i].Length; j += 15)
                                {
                                    if (j + 15 < newLineSplit[i].Length)
                                    {
                                        textResult += newLineSplit[i].Substring(j, 15);
                                        textResult += Environment.NewLine;
                                    }
                                    else
                                    {
                                        textResult += newLineSplit[i].Substring(j, newLineSplit[i].Length - j);
                                    }
                                }
                            }
                        } else
                        {
                            textResult += newLineSplit[i] + Environment.NewLine;   
                        }
                    }

                    text.setTextString(textResult);
                    layerList[currentLayerIndex].drawSurface.Add(text);
                } else
                {
                    int lineMultiple = (textBox.Text.Length / 15) + 2;
                    text.addEndPoint(new Point(startPoint.X + globalFontSize * 10, startPoint.Y + globalFontSize * lineMultiple));

                    string[] textSplit = textBox.Text.Split();
                    string tempText = "";
                    bool isLongString = false;

                    if (textSplit.Length > 1)
                    {
                        for (int i = 0; i < textSplit.Length; i++)
                        {
                            tempText += " " + textSplit[i] + " ";
                            tempText = tempText.Trim();

                            if (tempText.Length > 15)
                            {
                                isLongString = true;
                                i--;
                                string[] tempSplitText = tempText.Split();
                                for (int j = 0; j < tempSplitText.Length - 1; j++)
                                {
                                    textResult += " " + tempSplitText[j] + " ";
                                    textResult = textResult.Trim();
                                    textResult = Regex.Replace(textResult, @"\s+", " ");
                                }
                                textResult += Environment.NewLine;
                                tempText = "";
                            }
                        }


                        if (isLongString)
                        {
                            textResult += tempText;
                        }
                        else
                        {
                            textResult = tempText;
                        }

                        text.setTextString(textResult);
                        layerList[currentLayerIndex].drawSurface.Add(text);
                    } else
                    {
                        for (int i = 0; i < textBox.Text.Length; i+=15)
                        {
                            if (i + 15 < textBox.Text.Length)
                            {
                                textResult += textBox.Text.Substring(i, 15);
                                textResult += Environment.NewLine;
                            } else
                            {
                                textResult += textBox.Text.Substring(i, textBox.Text.Length - i);
                            }
                        }

                        text.setTextString(textResult);
                        layerList[currentLayerIndex].drawSurface.Add(text);
                    }
                }
            } else
            {
                int countNewLine = textBox.LineCount - 1;
                text.addEndPoint(new Point(startPoint.X + globalFontSize * 10, startPoint.Y + globalFontSize * (countNewLine + 2)));
                text.setTextString(textBox.Text);
                layerList[currentLayerIndex].drawSurface.Add(text);
            }
        }

        private void newTextBoxLostFocus(object sender, EventArgs e)
        {
            IShape text = layerList[currentLayerIndex].drawSurface[layerList[currentLayerIndex].drawSurface.Count - 1];
            layerList[currentLayerIndex].drawSurface.RemoveAt(layerList[currentLayerIndex].drawSurface.Count - 1);
            text.setFocus(false);

            if (!string.IsNullOrEmpty(text.getTextBox().Text))
            {
                layerList[currentLayerIndex].drawSurface.Add(text);
                toolUndoButton.Opacity = 1;
                toolRedoButton.Opacity = 0.3;
            }

            drawArea.Children.Clear();

            foreach (var item in layerList[currentLayerIndex].drawSurface)
            {
                drawArea.Children.Add(item.convertShapeType());
            }
        }

        private void WindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift) 
            { 
                isShiftDown = true;
            }

            if ((e.Key == Key.Z) && (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control) 
            {
                if (toolUndoButton.Opacity != 0.3)
                {
                    recoverList.Add(layerList[currentLayerIndex].drawSurface[layerList[currentLayerIndex].drawSurface.Count - 1]);
                    layerList[currentLayerIndex].drawSurface.RemoveAt(layerList[currentLayerIndex].drawSurface.Count - 1);
                    drawSurface.RemoveAt(drawSurface.Count - 1);

                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();

                    editShapeIndex = -1;
                    isEditting = false;

                    for (int i = 0; i < drawSurface.Count; i++)
                    {
                        drawSurface[i].setEdit(false);
                        drawArea.Children.Add(drawSurface[i].convertShapeType());
                    }

                    toolHorizontalButton.Opacity = 0.3;
                    toolVerticalButton.Opacity = 0.3;

                    if (checkIfDrawSurfaceEmpty(layerList[currentLayerIndex].drawSurface))
                    {
                        toolUndoButton.Opacity = 0.3;
                    }

                    toolRedoButton.Opacity = 1;
                }
            }

            if ((e.Key == Key.Y) && (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (toolRedoButton.Opacity != 0.3)
                {
                    layerList[currentLayerIndex].drawSurface.Add(recoverList[recoverList.Count - 1]);
                    drawSurface.Add(recoverList[recoverList.Count - 1]);
                    recoverList.RemoveAt(recoverList.Count - 1);

                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();

                    editShapeIndex = -1;
                    isEditting = false;

                    for (int i = 0; i < drawSurface.Count; i++)
                    {
                        drawSurface[i].setEdit(false);
                        drawArea.Children.Add(drawSurface[i].convertShapeType());
                    }

                    toolHorizontalButton.Opacity = 0.3;
                    toolVerticalButton.Opacity = 0.3;

                    if (recoverList.Count == 0)
                    {
                        toolRedoButton.Opacity = 0.3;
                    }

                    toolUndoButton.Opacity = 1;
                }
            }

            if (e.Key == Key.Delete)
            {
                if (editShapeIndex != -1)
                {
                    layerList[currentLayerIndex].drawSurface.RemoveAt(editShapeIndex);
                    drawSurface.RemoveAt(editShapeIndex);

                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();

                    editShapeIndex = -1;
                    isEditting = false;

                    for (int i = 0; i < drawSurface.Count; i++)
                    {
                        drawSurface[i].setEdit(false);
                        drawArea.Children.Add(drawSurface[i].convertShapeType());
                    }

                    toolHorizontalButton.Opacity = 0.3;
                    toolVerticalButton.Opacity = 0.3;

                    if (layerList[currentLayerIndex].drawSurface.Count == 0)
                    {
                        toolUndoButton.Opacity = 0.3;
                    }

                    return;
                }

                if (layerListView.Visibility == Visibility.Visible)
                {
                    if (layerList.Count > 1) 
                    {
                        layerList.RemoveAt(currentLayerIndex);
                        currentLayerIndex = 0;
                        layerListView.ItemsSource = "";
                        layerListView.ItemsSource = layerList;
                        layerListView.SelectedIndex = currentLayerIndex;
                    } else
                    {
                        MessageBox.Show("This is the last layer. Cannot delete", "Unable to delete", MessageBoxButton.OK, MessageBoxImage.Error);
                        layerButton.Focusable = false;
;                   }

                    return;
                }
            }

            if ((e.Key == Key.C) && (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (editShapeIndex != -1)
                {
                    newShape = null;
                    newShape = (IShape)drawSurface[editShapeIndex].Clone();

                    double distanceX = Math.Abs(newShape.getStartPoint().X - newShape.getEndPoint().X);
                    double distanceY = Math.Abs(newShape.getStartPoint().Y - newShape.getEndPoint().Y);

                    newShape.addStartPoint(new Point(0, 0));
                    newShape.addEndPoint(new Point(distanceX, distanceY));
                }

                if (selectedShape != null)
                {
                    if (selectedShape.shapeName.Equals("Rectangle Selection"))
                    {
                        List<int> listIndices = getShapeInsideRectangle(selectedShape);
                        listNewShape = new List<IShape>();

                        for (int i = 0; i < listIndices.Count; i++)
                        {
                            newShape = null;
                            newShape = (IShape)drawSurface[listIndices[i]].Clone();

                            double distanceX = Math.Abs(newShape.getStartPoint().X - newShape.getEndPoint().X);
                            double distanceY = Math.Abs(newShape.getStartPoint().Y - newShape.getEndPoint().Y);

                            newShape.addStartPoint(new Point(0, 0));
                            newShape.addEndPoint(new Point(distanceX, distanceY));

                            listNewShape.Add(newShape);
                        }
                    }

                    newShape = null;
                }
            }

            if ((e.Key == Key.X) && (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (editShapeIndex != -1)
                {
                    newShape = null;
                    newShape = (IShape)drawSurface[editShapeIndex].Clone();

                    double distanceX = Math.Abs(newShape.getStartPoint().X - newShape.getEndPoint().X);
                    double distanceY = Math.Abs(newShape.getStartPoint().Y - newShape.getEndPoint().Y);

                    newShape.addStartPoint(new Point(0, 0));
                    newShape.addEndPoint(new Point(distanceX, distanceY));

                    drawSurface.RemoveAt(editShapeIndex);

                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();

                    editShapeIndex = -1;
                    isEditting = false;

                    for (int i = 0; i < drawSurface.Count; i++)
                    {
                        drawSurface[i].setEdit(false);
                        drawArea.Children.Add(drawSurface[i].convertShapeType());
                    }

                    toolHorizontalButton.Opacity = 0.3;
                    toolVerticalButton.Opacity = 0.3;
                }

                if (selectedShape != null)
                {
                    if (selectedShape.shapeName.Equals("Rectangle Selection"))
                    {
                        List<int> listIndices = getShapeInsideRectangle(selectedShape);
                        listNewShape = new List<IShape>();

                        for (int i = 0; i < listIndices.Count; i++)
                        {
                            newShape = null;
                            newShape = (IShape)drawSurface[listIndices[i]].Clone();

                            double distanceX = Math.Abs(newShape.getStartPoint().X - newShape.getEndPoint().X);
                            double distanceY = Math.Abs(newShape.getStartPoint().Y - newShape.getEndPoint().Y);

                            newShape.addStartPoint(new Point(0, 0));
                            newShape.addEndPoint(new Point(distanceX, distanceY));

                            listNewShape.Add(newShape);
                        }

                        for (int i = listIndices.Count - 1; i >= 0; i--)
                        {
                            drawSurface.RemoveAt(listIndices[i]);
                        }
                    }

                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();

                    editShapeIndex = -1;
                    isEditting = false;

                    for (int i = 0; i < drawSurface.Count; i++)
                    {
                        drawSurface[i].setEdit(false);
                        drawArea.Children.Add(drawSurface[i].convertShapeType());
                    }

                    toolHorizontalButton.Opacity = 0.3;
                    toolVerticalButton.Opacity = 0.3;

                    newShape = null;
                }
            }

            if ((e.Key == Key.V) && (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (newShape != null)
                {
                    layerList[currentLayerIndex].drawSurface.Add(newShape);
                    drawSurface.Add(newShape);

                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();

                    editShapeIndex = -1;
                    isEditting = false;

                    for (int i = 0; i < drawSurface.Count; i++)
                    {
                        drawSurface[i].setEdit(false);
                        drawArea.Children.Add(drawSurface[i].convertShapeType());
                    }

                    toolHorizontalButton.Opacity = 0.3;
                    toolVerticalButton.Opacity = 0.3;

                    return;
                }

                if (listNewShape.Count > 0) 
                {
                    for (int i = 0; i < listNewShape.Count; i++)
                    {
                        layerList[currentLayerIndex].drawSurface.Add(listNewShape[i]);
                        drawSurface.Add(listNewShape[i]);
                    }

                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();

                    editShapeIndex = -1;
                    isEditting = false;

                    for (int i = 0; i < drawSurface.Count; i++)
                    {
                        drawSurface[i].setEdit(false);
                        drawArea.Children.Add(drawSurface[i].convertShapeType());
                    }

                    toolHorizontalButton.Opacity = 0.3;
                    toolVerticalButton.Opacity = 0.3;
                }
            }

            if ((e.Key == Key.S) && (System.Windows.Forms.Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (toolFileExportButton.Opacity != 0.3)
                {
                    dialogBorder.Visibility = Visibility.Visible;
                }
            }
        }

        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            isShiftDown = false;
        }

        private void WindowMouseLeave(object sender, MouseEventArgs e)
        {
            if (isDrawing) {
                isDrawing = false;
                if (selectedShape != null)
                {
                    layerList[currentLayerIndex].drawSurface.Add((IShape)selectedShape.Clone());
                    toolUndoButton.Opacity = 1;
                    toolRedoButton.Opacity = 0.3;
                    recoverList.Clear();
                }
            }

            if (isEditting)
            {
                isEditting = false;
                editType = "";
            }
        }

        private void toolBarMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                isDrawing = false;
                if (selectedShape != null)
                {
                    layerList[currentLayerIndex].drawSurface.Add((IShape)selectedShape.Clone());
                    toolUndoButton.Opacity = 1;
                    toolRedoButton.Opacity = 0.3;
                    recoverList.Clear();
                }
            }

            if (isEditting)
            {
                isEditting = false;
                editType = "";
            }
        }

        private void colorListviewPreviewMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem selectedItem = (ListViewItem)sender;
            colorListview.SelectedItem = selectedItem;

            selectedColor = (IColor)selectedItem.Content;

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void styleFillButtonClick(object sender, RoutedEventArgs e)
        {
            if (isShapeFill)
            {
                styleFillButton.Opacity = 0.3;
                isShapeFill = false;
            } else
            {
                styleFillButton.Opacity = 1;
                isShapeFill = true;
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private void customColorButtonClick(object sender, RoutedEventArgs e)
        {
            selectedColor = customColor;
            ColorDialog colorDialog = new ColorDialog();

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color color = colorDialog.Color;

                byte colorRed = color.R;
                byte colorGreen = color.G;
                byte colorBlue = color.B;
            
                if (!checkExistedColor(colorRed, colorGreen, colorBlue))
                {
                    IColor newCustomColor = (IColor)selectedColor.Clone();

                    newCustomColor.addColorRGB(colorRed, colorGreen, colorBlue);
                    colorList.Add(newCustomColor);

                    colorListview.ItemsSource = "";
                    colorListview.ItemsSource = colorList;

                    selectedColor = colorList[colorList.Count - 1];
                    colorListview.SelectedIndex = colorList.Count - 1;

                    colorListview.ScrollIntoView(colorListview.Items[colorListview.Items.Count - 1]);
                } else
                {
                    int index = getExistedColorByRGB(colorRed, colorGreen, colorBlue);
                    if (index != -1)
                    {
                        selectedColor = colorList[index];
                        colorListview.SelectedIndex = index;
                        colorListview.ScrollIntoView(colorListview.Items[index]);
                    }
                }
            }
        }

        private bool checkExistedColor(byte r, byte g, byte b)
        {
            for (int i = 0; i < colorList.Count; i++)
            {
                SolidColorBrush colorBrush = colorList[i].colorValue;
                System.Windows.Media.Color color = colorBrush.Color;

                byte colorRed = color.R;
                byte colorGreen = color.G;
                byte colorBlue = color.B;

                if (colorRed == r && colorGreen == g && colorBlue == b)
                {
                    return true;
                }
            }
            return false;
        }

        private int getExistedColorByRGB(byte r, byte g, byte b) 
        {
            for (int i = 0; i < colorList.Count; i++)
            {
                SolidColorBrush colorBrush = colorList[i].colorValue;
                System.Windows.Media.Color color = colorBrush.Color;

                byte colorRed = color.R;
                byte colorGreen = color.G;
                byte colorBlue = color.B;

                if (colorRed == r && colorGreen == g && colorBlue == b)
                {
                    return i;
                }
            }
            return -1;
        }

        private void toolUndoButtonMouseEnter(object sender, MouseEventArgs e)
        {
            if (toolUndoButton.Opacity == 0.3)
            {
                toolUndoButton.Cursor = null;
            } else
            {
                toolUndoButton.Cursor = Cursors.Hand;
            }
        }

        private void toolUndoButtonMouseLeave(object sender, MouseEventArgs e)
        {
            toolUndoButton.Cursor = null;
        }

        private void toolRedoButtonMouseEnter(object sender, MouseEventArgs e)
        {
            if (toolRedoButton.Opacity == 0.3)
            {
                toolRedoButton.Cursor = null;
            }
            else
            {
                toolRedoButton.Cursor = Cursors.Hand;
            }
        }

        private void toolRedoButtonMouseLeave(object sender, MouseEventArgs e)
        {
            toolRedoButton.Cursor = null;
        }

        private void toolUndoButtonClick(object sender, RoutedEventArgs e)
        {
            if (toolUndoButton.Opacity != 0.3)
            {
                recoverList.Add(layerList[currentLayerIndex].drawSurface[layerList[currentLayerIndex].drawSurface.Count - 1]);
                layerList[currentLayerIndex].drawSurface.RemoveAt(layerList[currentLayerIndex].drawSurface.Count - 1);
                drawSurface.RemoveAt(drawSurface.Count - 1);

                drawArea.Children.Clear();
                drawBackGround.Children.Clear();

                editShapeIndex = -1;
                isEditting = false;

                for (int i = 0; i < drawSurface.Count; i++)
                {
                    drawSurface[i].setEdit(false);
                    drawArea.Children.Add(drawSurface[i].convertShapeType());
                }

                toolHorizontalButton.Opacity = 0.3;
                toolVerticalButton.Opacity = 0.3;

                if (checkIfDrawSurfaceEmpty(layerList[currentLayerIndex].drawSurface))
                {
                    toolUndoButton.Opacity = 0.3;
                }

                toolRedoButton.Opacity = 1;
            }
        }

        private void toolRedoButtonClick(object sender, RoutedEventArgs e)
        {
            if (toolRedoButton.Opacity != 0.3)
            {
                layerList[currentLayerIndex].drawSurface.Add(recoverList[recoverList.Count - 1]);
                drawSurface.Add(recoverList[recoverList.Count - 1]);
                
                recoverList.RemoveAt(recoverList.Count - 1);

                drawArea.Children.Clear();
                drawBackGround.Children.Clear();

                editShapeIndex = -1;
                isEditting = false;

                for (int i = 0; i < drawSurface.Count; i++)
                {
                    drawSurface[i].setEdit(false);
                    drawArea.Children.Add(drawSurface[i].convertShapeType());
                }

                toolHorizontalButton.Opacity = 0.3;
                toolVerticalButton.Opacity = 0.3;

                if (recoverList.Count == 0)
                {
                    toolRedoButton.Opacity = 0.3;
                }

                toolUndoButton.Opacity = 1;
            }
        }

        private bool checkIfDrawSurfaceEmpty(List<IShape> surface)
        {
            if (surface.Count == 0)
            {
                return true;
            }

            foreach (IShape shape in surface)
            {
                if (shape.shapeName != "Free Line")
                {
                    return false;
                }
            }

            return true;
        }

        private void textBackGroundCustomClick(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color color = colorDialog.Color;

                backgroundRed = color.R;
                backgroundGreen = color.G;
                backgroundBlue = color.B;

                textBackgroundBorder.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(backgroundRed, backgroundGreen, backgroundBlue));
            }

            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            editShapeIndex = -1;
            isEditting = false;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                drawSurface[i].setEdit(false);
                drawArea.Children.Add(drawSurface[i].convertShapeType());
            }

            toolHorizontalButton.Opacity = 0.3;
            toolVerticalButton.Opacity = 0.3;
        }

        private bool isPointInsideShape(IShape shape, Point point)
        {
            bool isPointInsideHorizontal = false;
            bool isPointInsideVertical = false;

            if (!shape.shapeName.Equals("ShiftLine") && !shape.shapeName.Equals("Line"))
            {
                isPointInsideHorizontal =
                    (shape.getStartPoint().X <= point.X && shape.getEndPoint().X >= point.X) ||
                    (shape.getStartPoint().X >= point.X && shape.getEndPoint().X <= point.X);
                isPointInsideVertical =
                    (shape.getStartPoint().Y <= point.Y && shape.getEndPoint().Y >= point.Y) ||
                    (shape.getStartPoint().Y >= point.Y && shape.getEndPoint().Y <= point.Y);
            } else
            {
                if ((shape.getStartPoint().X == shape.getEndPoint().X) || (shape.getStartPoint().Y == shape.getEndPoint().Y)) 
                {
                    if (shape.getStartPoint().X == shape.getEndPoint().X)
                    {
                        double startPointX = shape.getStartPoint().X - 20; 
                        double endPointX = shape.getEndPoint().X + 20;

                        isPointInsideHorizontal =
                            (startPointX <= point.X && endPointX >= point.X) ||
                            (startPointX >= point.X && endPointX <= point.X);
                        isPointInsideVertical =
                            (shape.getStartPoint().Y <= point.Y && shape.getEndPoint().Y >= point.Y) ||
                            (shape.getStartPoint().Y >= point.Y && shape.getEndPoint().Y <= point.Y);
                    } else
                    {
                        double startPointY = shape.getStartPoint().Y - 20;
                        double endPointY = shape.getEndPoint().Y + 20;

                        isPointInsideHorizontal =
                            (shape.getStartPoint().X <= point.X && shape.getEndPoint().X >= point.X) ||
                            (shape.getStartPoint().X >= point.X && shape.getEndPoint().X <= point.X);
                        isPointInsideVertical =
                            (startPointY <= point.Y && endPointY >= point.Y) ||
                            (startPointY >= point.Y && endPointY <= point.Y);
                    }
                } else
                {
                    isPointInsideHorizontal =
                        (shape.getStartPoint().X <= point.X && shape.getEndPoint().X >= point.X) ||
                        (shape.getStartPoint().X >= point.X && shape.getEndPoint().X <= point.X);
                    isPointInsideVertical =
                        (shape.getStartPoint().Y <= point.Y && shape.getEndPoint().Y >= point.Y) ||
                        (shape.getStartPoint().Y >= point.Y && shape.getEndPoint().Y <= point.Y);
                }

            }

            if (isPointInsideHorizontal && isPointInsideVertical)
            {
                return true;
            } else
            {
                return false;
            }
        }

        private int getEditShape(Point point)
        {
            double minDistance = -1;
            int index =  -1;

            for (int i = 0; i < drawSurface.Count; i++)
            {
                if (!drawSurface[i].shapeName.Equals("Free Line"))
                {
                    if (isPointInsideShape(drawSurface[i], point))
                    {
                        double distance = calculateDistance(drawSurface[i].getCenterPoint(), point);
                        if (minDistance != -1)
                        {
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                index = i;
                            }
                        }
                        else
                        {
                            minDistance = distance;
                            index = i;
                        }
                    }
                }
            }

            if (minDistance != -1)
            {
                return index;
            } else
            {
                return -1;
            }
        }

        private double calculateDistance(Point point1, Point point2)
        {
            return Math.Sqrt(Math.Pow((point1.X - point2.X), 2) + Math.Pow((point1.Y - point2.Y), 2));
        }

        private void drawBackGroundMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isEditting)
            {
                isEditting = false;
                editType = "";
            }
        }

        private void toolHorizontalButtonClick(object sender, RoutedEventArgs e)
        {
            if (toolHorizontalButton.Opacity != 0.3)
            {
                if (layerList[currentLayerIndex].drawSurface[editShapeIndex].getFlipHorizontally())
                {
                    layerList[currentLayerIndex].drawSurface[editShapeIndex].setFlipHorizontally(false);
                    drawSurface[editShapeIndex].setFlipHorizontally(false);
                } else
                {
                    layerList[currentLayerIndex].drawSurface[editShapeIndex].setFlipHorizontally(true);
                    drawSurface[editShapeIndex].setFlipHorizontally(true);
                }

                redrawCanvasAfterEdit();
            }
        }

        private void toolVerticalButtonClick(object sender, RoutedEventArgs e)
        {
            if (toolVerticalButton.Opacity != 0.3)
            {
                if (layerList[currentLayerIndex].drawSurface[editShapeIndex].getFlipVertically())
                {
                    layerList[currentLayerIndex].drawSurface[editShapeIndex].setFlipVertically(false);
                    drawSurface[editShapeIndex].setFlipVertically(false);
                } else
                {
                    layerList[currentLayerIndex].drawSurface[editShapeIndex].setFlipVertically(true);
                    drawSurface[editShapeIndex].setFlipVertically(true);
                }

                redrawCanvasAfterEdit();
            }
        }

        private void toolHorizontalButtonMouseEnter(object sender, MouseEventArgs e)
        {
            if (toolHorizontalButton.Opacity == 0.3)
            {
                toolHorizontalButton.Cursor = null;
            } else
            {
                toolHorizontalButton.Cursor = Cursors.Hand;
            }
        }

        private void toolHorizontalButtonMouseLeave(object sender, MouseEventArgs e)
        {
            toolHorizontalButton.Cursor = null;
        }

        private void toolVerticalButtonMouseEnter(object sender, MouseEventArgs e)
        {
            if (toolVerticalButton.Opacity == 0.3)
            {
                toolVerticalButton.Cursor = null;
            }
            else
            {
                toolVerticalButton.Cursor = Cursors.Hand;
            }
        }

        private void toolVerticalButtonMouseLeave(object sender, MouseEventArgs e)
        {
            toolVerticalButton.Cursor = null;
        }

        private void redrawCanvasAfterEdit()
        {
            drawArea.Children.Clear();
            drawBackGround.Children.Clear();

            for (int i = 0; i < drawSurface.Count; i++)
            {
                if (i == editShapeIndex)
                {
                    drawSurface[i].setEdit(true);
                    drawBackGround.Children.Add(drawSurface[i].convertShapeType());
                    if (drawSurface[i].shapeName.Equals("Line") || drawSurface[i].shapeName.Equals("ShiftLine"))
                    {
                        if (drawSurface[i].shapeName.Equals("ShiftLine"))
                        {
                            Point oldStartPoint = drawSurface[i].getStartPoint();
                            Point oldEndPoint = drawSurface[i].getEndPoint();

                            for (int j = 0; j < allShapeList.Count; j++)
                            {
                                if (allShapeList[j].shapeName.Equals("Line"))
                                {
                                    drawSurface.RemoveAt(i);

                                    IShape newLine = (IShape)allShapeList[j].Clone();
                                    newLine.addStartPoint(oldStartPoint);
                                    newLine.addEndPoint(oldEndPoint);
                                    newLine.addWidthness((IWidthness)styleWidthCombobox.SelectedItem);
                                    newLine.addStrokeStyle((IStroke)styleStrokeCombobox.SelectedItem);
                                    newLine.addColor(selectedColor);
                                    newLine.setShapeFill(isShapeFill);
                                    newLine.setEdit(true);

                                    drawSurface.Insert(i, newLine);

                                    drawBackGround.Children.Add(drawSurface[i].convertShapeType());
                                    break;
                                }
                            }
                        }

                        EditGrid = drawSurface[i].getEditGrid();
                        StartButton = drawSurface[i].getStartButton();
                        EndButton = drawSurface[i].getEndButton();

                        EditGrid.Cursor = Cursors.SizeAll;
                        StartButton.Cursor = Cursors.SizeNS;
                        EndButton.Cursor = Cursors.SizeNS;

                        EditGrid.PreviewMouseRightButtonDown += EditGridPreviewMouseRightButtonDown;
                        EditGrid.PreviewMouseRightButtonUp += EdittingMouseUp;

                        StartButton.PreviewMouseRightButtonDown += StartButtonPreviewMouseRightButtonDown;
                        StartButton.PreviewMouseRightButtonUp += EdittingMouseUp;

                        EndButton.PreviewMouseRightButtonDown += EndButtonPreviewMouseRightButtonDown;
                        EndButton.PreviewMouseRightButtonUp += EdittingMouseUp;
                    }
                    else
                    {
                        EditGrid = drawSurface[i].getEditGrid();
                        LeftTopButton = drawSurface[i].getLeftTopButton();
                        RightTopButton = drawSurface[i].getRightTopButton();
                        LeftBottomButton = drawSurface[i].getLeftBottomButton();
                        RightBottomButton = drawSurface[i].getRightBottomButton();
                        LeftCenterButton = drawSurface[i].getLeftCenterButton();
                        RightCenterButton = drawSurface[i].getRightCenterButton();
                        TopCenterButton = drawSurface[i].getTopCenterButton();
                        BottomCenterButton = drawSurface[i].getBottomCenterButton();
                        RotateButton = drawSurface[i].getRotateButton();

                        EditGrid.Cursor = Cursors.SizeAll;
                        LeftTopButton.Cursor = Cursors.SizeNWSE;
                        RightBottomButton.Cursor = Cursors.SizeNWSE;
                        RightTopButton.Cursor = Cursors.SizeNESW;
                        LeftBottomButton.Cursor = Cursors.SizeNESW;
                        LeftCenterButton.Cursor = Cursors.SizeWE;
                        RightCenterButton.Cursor = Cursors.SizeWE;
                        TopCenterButton.Cursor = Cursors.SizeNS;
                        BottomCenterButton.Cursor = Cursors.SizeNS;
                        RotateButton.Cursor = Cursors.Hand;

                        EditGrid.PreviewMouseRightButtonDown += EditGridPreviewMouseRightButtonDown;
                        EditGrid.PreviewMouseRightButtonUp += EdittingMouseUp;

                        LeftTopButton.PreviewMouseRightButtonDown += LeftTopButtonPreviewMouseRightButtonDown;
                        LeftTopButton.PreviewMouseRightButtonUp += EdittingMouseUp;

                        RightTopButton.PreviewMouseRightButtonDown += RightTopButtonPreviewMouseRightButtonDown;
                        RightTopButton.PreviewMouseRightButtonUp += EdittingMouseUp;

                        LeftBottomButton.PreviewMouseRightButtonDown += LeftBottomButtonPreviewMouseRightButtonDown;
                        LeftBottomButton.PreviewMouseRightButtonUp += EdittingMouseUp;

                        RightBottomButton.PreviewMouseRightButtonDown += RightBottomButtonPreviewMouseRightButtonDown;
                        RightBottomButton.PreviewMouseRightButtonUp += EdittingMouseUp;

                        LeftCenterButton.PreviewMouseRightButtonDown += LeftCenterButtonPreviewMouseRightButtonDown;
                        LeftCenterButton.PreviewMouseRightButtonUp += EdittingMouseUp;

                        RightCenterButton.PreviewMouseRightButtonDown += RightCenterButtonPreviewMouseRightButtonDown;
                        RightCenterButton.PreviewMouseRightButtonUp += EdittingMouseUp;

                        TopCenterButton.PreviewMouseRightButtonDown += TopCenterButtonPreviewMouseRightButtonDown;
                        TopCenterButton.PreviewMouseRightButtonUp += EdittingMouseUp;

                        BottomCenterButton.PreviewMouseRightButtonDown += BottomCenterButtonPreviewMouseRightButtonDown;
                        BottomCenterButton.PreviewMouseRightButtonUp += EdittingMouseUp;

                        RotateButton.PreviewMouseRightButtonDown += RotateButtonPreviewMouseRightButtonDown;
                        RotateButton.PreviewMouseRightButtonUp += EdittingMouseUp;
                    }
                }
                else
                {
                    drawSurface[i].setEdit(false);
                    drawArea.Children.Add(drawSurface[i].convertShapeType());
                }
            }
        }

        private void layerButtonClick(object sender, RoutedEventArgs e)
        {
            if (layerListView.Visibility == Visibility.Visible)
            {
                layerListView.Visibility = Visibility.Collapsed;
                newLayerButton.Visibility = Visibility.Collapsed;
                layerInformStackPanel.Visibility = Visibility.Collapsed;
            } else if (layerListView.Visibility == Visibility.Collapsed)
            {
                layerListView.Visibility = Visibility.Visible;
                newLayerButton.Visibility = Visibility.Visible;
                layerInformStackPanel.Visibility = Visibility.Visible;

                drawArea.Children.Clear();
                drawBackGround.Children.Clear();

                editShapeIndex = -1;
                isEditting = false;

                for (int i = 0; i < drawSurface.Count; i++)
                {
                    drawSurface[i].setEdit(false);
                    drawArea.Children.Add(drawSurface[i].convertShapeType());
                }

                toolHorizontalButton.Opacity = 0.3;
                toolVerticalButton.Opacity = 0.3;

                isEditting = false;
                editShapeIndex = -1;
            }
        }

        private void newLayerButtonClick(object sender, RoutedEventArgs e)
        {
            string name = "Layer " + (layerList.Count + 1).ToString();
            layerList.Add(new Layer() { layerName = name, drawSurface = new List<IShape>() });
            layerListView.ItemsSource = "";
            layerListView.ItemsSource = layerList;
            layerListView.SelectedIndex = currentLayerIndex;
        }

        private void layerListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (layerListView.SelectedIndex != -1)
            {
                layerInformTextBlock.Text = "Layer " + (layerListView.SelectedIndex + 1).ToString();

                if (layerListView.SelectedItems.Count == 1)
                {
                    currentLayerIndex = layerListView.SelectedIndex;

                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();
                    drawSurface.Clear();

                    Layer layer = (Layer)layerListView.SelectedItems[0];
                    List<IShape> subDrawSurface = layer.drawSurface;
                    drawSurface.AddRange(subDrawSurface);

                    editShapeIndex = -1;
                    isEditting = false;

                    foreach (var item in drawSurface)
                    {
                        item.setEdit(false);
                        drawArea.Children.Add(item.convertShapeType());
                    }
                } else
                {
                    drawArea.Children.Clear();
                    drawBackGround.Children.Clear();
                    drawSurface.Clear();

                    editShapeIndex = -1;
                    isEditting = false;

                    for (int i = 0; i < layerListView.SelectedItems.Count; i++)
                    {
                        Layer layer = (Layer)layerListView.SelectedItems[i];
                        List<IShape> subDrawSurface = layer.drawSurface;
                        drawSurface.AddRange(subDrawSurface);
                    }

                    foreach (var item in drawSurface)
                    {
                        item.setEdit(false);
                        drawArea.Children.Add(item.convertShapeType());
                    }
                }
            }
        }

        private void toolFileImportButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Chọn bản lưu game để bắt đầu tải game";
            ofd.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            ofd.ShowDialog();
            string filePath = ofd.FileName;
            string FileName = (filePath.Split("\\")[filePath.Split("\\").Length - 1]).Split(".")[0];

            if (!filePath.Equals(""))
            {
                if (File.Exists(filePath))
                {
                    string[] fileLines = File.ReadAllLines(filePath);

                    if (fileLines.Length == 0)
                    {
                        MessageBox.Show("Bản lưu trống. Vui lòng chọn bản lưu khác", "Bản lưu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (fileLines[0].Equals("The Paint Application - Võ Tấn Lộc (21127101) - Nguyễn Thái Đan Sâm (21127414)"))
                    {
                        layerList.Clear();
                        string name = "";
                        List<IShape> surface = new List<IShape>();

                        for (int i = 1; i < fileLines.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(fileLines[i]))
                            {
                                if (fileLines[i].Contains("Layer"))
                                {
                                    name = fileLines[i];
                                } else
                                {
                                    IShape shape = null;
                                    string[] data = fileLines[i].Split(";");

                                    for (int j = 0; j < allShapeList.Count; j++)
                                    {
                                        if (allShapeList[j].shapeName.Equals(data[0]))
                                        {
                                            shape = (IShape)allShapeList[j].Clone();
                                            break;
                                        }
                                    }
                                    shape.addStartPoint(new Point(double.Parse(data[1]), double.Parse(data[2])));
                                    shape.addEndPoint(new Point(double.Parse(data[3]), double.Parse(data[4])));

                                    for (int j = 0; j < widthnessList.Count; j++)
                                    {
                                        if (widthnessList[j].widthnessName.Equals(data[5]))
                                        {
                                            shape.addWidthness((IWidthness)widthnessList[j].Clone());
                                            break;
                                        }
                                    }

                                    for (int j = 0; j < strokeList.Count; j++)
                                    {
                                        if (strokeList[j].strokeName.Equals(data[6]))
                                        {
                                            shape.addStrokeStyle((IStroke)strokeList[j].Clone());
                                            break;
                                        }
                                    }

                                    for (int j = 0; j < colorList.Count; j++)
                                    {
                                        if (colorList[j].colorName.Equals(data[7]))
                                        {
                                            shape.addColor((IColor)colorList[j].Clone());
                                            break;
                                        }
                                    }

                                    surface.Add(shape);
                                }
                            } else
                            {
                                layerList.Add(new Layer{ layerName = name, drawSurface = surface});
                                name = "";
                                surface = new List<IShape>();
                            }
                        }

                        layerListView.ItemsSource = "";
                        layerListView.ItemsSource = layerList;
                        layerListView.SelectedIndex = 0;
                        currentLayerIndex = 0;

                        drawArea.Children.Clear();
                        drawBackGround.Children.Clear();
                        drawSurface.Clear();

                        Layer layer = (Layer)layerListView.SelectedItems[0];
                        List<IShape> subDrawSurface = layer.drawSurface;
                        drawSurface.AddRange(subDrawSurface);

                        editShapeIndex = -1;
                        isEditting = false;

                        foreach (var item in drawSurface)
                        {
                            item.setEdit(false);
                            drawArea.Children.Add(item.convertShapeType());
                        }

                        fileTitleName.Text = FileName;
                    }
                    else
                    {
                        MessageBox.Show("Bản lưu không đúng định dạng của chương trình. Vui lòng chọn bản lưu khác", "Bản lưu không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("File không tồn tại. Vui lòng kiểm tra lại", "File không tồn tại", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void toolFileExportButtonMouseEnter(object sender, MouseEventArgs e)
        {
            if (toolFileExportButton.Opacity != 0.3)
            {
                toolFileExportButton.Cursor = Cursors.Hand;
            } else
            {
                toolFileExportButton.Cursor = null;
            }
        }

        private void toolFileExportButtonMouseLeave(object sender, MouseEventArgs e)
        {
            toolFileExportButton.Cursor = null;
        }

        private void toolFileExportButtonClick(object sender, RoutedEventArgs e)
        {
            if (toolFileExportButton.Opacity != 0.3)
            {
                dialogBorder.Visibility = Visibility.Visible;
            }
        }

        private void closeDialogButtonClick(object sender, RoutedEventArgs e)
        {
            dialogBorder.Visibility = Visibility.Collapsed;
        }

        private void fileNameTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string FileName = fileNameTextBox.Text;

                if (!FileName.Equals(""))
                {
                    var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                    string folderPath = "";

                    if (dialog.ShowDialog(this).GetValueOrDefault())
                    {
                        folderPath = dialog.SelectedPath;
                    }

                    if (!folderPath.Equals(""))
                    {
                        folderPath = Path.Combine(folderPath, FileName + ".txt");
                        if (!File.Exists(folderPath))
                        {
                            var myfile = File.Create(folderPath);
                            myfile.Close();

                            StreamWriter streamWriter = new StreamWriter(folderPath);

                            streamWriter.WriteLine("The Paint Application - Võ Tấn Lộc (21127101) - Nguyễn Thái Đan Sâm (21127414)");

                            for (int i = 0; i < layerList.Count; i++)
                            {
                                streamWriter.WriteLine(layerList[i].layerName);
                                for (int j = 0; j < layerList[i].drawSurface.Count; j++)
                                {
                                    if (!layerList[i].drawSurface[j].shapeName.Equals("Text") && !layerList[i].drawSurface[j].shapeName.Equals("Free Line"))
                                    {
                                        string data = layerList[i].drawSurface[j].shapeName + ";" +
                                            layerList[i].drawSurface[j].getStartPoint().X + ";" + layerList[i].drawSurface[j].getStartPoint().Y + ";" +
                                            layerList[i].drawSurface[j].getEndPoint().X + ";" + layerList[i].drawSurface[j].getEndPoint().Y + ";" 
                                            + layerList[i].drawSurface[j].getWidthness().widthnessName + ";"
                                            + layerList[i].drawSurface[j].getStrokeStyle().strokeName + ";"
                                            + layerList[i].drawSurface[j].getColor().colorName;
                                        streamWriter.WriteLine(data);
                                    }
                                }
                                streamWriter.WriteLine();
                            }
                            streamWriter.Close();

                            MessageBoxResult result = MessageBox.Show("Lưu bản vẽ thành công", "Lưu thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                            if (result == MessageBoxResult.OK)
                            {
                                dialogBorder.Visibility = Visibility.Collapsed;
                                fileTitleName.Text = FileName;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Tên file đã tồn tại trên hệ thống. Vui lòng nhập lại", "Tên file đã tồn tại", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng nhập tên file để bắt đầu quá trình lưu game", "Tên file không hợp lệ", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private List<int> getShapeInsideRectangle(IShape selectionRectangle)
        {
            List<int> listIndices = new List<int>();

            double startSelectionX = selectionRectangle.getStartPoint().X;
            double startSelectionY = selectionRectangle.getStartPoint().Y;

            double endSelectionX = selectionRectangle.getEndPoint().X;
            double endSelectionY = selectionRectangle.getEndPoint().Y;

            for (int i  = 0; i < drawSurface.Count; i++)
            {
                double startPointX = drawSurface[i].getStartPoint().X;
                double startPointY = drawSurface[i].getStartPoint().Y;

                double endPointX = drawSurface[i].getEndPoint().X;
                double endPointY = drawSurface[i].getEndPoint().Y;

                bool insideStartPoint = false;
                bool insideEndPoint = false;

                if (startPointX > startSelectionX && startPointY > startSelectionY)
                {
                    insideStartPoint = true;
                }

                if (endPointX < endSelectionX && endPointY < endSelectionY)
                {
                    insideEndPoint = true;
                }

                if (insideStartPoint && insideEndPoint)
                {
                    listIndices.Add(i);
                }
            }

            return listIndices;
        }
    }
}