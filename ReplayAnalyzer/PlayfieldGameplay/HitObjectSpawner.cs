using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.HitObjects.Osu;
using ReplayAnalyzer.HitObjects.Taiko;
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
            if (MainWindow.replay.GameMode == OsuFileParsers.Classes.Replay.GameMode.OsuCatch)
            {// the animation loop will take care of updating all positions based on current time and object spawn time
                if (direction >= 0)
                {
                    for (int i = 0; i < HitObjects.Count; i++)
                    {// number 50 i took out of my ass and somehow it made taiko forward seeking correct yaaay
                        if (HitObjects[i].SpawnTime > time)
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
                    for (int i = 0; i < HitObjects.Count; i++)
                    {
                        if (HitObjectManager.GetEndTime(HitObjects[i]) >= time)
                        {
                            idx = i;
                            break;
                        }
                    }

                    FirstObjectIndex = idx;
                    LastObjectIndex = idx;
                    CurrentObjectIndex = idx;
                }
            }
            if (MainWindow.replay.GameMode != OsuFileParsers.Classes.Replay.GameMode.Osu)
            {// the animation loop will take care of updating all positions based on current time and object spawn time
                if (direction >= 0)
                {
                    for (int i = 0; i < HitObjects.Count; i++)
                    {// number 50 i took out of my ass and somehow it made taiko forward seeking correct yaaay
                        if (HitObjects[i].Judgement.SpawnTime > time)
                        {
                            while (i - 1 >= 0 && HitObjects[i - 1].Judgement.SpawnTime == HitObjects[i].Judgement.SpawnTime)
                            {
                                i--;
                            }

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
                    for (int i = 0; i < HitObjects.Count; i++)
                    {
                        if (HitObjectManager.GetEndTime(HitObjects[i]) >= time)
                        {
                            // for chords to spawn correctly
                            while (i - 1 >= 0 && HitObjectManager.GetEndTime(HitObjects[i - 1]) == HitObjectManager.GetEndTime(HitObjects[i]))
                            {
                                i--;
                            }
                            
                            idx = i;
                            break;
                        }
                    }

                    FirstObjectIndex = idx;
                    LastObjectIndex = idx;
                    CurrentObjectIndex = idx;
                }
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

                        if ((obj is OsuSliderData || obj is OsuSpinnerData) && HitObjectManager.GetEndTime(obj) > time)
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
            /*
            // for the love of god please never delete this coz its so useful to just fix incorrect miss or anything stuff
            List<HitObjectData> HOWMANYTIMESWILLIDOTHIS = new List<HitObjectData>();
            List<HitObjectData> HOWMANYTIMESWILLIDOTHIS2 = new List<HitObjectData>();
            List<HitObjectData> HOWMANYTIMESWILLIDOTHIS3 = new List<HitObjectData>();
            List<HitObjectData> HOWMANYTIMESWILLIDOTHIS4 = new List<HitObjectData>();
            List<HitObjectData> HOWMANYTIMESWILLIDOTHIS5 = new List<HitObjectData>();
            List<HitObjectData> HOWMANYTIMESWILLIDOTHIS6 = new List<HitObjectData>();
            List<HitObjectData> HOWMANYTIMESWILLIDOTHIS7 = new List<HitObjectData>();
            List<HitObjectData> HOWMANYTIMESWILLIDOTHIS8 = new List<HitObjectData>();
            List<DataHitJudgement> aa = new List<DataHitJudgement>();
            foreach (var a in MainWindow.map.HitObjects)
            {
                if (a.Judgement.Judgement == -727)
                {
                    HOWMANYTIMESWILLIDOTHIS.Add(a);
                }
                if (a is ManiaLongNoteData ln && ln.TailJudgement.Judgement == -727)
                {
                    HOWMANYTIMESWILLIDOTHIS.Add(a);
                }
            
                if (a.Judgement.Judgement == 0)
                {
                    HOWMANYTIMESWILLIDOTHIS2.Add(a);
                    aa.Add(a.Judgement);
                }
                if (a is ManiaLongNoteData ln1 && ln1.TailJudgement.Judgement == 0)
                {
                    HOWMANYTIMESWILLIDOTHIS2.Add(a);
                }
            
                if (a.Judgement.Judgement == 50)
                {
                    HOWMANYTIMESWILLIDOTHIS3.Add(a);
                }
                if (a is ManiaLongNoteData ln2 && ln2.TailJudgement.Judgement == 50)
                {
                    HOWMANYTIMESWILLIDOTHIS3.Add(a);
                }
            
                if (a.Judgement.Judgement == 100)
                {
                    HOWMANYTIMESWILLIDOTHIS4.Add(a);
                }
                if (a is ManiaLongNoteData ln3 && ln3.TailJudgement.Judgement == 100)
                {
                    HOWMANYTIMESWILLIDOTHIS4.Add(a);
                }
            
                if (a.Judgement.Judgement == 200)
                {
                    HOWMANYTIMESWILLIDOTHIS8.Add(a);
                }
                if (a is ManiaLongNoteData ln4 && ln4.TailJudgement.Judgement == 200)
                {
                    HOWMANYTIMESWILLIDOTHIS8.Add(a);
                }
            
                if (a.Judgement.Judgement == 300)
                {
                    HOWMANYTIMESWILLIDOTHIS5.Add(a);
                }
                if (a is ManiaLongNoteData ln5 && ln5.TailJudgement.Judgement == 300)
                {
                    HOWMANYTIMESWILLIDOTHIS5.Add(a);
                }
            
                if (a.Judgement.Judgement == 320)
                {
                    HOWMANYTIMESWILLIDOTHIS7.Add(a);
                }
                if (a is ManiaLongNoteData ln6 && ln6.TailJudgement.Judgement == 320)
                {
                    HOWMANYTIMESWILLIDOTHIS7.Add(a);
                }
            
                if (a is SliderData s && s.AllTicksHit == false)
                {
                    HOWMANYTIMESWILLIDOTHIS6.Add(a);
                }
            }
            */

            switch (MainWindow.replay.GameMode)
            {
                case OsuFileParsers.Classes.Replay.GameMode.Osu:
                    SpawnOsuHitObject(hitObjectData, index, updateCurrentIndex);
                    break;
                case OsuFileParsers.Classes.Replay.GameMode.OsuMania:
                    SpawnManiaHitObject(hitObjectData);
                    break;
                case OsuFileParsers.Classes.Replay.GameMode.OsuTaiko:
                    SpawnTaikoHitObject(hitObjectData);
                    break;
                case OsuFileParsers.Classes.Replay.GameMode.OsuCatch:
                    SpawnCatchHitObject(hitObjectData);
                    break;
                default:
                    throw new Exception("Wrong game mode... somehow");
            }
        }

        private static void SpawnCatchHitObject(HitObjectData hitObjectData)
        {
            // -100 so notes are spawned a bit above the visible playfield
            if (CurrentObjectIndex <= HitObjects.Count - 1 && hitObjectData != null
            &&  GamePlayClock.TimeElapsed > hitObjectData.SpawnTime - TaikoPlayfield.ScrollSpeed)
            {
                if (!HitObjectManager.GetAliveDataObjects().Contains(hitObjectData))
                {
                    if (hitObjectData is CatchFruitData)
                    {
                        CatchFruit circle = CatchFruit.Create((CatchFruitData)hitObjectData, CurrentObjectIndex);
                        CatchPlayfield.Playfield.Children.Add(circle);
                        HitObjectManager.GetAliveHitObjects().Add(circle);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                    else if (hitObjectData is CatchJuiceStreamData)
                    {
                        CatchJuiceStream drumRoll = CatchJuiceStream.Create((CatchJuiceStreamData)hitObjectData, CurrentObjectIndex);
                        CatchPlayfield.Playfield.Children.Add(drumRoll);
                        HitObjectManager.GetAliveHitObjects().Add(drumRoll);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                    else if (hitObjectData is CatchBananaShowerData)
                    {
                        CatchBananaShower spinner = CatchBananaShower.Create((CatchBananaShowerData)hitObjectData, CurrentObjectIndex);
                        CatchPlayfield.Playfield.Children.Add(spinner);
                        HitObjectManager.GetAliveHitObjects().Add(spinner);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                }

                CurrentObjectIndex++;
            }
        }

        private static void SpawnTaikoHitObject(HitObjectData hitObjectData)
        {
            // -100 so notes are spawned a bit above the visible playfield
            if (CurrentObjectIndex <= HitObjects.Count - 1 && hitObjectData != null
            &&  GamePlayClock.TimeElapsed > hitObjectData.SpawnTime - TaikoPlayfield.ScrollSpeed)
            {
                if (!HitObjectManager.GetAliveDataObjects().Contains(hitObjectData))
                {
                    if (hitObjectData is TaikoHitCircleData)
                    {
                        TaikoHitCircle circle = TaikoHitCircle.Create((TaikoHitCircleData)hitObjectData, CurrentObjectIndex);
                        TaikoPlayfield.Playfield.Children.Add(circle);
                        HitObjectManager.GetAliveHitObjects().Add(circle);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                    else if (hitObjectData is TaikoDrumRollData)
                    {
                        TaikoDrumRoll drumRoll = TaikoDrumRoll.Create((TaikoDrumRollData)hitObjectData, CurrentObjectIndex);
                        TaikoPlayfield.Playfield.Children.Add(drumRoll);
                        HitObjectManager.GetAliveHitObjects().Add(drumRoll);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                    else if (hitObjectData is TaikoSpinnerData)
                    {
                        TaikoSpinner spinner = TaikoSpinner.Create((TaikoSpinnerData)hitObjectData, CurrentObjectIndex);
                        TaikoPlayfield.Playfield.Children.Add(spinner);
                        HitObjectManager.GetAliveHitObjects().Add(spinner);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                }

                CurrentObjectIndex++;
            }
        }

        private static void SpawnManiaHitObject(HitObjectData hitObjectData)
        {
            // -100 so notes are spawned a bit above the visible playfield
            if (CurrentObjectIndex <= HitObjects.Count - 1 && hitObjectData != null
            &&  GamePlayClock.TimeElapsed > hitObjectData.SpawnTime - ManiaPlayfield.ScrollSpeed - 100)
            {
                if (!HitObjectManager.GetAliveDataObjects().Contains(hitObjectData))
                {
                    // find a nice way to not spawn notes from seeking... somehow but this while loop is kinda rude
                    if (hitObjectData is ManiaNoteData)
                    {
                        ManiaNote note = ManiaNote.Create((ManiaNoteData)hitObjectData, CurrentObjectIndex);
                        ManiaPlayfield.Playfield.Children.Add(note);
                        HitObjectManager.GetAliveHitObjects().Add(note);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                    else if (hitObjectData is ManiaLongNoteData)
                    {
                        ManiaLongNote note = ManiaLongNote.Create((ManiaLongNoteData)hitObjectData, CurrentObjectIndex);
                        ManiaPlayfield.Playfield.Children.Add(note);
                        HitObjectManager.GetAliveHitObjects().Add(note);
                        HitObjectManager.GetAliveDataObjects().Add(hitObjectData);
                    }
                }

                CurrentObjectIndex++;
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
                    if (hitObjectData is OsuCircleData)
                    {
                        HitCircle circle = HitCircle.Create((OsuCircleData)hitObjectData, diameter, comboNumber, index, comboColourIndex);
                        InitializeObject(circle);
                    }
                    else if (hitObjectData is OsuSliderData)
                    {
                        Slider slider = Slider.Create((OsuSliderData)hitObjectData, diameter, comboNumber, index, comboColourIndex);
                        InitializeObject(slider);
                    }
                    else
                    {
                        Spinner spinner = Spinner.Create((OsuSpinnerData)hitObjectData, diameter, index);
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
