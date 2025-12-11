using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.Skins;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.Objects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class HitObjectSpawner
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static OsuMath OsuMath = new OsuMath();

        private static HitObject LastObject = null;
        private static HitObjectData LastObject1 = null;
        private static int LastObjectIndex = 0;

        private static HitObject CurrentObject = null;
        private static HitObjectData CurrentObject1 = null;
        private static int CurrentObjectIndex = 0;

        private static HitObject FirstObject = null;
        private static HitObjectData FirstObject1 = null;
        private static int FirstObjectIndex = 0;

        private static Color ComboColour = Color.Transparent;
        private static List<Color> Colours = Colours = SkinIniProperties.GetComboColours();

        private static List<HitObjectData> HitObjects
        {
            get
            {
                return MainWindow.map.HitObjects;
            }
        }
        
        private static List<HitObjectData> AliveHitObjectsData = new List<HitObjectData>();
        
        public static void UpdateHitObjects1()
        {
            GetCurrentObject1(ref CurrentObject1, CurrentObjectIndex);
            SpawnObject1(CurrentObject1, CurrentObjectIndex, true);
        }
        
        public static void UpdateHitObjectBackwards1()
        {
            GetCurrentObject1(ref LastObject1, LastObjectIndex);
            SpawnObject1(LastObject1, LastObjectIndex, false, true);
        }
        
        public static void UpdateHitObjectForward1()
        {
            GetCurrentObject1(ref FirstObject1, FirstObjectIndex);
            SpawnObject1(FirstObject1, FirstObjectIndex);
        }
        
        public static void FindObjectIndexAfterSeek1(long time, double direction)
        {
            int idx = -1;
            if (direction >= 0) //forward
            {
                double arTime = OsuMath.GetApproachRateTiming();
                for (int i = 0; i < HitObjects.Count; i++)
                {
                    double objectEndTime = HitObjectManager.GetEndTime1(HitObjects[i]);
                    if (objectEndTime >= time + arTime)
                    {
                        idx = i;
                        break;
                    }
        
                    if (i == HitObjects.Count - 1)
                    {
                        idx = i;
                        break;
                    }
                }
        
                if (idx >= 0)
                {
                    FirstObjectIndex = idx;
                    CurrentObjectIndex = idx;
                    UpdateHitObjectForward1();
                }
            }
            else //back
            {
                for (int i = 0; i < HitObjects.Count; i++)
                {
                    HitObjectData obj = HitObjects[i];
        
                    if (obj is SliderData && HitObjectManager.GetEndTime1(obj) > time)
                    {
                        idx = i;
                        if ((obj.IsHit == true && obj.HitAt > time) || (obj.IsHit == false && obj.SpawnTime > time))
                        {
                            //Canvas sliderHead = obj.Children[1] as Canvas;
                            //HitObjectManager.ShowSliderHead(sliderHead);
                        }
        
                        break;
                    }
        
                    // god im fucking stupid this should be always on top... let this be reminder to stop being stupid
                    if (obj.IsHit == true && obj.HitAt > time )//&& obj.Visibility != Visibility.Visible)
                    {
                        idx = i;
                        break;
                    }
        
                    if (obj.IsHit == false && HitObjectManager.GetEndTime1(obj) > time )//&& obj.Visibility != Visibility.Visible)
                    {
                        idx = i;
                        break;
                    }
        
                    if (i == HitObjects.Count - 1)
                    {
                        idx = i;
                        break;
                    }
                }
        
                if (idx != -1)
                {
                    LastObjectIndex = idx;
                    CurrentObjectIndex = idx + HitObjectManager.GetAliveHitObjects().Count;
                    UpdateHitObjectBackwards1();
                }
            }
        }
        
        public static void CatchUpToAliveHitObjects1(long time)
        {
            // first object
            FindObjectIndexAfterSeek1(time, -1);
        
            // last object
            FindObjectIndexAfterSeek1(time, 1);
        
            // fill in middle objects (needs first and last object index up to date hence last in execution
            UpdateHitObjectsBetweenFirstAndLast1();
        }
        
        private static void UpdateHitObjectsBetweenFirstAndLast1()
        {
            CurrentObjectIndex = LastObjectIndex + 1;
            while (CurrentObjectIndex <= FirstObjectIndex)
            {
                GetCurrentObject1(ref CurrentObject1, CurrentObjectIndex);
                SpawnObject1(CurrentObject1, CurrentObjectIndex);
                CurrentObjectIndex++;
            }
            // small correction for sometimes not spawning last object? at least i think thats whats happening
            CurrentObjectIndex--;
        }
        
        private static void GetCurrentObject1(ref HitObjectData hitObject, int index)
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
        
        private static void SpawnObject1(HitObjectData hitObject, int index, bool updateCurrentIndex = false, bool reversed = false)
        {
            //List<HitObjectData> dada = new List<HitObjectData>();
            //
            //foreach(var d in MainWindow.map.HitObjects)
            //{
            //    if (d.IsHit == true)
            //    {
            //        dada.Add(d);
            //    }
            //}
        
            if (GamePlayClock.TimeElapsed > hitObject.SpawnTime - OsuMath.GetApproachRateTiming()
            && CurrentObjectIndex < HitObjects.Count)
            {
                if (!GetAliveHitObjectsData().Contains(hitObject))
                {
                    AliveHitObjectsData.Add(hitObject);
                    int objectIndex = reversed == false ? index : index + 1;
                    if (index == 0 || HitObjects[objectIndex].Type.HasFlag(ObjectType.StartNewCombo))
                    {
                        if (ComboColour != Color.Transparent)
                        {
                            ComboColour = UpdateComboColour(ComboColour, Colours);
                        }
                        else
                        {
                            ComboColour = Colours[0];
                        }
                    }
        
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;
                    double scale = MainWindow.OsuPlayfieldObjectScale;
        
                    if (hitObject is CircleData)
                    {
                        HitCircle circle = HitCircle.CreateCircle((CircleData)hitObject, diameter, hitObject.ComboNumber, index, ComboColour);
                        InitializeObject(circle);
                    }
                    else if (hitObject is SliderData)
                    {
                        Slider slider = Slider.CreateSlider((SliderData)hitObject, diameter, hitObject.ComboNumber, index, ComboColour);
                        InitializeObject(slider);
                    }
                    else
                    {
                        Spinner spinner = Spinner.CreateSpinner((SpinnerData)hitObject, diameter, index);
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
        
                    hitObject.Visibility = Visibility.Visible;
        
                    HitObjectAnimations.Start(hitObject);
                    if (GamePlayClock.IsPaused())
                    {
                        HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
                    }
                }
            }
        }
        
        private static Color UpdateComboColour(Color comboColour, List<Color> colours)
        {
            int currentColourIndex = colours.IndexOf(comboColour);
        
            if (currentColourIndex + 1 > colours.Count - 1)
            {
                currentColourIndex = -1;
            }
        
            currentColourIndex++;
            return colours[currentColourIndex];
        }
        
        public static List<HitObjectData> GetAliveHitObjectsData()
        {
            return AliveHitObjectsData;
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

        public static void FindObjectIndexAfterSeek(long time, double direction)
        {
            int idx = -1;
            if (direction >= 0) //forward
            {
                double arTime = OsuMath.GetApproachRateTiming();
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
                    CurrentObjectIndex = idx;
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
                        if ((obj.IsHit == true && obj.HitAt > time) || (obj.IsHit == false && obj.SpawnTime > time))
                        {
                            Canvas sliderHead = obj.Children[1] as Canvas;
                            HitObjectManager.ShowSliderHead(sliderHead);
                        }

                        break;
                    }

                    // god im fucking stupid this should be always on top... let this be reminder to stop being stupid
                    if (obj.IsHit == true && obj.HitAt > time && obj.Visibility != Visibility.Visible)
                    {
                        idx = i;
                        break;
                    }
        
                    if (obj.IsHit == false && HitObjectManager.GetEndTime(obj) > time && obj.Visibility != Visibility.Visible)
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

        public static void CatchUpToAliveHitObjects(long time)
        {
            // first object
            FindObjectIndexAfterSeek(time, -1);

            // last object
            FindObjectIndexAfterSeek(time, 1);

            // fill in middle objects (needs first and last object index up to date hence last in execution
            UpdateHitObjectsBetweenFirstAndLast();
        }

        private static void UpdateHitObjectsBetweenFirstAndLast()
        {
            CurrentObjectIndex = LastObjectIndex + 1;
            while (CurrentObjectIndex <= FirstObjectIndex)
            {
                GetCurrentObject(ref CurrentObject, CurrentObjectIndex);
                SpawnObject(CurrentObject);
                CurrentObjectIndex++;
            }
            // small correction for sometimes not spawning last object? at least i think thats whats happening
            CurrentObjectIndex--;
        }

        private static void SpawnObject(HitObject hitObject, bool updateCurrentIndex = false)
        {
            if (GamePlayClock.TimeElapsed > hitObject.SpawnTime - OsuMath.GetApproachRateTiming()
            &&  CurrentObjectIndex < OsuBeatmap.HitObjectDictByIndex.Count)
            {
                if (!HitObjectManager.GetAliveHitObjects().Contains(hitObject))
                {
                    Window.playfieldCanva.Children.Add(hitObject);
                    HitObjectManager.GetAliveHitObjects().Add(hitObject);

                    hitObject.Visibility = Visibility.Visible;

                    if (hitObject is HitCircle)
                    {
                        HitObjectAnimations.ApplyHitCircleAnimations(hitObject as HitCircle);
                    }
                    else if (hitObject is Slider)
                    {
                        HitObjectAnimations.ApplySliderAnimations(hitObject as Slider);
                    }
                    else
                    {

                        HitObjectAnimations.ApplySpinnerAnimations(hitObject as Spinner);
                    }

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
