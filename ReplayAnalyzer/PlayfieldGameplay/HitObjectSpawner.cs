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

        public static void ResetFields()
        {
            LastObject = null;
            LastObjectIndex = 0;

            CurrentObject = null;
            CurrentObjectIndex = 0;

            FirstObject = null;
            FirstObjectIndex = 0;
        }

        public static void UpdateHitObjects()
        {
            GetCurrentObject(ref CurrentObject, CurrentObjectIndex);
            SpawnObject(CurrentObject, true);
        }

        public static void UpdateHitObjectBackwards()
        {
            GetCurrentObject(ref LastObject, LastObjectIndex);
            SpawnObject(LastObject);
        }

        public static void UpdateHitObjectForward()
        {
            GetCurrentObject(ref FirstObject, FirstObjectIndex);
            SpawnObject(FirstObject);
        }

        public static void UpdateHitObjectsBetweenFirstAndLast()
        {
            CurrentObjectIndex = LastObjectIndex + 1;
            while (CurrentObjectIndex <= FirstObjectIndex)
            {
                GetCurrentObject(ref CurrentObject, CurrentObjectIndex);

                //if (!HitObjectManager.GetAliveHitObjects().Contains(CurrentObject))
                //{
                //    Window.playfieldCanva.Children.Add(CurrentObject);
                //    HitObjectManager.GetAliveHitObjects().Add(CurrentObject);
                //
                //    CurrentObject.Visibility = Visibility.Visible;
                //
                //    HitObjectAnimations.Start(CurrentObject);
                //    if (GamePlayClock.IsPaused())
                //    {
                //        HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
                //    }
                //}

                SpawnObject(CurrentObject);
                CurrentObjectIndex++;
            }
        }

        public static void FindObjectIndexAfterSeek(long time, double direction)
        {
            int idx = -1;
            if (direction >= 0) //forward
            {
                double arTime = Math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
                for (int i = 0; i < OsuBeatmap.HitObjectDictByIndex.Count; i++)
                {
                    double objectEndTime = HitObjectManager.GetEndTime(OsuBeatmap.HitObjectDictByIndex[i]);
                    if (objectEndTime >= time + arTime)
                    {
                        idx = i;
                        break;
                    }

                    if (i == OsuBeatmap.HitObjectDictByIndex.Count - 1)
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

                    if (obj is Slider && HitObjectManager.GetEndTime(obj) > time)
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

                    // god im fucking stupid this should be always on top... let this be reminder to stop being stupid
                    if (obj.IsHit == true && obj.HitAt >= time)
                    {
                        idx = i;
                        break;
                    }

                    if (obj.IsHit == false && HitObjectManager.GetEndTime(obj) > time)
                    {
                        idx = i;
                        break;
                    }

                    if (i == OsuBeatmap.HitObjectDictByIndex.Count - 1)
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx != -1)
                {
                    LastObjectIndex = idx;
                    CurrentObjectIndex = idx + HitObjectManager.GetAliveHitObjects().Count;
                    UpdateHitObjectBackwards();
                }
            }
        }

        private static void SpawnObject(HitObject hitObject, bool updateCurrentIndex = false)
        {
            if (GamePlayClock.TimeElapsed > hitObject.SpawnTime - Math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
            &&  CurrentObjectIndex < OsuBeatmap.HitObjectDictByIndex.Count - 1)
            {
                if (!HitObjectManager.GetAliveHitObjects().Contains(hitObject))
                {

                    if ((GamePlayClock.TimeElapsed > HitObjectManager.GetEndTime(hitObject)
                    || (GamePlayClock.TimeElapsed > hitObject.HitAt && hitObject.IsHit == true)) && hitObject is HitCircle)
                    {
                        
                        var s = "FREE MEE";
                        return;
                    }

                    Window.playfieldCanva.Children.Add(hitObject);
                    HitObjectManager.GetAliveHitObjects().Add(hitObject);

                    hitObject.Visibility = Visibility.Visible;

                    HitObjectAnimations.Start(hitObject);
                    if (GamePlayClock.IsPaused())
                    {
                        HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
                    }
                }

                if (updateCurrentIndex == true)
                {
                    CurrentObjectIndex++;
                }
            }
        }

        private static void GetCurrentObject(ref HitObject hitObject, int index)
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
