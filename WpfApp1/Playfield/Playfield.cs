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

        private static int HitObjectIndex = 1;
        private static Canvas HitObject = null!;
        private static HitObject HitObjectProperties = null!;

        private static int cursorPositionIndex = 0;

        public static void Update()
        {
            if (HitObject != Window.playfieldCanva.Children[HitObjectIndex] as Canvas)
            {
                HitObject = Window.playfieldCanva.Children[HitObjectIndex] as Canvas;
                HitObjectProperties = (HitObject)HitObject.DataContext;
            }

            if (GamePlayClock.TimeElapsed > HitObjectProperties.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
            && HitObjectIndex <= MainWindow.map.HitObjects.Count)
            {
                AliveCanvasObjects.Add(HitObject);
                HitObject.Visibility = Visibility.Visible;
                HitObjectAnimations.Start(HitObject);

                HitObjectIndex++;
            }
        }

        public static void UpdateAfterSeek(long time)
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
