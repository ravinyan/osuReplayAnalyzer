using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#nullable disable

namespace WpfApp1.Playfield
{
    public static class ResizePlayfield
    {
        public static void ResizePlayfieldCanva(SizeChangedEventArgs e, Canvas playfieldCanva, Border playfieldBorder)
        {
            const double AspectRatio = 1.33;
            double height = (e.NewSize.Height / AspectRatio);
            double width = (e.NewSize.Width / AspectRatio);

            double osuScale = Math.Min(height / 384, width / 512);
            double radius = ((54.4) - (4.48) * 4) * osuScale;

            playfieldCanva.Width = 512 * osuScale;
            playfieldCanva.Height = 384 * osuScale;

            playfieldBorder.Width = (512 * osuScale) + radius;
            playfieldBorder.Height = (384 * osuScale) + radius;

            AdjustCanvasHitObjectsPlacementAndSize(radius, playfieldCanva);
        }

        private static void AdjustCanvasHitObjectsPlacementAndSize(double radius, Canvas playfieldCanva)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);

            for (int i = 0; i < playfieldCanva.Children.Count; i++)
            {
                // need FrameworkElement for widht and height values coz UiElement doesnt have it
                FrameworkElement circle = (FrameworkElement)playfieldCanva.Children[i];
                // https://osu.ppy.sh/wiki/en/Client/Playfield
                HitObject hitObject = MainWindow.map.HitObjects[i];
                int baseHitObjectX = hitObject.X;
                int baseHitObjectY = hitObject.Y;

                //int baseHitObjectX;
                //int baseHitObjectY;
                //if (i == 0)
                //{
                //    baseHitObjectX = 300;
                //    baseHitObjectY = 200;
                //}
                //else if (i == 1)
                //{
                //    baseHitObjectX = 0;
                //    baseHitObjectY = 0;
                //}
                //else if (i == 2)
                //{
                //    baseHitObjectX = 0;
                //    baseHitObjectY = 384;
                //}
                //else
                //{
                //    baseHitObjectX = 512;
                //    baseHitObjectY = 0;
                //}

                int childrenCount = VisualTreeHelper.GetChildrenCount(circle);
                for (int j = 0; j < childrenCount; j++)
                {
                    Image c = VisualTreeHelper.GetChild(circle, j) as Image;

                    if (c == null)
                    {
                        StackPanel s = VisualTreeHelper.GetChild(circle, j) as StackPanel;
                        foreach (Image children in s.Children)
                        {
                            children.Height = ((radius / 2) * 0.7);
                        }
                    }
                    else
                    {
                        c.Width = radius;
                        c.Height = radius;
                    }
                }

                circle.Width = radius;
                circle.Height = radius;

                Canvas.SetTop(circle, (baseHitObjectY * playfieldScale) - (radius / 2));
                Canvas.SetLeft(circle, (baseHitObjectX * playfieldScale) - (radius / 2));
            }
        }
    }
}
