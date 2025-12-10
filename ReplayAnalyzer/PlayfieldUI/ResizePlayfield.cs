using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.Objects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ReplayAnalyzer.AnalyzerTools;
using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;

#nullable disable

namespace ReplayAnalyzer.PlayfieldUI
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

            MainWindow.OsuPlayfieldObjectScale = playfieldScale;
            MainWindow.OsuPlayfieldObjectDiameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * playfieldScale * 2;

            //for (int i = 0; i < OsuBeatmap.HitObjectDictByIndex.Count; i++)
            //HitObject hitObject = OsuBeatmap.HitObjectDictByIndex[i];
            for (int i = 0; i < OsuBeatmap.HitObjectDictByIndex.Count; i++)
            {
                // correct diameter would be 71.167999999999992
                HitObject hitObject = OsuBeatmap.HitObjectDictByIndex[i];
                //HitObjectData hitObject = MainWindow.map.HitObjects[i];

                //hitObject.Width = diameter * playfieldScale;
                //hitObject.Height = diameter * playfieldScale;

                //hitObject.LayoutTransform = new ScaleTransform(playfieldScale, playfieldScale);

                // i dont understand why render transform doesnt work on circles but works on sliders...
                // and im too scared to understand... at least it works
                if (hitObject is Objects.Slider)
                {
                    hitObject.RenderTransform = new TranslateTransform(playfieldScale, playfieldScale);
                }
                else if (hitObject is HitCircle)
                {
                    Canvas.SetLeft(hitObject, hitObject.X * playfieldScale - diameter / 2);
                    Canvas.SetTop(hitObject, hitObject.Y * playfieldScale - diameter / 2);
                }
                else
                {
                    Canvas.SetLeft(hitObject, (playfieldCanva.Width - playfieldCanva.Width) / 2);
                    Canvas.SetTop(hitObject, (playfieldCanva.Height - playfieldCanva.Height) / 2);   
                }
            }

            foreach (var hm in Analyzer.HitMarkers)
            {
                Canvas.SetTop(hm.Value, hm.Value.Position.Y * playfieldScale - Window.playfieldCursor.Width / 2);
                Canvas.SetLeft(hm.Value, hm.Value.Position.X * playfieldScale - Window.playfieldCursor.Width / 2);
            }
        }
    }
}
