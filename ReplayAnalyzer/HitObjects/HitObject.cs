using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace ReplayAnalyzer.HitObjects
{
    public class HitObject : Canvas, IDisposable
    {
        private static List<BitmapSource> HitCircleBitmapColours = new List<BitmapSource>();

        public double X { get; set; }
        public double Y { get; set; }
        public Vector2 BaseSpawnPosition { get; set; }
        public int SpawnTime { get; set; }
        public int StackHeight { get; set; }
        public HitJudgement? Judgement { get; set; } = new HitJudgement(HitObjectJudgement.None, 0);

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

        public static Image ApplyComboColourToHitCircle(Bitmap hitObject, int comboColourIndex, double diameter)
        {
            float opacity = GetHitCicleOpacity(hitObject);

            if (HitCircleBitmapColours.Count == 0)
            {
                List<Color> colours = SkinIniProperties.GetComboColours();
                foreach (Color colour in colours)
                {
                    HitCircleBitmapColours.Add(CreateBitmapSource(hitObject, colour));
                }

                HitCircleBitmapColours.Add(CreateBitmapSource(hitObject, Color.FromArgb(255, 0, 0)));

                hitObject.Dispose(); // begone
            }

            Image recoloredHitObject = new Image();
            recoloredHitObject.Source = HitCircleBitmapColours[comboColourIndex];
            recoloredHitObject.Opacity = opacity;
            recoloredHitObject.Width = diameter;
            recoloredHitObject.Height = diameter;

            return recoloredHitObject;
        }

        public static void SetColour(Image image, int index)
        {
            image.Source = HitCircleBitmapColours[index];
        }

        private static BitmapSource CreateBitmapSource(Bitmap hitObject, Color colour)
        {
            Graphics g = Graphics.FromImage(hitObject);

            ColorMatrix colorMatrix = new ColorMatrix(
            new float[][]
            {//              R  G  B  A  W (brightness)
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, colour.A, 0},
                new float[] { colour.R / 255f, colour.G / 255f, colour.B / 255f, 0, 1}
            });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            g.DrawImage(hitObject, new Rectangle(0, 0, hitObject.Width, hitObject.Height),
                        0, 0, hitObject.Width, hitObject.Height, GraphicsUnit.Pixel, attributes);

            Bitmap bitmap = new Bitmap(hitObject);
            nint hBitmap = bitmap.GetHbitmap();

            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, nint.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmapSource.Freeze();

            g.Dispose();

            return bitmapSource;
        }

        private static float GetHitCicleOpacity(Bitmap hitObject)
        {
            Color alpha = hitObject.GetPixel(hitObject.Width / 2, hitObject.Height / 2);

            return alpha.A / 255f;
        }

        // am i using even this disposable right? i have no clue
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(disposing: true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                Dispatcher.Invoke(() =>
                {
                    for (int i = this.Children.Count - 1; i >= 0; i--)
                    {
                        if (this.Children[i] is Image)
                        {
                            var a = this.Children[i] as Image;
                            a.Source = null;
                            a.UpdateLayout();
                            this.Children[i] = a;
                        }

                        this.Children.Remove(Children[i]);
                    }
                    this.Children.Capacity = 0;
                });

                // Note disposing has been done.
                disposed = true;
            }
        }

        ~HitObject()
        {
            //Dispose(disposing: false);
        }
    }
}

