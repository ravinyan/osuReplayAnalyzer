using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp1.Animations;
using WpfApp1.Skinning;
using static System.Net.Mime.MediaTypeNames;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace WpfApp1.Objects
{
    public class HitCircle
    {
        private static string skinPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuFileParser\\WpfApp1\\Skins\\Komori - PeguLian II (PwV)";

        public static Canvas CreateCircle(HitObject circle, double radius, int currentComboNumber, double osuScale, int index)
        {
            Canvas hitObject = new Canvas();
            hitObject.DataContext = circle;
            hitObject.Width = radius;
            hitObject.Height = radius;

            Color comboColor = Color.FromArgb(220, 24, 214);





            //BitmapFrame hc = BitmapDecoder.Create(new Uri($"{skinPath}\\hitcircle.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.First();
            //BitmapFrame hco = BitmapDecoder.Create(new Uri($"{skinPath}\\hitcircleoverlay.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.First();
            //
            //var imageStream = BitmapDecoder.Create(new Uri($"{skinPath}\\hitcircle.png"), BitmapCreateOptions.None, BitmapCacheOption.OnLoad).Frames.First();
            //
            //// its square so just width
            //double imgWidth = radius;
            //
            //
            //
            //DrawingVisual drawingVisual = new DrawingVisual();
            //using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            //{
            //    drawingContext.DrawImage(hc, new Rect(0, 0, imgWidth, imgWidth));
            //    drawingContext.DrawImage(hco, new Rect(0, 0, imgWidth, imgWidth));
            //}
            //
            //RenderTargetBitmap bmp = new RenderTargetBitmap((int)imgWidth, (int)imgWidth, 96, 96, PixelFormats.Pbgra32);
            //bmp.Render(drawingVisual);
            //
            //MemoryStream stream = new MemoryStream();
            //PngBitmapEncoder encoder = new PngBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(bmp));
            //encoder.Save(stream);
            //
            //Image hitCircle = new Image()
            //{
            //    Width = imgWidth,
            //    Height = imgWidth,
            //    Source = bmp,
            //};
            //
            //

            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle.png"), comboColor, radius);
            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay.png")),
            };

            //Image hitCircleBitmap = MergeHitCircleImages($"{skinPath}\\hitcircle.png",
            //                                                    new Bitmap($"{skinPath}\\hitcircleoverlay.png"),
            //                                                    comboColor, radius, hitObject);

           //Image image = new Image()
           //{
           //    Width = radius,
           //    Height = radius,
           //    Source = hitCircleBitmap,
           //};


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
            // this is very hungry and eating very big memory (memory leak)? idk potentially
            HitCircleAnimation.ApplyHitCircleAnimations(hitObject);

            return hitObject;
        }

        public static Image MergeHitCircleImages(string bitmap1, Bitmap bitmap2, Color comboColour, double radius, Canvas hitObject)
        {
            Bitmap b2 = new Bitmap(bitmap2, new System.Drawing.Size((int)radius, (int)radius));



            Bitmap b1 = new Bitmap(bitmap1);

            Image image = new Image();
            SkinHitCircle.ApplyComboColourToHitObject(b1, comboColour, radius);


            BitmapImage bi = new BitmapImage(new Uri(bitmap1));
            image.Source = bi;

     

            BmpBitmapEncoder bbe = new BmpBitmapEncoder();
            bbe.Frames.Add(BitmapFrame.Create(new Uri(image.Source.ToString(), UriKind.RelativeOrAbsolute)));

            MemoryStream ms = new MemoryStream();
            bbe.Save(ms);
            System.Drawing.Image newImage = System.Drawing.Image.FromStream(ms);

           



            Bitmap mainBitmap = new Bitmap((int)radius, (int)radius);

            using (Graphics g = Graphics.FromImage(mainBitmap))
            {
                g.DrawImage(newImage, System.Drawing.Point.Empty);
                g.DrawImage(b2, System.Drawing.Point.Empty);
            }

            IntPtr hBitmap = mainBitmap.GetHbitmap();
            BitmapSource result = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            
            Image recoloredHitObject = new Image();
            recoloredHitObject.Source = result;
            recoloredHitObject.Width = radius;
            recoloredHitObject.Height = radius;

            return recoloredHitObject;
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
