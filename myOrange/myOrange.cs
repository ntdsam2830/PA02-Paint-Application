﻿using myColor;
using System.Windows.Media;

namespace myOrange
{
    public class myOrange : IColor
    {
        public string colorName => "Orange";
        public SolidColorBrush colorValue => new SolidColorBrush(Color.FromRgb(255, 165, 0));
        public void addColorRGB(byte r, byte g, byte b) { }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
