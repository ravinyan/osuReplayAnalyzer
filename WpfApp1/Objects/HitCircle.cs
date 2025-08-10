using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp1.Animations;
using WpfApp1.Skinning;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace WpfApp1.Objects
{
    public class HitCircle
    {
        private static string skinPath = FilePath.GetSkinPath();

        public static Canvas CreateCircle(HitObject circle, double radius, int currentComboNumber, double osuScale, int index)
        {
            Canvas hitObject = new Canvas();
            hitObject.DataContext = circle;
            hitObject.Width = radius;
            hitObject.Height = radius;

            Color comboColor = Color.FromArgb(220, 24, 214);

            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle.png"), comboColor, radius);
            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay.png")),
            };

            Grid comboNumber = AddComboNumber(currentComboNumber, radius);

            Image approachCircle = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\approachcircle.png")),
                Name = "ApproachCircle",
            };

            hitObject.Children.Add(hitCircle);
            hitObject.Children.Add(hitCircleBorder2);
            hitObject.Children.Add(comboNumber);
            hitObject.Children.Add(approachCircle);

            Canvas.SetLeft(hitObject, (circle.X * osuScale) - (radius / 2));
            Canvas.SetTop(hitObject, (circle.Y * osuScale) - (radius / 2));

            // circles 1 2 3 were rendered so 3 was on top...
            // (0 - index) gives negative value so that 1 will be rendered on top
            // basically correct zindexing like it should be for every object
            Canvas.SetZIndex(hitObject, 0 - index);
            
            hitObject.Name = $"CircleHitObject{index}";

            hitObject.Visibility = Visibility.Collapsed;

            HitCircleAnimation.ApplyHitCircleAnimations(hitObject);

            return hitObject;
        }

        public static Grid AddComboNumber(int comboNumber, double radius)
        {
            Grid grid = new Grid();
            grid.Width = radius;
            grid.Height = radius;

            StackPanel numberPanel = new StackPanel();
            numberPanel.Orientation = Orientation.Horizontal;
            numberPanel.HorizontalAlignment = HorizontalAlignment.Center;

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

            grid.Children.Add(numberPanel);

            return grid;
        }
    }
}
