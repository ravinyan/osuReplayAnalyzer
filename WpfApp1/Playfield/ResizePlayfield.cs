using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#nullable disable

namespace WpfApp1.Playfield
{
    public static class ResizePlayfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void ResizePlayfieldCanva(Canvas playfieldCanva, Border playfieldBorder)
        {
            const double AspectRatio = 1.33;
            double height = (Window.Height / AspectRatio);
            double width = (Window.Width / AspectRatio);

            double osuScale = Math.Min(height / (384), width / 512);

            double diameter = ((54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * osuScale) * 2;

            playfieldCanva.Width = 512 * osuScale;
            playfieldCanva.Height = 384 * osuScale;

            playfieldBorder.Width = (512 * osuScale);
            playfieldBorder.Height = (384 * osuScale);

            if (Playfield.AliveHitObjectCount() != 0)
            {
                AdjustCanvasHitObjectsPlacementAndSize(diameter, playfieldCanva, Playfield.GetAliveHitObjects());
            }
        }

        private static void AdjustCanvasHitObjectsPlacementAndSize(double diameter, Canvas playfieldCanva, List<Canvas> aliveObjects)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);

            for (int i = 0; i < MainWindow.map.HitObjects.Count; i++)
            {
                Canvas hitObject = (Canvas)playfieldCanva.Children[i];

                hitObject.LayoutTransform = new ScaleTransform(playfieldScale, playfieldScale);
                hitObject.RenderTransform = new TranslateTransform(playfieldScale, playfieldScale);
            }
        }
    }
}
