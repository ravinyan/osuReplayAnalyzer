using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.PlayfieldGameplay;
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
            
            AdjustCanvasHitObjectsPlacementAndSize(diameter, Window.playfieldCanva);
        }

        private static void AdjustCanvasHitObjectsPlacementAndSize(double diameter, Canvas playfieldCanva)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);
            double objectDiameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * playfieldScale * 2;

            MainWindow.OsuPlayfieldObjectScale = playfieldScale;
            MainWindow.OsuPlayfieldObjectDiameter = objectDiameter;

            for (int i = 0; i < MainWindow.map.HitObjects.Count; i++)
            {
                HitObjectData hitObjectData = MainWindow.map.HitObjects[i];

                hitObjectData.X = hitObjectData.BaseX * playfieldScale;
                hitObjectData.Y = hitObjectData.BaseY * playfieldScale;
            }

            // i dont know what im doing right here i was very sleepy and just threw numbers until stuff worked
            for (int i = 0; i < HitObjectManager.GetAliveHitObjects().Count; i++)
            {
                HitObject hitObject = HitObjectManager.GetAliveHitObjects()[i];

                // to all HitObjects there is automatically applied playfieldScale (MainWindow.OsuPlayfieldObjectDiameter)
                // causing size and position of alive hit objects to be incorrect after resizing...
                // objectDiameter is NEW size AFTER performing resizing (it have updated playfieldScale)
                // hitObject.Width is OLD size (it has old playfieldScale before resizing + doesnt change)
                // this provides counter scale to have correct size for alive objects after resizing
                double counterScale = objectDiameter / hitObject.Width;
                hitObject.LayoutTransform = new ScaleTransform(counterScale, counterScale);

                HitObjectData f = HitObjectManager.TransformHitObjectToDataObject(hitObject);
                if (hitObject is Slider)
                {
                    // update X and Y with new playfieldScale
                    f.X = f.BaseX * playfieldScale;
                    f.Y = f.BaseY * playfieldScale;
                    hitObject.X = f.X;
                    hitObject.Y = f.Y;
                    hitObject.RenderTransform = new TranslateTransform(playfieldScale, playfieldScale);
                }
                else if (hitObject is HitCircle)
                {
                    // update X and Y with new playfieldScale
                    f.X = f.BaseX * playfieldScale;
                    f.Y = f.BaseY * playfieldScale;
                    hitObject.X = f.X;
                    hitObject.Y = f.Y;

                    Canvas.SetLeft(hitObject, hitObject.X - diameter / 2);
                    Canvas.SetTop(hitObject, hitObject.Y - diameter / 2);   
                }
                else
                {
                    Canvas.SetLeft(hitObject, (playfieldCanva.Width - playfieldCanva.Width) / 2);
                    Canvas.SetTop(hitObject, (playfieldCanva.Height - playfieldCanva.Height) / 2);
                }
            }

            foreach (HitMarker hm in HitMarkerManager.GetAliveHitMarkers())
            {
                Canvas.SetTop(hm, hm.Position.Y * playfieldScale - Window.playfieldCursor.Width / 2);
                Canvas.SetLeft(hm, hm.Position.X * playfieldScale - Window.playfieldCursor.Width / 2);
            }

            foreach (HitMarkerData hm in HitMarkerData.HitMarkersData)
            {
                hm.Position.X = hm.BasePosition.X * (float)playfieldScale;
                hm.Position.Y = hm.BasePosition.Y * (float)playfieldScale;
            }
        }
    }
}
