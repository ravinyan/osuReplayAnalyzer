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
        private static List<ImageAttributes> Attributes = CreateAttributes();

        public static Image ApplyComboColourToHitObject(Bitmap hitObject, Color comboColour, double diameter)
        {
            float opacity = GetHitCicleOpacity(hitObject);
            Graphics g = Graphics.FromImage(hitObject);

            ColorMatrix colorMatrix = new ColorMatrix(
            new float[][]
            {//              R  G  B  A  W (brightness)
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, comboColour.A, 0},
                new float[] {comboColour.R / 255f, comboColour.G / 255f, comboColour.B / 255f, 0, 1}
            });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            
            g.DrawImage(hitObject, new Rectangle(0, 0, hitObject.Width, hitObject.Height),
                        0, 0, hitObject.Width, hitObject.Height, GraphicsUnit.Pixel, attributes);         
            g.Dispose();

            nint hBitmap = hitObject.GetHbitmap();
            BitmapSource recoloredImage = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, nint.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            hitObject.Dispose(); // begone

            Image recoloredHitObject = new Image();
            recoloredHitObject.Source = recoloredImage;
            recoloredHitObject.Opacity = opacity;
            recoloredHitObject.Width = diameter;
            recoloredHitObject.Height = diameter;

            return recoloredHitObject;
        }

        private static List<ImageAttributes> CreateAttributes()
        {
            List<ImageAttributes> attributes = new List<ImageAttributes>();
            var colours = SkinIniProperties.GetComboColours();

            foreach (var colour in colours)
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {//              R  G  B  A  W (brightness)
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, colour.A, 0},
                    new float[] { colour.R / 255f, colour.G / 255f, colour.B / 255f, 0, 1}
                });

                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                attributes.Add(imageAttributes);
            }

            return attributes;
        }

        private static float GetHitCicleOpacity(Bitmap hitObject)
        {
            Color alpha = hitObject.GetPixel(hitObject.Width / 2, hitObject.Height / 2);

            return alpha.A / 255f;
        }
    }
}
