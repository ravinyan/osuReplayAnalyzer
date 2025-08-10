using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;

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

            if (Playfield.AliveHitObjectCount() != 0)
            {
                AdjustCanvasHitObjectsPlacementAndSize(diameter, playfieldCanva, Playfield.GetAliveHitObjects());
            }
        }

        private static void AdjustCanvasHitObjectsPlacementAndSize(double diameter, Canvas playfieldCanva, List<Canvas> aliveObjects)
        {
            // use tranformations maybe? coz it would resize everything anyway just need to know how to math it
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);
 
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                Canvas hitObject = aliveObjects[i];

                HitObject baseHitObject = (HitObject)aliveObjects[i].DataContext;
                int baseHitObjectX = baseHitObject.X;
                int baseHitObjectY = baseHitObject.Y;

                hitObject.Width = diameter;
                hitObject.Height = diameter;

                if (baseHitObject is Circle)
                {
                    

                    for (int j = 0; j < VisualTreeHelper.GetChildrenCount(hitObject); j++)
                    {
                        Image c = VisualTreeHelper.GetChild(hitObject, j) as Image;
                        if (c == null)
                        {
                            // ok this is messy i guess but oh well
                            Grid g = VisualTreeHelper.GetChild(hitObject, j) as Grid;
                            g.Width = diameter;
                            g.Height = diameter;

                            StackPanel sp = VisualTreeHelper.GetChild(g, 0) as StackPanel;
                            foreach (Image spChild in sp.Children)
                            {
                                spChild.Height = (((diameter) / 2) * 0.7);
                            }
                        }
                        else
                        {
                            if (c.Name == "ApproachCircle")
                            {
                                c.Width = hitObject.Width;
                                c.Height = hitObject.Height;
                            }
                            else
                            {
                                c.Width = diameter;
                                c.Height = diameter;
                            }
                        }
                    }
                }
                else if (baseHitObject is Slider)
                {
                    

                    //for (int j = 0; j < VisualTreeHelper.GetChildrenCount(hitObject); j++)
                    //{
                    //    switch (j)
                    //    {
                    //        case 0: // head
                    //            Canvas sliderHead = VisualTreeHelper.GetChild(hitObject, j) as Canvas;

                    //            sliderHead.Width = diameter;
                    //            sliderHead.Height = diameter;

                    //            for (int k = 0; k < VisualTreeHelper.GetChildrenCount(sliderHead); k++)
                    //            {
                    //                Image c = VisualTreeHelper.GetChild(sliderHead, k) as Image;
                    //                if (c == null)
                    //                {
                    //                    // ok this is messy i guess but oh well
                    //                    Grid g = VisualTreeHelper.GetChild(sliderHead, k) as Grid;
                    //                    g.Width = diameter;
                    //                    g.Height = diameter;

                    //                    StackPanel sp = VisualTreeHelper.GetChild(g, 0) as StackPanel;
                    //                    foreach (Image spChild in sp.Children)
                    //                    {
                    //                        spChild.Height = (((diameter) / 2) * 0.7);
                    //                    }
                    //                }
                    //                else
                    //                {
                    //                    if (c.Name == "ApproachCircle")
                    //                    {
                    //                        c.Width = sliderHead.Width;
                    //                        c.Height = sliderHead.Height;
                    //                    }
                    //                    else
                    //                    {
                    //                        c.Width = diameter;
                    //                        c.Height = diameter;
                    //                    }
                    //                }
                    //            }
                    //    break;
                    //        case 1: // body
                    //            Canvas sliderBody = VisualTreeHelper.GetChild(hitObject, j) as Canvas;

                    //            for (int k = 0; k < VisualTreeHelper.GetChildrenCount(sliderBody); k++)
                    //            {

                    //            }
                    //            break;
                    //        case 2: // tail
                    //            Canvas sliderTail = VisualTreeHelper.GetChild(hitObject, j) as Canvas;

                    //            sliderTail.Width = diameter;
                    //            sliderTail.Height = diameter;

                    //            for (int k = 0; k < VisualTreeHelper.GetChildrenCount(sliderTail); k++)
                    //            {
                    //                Image img = VisualTreeHelper.GetChild(sliderTail, k) as Image;
                    //                img.Width = diameter;
                    //                img.Height = diameter;
                    //            }
                    //            break;
                    //    }
                    //}
                }
                else if (baseHitObject is Spinner)
                {

                }

                Canvas.SetTop(hitObject, (baseHitObjectY * playfieldScale) - (hitObject.Width / 2));
                Canvas.SetLeft(hitObject, (baseHitObjectX * playfieldScale) - (hitObject.Height / 2));
            }
        }
    }
}
