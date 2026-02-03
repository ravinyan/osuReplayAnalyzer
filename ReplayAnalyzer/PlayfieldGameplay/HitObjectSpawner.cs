using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.GameplaySkin;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.HitObjects.Slider;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class HitObjectSpawner
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static OsuMath OsuMath = new OsuMath();

        private static HitObjectData LastObject = null;
        private static int LastObjectIndex = 0;

        private static HitObjectData CurrentObject = null;
        private static int CurrentObjectIndex = 0;

        private static HitObjectData FirstObject = null;
        private static int FirstObjectIndex = 0;

        private static List<Color> Colours = SkinIniProperties.GetComboColours();

        private static List<HitObjectData> HitObjects
        {
            get
            {
                return MainWindow.map.HitObjects;
            }
        }
        
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
            SpawnObject(CurrentObject, CurrentObjectIndex, true);
        }
        
        public static void UpdateHitObjectBackwards()
        {
            GetCurrentObject(ref LastObject, LastObjectIndex);
            SpawnObject(LastObject, LastObjectIndex, false, true);
        }
        
        public static void UpdateHitObjectForward()
        {
            GetCurrentObject(ref FirstObject, FirstObjectIndex);
            SpawnObject(FirstObject, FirstObjectIndex);
        }
        
        public static void UpdateHitObjectAfterSeek(long time, double direction)
        {
            int idx = -1;
            if (direction >= 0) //forward
            {
                double arTime = OsuMath.GetApproachRateTiming();
                for (int i = 0; i <= HitObjects.Count; i++)
                {
                    if (i == HitObjects.Count)
                    {
                        idx = i;
                        break;
                    }

                    double objectEndTime = HitObjectManager.GetEndTime(HitObjects[i]);
                    if (objectEndTime >= time + arTime)
                    {
                        idx = i;
                        break;
                    }
                }
        
                if (idx >= 0)
                {
                    FirstObjectIndex = idx;
                    CurrentObjectIndex = idx;
                    UpdateHitObjectForward();
                }
            }
            else //back
            {
                for (int i = 0; i < HitObjects.Count + 1; i++)
                {
                    if (i == HitObjects.Count)
                    {
                        idx = i;
                        break;
                    }

                    HitObjectData obj = HitObjects[i];
        
                    if (obj is SliderData && HitObjectManager.GetEndTime(obj) > time)
                    {
                        idx = i;
                        break;
                    }
        
                    // god im fucking stupid this should be always on top... let this be reminder to stop being stupid
                    if (obj.IsHit == true && obj.HitAt > time)
                    {
                        idx = i;
                        break;
                    }
        
                    if (obj.IsHit == false && obj.SpawnTime > time)
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
        
        public static void CatchUpToAliveHitObjects(long time)
        {
            // first object
            UpdateHitObjectAfterSeek(time, -1);
        
            // last object
            UpdateHitObjectAfterSeek(time, 1);
        
            // fill in middle objects (needs first and last object index up to date hence last in execution
            UpdateHitObjectsBetweenFirstAndLast();
        }
        
        private static void UpdateHitObjectsBetweenFirstAndLast()
        {
            CurrentObjectIndex = LastObjectIndex + 1;
            while (CurrentObjectIndex <= FirstObjectIndex)
            {
                GetCurrentObject(ref CurrentObject, CurrentObjectIndex);
                SpawnObject(CurrentObject, CurrentObjectIndex);
                CurrentObjectIndex++;
            }
            // small correction for sometimes not spawning last object? at least i think thats whats happening
            CurrentObjectIndex--;
        }
        
        private static void GetCurrentObject(ref HitObjectData hitObject, int index)
        {
            if (index >= HitObjects.Count)
            {
                return;
            }
        
            if (hitObject != HitObjects[index])
            {
                hitObject = HitObjects[index];
            }
        }
        
        private static void SpawnObject(HitObjectData hitObjectData, int index, bool updateCurrentIndex = false, bool reversed = false)
        {
            // for the love of god please never delete this coz its so useful to just fix incorrect miss or anything stuff
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS2 = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS3 = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS4 = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS5 = new List<HitObjectData>();
            //foreach (var a in MainWindow.map.HitObjects)
            //{
            //    if (a.Judgement == -727)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS.Add(a);
            //    }
            //
            //    if (a.Judgement == 0)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS2.Add(a);
            //    }
            //
            //    if (a.Judgement == 50)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS3.Add(a);
            //    }
            //
            //    if (a.Judgement == 100)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS4.Add(a);
            //    }
            //
            //    if (a.Judgement == 300)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS5.Add(a);
            //    }
            //}

            if (hitObjectData != null && CurrentObjectIndex <= HitObjects.Count - 1 
            &&  GamePlayClock.TimeElapsed > hitObjectData.SpawnTime - OsuMath.GetApproachRateTiming())
            {
                if (!HitObjectManager.GetAliveDataObjects().Contains(hitObjectData))
                {
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;
                    if (hitObjectData is CircleData)
                    {
                        HitCircle circle = HitCircle.CreateCircle((CircleData)hitObjectData, diameter, hitObjectData.ComboNumber, index, Colours.IndexOf(hitObjectData.RGBValue));
                        InitializeObject(circle);
                    }
                    else if (hitObjectData is SliderData)
                    {
                        Slider slider = Slider.CreateSlider((SliderData)hitObjectData, diameter, hitObjectData.ComboNumber, index, Colours.IndexOf(hitObjectData.RGBValue));
                        if (GamePlayClock.TimeElapsed > slider.SpawnTime + OsuMath.GetOverallDifficultyHitWindow50()
                        ||  reversed == true)
                        {
                            HitObjectManager.RemoveSliderHead(slider.Children[1] as Canvas);
                        }

                        InitializeObject(slider);
                    }
                    else
                    {
                        Spinner spinner = Spinner.CreateSpinner((SpinnerData)hitObjectData, diameter, index);
                        InitializeObject(spinner);
                    }
                }
        
                if (updateCurrentIndex == true)
                {
                    CurrentObjectIndex++;
                }
        
                void InitializeObject(HitObject hitObject)
                {
                    Window.playfieldCanva.Children.Add(hitObject);
                    HitObjectManager.GetAliveHitObjects().Add(hitObject);
                    HitObjectManager.GetAliveDataObjects().Add(hitObjectData);

                    hitObject.Visibility = Visibility.Visible;

                    if (MainWindow.IsReplayPreloading == true)
                    {
                        return;
                    }

                    HitObjectAnimations.Start(hitObject);
                    if (GamePlayClock.IsPaused())
                    {
                        HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
                    }
                }
            }
        }
    }
}
