using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace ReplayAnalyzer.HitObjects
{
    public class HitObject : Canvas, IDisposable
    {
        public static List<BitmapSource> HitCircleBitmapColours = new List<BitmapSource>();

        public double X { get; set; }
        public double Y { get; set; }
        public Vector2 BaseSpawnPosition { get; set; }
        public int SpawnTime { get; set; }
        public HitJudgement Judgement { get; set; } = new HitJudgement(HitObjectJudgement.None, 0);

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
                    Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ComboNumber, $"{comboNumber}"))),
                };

                numberPanel.Children.Add(hitCircleNumber);
            }
            else if (comboNumber <= 99)
            {
                char[] number = comboNumber.ToString().ToCharArray();

                Image hitCircleNumber = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ComboNumber, $"{number[0]}"))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ComboNumber, $"{number[1]}"))),
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
                    Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ComboNumber, $"{number[0]}"))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ComboNumber, $"{number[1]}"))),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ComboNumber, $"{number[2]}"))),
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
                    Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ComboNumber, $"{7}"))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = diameter / 2 * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ComboNumber, $"{2}"))),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = diameter / 2 * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ComboNumber, $"{7}"))),
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
            //Bitmap hitObject = new Bitmap(hitObjectBase, new System.Drawing.Size(256 / hitObjectBase.Width, 256 / hitObjectBase.Height));
            float opacity = GetHitCicleOpacity(hitObject);

            HitCircleBitmapColours.Clear();
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
                new float[] {colour.R /255, 0, 0, 0, 0},
                new float[] {0, colour.G / 255, 0, 0, 0},
                new float[] {0, 0, colour.B / 255, 0, 0},
                new float[] {0, 0, 0, colour.A, 0},
                new float[] { colour.R / 255f, colour.G / 255f, colour.B / 255f, 0, 1}
            });

            // https://www.csharphelper.com/howtos/howto_color_matrix.html
            // https://docs.rainmeter.net/tips/colormatrix-guide/
            // https://www.graficaobscura.com/matrix/index.html
            //ColorMatrix colorMatrix = new ColorMatrix(
            //new float[][]
            //{//              R  G  B  A  W (brightness)
            //    new float[] {colour.R / 255.0f, 0, 0, 0, 0},
            //    new float[] {0, colour.G / 255.0f, 0, 0, 0},
            //    new float[] {0, 0, colour.B / 255.0f, 0, 0},
            //    new float[] {0, 0, 0, 1, 0},
            //    new float[] {0, 0, 0, 0, 1}
            //});
            var s = 0;
            float a =  (float)((1.0 - s) * colour.R + s) / 255.0f ;
            float b =  (float)((1.0 - s) * colour.R    ) / 255.0f ;
            float c =  (float)((1.0 - s) * colour.R    ) / 255.0f ;
            float d =  (float)((1.0 - s) * colour.G    ) / 255.0f ;
            float e =  (float)((1.0 - s) * colour.G + s) / 255.0f ;
            float f =  (float)((1.0 - s) * colour.G    ) / 255.0f ;
            float u =  (float)((1.0 - s) * colour.B    ) / 255.0f ;
            float h =  (float)((1.0 - s) * colour.B    ) / 255.0f ;
            float i =  (float)((1.0 - s) * colour.B + s) / 255.0f ;

            float R = colour.R / 255.0f;
            float G = colour.G / 255.0f;
            float B = colour.B / 255.0f;
            float A = colour.A / 255.0f;
            //ColorMatrix colorMatrix = new ColorMatrix(
            //new float[][]
            //{//              R  G  B  A  W (brightness)
            //    new float[] {R, 0, 0, 0, 0},
            //    new float[] {0, G, 0, 0, 0},
            //    new float[] {0, 0, B, 0, 0},
            //    new float[] {0, 0, 0, A, 0},
            //    new float[] {0.0f, 0.0f, 0.0f, 1, 0}
            //});
            
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            //g.DrawImage(hitObject, new Rectangle(0, 0, hitObject.Width, hitObject.Height),
            //           0, 0, hitObject.Width, hitObject.Height, GraphicsUnit.Pixel, attributes);
            short baseSize = 256;
            g.DrawImage(hitObject, new Rectangle(0, 0, baseSize, baseSize),
                        0, 0, baseSize, baseSize, GraphicsUnit.Pixel, attributes);

            Bitmap bitmap = new Bitmap(hitObject);
            nint hBitmap = bitmap.GetHbitmap();

            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, nint.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            TransformedBitmap borderSource = new TransformedBitmap(bitmapSource, new ScaleTransform(256 / bitmapSource.Width, 256 / bitmapSource.Height));
            bitmapSource.Freeze();
            //borderSource.Freeze();
            

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

