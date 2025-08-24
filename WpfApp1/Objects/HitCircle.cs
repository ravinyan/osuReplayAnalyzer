using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp1.Animations;
using WpfApp1.Skinning;
using WpfApp1.Skins;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace WpfApp1.Objects
{
    public class HitCircle
    {
        public static Canvas CreateCircle(HitObject circle, double radius, int currentComboNumber, double osuScale, int index, Color comboColour)
        {
            Canvas hitObject = new Canvas();
            hitObject.DataContext = circle;
            hitObject.Width = radius;
            hitObject.Height = radius;

            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap(SkinElement.HitCircle()), comboColour, radius);

            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri(SkinElement.HitCircleOverlay())),
            };
            
            Grid comboNumber = AddComboNumber(currentComboNumber, radius);

            //Image approachCircle = SkinHitCircle.ApplyComboColourToApproachCircle(new Bitmap($"{skinPath}\\approachcircle.png"), comboColor , radius);
            Image approachCircle = new Image()
            {
                Height = radius,
                Width = radius,
                Source = new BitmapImage(new Uri(SkinElement.ApproachCircle())),
                RenderTransform = new ScaleTransform(),
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

            HitObjectAnimations.ApplyHitCircleAnimations(hitObject);

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
                    Height = (radius / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(comboNumber))),
                };
            
                numberPanel.Children.Add(hitCircleNumber);
            }
            else if (comboNumber <= 99)
            {
                char[] number = comboNumber.ToString().ToCharArray();
            
                Image hitCircleNumber = new Image()
                {
                    Height = (radius / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[0]))),
                };
            
                Image hitCircleNumber2 = new Image()
                {
                    Height = (radius / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[1]))),
                };
            
                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
            }
            else if (comboNumber <= 999)
            {
                char[] number = comboNumber.ToString().ToCharArray();
            
                Image hitCircleNumber = new Image()
                {
                    Height = (radius / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[0]))),
                };
            
                Image hitCircleNumber2 = new Image()
                {
                    Height = (radius / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[1]))),
                };
            
                Image hitCircleNumber3 = new Image()
                {
                    Height = (radius / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[2]))),
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
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(7))),
                };
            
                Image hitCircleNumber2 = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(2))),
                };
            
                Image hitCircleNumber3 = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(7))),
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
