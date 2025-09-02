using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.OsuMaths;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;

#nullable disable

namespace WpfApp1.Playfield
{
    public static class Playfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static readonly Ellipse Cursor = Window.playfieldCursor;

        private static OsuMath math = new OsuMath();

        private static List<Canvas> AliveCanvasObjects = new List<Canvas>();

        private static int HitObjectIndex = 0;
        private static Canvas HitObject = null!;
        private static HitObject HitObjectProperties = null!;

        private static ReplayFrame CurrentFrame = null!;

        private static int cursorPositionIndex = 0;

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
            if (cursorPositionIndex < MainWindow.replay.FramesDict.Count
            &&  CurrentFrame != MainWindow.replay.FramesDict[cursorPositionIndex])
            {
                CurrentFrame = MainWindow.replay.FramesDict[cursorPositionIndex];
            }

            while (cursorPositionIndex < MainWindow.replay.FramesDict.Count && GamePlayClock.TimeElapsed >= CurrentFrame.Time)
            {
                double osuScale = Math.Min(Window.playfieldCanva.Width / 512, Window.playfieldCanva.Height / 384);

                Canvas.SetLeft(Cursor, (CurrentFrame.X * osuScale) - (Cursor.Width / 2));
                Canvas.SetTop(Cursor, (CurrentFrame.Y * osuScale) - (Cursor.Width / 2));

                AddHitMarker(CurrentFrame, osuScale, true);

                cursorPositionIndex++;
                CurrentFrame = cursorPositionIndex < MainWindow.replay.FramesDict.Count
                    ? MainWindow.replay.FramesDict[cursorPositionIndex]
                    : MainWindow.replay.FramesDict[MainWindow.replay.FramesDict.Count - 1];
            }
        }

        private static bool isHeldL = false;
        private static bool isHeldR = false;
        private static void AddHitMarker(ReplayFrame frame, double osuScale, bool aaa = true)
        {
            bool leftClick = false;
            bool rightClick = false;

            if (frame.Click == Clicks.M1 || frame.Click == Clicks.K1)
            {
                leftClick = true;
            }
            else if (frame.Click == Clicks.M2 || frame.Click == Clicks.K2)
            {
                rightClick = true;
            }
            else if (frame.Click == Clicks.M12 || frame.Click == Clicks.K12)
            {
                leftClick = true;
                rightClick = true;
            }

            if (isHeldL == true && leftClick == false)
            {
                isHeldL = false;
            }
            else if (isHeldL == false && leftClick == true)
            {
                isHeldL = true;
                Add(frame, osuScale, "left");
            }

            if (isHeldR == true && rightClick == false)
            {
                isHeldR = false;
            }
            else if (isHeldR == false && rightClick == true)
            {
                isHeldR = true;
                Add(frame, osuScale, "right");
            }

            void Add(ReplayFrame frame, double osuScale, string direction)
            {
                TextBlock hitMarker = new TextBlock();
                hitMarker.Loaded += async delegate (object sender, RoutedEventArgs e)
                {
                    await Task.Delay(500);
                    Window.playfieldCanva.Children.Remove(hitMarker);
                };

                hitMarker.FontSize = 16;
                hitMarker.Width = 20;
                hitMarker.Height = 20;
                hitMarker.Text = "X";

                if (direction == "left")
                {
                    hitMarker.Foreground = System.Windows.Media.Brushes.Cyan;
                }
                else if (direction == "right")
                {
                    hitMarker.Foreground = System.Windows.Media.Brushes.Red;
                }

                Canvas.SetLeft(hitMarker, (frame.X * osuScale) - (Cursor.Width / 2));
                Canvas.SetTop(hitMarker, (frame.Y * osuScale) - (Cursor.Width / 2));
                Canvas.SetZIndex(hitMarker, 999999999);

                Window.playfieldCanva.Children.Add(hitMarker);
            }
        }

        public static void UpdateCursorPositionAfterSeek(ReplayFrame frame)
        {
            cursorPositionIndex = MainWindow.replay.Frames.IndexOf(frame);
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
                return obj.SpawnTime;
            }
        }
    }
}
