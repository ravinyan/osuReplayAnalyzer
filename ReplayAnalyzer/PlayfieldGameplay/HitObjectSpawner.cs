using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.HitObjects.Osu;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using Slider = ReplayAnalyzer.HitObjects.Osu.Slider;

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
            SpawnObject(LastObject, LastObjectIndex, false);
        }
        
        public static void UpdateHitObjectForward()
        {
            GetCurrentObject(ref FirstObject, FirstObjectIndex);
            SpawnObject(FirstObject, FirstObjectIndex);
        }
        
        public static void UpdateHitObjectAfterSeek(long time, double direction)
        {
            int idx = -1;
            // mania, taiko and catch have very simple rules for seeking... unlike osu... sigh
            if (MainWindow.replay.GameMode != OsuFileParsers.Classes.Replay.GameMode.Osu)
            {// the animation loop will take care of updating all positions based on current time and object spawn time
                for (int i = 0; i <= HitObjects.Count; i++)
                {
                    if (HitObjects[i].SpawnTime >= time - HitObjectAnimations.ScrollSpeed)
                    {
                        idx = i;
                        break;
                    }
                }

                FirstObjectIndex = idx;
                LastObjectIndex = idx;
                CurrentObjectIndex = idx;
            }
            else
            {
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

                        if ((obj is SliderData || obj is SpinnerData) && HitObjectManager.GetEndTime(obj) > time)
                        {
                            idx = i;
                            break;
                        }

                        // god im fucking stupid this should be always on top... let this be reminder to stop being stupid
                        if (obj.Judgement.Judgement > 0 && obj.Judgement.SpawnTime > time)
                        {
                            idx = i;
                            break;
                        }

                        if (obj.Judgement.Judgement <= 0 && obj.Judgement.SpawnTime > time)
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
        
        private static void SpawnObject(HitObjectData hitObjectData, int index, bool updateCurrentIndex = false)
        {
            // for the love of god please never delete this coz its so useful to just fix incorrect miss or anything stuff
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS2 = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS3 = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS4 = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS5 = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS6 = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS7 = new List<HitObjectData>();
            //List<HitObjectData> HOWMANYTIMESWILLIDOTHIS8 = new List<HitObjectData>();
            //List<DataHitJudgement> aa = new List<DataHitJudgement>();
            //foreach (var a in MainWindow.map.HitObjects)
            //{
            //    if (a.Judgement.Judgement == -727)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS.Add(a);
            //    }
            //
            //    if (a.Judgement.Judgement == 0)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS2.Add(a);
            //        aa.Add(a.Judgement);
            //    }
            //
            //    if (a.Judgement.Judgement == 50)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS3.Add(a);
            //    }
            //
            //    if (a.Judgement.Judgement == 100)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS4.Add(a);
            //    }
            //
            //    if (a.Judgement.Judgement == 200)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS8.Add(a);
            //    }
            //
            //    if (a.Judgement.Judgement == 300)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS5.Add(a);
            //    }
            //
            //    if (a.Judgement.Judgement == 320)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS7.Add(a);
            //    }
            //
            //    if (a is SliderData s && s.AllTicksHit == false)
            //    {
            //        HOWMANYTIMESWILLIDOTHIS6.Add(a);
            //    }
            //}

            switch (MainWindow.replay.GameMode)
            {
                case OsuFileParsers.Classes.Replay.GameMode.Osu:
                    SpawnOsuHitObject(hitObjectData, index, updateCurrentIndex);
                    break;
                case OsuFileParsers.Classes.Replay.GameMode.OsuMania:
                    SpawnManiaHitObject(hitObjectData, updateCurrentIndex);
                    break;
                case OsuFileParsers.Classes.Replay.GameMode.OsuTaiko: 
                    break;
                case OsuFileParsers.Classes.Replay.GameMode.OsuCatch:
                    break;
                default:
                    throw new Exception("Wrong game mode... somehow");
            }
        }

        private static void SpawnManiaHitObject(HitObjectData hitObjectData, bool updateCurrentIndex)
        {
            // experimenting but while loop is a must here for chords to spawn correctly
            while (CurrentObjectIndex <= HitObjects.Count - 1 && hitObjectData != null
            &&     GamePlayClock.TimeElapsed > hitObjectData.SpawnTime - HitObjectAnimations.ScrollSpeed)
            {
                if (!HitObjectManager.GetAliveDataObjects().Contains(hitObjectData))
                {
                    if (hitObjectData is ManiaNoteData)
                    {
                        ManiaNote note = ManiaNote.CreateManiaNote((ManiaNoteData)hitObjectData, CurrentObjectIndex);
                        ManiaPlayfield.Playfield.Children.Add(note);
                        HitObjectManager.GetAliveHitObjects().Add(note);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                    else if (hitObjectData is ManiaLongNoteData)
                    {
                        ManiaLongNote note = ManiaLongNote.CreateManiaNote((ManiaLongNoteData)hitObjectData, CurrentObjectIndex);
                        ManiaPlayfield.Playfield.Children.Add(note);
                        HitObjectManager.GetAliveHitObjects().Add(note);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                }

                if (updateCurrentIndex == true)
                {
                    CurrentObjectIndex++;
                }

                // this code is for while loop to correctly update chords
                if (CurrentObjectIndex < HitObjects.Count)
                {
                    if (hitObjectData != HitObjects[CurrentObjectIndex])
                    {
                        hitObjectData = HitObjects[CurrentObjectIndex];
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static void SpawnOsuHitObject(HitObjectData hitObjectData, int index, bool updateCurrentIndex)
        {
            if (hitObjectData != null && CurrentObjectIndex <= HitObjects.Count - 1 
            &&  GamePlayClock.TimeElapsed > hitObjectData.SpawnTime - OsuMath.GetApproachRateTiming())
            {
                if (!HitObjectManager.GetAliveDataObjects().Contains(hitObjectData))
                {
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;
                    int comboColourIndex = SkinIniProperties.GetComboColours().IndexOf(hitObjectData.RGBValue);
                    int comboNumber = hitObjectData.ComboNumber;
                    if (hitObjectData is CircleData)
                    {
                        HitCircle circle = HitCircle.CreateCircle((CircleData)hitObjectData, diameter, comboNumber, index, comboColourIndex);
                        InitializeObject(circle);
                    }
                    else if (hitObjectData is SliderData)
                    {
                        Slider slider = Slider.CreateSlider((SliderData)hitObjectData, diameter, comboNumber, index, comboColourIndex);
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
                    OsuPlayfield.Playfield.Children.Add(hitObject);
                    HitObjectManager.GetAliveHitObjects().Add(hitObject);
                    HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
            
                    hitObject.Visibility = Visibility.Visible;
                    hitObject.Opacity = 0;
                }
            }
        }
    }
}
