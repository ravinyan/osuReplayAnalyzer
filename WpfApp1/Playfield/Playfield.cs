using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using System.Windows;
using System.Windows.Controls;
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

        private static OsuMath math = new OsuMath();

        private static List<Canvas> AliveCanvasObjects = new List<Canvas>();

        private static int HitObjectIndex = 0;
        private static Canvas HitObject = null!;
        private static HitObject HitObjectProperties = null!;

        private static ReplayFrame CurrentFrame = null!;

        private static int cursorPositionIndex = 0;

        public static void UpdateHitObjects()
        {
            if (HitObject != OsuBeatmap.HitObjectDict2[HitObjectIndex])
            {
                HitObject = OsuBeatmap.HitObjectDict2[HitObjectIndex];
                HitObjectProperties = (HitObject)HitObject.DataContext;
            }

            if (GamePlayClock.TimeElapsed > HitObjectProperties.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
            && HitObjectIndex <= MainWindow.map.HitObjects.Count && HitObject.Visibility != Visibility.Visible)
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
        }

        public static void UpdateCursor()
        {
            if (CurrentFrame != MainWindow.replay.Frames[cursorPositionIndex])
            {
                CurrentFrame = MainWindow.replay.Frames[cursorPositionIndex];
            }

            while (GamePlayClock.TimeElapsed >= CurrentFrame.Time)
            {
                const double AspectRatio = 1.33;
                double height = Window.playfieldCanva.Height / AspectRatio;
                double width = Window.playfieldCanva.Width / AspectRatio;
                double osuScale = Math.Min(Window.playfieldCanva.Width / 512, Window.playfieldCanva.Height / 384);

                Canvas.SetLeft(Window.playfieldCursor, (CurrentFrame.X * osuScale) - (Window.playfieldCursor.Width / 2));
                Canvas.SetTop(Window.playfieldCursor, (CurrentFrame.Y * osuScale) - (Window.playfieldCursor.Width / 2));

                cursorPositionIndex++;
                CurrentFrame = MainWindow.replay.Frames[cursorPositionIndex];
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
