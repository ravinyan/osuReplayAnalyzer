using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.AnalyzerTools.CursorPath;
using ReplayAnalyzer.AnalyzerTools.FrameMarkers;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Slider = ReplayAnalyzer.HitObjects.Slider;

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
            
            AdjustCanvasObjectsPlacementAndSize(diameter, Window.playfieldCanva);
        }

        private static void AdjustCanvasObjectsPlacementAndSize(double diameter, Canvas playfieldCanva)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);
            double objectDiameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * playfieldScale * 2;

            MainWindow.OsuPlayfieldObjectScale = playfieldScale;
            MainWindow.OsuPlayfieldObjectDiameter = objectDiameter;

            RepositionHitObjects(playfieldScale, diameter, objectDiameter, playfieldCanva);
            RepositionHitMarkers(playfieldScale);
            RepositionFrameMarkers(playfieldScale);
            RepositionCursorPath(playfieldScale);
        }

        private static void RepositionHitObjects(double playfieldScale, double diameter, double objectDiameter, Canvas playfieldCanva)
        {
            for (int i = 0; i < MainWindow.map.HitObjects.Count; i++)
            {
                HitObjectData hitObjectData = MainWindow.map.HitObjects[i];
                hitObjectData.X = hitObjectData.BaseX * playfieldScale;
                hitObjectData.Y = hitObjectData.BaseY * playfieldScale;
            }

            for (int i = 0; i < HitObjectManager.GetAliveHitObjects().Count; i++)
            {
                HitObject hitObject = HitObjectManager.GetAliveHitObjects()[i];

                if (hitObject is Spinner)
                {
                    double spinnerCounterScale = playfieldCanva.Width / hitObject.Width;
                    hitObject.LayoutTransform = new ScaleTransform(spinnerCounterScale, spinnerCounterScale);

                    Canvas.SetLeft(hitObject, 0);
                    Canvas.SetTop(hitObject, 0);

                    continue;
                }

                // objectDiameter = current size with new playfieldScale, hitObject.Width = old size with old playfieldScale
                // this makes counterScale so that hit object scale value (its size) is correcty updated
                double counterScale = objectDiameter / hitObject.Width;
                hitObject.LayoutTransform = new ScaleTransform(counterScale, counterScale);

                // update X and Y with new playfieldScale for sliders and circles (spinners dont need it)
                HitObjectData f = HitObjectManager.TransformHitObjectToDataObject(hitObject);
                f.X = f.BaseX * playfieldScale;
                f.Y = f.BaseY * playfieldScale;
                hitObject.X = f.X;
                hitObject.Y = f.Y;

                // update positions
                if (hitObject is Slider)
                {
                    hitObject.RenderTransform = new TranslateTransform(playfieldScale, playfieldScale);
                }
                else if (hitObject is HitCircle)
                {
                    Canvas.SetLeft(hitObject, hitObject.X - diameter / 2);
                    Canvas.SetTop(hitObject, hitObject.Y - diameter / 2);
                }
            }
        }

        private static void RepositionCursorPath(double playfieldScale)
        {
            foreach (CursorPathData cp in CursorPathData.CursorPathsData)
            {
                cp.LineStart = cp.BaseLineStart * (float)playfieldScale;
                cp.LineEnd = cp.BaseLineEnd * (float)playfieldScale;
            }

            foreach (CursorPath cp in CursorPathManager.GetAliveCursorPaths())
            {
                Canvas.SetTop(cp, cp.LineStart.Y - Window.playfieldCursor.Width / 2);
                Canvas.SetLeft(cp, cp.LineStart.X - Window.playfieldCursor.Width / 2);
            }
        }

        private static void RepositionFrameMarkers(double playfieldScale)
        {
            foreach (FrameMarkerData fm in FrameMarkerData.FrameMarkersData)
            {
                fm.Position.X = fm.BasePosition.X * (float)playfieldScale;
                fm.Position.Y = fm.BasePosition.Y * (float)playfieldScale;
            }

            foreach (FrameMarker fm in FrameMarkerManager.GetAliveFrameMarkers())
            {
                Canvas.SetTop(fm, fm.Position.Y - Window.playfieldCursor.Width / 2);
                Canvas.SetLeft(fm, fm.Position.X - Window.playfieldCursor.Width / 2);
            }
        }

        private static void RepositionHitMarkers(double playfieldScale)
        {
            foreach (HitMarkerData hm in HitMarkerData.HitMarkersData)
            {
                hm.Position.X = hm.BasePosition.X * (float)playfieldScale;
                hm.Position.Y = hm.BasePosition.Y * (float)playfieldScale;
            }

            foreach (HitMarker hm in HitMarkerManager.GetAliveHitMarkers())
            {
                Canvas.SetTop(hm, hm.Position.Y - Window.playfieldCursor.Width / 2);
                Canvas.SetLeft(hm, hm.Position.X - Window.playfieldCursor.Width / 2);
            }
        }
    }
}
