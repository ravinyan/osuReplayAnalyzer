using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#nullable disable

namespace WpfApp1.Playfield
{
    public static class ResizePlayfield
    {
        public static void ResizePlayfieldCanva(SizeChangedEventArgs e, Canvas playfieldCanva, Border playfieldBorder, List<FrameworkElement> aliveObjects)
        {
            const double AspectRatio = 1.33;
            double height = (e.NewSize.Height / AspectRatio);
            double width = (e.NewSize.Width / AspectRatio);

            // +35 is to offset overlapping with music player and playfield grid... scuffed but works
            double osuScale = Math.Min(height / (384 + 35), width / 512);
            double diameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * osuScale * 2;

            playfieldCanva.Width = 512 * osuScale;
            playfieldCanva.Height = 384 * osuScale;

            playfieldBorder.Width = (512 * osuScale) + diameter;
            playfieldBorder.Height = (384 * osuScale) + diameter;

            AdjustCanvasHitObjectsPlacementAndSize(diameter, playfieldCanva, aliveObjects);
        }

        private static void AdjustCanvasHitObjectsPlacementAndSize(double diameter, Canvas playfieldCanva, List<FrameworkElement> aliveObjects)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);

            // change to visible canvas objects after some reworking with code 
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                FrameworkElement circle = aliveObjects[i];

                HitObject objectBasePosition = (HitObject)aliveObjects[i].DataContext;
                int baseHitObjectX = objectBasePosition.X;
                int baseHitObjectY = objectBasePosition.Y;

                circle.Width = diameter;
                circle.Height = diameter;

                for (int j = 0; j < VisualTreeHelper.GetChildrenCount(circle); j++)
                {
                    Image c = VisualTreeHelper.GetChild(circle, j) as Image;
                    if (c == null)
                    {
                        // stack panel children (numbers on circles)
                        StackPanel s = VisualTreeHelper.GetChild(circle, j) as StackPanel;
                        foreach (Image sChild in s.Children)
                        {
                            sChild.Height = (((diameter) / 2) * 0.7); 
                        }
                    }
                    else
                    {
                        if (c.Name == "ApproachCircle")
                        {
                            c.Width = circle.Width;
                            c.Height = circle.Height;
                        }
                        else
                        {
                            c.Width = diameter;
                            c.Height = diameter;
                        } 
                    }
                }

                Canvas.SetTop(circle, (baseHitObjectY * playfieldScale) - (circle.Width / 2));
                Canvas.SetLeft(circle, (baseHitObjectX * playfieldScale) - (circle.Height / 2));
            }
        }
    }
}
