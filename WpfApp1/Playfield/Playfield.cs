using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
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
        private static OsuMath math = new OsuMath();

        private static List<Canvas> AliveCanvasObjects = new List<Canvas>();

        public static void Update(long time)
        {
            double ArTime = math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
            Dictionary<long, Canvas> hitObjects = OsuBeatmap.HitObjectDict;

            List<KeyValuePair<long, Canvas>> aliveHitObjects = hitObjects.Where(
                x => x.Key - ArTime < time 
                && GetEndTime(x.Value) > time && x.Value.Visibility != Visibility.Visible).ToList();

            foreach (var o in aliveHitObjects)
            {
                AliveCanvasObjects.Add(o.Value);
                
                o.Value.Visibility = Visibility.Visible;
                HitObjectAnimations.Start(o.Value);
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
