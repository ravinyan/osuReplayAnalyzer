using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.Objects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class HitObjectSpawner
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static OsuMath Math = new OsuMath();

        private static HitObject LastObject = null;
        private static int LastObjectIndex = 0;

        private static HitObject CurrentObject = null;
        private static int CurrentObjectIndex = 0;

        private static HitObject FirstObject = null;
        private static int FirstObjectIndex = 0;

        public static void UpdateHitObjects()
        {
            FindCurrentObject(ref CurrentObject, CurrentObjectIndex);
            SpawnObject(CurrentObject, true);
        }

        public static void UpdateHitObjectBackwards()
        {
            FindCurrentObject(ref LastObject, LastObjectIndex);
            SpawnObject(LastObject);
        }

        public static void UpdateHitObjectForward()
        {
            FindCurrentObject(ref FirstObject, FirstObjectIndex);
            SpawnObject(FirstObject);
        }

        public static void FindObjectIndexAfterSeek(long time, double direction)
        {
            int idx = -1;
            if (direction > 0) //forward
            {
                double arTime = Math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
                for (int i = 0; i < OsuBeatmap.HitObjectDictByIndex.Count; i++)
                {
                    if (Playfield.GetEndTime(OsuBeatmap.HitObjectDictByIndex[i]) >= time + arTime)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx >= 0)
                {
                    FirstObjectIndex = idx;
                    UpdateHitObjectForward();
                }
            }
            else //back
            {
                for (int i = 0; i < OsuBeatmap.HitObjectDictByIndex.Count; i++)
                {
                    HitObject obj = OsuBeatmap.HitObjectDictByIndex[i];

                    if (obj is Slider && Playfield.GetEndTime(obj) > time)
                    {
                        idx = i;

                        if (obj.IsHit == true && obj.HitAt > time)
                        {
                            Canvas sliderHead = obj.Children[1] as Canvas;
                            for (int j = 0; j <= 3; j++)
                            {
                                sliderHead.Children[j].Visibility = Visibility.Visible;
                            }

                            if (sliderHead.Children.Count > 4)
                            {
                                sliderHead.Children[4].Visibility = Visibility.Collapsed;
                            }
                            sliderHead.Visibility = Visibility.Visible;
                        }

                        break;
                    }

                    if (obj.IsHit == false && Playfield.GetEndTime(obj) > time)
                    {
                        idx = i;
                        break;
                    }

                    if (obj.IsHit == true && obj.HitAt > time)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx != -1)
                {
                    LastObjectIndex = idx;
                    CurrentObjectIndex = idx + Playfield.GetAliveHitObjects().Count;
                    UpdateHitObjectBackwards();
                }
            }
        }

        private static void SpawnObject(HitObject hitObject, bool updateCurrentIndex = false)
        {
            if (GamePlayClock.TimeElapsed > hitObject.SpawnTime - Math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
            &&  CurrentObjectIndex < OsuBeatmap.HitObjectDictByIndex.Count)
            {
                if (!Playfield.GetAliveHitObjects().Contains(hitObject))
                {
                    Window.playfieldCanva.Children.Add(hitObject);
                    Playfield.GetAliveHitObjects().Add(hitObject);

                    hitObject.Visibility = Visibility.Visible;

                    HitObjectAnimations.Start(hitObject);
                    if (GamePlayClock.IsPaused())
                    {
                        HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());
                    }
                }
                
                if (updateCurrentIndex == true)
                {
                    CurrentObjectIndex++;
                }
            }
        }

        private static void FindCurrentObject(ref HitObject hitObject, int index)
        {
            if (index >= OsuBeatmap.HitObjectDictByIndex.Count)
            {
                return;
            }

            if (hitObject != OsuBeatmap.HitObjectDictByIndex[index])
            {
                hitObject = OsuBeatmap.HitObjectDictByIndex[index];
            }
        }
    }
}
