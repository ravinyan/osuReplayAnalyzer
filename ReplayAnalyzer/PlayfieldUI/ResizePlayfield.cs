using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.AnalyzerTools.Cursor;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Osu;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Slider = ReplayAnalyzer.HitObjects.Osu.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldUI
{
    public static class ResizePlayfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void ResizePlayfieldCanva()
        {
            // old implementation will delete if new one works fine
            //const double AspectRatio = 1.33;
            //double height = (Window.ActualHeight - Window.musicControlUI.ActualHeight) / AspectRatio;
            //double width = Window.ActualWidth / AspectRatio;
            //double osuScale = Math.Min(height / 384, width / 512);
            //double diameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * osuScale * 2;
            //
            //if (MainWindow.replay.GameMode == OsuFileParsers.Classes.Replay.GameMode.Osu)
            //{
            //    OsuPlayfield.Playfield.Width = 512 * osuScale;
            //    OsuPlayfield.Playfield.Height = 384 * osuScale;
            //
            //    OsuPlayfield.PlayfieldBorder.Width = 512 * osuScale + 7 + diameter;
            //    OsuPlayfield.PlayfieldBorder.Height = 384 * osuScale + 7 + diameter;
            //    AdjustCanvasObjectsPlacementAndSize(diameter, OsuPlayfield.Playfield);
            //}
        }

        private static void AdjustCanvasObjectsPlacementAndSize(double diameter, Canvas playfieldCanva)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);
            double objectDiameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * playfieldScale * 2;

            MainWindow.OsuPlayfieldObjectScale = playfieldScale;
            MainWindow.OsuPlayfieldObjectDiameter = objectDiameter;

            // random field jumpscare
            HitObjectAnimations.ShouldUpdateScale = true;
            RepositionHitObjects(playfieldScale, diameter, objectDiameter, playfieldCanva);
            RepositionGameplayCursor(playfieldScale);
            RepositionHitMarkers(playfieldScale);
            RepositionFrameMarkers(playfieldScale);
            RepositionCursorPath(playfieldScale);
        }

        private static void RepositionGameplayCursor(double playfieldScale)
        {
            if (CursorManager.CursorPositionIndex > 0 && CursorManager.CursorPositionIndex < MainWindow.replay.FramesDict.Count)
            {
                double cursorX = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].X * playfieldScale - OsuPlayfield.PlayfieldCursor.Width / 2;
                double cursorY = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Y * playfieldScale - OsuPlayfield.PlayfieldCursor.Width / 2;

                Canvas.SetLeft(OsuPlayfield.PlayfieldCursor, cursorX);
                Canvas.SetTop(OsuPlayfield.PlayfieldCursor, cursorY);
            }
        }

        private static void RepositionHitObjects(double playfieldScale, double diameter, double objectDiameter, Canvas playfieldCanva)
        {
            for (int i = 0; i < MainWindow.map.HitObjects.Count; i++)
            {
                HitObjectData hitObjectData = MainWindow.map.HitObjects[i];
                hitObjectData.X = (hitObjectData.BaseX + hitObjectData.StackOffset) * playfieldScale;
                hitObjectData.Y = (hitObjectData.BaseY + hitObjectData.StackOffset) * playfieldScale;
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
                hitObject.X = f.X;
                hitObject.Y = f.Y;

                // update positions
                if (hitObject is Slider)
                {
                    // why did this broke stacking positions a bit... why this doesnt affect slider positions and their
                    // position is correct by doing nothing... what the f i will leave this commented for now
                    // coz i guess scale transform updates slider fully since slider object doesnt have X and Y pos only its children
                    //hitObject.RenderTransform = new TranslateTransform(playfieldScale, playfieldScale);
                }
                else if (hitObject is HitCircle)
                {
                    Canvas.SetLeft(hitObject, f.X - diameter / 2);
                    Canvas.SetTop(hitObject, f.Y - diameter / 2);
                }
            }
        }

        private static void RepositionCursorPath(double playfieldScale)
        {
            int pathCount = CursorPathManager.GetAliveCursorPaths().Count;
            int currentPathIndex = CursorPathManager.CursorPathIndex - pathCount;

            // clear what is alive coz updating current values is pain in the ass
            List<CursorPath> aliveCursorPaths = new List<CursorPath>();
            for (int i = 0; i < pathCount; i++)
            {
                aliveCursorPaths.Add(CursorPathManager.GetAliveCursorPaths()[i]);
            }

            foreach (CursorPath cp in aliveCursorPaths)
            {
                CursorPathManager.GetAliveCursorPaths().Remove(cp);
                OsuPlayfield.Playfield.Children.Remove(cp);
            }
            aliveCursorPaths.Clear();

            // re create all paths now with new and correct values
            for (int i = 0; i < pathCount; i++)
            {
                CursorPath newPath = CursorPath.Create(currentPathIndex + i);
                if (newPath != null)
                {
                    OsuPlayfield.Playfield.Children.Add(newPath);
                    CursorPathManager.GetAliveCursorPaths().Add(newPath);

                    if (currentPathIndex + i >= MainWindow.replay.FramesDict.Count)
                    {
                        break;
                    }
                }  
            }
        }

        private static void RepositionFrameMarkers(double playfieldScale)
        {
            int frameMarkerCount = FrameMarkerManager.GetAliveFrameMarkers().Count;
            int frameMarkerIndex = FrameMarkerManager.FrameMarkerIndex - frameMarkerCount;

            // clear what is alive coz updating current values is pain in the ass
            List<FrameMarker> aliveFrameMarkers = new List<FrameMarker>();
            for (int i = 0; i < frameMarkerCount; i++)
            {
                aliveFrameMarkers.Add(FrameMarkerManager.GetAliveFrameMarkers()[i]);
            }

            foreach (FrameMarker fm in aliveFrameMarkers)
            {
                FrameMarkerManager.GetAliveFrameMarkers().Remove(fm);
                OsuPlayfield.Playfield.Children.Remove(fm);
            }
            aliveFrameMarkers.Clear();

            for (int i = 0; i < frameMarkerCount + 1; i++)
            {
                FrameMarker newMarker = FrameMarker.Create(frameMarkerIndex + i);
                if (newMarker != null)
                {
                    OsuPlayfield.Playfield.Children.Add(newMarker);
                    FrameMarkerManager.GetAliveFrameMarkers().Add(newMarker);

                    if (frameMarkerIndex + i >= MainWindow.replay.FramesDict.Count)
                    {
                        break;
                    }
                }
            }
        }

        private static void RepositionHitMarkers(double playfieldScale)
        {
            int currentMarkerIndex = -1;
            if (HitMarkerManager.GetAliveHitMarkers().Count > 0)
            {
                currentMarkerIndex = int.Parse(HitMarkerManager.GetAliveHitMarkers()[0].Name.Substring(9));
            }

            int aliveMarkers = HitMarkerManager.GetAliveDataHitMarkers().Count;
            for (int i = 0; i < HitMarkerData.HitMarkersData.Count; i++)
            {
                HitMarkerData hmd = HitMarkerData.HitMarkersData[i];
                hmd.Position.X = hmd.BasePosition.X * (float)playfieldScale;
                hmd.Position.Y = hmd.BasePosition.Y * (float)playfieldScale;

                if (currentMarkerIndex != -1 && i >= currentMarkerIndex && i < currentMarkerIndex + aliveMarkers)
                {
                    HitMarker hm = HitMarkerManager.GetAliveHitMarkers()[i - currentMarkerIndex];
                    Canvas.SetLeft(hm, hmd.Position.X - hm.Width / 2);
                    Canvas.SetTop(hm, hmd.Position.Y - hm.Width / 2);
                }
            }
        }
    }
}
