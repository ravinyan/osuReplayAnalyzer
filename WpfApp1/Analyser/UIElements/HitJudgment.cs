using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp1.Skins;

namespace WpfApp1.Analyser.UIElements
{
    public class HitJudgment : Image
    {
        public long SpawnTime { get; set; }
        public long EndTime { get; set; }

        public HitJudgment(string skinUri, double width, double height)
        {
            Source = new BitmapImage(new Uri(skinUri));
            Width = width; 
            Height = height;
        }
    }
}
