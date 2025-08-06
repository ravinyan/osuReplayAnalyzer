using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace WpfApp1.Skinning
{
    public static class SkinHitCircle
    {
        public static Image ApplyComboColourToHitObject(Bitmap hitObject, Color comboColor, double radius)
        {
            float opacity = GetHitCicleOpacity(hitObject);
            Graphics g = Graphics.FromImage(hitObject);

            ColorMatrix colorMatrix = new ColorMatrix(
            new float[][]
            {//              R  G  B  A  W (brightness)
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, comboColor.A, 0},
                new float[] {comboColor.R / 255f, comboColor.G / 255f, comboColor.B / 255f, 0, 1}
            });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            
            g.DrawImage(hitObject, new Rectangle(0, 0, hitObject.Width, hitObject.Height),
                        0, 0, hitObject.Width, hitObject.Height, GraphicsUnit.Pixel, attributes);
         
            g.Dispose();

            IntPtr hBitmap = hitObject.GetHbitmap();
            BitmapSource recoloredImage = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Image recoloredHitObject = new Image();
            recoloredHitObject.Source = recoloredImage;
            recoloredHitObject.Opacity = opacity;
            recoloredHitObject.Width = radius;
            recoloredHitObject.Height = radius;

            return recoloredHitObject;
        }

        private static float GetHitCicleOpacity(Bitmap hitObject)
        {
            Color alpha = hitObject.GetPixel(hitObject.Width / 2, hitObject.Height / 2);

            return alpha.A / 255f;
        }
    }
}
