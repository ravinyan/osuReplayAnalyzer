using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.Cursor;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Osu;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Slider = ReplayAnalyzer.HitObjects.Osu.Slider;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class OsuPlayfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static Canvas Playfield { get; private set; } = new Canvas();
        public static Border PlayfieldBorder { get; private set; } = new Border();
        public static Canvas PlayfieldCursor { get; private set; } = new Canvas();

        public static bool Create()
        {
            // osu playfield never changes so it can be created only once
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            if (window.playfieldGrid.Children.Contains(PlayfieldBorder))
            {
                Playfield.Visibility       = Visibility.Visible;
                PlayfieldBorder.Visibility = Visibility.Visible;
                PlayfieldCursor.Visibility = Visibility.Visible;
                return true;
            }

            CreateBorder();
            CreatePlayfield();
            CreateCursor();

            window.playfieldGrid.Children.Add(PlayfieldBorder);
            return true;
        }

        public static void Dispose()
        {
            Playfield.Visibility       = Visibility.Collapsed;
            PlayfieldBorder.Visibility = Visibility.Collapsed;
            PlayfieldCursor.Visibility = Visibility.Collapsed;
        }

        public static void UpdateGameplayLoop()
        {
            CursorManager.UpdateCursorPosition();
            HitDetection.CheckIfObjectWasHit();

            FrameMarkerManager.UpdateFrameMarker();
            CursorPathManager.UpdateCursorPath();

            SliderEndJudgement.UpdateSliderBodyEvents();
            SliderReverseArrow.UpdateSliderRepeats();
            SliderTick.UpdateSliderTicks();

            HitObjectManager.HandleVisibleHitObjects();
            HitJudgementManager.HandleAliveHitJudgements();

            HitMarkerManager.HandleAliveHitMarkers();
            FrameMarkerManager.HandleAliveFrameMarkers();
            CursorPathManager.HandleAliveCursorPaths();
        }

        public static void PreloadReplay()
        {
            for (int i = 0; i < MainWindow.replay.FramesDict.Count; i++)
            {
                long time = MainWindow.replay.FramesDict[i].Time;
                GamePlayClock.Seek(time);

                HitObjectSpawner.UpdateHitObjects();
                CursorManager.UpdateCursorPosition();
                HitMarkerManager.UpdateIndexAfterSeek(time);

                SliderEndJudgement.UpdateSliderBodyEvents();
                SliderReverseArrow.UpdateSliderRepeats(true);
                SliderTick.UpdateSliderTicks(true);

                HitObjectManager.HandleVisibleHitObjects();
                HitMarkerManager.HandleAliveHitMarkers();
                HitJudgementManager.HandleAliveHitJudgements();
            }

            PlayfieldGameplay.Playfield.ResetPlayfieldFields();

            // clear stuck objects except cursor which is at index 0
            for (int i = Playfield.Children.Count - 1; i >= 0; i--)
            {
                Playfield.Children.Remove(Playfield.Children[i]);
            }

            HitMarkerManager.GetAliveDataHitMarkers().Clear();
        }

        // might be useless
        public static void SeekGameplay(double direction, ReplayFrame f)
        {
            CursorManager.UpdateCursorPositionAfterSeek(f);
            SliderTick.UpdateSliderTicks();
            HitMarkerManager.UpdateIndexAfterSeek(f.Time);
            FrameMarkerManager.UpdateIndexAfterSeek(direction, f);
            CursorPathManager.UpdateIndexAfterSeek(direction, f);
        }

        public static void Resize()
        {
            const double AspectRatio = 1.33;
            double height = (Window.ActualHeight - Window.musicControlUI.ActualHeight) / AspectRatio;
            double width = Window.ActualWidth / AspectRatio;
            double playfieldScale = Math.Min(height / 384, width / 512);

            // this still needs to be applied before object scale i guess
            Playfield.Width = 512 * playfieldScale;
            Playfield.Height = 384 * playfieldScale;

            double objectScale = Math.Min(Playfield.Width / 512, Playfield.Height / 384);
            double objectDiameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * objectScale * 2;

            PlayfieldBorder.Width = 512 * objectScale + 7 + objectDiameter;
            PlayfieldBorder.Height = 384 * objectScale + 7 + objectDiameter;

            MainWindow.OsuPlayfieldObjectScale = objectScale;
            MainWindow.OsuPlayfieldObjectDiameter = objectDiameter;

            // just need to yoink OsuPlayfieldObjectScale for other game modes
            if (MainWindow.replay.GameMode != GameMode.Osu)
            {
                return;
            }

            HitObjectAnimations.ShouldUpdateScale = true;
            RepositionHitObjects(objectScale, objectDiameter);
            RepositionGameplayCursor(objectScale);
            RepositionHitMarkers(objectScale);
            RepositionFrameMarkers(objectScale);
            RepositionCursorPath(objectScale);
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

        private static void RepositionHitObjects(double playfieldScale, double objectDiameter)
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
                    double spinnerCounterScale = Playfield.Width / hitObject.Width;
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
                    Canvas.SetLeft(hitObject, f.X - objectDiameter / 2);
                    Canvas.SetTop(hitObject, f.Y - objectDiameter / 2);
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

        private static void CreateCursor()
        {
            if (PlayfieldCursor == null)
            {
                PlayfieldCursor = new Canvas();
            }

            PlayfieldCursor.Name = "PlayfieldCursor";
            PlayfieldCursor.Width = 35;
            PlayfieldCursor.Height = 35;
            Canvas.SetZIndex(PlayfieldCursor, 99);
        }

        private static void CreatePlayfield()
        {
            if (Playfield == null)
            {
                Playfield = new Canvas();
            }

            Playfield.Name = "OsuPlayfield";
            Playfield.Width = 512;
            Playfield.Height = 384;
            Playfield.ClipToBounds = false;

            PlayfieldBorder!.Child = Playfield;
        }

        private static void CreateBorder()
        {
            if (PlayfieldBorder == null)
            {
                PlayfieldBorder = new Border();
            }

            PlayfieldBorder.Name = "OsuPlayfieldBorder";
            PlayfieldBorder.Visibility = Visibility.Visible;
            PlayfieldBorder.BorderBrush = Brushes.White;
            // in MainWindow.xaml there is grid and this playfield is in this position
            Grid.SetColumn(PlayfieldBorder, 1);
            Grid.SetRow(PlayfieldBorder, 1);
        }
    }
}
