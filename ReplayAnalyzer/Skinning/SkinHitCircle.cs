using ReplayAnalyzer.Objects;
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

        private static BitmapSource IHATEWPF = null;

        public static Image ApplyComboColourToHitObject(Bitmap hitObject, Color comboColour, double diameter)
        {
            //CreateAttributes(hitObject, diameter);

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
            

            if (IHATEWPF == null)
            {
                Bitmap bitmap = new Bitmap(hitObject);
                nint hBitmap = bitmap.GetHbitmap();
                IHATEWPF = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, nint.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                IHATEWPF.Freeze();
            }
            g.Dispose();

            hitObject.Dispose(); // begone
            
            Image recoloredHitObject = new Image();
            recoloredHitObject.Source = IHATEWPF;
            recoloredHitObject.Opacity = opacity;
            recoloredHitObject.Width = diameter;
            recoloredHitObject.Height = diameter;

            return recoloredHitObject;
        }

        private static void CreateAttributes(Bitmap hitObject, double diameter)
        {
            if (ColouredHitCIrcles.Count != 0)
            {
                return;
            }

            List<Color> colours = SkinIniProperties.GetComboColours();
            foreach (Color colour in colours)
            {
                float opacity = GetHitCicleOpacity(hitObject);
                Graphics g = Graphics.FromImage(hitObject);

                ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {//              R  G  B  A  W (brightness)
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, colour.A, 0},
                new float[] {colour.R / 255f, colour.G / 255f, colour.B / 255f, 0, 1}
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

                ColouredHitCIrcles.Add(recoloredHitObject);
            }
        }

        private static float GetHitCicleOpacity(Bitmap hitObject)
        {
            Color alpha = hitObject.GetPixel(hitObject.Width / 2, hitObject.Height / 2);

            return alpha.A / 255f;
        }
    }
}
