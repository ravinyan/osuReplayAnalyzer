using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
using Circle = ReplayParsers.Classes.Beatmap.osu.Objects.Circle;
using WpfApp1.Beatmaps;
using ReplayParsers.Classes.Replay;
#nullable disable

namespace WpfApp1.PlayfieldUI
{
    public static class ResizePlayfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void ResizePlayfieldCanva()
        {
            const double AspectRatio = 1.33;
            double height = (Window.ActualHeight - Window.musicControlUI.ActualHeight) / AspectRatio;
            double width = Window.ActualWidth / AspectRatio;
            double osuScale = Math.Min(height / 384, width / 512);
            double diameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * osuScale * 2;
            

            Window.playfieldCanva.Width = 512 * osuScale;
            Window.playfieldCanva.Height = 384 * osuScale;

            Window.playfieldBorder.Width = 512 * osuScale + 7 + diameter;
            Window.playfieldBorder.Height = 384 * osuScale + 7 + diameter;
            
            AdjustCanvasHitObjectsPlacementAndSize(diameter, Window.playfieldCanva);
        }

        private static void AdjustCanvasHitObjectsPlacementAndSize(double diameter, Canvas playfieldCanva)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);

            for (int i = 0; i < OsuBeatmap.HitObjectDictByIndex.Count; i++)
            {
                Canvas hitObject = OsuBeatmap.HitObjectDictByIndex[i];
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
                    Canvas.SetTop(hitObject, hitObjectData.Y * playfieldScale - diameter / 2);
                    Canvas.SetLeft(hitObject, hitObjectData.X * playfieldScale - diameter / 2);
                }
            }

            foreach (var hm in Analyser.Analyser.HitMarkers)
            {
                var dc = hm.Value.DataContext as ReplayFrame;

                Canvas.SetTop(hm.Value, dc.Y * playfieldScale - Window.playfieldCursor.Width / 2);
                Canvas.SetLeft(hm.Value, dc.X * playfieldScale - Window.playfieldCursor.Width / 2);
            }
        }
    }
}
