using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp1.Skins;

namespace WpfApp1.Analyser.UIElements
{
    public class HitJudgment
    {
        public static Image Image300()
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(SkinElement.Hit300()));

            return image;
        }

        public static Image Image100()
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(SkinElement.Hit100()));

            return image;
        }

        public static Image Image50()
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(SkinElement.Hit50()));

            return image;
        }

        public static Image ImageMiss()
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(SkinElement.HitMiss()));

            return image;
        }
    }
}
