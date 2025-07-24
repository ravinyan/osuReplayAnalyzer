using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp1.Animations;
using WpfApp1.Skinning;
using Image = System.Windows.Controls.Image;
using Color = System.Drawing.Color;

namespace WpfApp1.Objects
{
    public static class HitCircle
    {
        private static string skinPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuFileParser\\WpfApp1\\Skins\\Komori - PeguLian II (PwV)";
        private static int index = 1;

        public static Grid CreateCircle(HitObject circle, double radius, int comboNumber)
        {
            Grid hitObject = new Grid();
            hitObject.DataContext = circle;
            hitObject.Width = radius;
            hitObject.Height = radius;

            Color comboColor = Color.FromArgb(220, 24, 214);

            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle@2x.png"), comboColor);
            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png")),
            };

            StackPanel numberPanel = new StackPanel();
            numberPanel.HorizontalAlignment = HorizontalAlignment.Center;
            numberPanel.Orientation = Orientation.Horizontal;

            if (comboNumber <= 9)
            {
                Image hitCircleNumber = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-{comboNumber}.png")),
                };

                numberPanel.Children.Add(hitCircleNumber);
            }
            else if (comboNumber <= 99)
            {
                char[] number = comboNumber.ToString().ToCharArray();

                Image hitCircleNumber = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-{number[0]}.png")),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-{number[1]}.png")),
                };

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
            }
            else if (comboNumber <= 999)
            {
                char[] number = comboNumber.ToString().ToCharArray();

                Image hitCircleNumber = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-{number[0]}.png")),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-{number[1]}.png")),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-{number[2]}.png")),
                };

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
                numberPanel.Children.Add(hitCircleNumber3);
            }
            else
            {
                Image hitCircleNumber = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-7.png")),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-2.png")),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-7.png")),
                };

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
                numberPanel.Children.Add(hitCircleNumber3);
            }

            Image approachCircle = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\approachcircle.png")),
                Name = "ApproachCircle",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            hitObject.Children.Add(hitCircle);
            hitObject.Children.Add(hitCircleBorder2);
            hitObject.Children.Add(numberPanel);
            hitObject.Children.Add(approachCircle);

            Canvas.SetLeft(hitObject, circle.X);
            Canvas.SetTop(hitObject, circle.Y);
            
            hitObject.Visibility = Visibility.Collapsed;

            hitObject.Name = $"HitObject{index}";
            index++;

            HitCircleAnimation.ApplyHitCircleAnimations(hitObject);

            return hitObject;
        }
    }
}
