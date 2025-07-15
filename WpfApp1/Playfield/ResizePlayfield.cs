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

            // +35 is to offset overlapping with music player and playfield grid... scuffed but works
            double osuScale = Math.Min(height / (384 + 35), width / 512);
            double diameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * osuScale * 2;

            playfieldCanva.Width = 512 * osuScale;
            playfieldCanva.Height = 384 * osuScale;

            playfieldBorder.Width = (512 * osuScale) + diameter;
            playfieldBorder.Height = (384 * osuScale) + diameter;

            AdjustCanvasHitObjectsPlacementAndSize(diameter, playfieldCanva);
        }

        private static void AdjustCanvasHitObjectsPlacementAndSize(double diameter, Canvas playfieldCanva)
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
                //    baseHitObjectX = 310;
                //    baseHitObjectY = 210;
                //}
                //else if (i == 2)
                //{
                //    baseHitObjectX = 320;
                //    baseHitObjectY = 220;
                //}
                //else
                //{
                //    baseHitObjectX = 512;
                //    baseHitObjectY = 0;
                //}
                
                for (int j = 0; j < VisualTreeHelper.GetChildrenCount(circle); j++)
                {
                    Image c = VisualTreeHelper.GetChild(circle, j) as Image;
                    if (c == null)
                    {
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
                            //scuffed
                            c.Width = diameter * 5;
                            c.Height = diameter * 5;
                        }
                        else
                        {
                            c.Width = diameter;
                            c.Height = diameter;
                        }
                            
                    }
                }

                //scuffed
                circle.Width = diameter * 5;
                circle.Height = diameter * 5;

                Canvas.SetTop(circle, (baseHitObjectY * playfieldScale) - (diameter / 2));
                Canvas.SetLeft(circle, (baseHitObjectX * playfieldScale) - (diameter / 2));
            }
        }
    }
}
