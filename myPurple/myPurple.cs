﻿using myColor;
using System.Windows.Media;

namespace myPurple
{
    public class myPurple : IColor
    {
        public string colorName => "Purple";
        public SolidColorBrush colorValue => new SolidColorBrush(Color.FromRgb(128, 0, 128));
        public void addColorRGB(byte r, byte g, byte b) { }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
