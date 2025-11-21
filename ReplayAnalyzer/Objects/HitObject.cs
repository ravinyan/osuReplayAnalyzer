using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;
using System.Windows;
using ReplayAnalyzer.Skins;
using ReplayAnalyzer.PlayfieldGameplay;
using static ReplayAnalyzer.PlayfieldGameplay.HitJudgementManager;

namespace ReplayAnalyzer.Objects
{
    // i know its not really needed since there is DataContext in canvas classes, but i want to practice inheritance
    // and learn how to nicely use it and implement it instead of doing it messy... and maybe it will make code look nicer too
    public class HitObject : Canvas
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public int SpawnTime { get; set; }
        public int StackHeight { get; set; }
        public double HitAt { get; set; }
        public bool IsHit { get; set; }
        public HitObjectJudgement Judgement { get; set; } = HitObjectJudgement.None;

        public static Grid AddComboNumber(int comboNumber, double diameter)
        {
            Grid grid = new Grid();
            grid.Width = diameter;
            grid.Height = diameter;

            StackPanel numberPanel = new StackPanel();
            numberPanel.Orientation = Orientation.Horizontal;
            numberPanel.HorizontalAlignment = HorizontalAlignment.Center;

            if (comboNumber <= 9)
            {
                Image hitCircleNumber = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(comboNumber))),
                };

                numberPanel.Children.Add(hitCircleNumber);
            }
            else if (comboNumber <= 99)
            {
                char[] number = comboNumber.ToString().ToCharArray();

                Image hitCircleNumber = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[0]))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = diameter / 2 * 0.8,
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
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[0]))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[1]))),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = diameter / 2 * 0.8,
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
                    Height = diameter / 2 * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(7))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = diameter / 2 * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(2))),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = diameter / 2 * 0.7,
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
