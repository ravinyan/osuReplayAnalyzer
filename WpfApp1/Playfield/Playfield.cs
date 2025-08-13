using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Animations;
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

        private static Canvas HitObject = null!;
        private static HitObject HitObjectProperties = null!;
        private static List<Canvas> AliveCanvasObjects = new List<Canvas>();

        private static int HitObjectIndex = 0;

        public static void HandleVisibleCircles()
        {
            if (HitObjectIndex <= MainWindow.map.HitObjects.Count)
            {
                if (HitObject != Window.playfieldCanva.Children[HitObjectIndex] as Canvas)
                {
                    HitObject = Window.playfieldCanva.Children[HitObjectIndex] as Canvas;
                    HitObjectProperties = (HitObject)HitObject.DataContext;
                }

                if (GamePlayClock.GetElapsedTime() > HitObjectProperties.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
                && HitObjectIndex <= MainWindow.map.HitObjects.Count)
                {
                    AliveCanvasObjects.Add(HitObject);
                    HitObject.Visibility = Visibility.Visible;
                    HitCircleAnimation.Start(HitObject);

                    HitObjectIndex++;
                }

                for (int i = 0; i < AliveCanvasObjects.Count; i++)
                {
                    Canvas obj = AliveCanvasObjects[i];
                    HitObject ep = (HitObject)obj.DataContext;

                    double timeToDelete;
                    if (ep is Circle)
                    {
                        timeToDelete = ep.SpawnTime;
                    }
                    else if (ep is Slider)
                    {
                        Slider s = (Slider)ep;
                        // this is kinda incorrect or something idk if something feels weird after i add slider ball
                        // then fix this if not then dont fix this
                        timeToDelete = math.GetSliderEndTime(s, MainWindow.map.Difficulty.SliderMultiplier);
                    }
                    else
                    {
                        Spinner s = (Spinner)ep;
                        timeToDelete = s.EndTime;
                    }

                    if (GamePlayClock.GetElapsedTime() > timeToDelete)
                    {
                        obj.Visibility = Visibility.Collapsed;
                        AliveCanvasObjects.Remove(obj);
                    }
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
    }
}
