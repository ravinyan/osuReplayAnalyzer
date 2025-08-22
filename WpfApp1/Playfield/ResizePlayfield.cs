using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
using Circle = ReplayParsers.Classes.Beatmap.osu.Objects.Circle;
using WpfApp1.Beatmaps;
#nullable disable

namespace WpfApp1.Playfield
{
    public static class ResizePlayfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void ResizePlayfieldCanva(Canvas playfieldCanva, Border playfieldBorder)
        {
            const double AspectRatio = 1.33;
            double height = ((Window.ActualHeight - Window.musicControlUI.ActualHeight) / AspectRatio);
            double width = (Window.ActualWidth / AspectRatio);

            double osuScale = Math.Min(height / (384), width / 512);

            double diameter = ((54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * osuScale) * 2;

            playfieldCanva.Width = (512 * osuScale);
            playfieldCanva.Height = (384 * osuScale);

            playfieldBorder.Width = (512 * osuScale) + 7 + diameter;
            playfieldBorder.Height = (384 * osuScale) + 7 + diameter;

            AdjustCanvasHitObjectsPlacementAndSize(diameter, playfieldCanva);
        }

        private static void AdjustCanvasHitObjectsPlacementAndSize(double diameter, Canvas playfieldCanva)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);

            for (int i = 1; i < OsuBeatmap.HitObjectDict2.Count; i++)
            {
                Canvas hitObject = (Canvas)OsuBeatmap.HitObjectDict2[i];
                HitObject hitObjectData = (HitObject)hitObject.DataContext;

                hitObject.LayoutTransform = new ScaleTransform(playfieldScale, playfieldScale);

                // i dont understand why render transform doesnt work on circles but works on sliders...
                // and im too scared to understand... at least it works
                if (hitObjectData is Slider)
                {
                    hitObject.RenderTransform = new TranslateTransform(playfieldScale, playfieldScale);
                }
                else if (hitObjectData is Circle)
                {
                    Canvas.SetTop(hitObject, (hitObjectData.Y * playfieldScale) - (diameter / 2));
                    Canvas.SetLeft(hitObject, (hitObjectData.X * playfieldScale) - (diameter / 2));
                }
            }
        }
    }
}
