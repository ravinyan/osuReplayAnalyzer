using ReplayAnalyzer.Skins;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace ReplayAnalyzer.Skinning
{
    public static class SkinHitCircle
    {
        public static List<Image> ColouredHitCIrcles = new List<Image>();

        private static List<BitmapSource> IHATEWPF = new List<BitmapSource>();

        public static Image ApplyComboColourToHitObject(Bitmap hitObject, int comboColourIndex, double diameter)
        {
            float opacity = GetHitCicleOpacity(hitObject);

            if (IHATEWPF.Count == 0)
            {
                List<Color> colours = SkinIniProperties.GetComboColours();
                foreach (Color colour in colours)
                {
                    IHATEWPF.Add(CreateBitmapSource(hitObject, colour));
                }
               
                hitObject.Dispose(); // begone
            }
           
            Image recoloredHitObject = new Image();
            recoloredHitObject.Source = IHATEWPF[comboColourIndex];
            recoloredHitObject.Opacity = opacity;
            recoloredHitObject.Width = diameter;
            recoloredHitObject.Height = diameter;

            return recoloredHitObject;
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
    }
}
