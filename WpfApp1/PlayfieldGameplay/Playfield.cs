using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.OsuMaths;
using WpfApp1.PlayfieldUI.UIElements;
using WpfApp1.Skins;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;

#nullable disable

namespace WpfApp1.PlayfieldGameplay
{
    public static class Playfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static readonly Ellipse Cursor = Window.playfieldCursor;

        private static OsuMath math = new OsuMath();

        private static List<Canvas> AliveCanvasObjects = new List<Canvas>();
        public static List<TextBlock> AliveHitMarkers = new List<TextBlock>();

        private static int HitObjectIndex = 0;
        private static Canvas HitObject = null!;
        private static HitObject HitObjectProperties = null!;

        private static int CursorPositionIndex = 0;

        private static int HitMarkerIndex = 0;
        private static ReplayFrame CurrentFrame = null;

        public static void UpdateHitMarkers()
        {
            TextBlock marker = Analyser.Analyser.HitMarkers[HitMarkerIndex];
            ReplayFrame frame = marker.DataContext as ReplayFrame;
            if (GamePlayClock.TimeElapsed >= frame.Time && !Window.playfieldCanva.Children.Contains(marker))
            {
                CurrentFrame = frame;
                Window.playfieldCanva.Children.Add(marker);
                HitMarkerAnimation.Start(marker);
                AliveHitMarkers.Add(marker);

                if (AliveHitObjectCount() > 0)
                {
                    Canvas objectToHit = GetAliveHitObjects().First();
                    HitObject prop = objectToHit.DataContext as HitObject;

                    double osuScale = Math.Min(Window.playfieldCanva.Width / 512, Window.playfieldCanva.Height / 384);
                    double radius = (double)((54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * osuScale) * 2;
                    float X = (float)((prop.X * osuScale) - (radius / 2));
                    float Y = (float)((prop.Y * osuScale) - (radius / 2));

                    System.Drawing.Drawing2D.GraphicsPath Ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                    Ellipse.AddEllipse(X, Y, (float)(radius), (float)(radius));
                    
                    System.Drawing.Point pt = new System.Drawing.Point((int)(frame.X * osuScale), (int)(frame.Y * osuScale));
                    if (Ellipse.IsVisible(pt))
                    {
                        // sliders have set end time no matter what i think but circles dont so when circle is hit then delete it
                        if (prop is Circle && (frame.Time + 400 >= prop.SpawnTime && frame.Time - 400 <= prop.SpawnTime))
                        {
                            Window.playfieldCanva.Children.Remove(objectToHit);
                            AliveCanvasObjects.Remove(objectToHit);
                            objectToHit.Visibility = Visibility.Collapsed;
                        }

                        GetHitJudgment(prop, frame, X, Y);
                    }
                }

                HitMarkerIndex++;
            }
            else if (GamePlayClock.TimeElapsed < frame.Time && Window.playfieldCanva.Children.Contains(marker))
            {
                Window.playfieldCanva.Children.Remove(marker);
                AliveHitMarkers.Remove(marker);
            
                HitMarkerIndex--;
            
                TextBlock newMarker = Analyser.Analyser.HitMarkers[HitMarkerIndex];
                ReplayFrame newFrame = marker.DataContext as ReplayFrame;
                CurrentFrame = newFrame;
            }
        }

        private static void GetHitJudgment(HitObject prop, ReplayFrame frame, float X, float Y)
        {
            double H300 = math.GetOverallDifficultyHitWindow300(MainWindow.map.Difficulty.OverallDifficulty);
            double H100 = math.GetOverallDifficultyHitWindow100(MainWindow.map.Difficulty.OverallDifficulty);
            double H50 = math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);

            Image img = new Image();
            if (frame.Time <= prop.SpawnTime + H300 && frame.Time >= prop.SpawnTime - H300)
            {
                img.Source = new BitmapImage(new Uri(SkinElement.Hit300()));
                JudgementCounter.Increment300();
            }
            else if (frame.Time <= prop.SpawnTime + H100 && frame.Time >= prop.SpawnTime - H100)
            {
                img.Source = new BitmapImage(new Uri(SkinElement.Hit100()));
                JudgementCounter.Increment100();
            }
            else if (frame.Time <= prop.SpawnTime + H50 && frame.Time >= prop.SpawnTime - H50)
            {
                img.Source = new BitmapImage(new Uri(SkinElement.Hit50()));
                JudgementCounter.Increment50();
            }
            else
            {
                img.Source = new BitmapImage(new Uri(SkinElement.HitMiss()));
                JudgementCounter.IncrementMiss();
            }

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

                if (hitMarkFrame.Time >= frame.Time)
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
            if (HitObjectIndex < OsuBeatmap.HitObjectDict2.Count && HitObject != OsuBeatmap.HitObjectDict2[HitObjectIndex])
            {
                HitObject = OsuBeatmap.HitObjectDict2[HitObjectIndex];
                HitObjectProperties = (HitObject)HitObject.DataContext;
            }

            if (HitObjectIndex < OsuBeatmap.HitObjectDict2.Count 
            &&  GamePlayClock.TimeElapsed > HitObjectProperties.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
            &&  HitObjectIndex <= MainWindow.map.HitObjects.Count && HitObject.Visibility != Visibility.Visible
            &&  !AliveCanvasObjects.Contains(HitObject))
            {
                AliveCanvasObjects.Add(HitObject);
                Window.playfieldCanva.Children.Add(OsuBeatmap.HitObjectDict2[HitObjectIndex]);
                HitObject.Visibility = Visibility.Visible;

                HitObjectAnimations.Start(HitObject);

                HitObjectIndex++;
            }
        }

        public static void UpdateHitObjectIndexAfterSeek(long time)
        {
            double ArTime = math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
            List<KeyValuePair<long, Canvas>> hitObjects = OsuBeatmap.HitObjectDict.ToList();

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
                var item = OsuBeatmap.HitObjectDict.First(x => x.Key > time);
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
                double osuScale = Math.Min(Window.playfieldCanva.Width / 512, Window.playfieldCanva.Height / 384);

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
