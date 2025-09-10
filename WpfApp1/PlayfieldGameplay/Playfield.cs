using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.OsuMaths;
using WpfApp1.PlayfieldUI.UIElements;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;

#nullable disable

namespace WpfApp1.PlayfieldGameplay
{
    public static class Playfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static OsuMath math = new OsuMath();

        private static List<Canvas> AliveCanvasObjects = new List<Canvas>();
        public static List<Canvas> AliveHitMarkers = new List<Canvas>();

        private static int HitObjectIndex = 0;
        private static Canvas HitObject = null!;
        private static HitObject HitObjectProperties = null!;

        private static int CursorPositionIndex = 0;

        private static int HitMarkerIndex = 0;
        private static ReplayFrame CurrentFrame = null;
        private static Canvas Marker = null;
        private static ReplayFrame MarkerFrame = null;

        public static void UpdateHitMarkers()
        {
            if (HitMarkerIndex < Analyser.Analyser.HitMarkers.Count && Marker != Analyser.Analyser.HitMarkers[HitMarkerIndex])
            {
                Marker = Analyser.Analyser.HitMarkers[HitMarkerIndex];
                MarkerFrame = Marker.DataContext as ReplayFrame;
            }

            if (GamePlayClock.TimeElapsed >= MarkerFrame.Time && !Window.playfieldCanva.Children.Contains(Marker))
            {
                CurrentFrame = MarkerFrame;
                Window.playfieldCanva.Children.Add(Marker);
                HitMarkerAnimation.Start(Marker);
                AliveHitMarkers.Add(Marker);

                if (AliveHitObjectCount() > 0)
                {
                    Canvas objectToHit = GetAliveHitObjects().First();
                    HitObject prop = objectToHit.DataContext as HitObject;

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;
                    float X = (float)((prop.X * osuScale) - (diameter / 2));
                    float Y = (float)((prop.Y * osuScale) - (diameter / 2));

                    // this Ellipse is hitbox area of circle and is created here coz actual circle cant have this as children
                    // then it check if Point (current hit marker location) is inside this Ellipse with Ellipse.Visible(pt)
                    System.Drawing.Drawing2D.GraphicsPath Ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                    Ellipse.AddEllipse(X, Y, (float)(diameter), (float)(diameter));
                    
                    System.Drawing.PointF pt = new System.Drawing.PointF((float)(MarkerFrame.X * osuScale), (float)(MarkerFrame.Y * osuScale));
                    if (Ellipse.IsVisible(pt))
                    {
                        // sliders have set end time no matter what i think but circles dont so when circle is hit then delete it
                        if (prop is Circle && (MarkerFrame.Time + 400 >= prop.SpawnTime && MarkerFrame.Time - 400 <= prop.SpawnTime))
                        {
                            if (objectToHit.Visibility != Visibility.Collapsed)
                            {
                                double judgementX = (prop.X * osuScale - (diameter / 2));
                                double judgementY = (prop.Y * osuScale - (diameter));
                                GetHitJudgment(prop, MarkerFrame, judgementX, judgementY, diameter);
                            }

                            Window.playfieldCanva.Children.Remove(objectToHit);
                            AliveCanvasObjects.Remove(objectToHit);
                            objectToHit.Visibility = Visibility.Collapsed;
                        }
                        else if (prop is Slider && (MarkerFrame.Time + 400 >= prop.SpawnTime && MarkerFrame.Time - 400 <= prop.SpawnTime))
                        {
                            Canvas sliderHead = objectToHit.Children[0] as Canvas;

                            if (sliderHead.Visibility != Visibility.Collapsed)
                            {
                                double judgementX = (prop.X * osuScale - (diameter / 2));
                                double judgementY = (prop.Y * osuScale - (diameter));
                                GetHitJudgment(prop, MarkerFrame, judgementX, judgementY, diameter);

                                sliderHead.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }

                HitMarkerIndex++;
            }
            else if (GamePlayClock.TimeElapsed < MarkerFrame.Time && Window.playfieldCanva.Children.Contains(Marker))
            {
                Window.playfieldCanva.Children.Remove(Marker);
                AliveHitMarkers.Remove(Marker);
            
                HitMarkerIndex--;
            
                Canvas newMarker = Analyser.Analyser.HitMarkers[HitMarkerIndex];
                ReplayFrame newFrame = Marker.DataContext as ReplayFrame;
                CurrentFrame = newFrame;
            }
        }

        private static void GetHitJudgment(HitObject prop, ReplayFrame frame, double X, double Y, double diameter)
        {
            double H300 = math.GetOverallDifficultyHitWindow300(MainWindow.map.Difficulty.OverallDifficulty);
            double H100 = math.GetOverallDifficultyHitWindow100(MainWindow.map.Difficulty.OverallDifficulty);
            double H50 = math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);

            Image img;
            if (frame.Time <= prop.SpawnTime + H300 && frame.Time >= prop.SpawnTime - H300)
            {
                img = Analyser.UIElements.HitJudgment.Image300();
                JudgementCounter.Increment300();
            }
            else if (frame.Time <= prop.SpawnTime + H100 && frame.Time >= prop.SpawnTime - H100)
            {
                img = Analyser.UIElements.HitJudgment.Image100();
                JudgementCounter.Increment100();
            }
            else if (frame.Time <= prop.SpawnTime + H50 && frame.Time >= prop.SpawnTime - H50)
            {
                img = Analyser.UIElements.HitJudgment.Image50();
                JudgementCounter.Increment50();
            }
            else
            {
                img = Analyser.UIElements.HitJudgment.ImageMiss();
                JudgementCounter.IncrementMiss();
            }

            img.Width = diameter;
            img.Height = diameter;

            Window.playfieldCanva.Children.Add(img);

            Canvas.SetLeft(img, X);
            Canvas.SetTop(img, Y);

            img.Loaded += async delegate (object sender, RoutedEventArgs e)
            {
                await Task.Delay(800);
                Window.playfieldCanva.Children.Remove(img);
            };
        }

        public static void UpdateHitMarkerIndexAfterSeek(ReplayFrame frame)
        {
            int i;
            bool found = false;
            for (i = 0; i < Analyser.Analyser.HitMarkers.Count; i++)
            {
                ReplayFrame hitMarkFrame = Analyser.Analyser.HitMarkers[i].DataContext as ReplayFrame;

                if (hitMarkFrame.Time >= frame.Time || i == Analyser.Analyser.HitMarkers.Count - 1)
                {
                    found = true;
                    break;
                }
            }

            if (found == true)
            {
                HitMarkerIndex = i;
            }
        }

        public static void UpdateHitObjects()
        {
            if (HitObjectIndex < OsuBeatmap.HitObjectDictByIndex.Count && HitObject != OsuBeatmap.HitObjectDictByIndex[HitObjectIndex])
            {
                HitObject = OsuBeatmap.HitObjectDictByIndex[HitObjectIndex];
                HitObjectProperties = (HitObject)HitObject.DataContext;
            }

            if (HitObjectIndex < OsuBeatmap.HitObjectDictByIndex.Count 
            &&  GamePlayClock.TimeElapsed > HitObjectProperties.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
            &&  HitObjectIndex <= MainWindow.map.HitObjects.Count && HitObject.Visibility != Visibility.Visible
            &&  !AliveCanvasObjects.Contains(HitObject))
            {
                AliveCanvasObjects.Add(HitObject);
                Window.playfieldCanva.Children.Add(OsuBeatmap.HitObjectDictByIndex[HitObjectIndex]);
                HitObject.Visibility = Visibility.Visible;

                HitObjectAnimations.Start(HitObject);

                HitObjectIndex++;

                if (HitObjectIndex > MainWindow.map.HitObjects.Count)
                {
                    Window.musicPlayer.MediaPlayer.Pause();
                }
            }
        }

        public static void UpdateHitObjectIndexAfterSeek(long time)
        {
            double ArTime = math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
            List<KeyValuePair<long, Canvas>> hitObjects = OsuBeatmap.HitObjectDictByTime.ToList();

            List<KeyValuePair<long, Canvas>> aliveHitObjects = hitObjects.Where(
                x => x.Key - ArTime < time 
                && GetEndTime(x.Value) > time && x.Value.Visibility != Visibility.Visible).ToList();

            bool found = false;
            int i;
            for (i = 0; i < hitObjects.Count; i++)
            {
                if (aliveHitObjects.Count > 0 && hitObjects[i].Value == aliveHitObjects[0].Value)
                {
                    found = true;
                    break;
                }
            }

            if (found == true)
            {
                HitObjectIndex = i;
            }
            else
            {
                var item = OsuBeatmap.HitObjectDictByTime.FirstOrDefault(
                    x => x.Key > time, OsuBeatmap.HitObjectDictByTime.Last());
                HitObjectIndex = hitObjects.IndexOf(item);
            }
        }

        public static void UpdateCursor()
        {
            if (CursorPositionIndex < MainWindow.replay.FramesDict.Count
            &&  CurrentFrame != MainWindow.replay.FramesDict[CursorPositionIndex])
            {
                CurrentFrame = MainWindow.replay.FramesDict[CursorPositionIndex];
            }

            while (CursorPositionIndex < MainWindow.replay.FramesDict.Count && GamePlayClock.TimeElapsed >= CurrentFrame.Time)
            {
                double osuScale = MainWindow.OsuPlayfieldObjectScale;

                Canvas.SetLeft(Window.playfieldCursor, (CurrentFrame.X * osuScale) - (Window.playfieldCursor.Width / 2));
                Canvas.SetTop(Window.playfieldCursor, (CurrentFrame.Y * osuScale) - (Window.playfieldCursor.Width / 2));

                CursorPositionIndex++;
                CurrentFrame = CursorPositionIndex < MainWindow.replay.FramesDict.Count
                    ? MainWindow.replay.FramesDict[CursorPositionIndex]
                    : MainWindow.replay.FramesDict[MainWindow.replay.FramesDict.Count - 1];
            }
        }

        public static void UpdateCursorPositionAfterSeek(ReplayFrame frame)
        {
            CursorPositionIndex = MainWindow.replay.Frames.IndexOf(frame);
        }

        public static void HandleVisibleCircles()
        {
            for (int i = 0; i < AliveCanvasObjects.Count; i++)
            {
                Canvas obj = AliveCanvasObjects[i];
                HitObject ep = (HitObject)obj.DataContext;

                long elapsedTime = GamePlayClock.TimeElapsed;
                if (elapsedTime >= GetEndTime(obj)
                ||  elapsedTime <= ep.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate))
                {
                    if (ep is Circle && obj.Visibility == Visibility.Visible)
                    {
                        MainWindow window = (MainWindow)Application.Current.MainWindow;

                        Image miss = Analyser.UIElements.HitJudgment.ImageMiss();
                        miss.Width = MainWindow.OsuPlayfieldObjectDiameter;
                        miss.Height = MainWindow.OsuPlayfieldObjectDiameter;

                        miss.Loaded += async delegate (object sender, RoutedEventArgs e)
                        {
                            await Task.Delay(800);
                            window.playfieldCanva.Children.Remove(miss);
                        };

                        JudgementCounter.IncrementMiss();
                        window.playfieldCanva.Children.Add(miss);
                    }

                    Window.playfieldCanva.Children.Remove(obj);
                    obj.Visibility = Visibility.Collapsed;
                    AliveCanvasObjects.Remove(obj);
                }
            }
        }

        public static int AliveHitObjectCount()
        {
            return AliveCanvasObjects.Count;
        }

        public static List<Canvas> GetAliveHitObjects()
        {
            return AliveCanvasObjects;
        }

        private static double GetEndTime(Canvas o)
        {
            if (o.DataContext is Slider)
            {
                Slider obj = o.DataContext as Slider;
                return (int)obj.EndTime;
            }
            else if (o.DataContext is Spinner)
            {
                Spinner obj = o.DataContext as Spinner;
                return obj.EndTime;
            }
            else
            {
                Circle obj = o.DataContext as Circle;
                return obj.SpawnTime + math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);
            }
        }
    }
}
