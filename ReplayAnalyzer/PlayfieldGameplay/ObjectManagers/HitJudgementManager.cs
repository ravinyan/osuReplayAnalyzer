using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.Cursor;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers
{
    public class HitJudgementManager
    {
        private static List<HitJudgmentUI> AliveHitJudgements = new List<HitJudgmentUI>();

        public static void ResetFields()
        {
            AliveHitJudgements.Clear();
        }

        public static void ApplyCatchJudgement(Vector2 position, long hitTime, HitObjectJudgement judgement, HitObject hitObject = null!)
        {
            switch (judgement)
            {
                case HitObjectJudgement.Great:
                    break;
                case HitObjectJudgement.Ok:
                    AddHitJudgementToTimeline(judgement, hitTime);
                    break;
                case HitObjectJudgement.Miss:
                    ApplyHitJudgementValuesToHitObject(hitObject, judgement, hitTime);
                    AddHitJudgementToTimeline(judgement, hitTime);
                    break;
                default:
                    throw new Exception($"Judgement value doesnt exist: {judgement}");
            }
        }

        public static void ManiaApplyTailJudgement(ManiaLongNote note, Vector2 position, long hitTime, HitObjectJudgement judgement)
        {
            switch (judgement)
            {
                case HitObjectJudgement.Perfect:
                    ManiaApplyHitJudgementValuesToLongNoteTail(note, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Great:
                    ManiaApplyHitJudgementValuesToLongNoteTail(note, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Good:
                    ManiaApplyHitJudgementValuesToLongNoteTail(note, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Ok:
                    AddHitJudgementToTimeline(judgement, hitTime);
                    ManiaApplyHitJudgementValuesToLongNoteTail(note, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Meh:
                    AddHitJudgementToTimeline(judgement, hitTime);
                    ManiaApplyHitJudgementValuesToLongNoteTail(note, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Miss:
                    AddHitJudgementToTimeline(judgement, hitTime);
                    ManiaApplyHitJudgementValuesToLongNoteTail(note, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                default:
                    throw new Exception($"Judgement value doesnt exist: {judgement}");
            }
        }

        public static void ApplyJudgement(HitObject hitObject, Vector2 position, long hitTime, HitObjectJudgement judgement)
        {
            switch (judgement)
            {
                case HitObjectJudgement.Perfect:
                    ApplyHitJudgementValuesToHitObject(hitObject, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Great:
                    ApplyHitJudgementValuesToHitObject(hitObject, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Good:
                    AddHitJudgementToTimeline(HitObjectJudgement.Ok, hitTime); // this for now for visualization
                    ApplyHitJudgementValuesToHitObject(hitObject, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Ok:
                    AddHitJudgementToTimeline(judgement, hitTime);
                    ApplyHitJudgementValuesToHitObject(hitObject, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Meh:
                    AddHitJudgementToTimeline(judgement, hitTime);
                    ApplyHitJudgementValuesToHitObject(hitObject, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.Miss:
                    AddHitJudgementToTimeline(judgement, hitTime);
                    ApplyHitJudgementValuesToHitObject(hitObject, judgement, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);           
                    break;
                case HitObjectJudgement.SliderEndHit:
                    if (MainWindow.replay.IsLazer == false || ClassicMod.IsClassicEnabled == true)
                    {
                        ApplyStandardSliderEndJudgement((HitObjects.Osu.Slider)hitObject, position, hitTime);
                    }
                    else
                    {
                        ApplySliderEndJudgementToSlider((HitObjects.Osu.Slider)hitObject, judgement, hitTime);
                    }
                    break;
                case HitObjectJudgement.SliderTickMiss:
                    AddHitJudgementToTimeline(HitObjectJudgement.Miss, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.SliderEndMiss:
                    if (StrictTrackingMod.IsStrictTrackingEnabled == true)// thats how it works in osu lazer
                    {
                        AddHitJudgementToTimeline(HitObjectJudgement.Miss, hitTime);
                        ApplySliderEndJudgementToSlider((HitObjects.Osu.Slider)hitObject, judgement, hitTime);
                        SpawnHitJudgementVisual(HitObjectJudgement.SliderTickMiss, position, hitTime);
                    }
                    else
                    {
                        if (MainWindow.replay.IsLazer == false || ClassicMod.IsClassicEnabled == true)
                        {
                            ApplyStandardSliderEndJudgement((HitObjects.Osu.Slider)hitObject, position, hitTime);
                        }
                        else
                        {
                            SpawnHitJudgementVisual(judgement, position, hitTime);
                            ApplySliderEndJudgementToSlider((HitObjects.Osu.Slider)hitObject, judgement, hitTime);
                        }
                    }
                    break;
                default:
                    throw new Exception($"Judgement value doesnt exist: {judgement}");
            }
        }

        private static void AddHitJudgementToTimeline(HitObjectJudgement judgement, long hitTime)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return;
            }

            JudgementTimeline.CreateJudgementLine(judgement, hitTime);
        }

        private static void ApplySliderEndJudgementToSlider(HitObjects.Osu.Slider slider, HitObjectJudgement judgement, long hitTime)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return;
            }

            OsuSliderData sliderData = (OsuSliderData)HitObjectManager.TransformHitObjectToDataObject(slider);
            sliderData.SliderEndJudgement.Judgement = (int)judgement;
            sliderData.SliderEndJudgement.SpawnTime = hitTime;

            slider.SliderEndJudgement.Judgement = judgement;
            slider.SliderEndJudgement.SpawnTime = hitTime;
        }

        private static void ApplyStandardSliderEndJudgement(HitObjects.Osu.Slider s, Vector2 position, long hitTime)
        {
            OsuSliderData sd = (OsuSliderData)HitObjectManager.TransformHitObjectToDataObject(s);
            if (sd.MissedEventsCount == 0) // 100% slider completion from wiki
            {
                SpawnHitJudgementVisual(HitObjectJudgement.Great, position, hitTime);
                ApplySliderEndJudgementToSlider(s, HitObjectJudgement.Great, hitTime);
            }
            else if (sd.EventsCount / 2 >= sd.MissedEventsCount) // >=50% slider completion
            {
                SpawnHitJudgementVisual(HitObjectJudgement.Ok, position, hitTime);
                AddHitJudgementToTimeline(HitObjectJudgement.Ok, hitTime);
                ApplySliderEndJudgementToSlider(s, HitObjectJudgement.Ok, hitTime);
            }
            else if (sd.EventsCount - sd.MissedEventsCount >= 1) // at least 1 event is hit
            {
                SpawnHitJudgementVisual(HitObjectJudgement.Meh, position, hitTime);
                AddHitJudgementToTimeline(HitObjectJudgement.Meh, hitTime);
                ApplySliderEndJudgementToSlider(s, HitObjectJudgement.Meh, hitTime);
            }
            else if (sd.EventsCount == sd.MissedEventsCount) // 0% completion all missed
            {
                SpawnHitJudgementVisual(HitObjectJudgement.Miss, position, hitTime);
                AddHitJudgementToTimeline(HitObjectJudgement.Miss, hitTime);
                ApplySliderEndJudgementToSlider(s, HitObjectJudgement.Miss, hitTime);
            }
        }

        // if it has judgement applied then let it be saved and use only saved values
        private static void ApplyHitJudgementValuesToHitObject(HitObject hitObject, HitObjectJudgement judgement, long hitTime)
        {
            if (MainWindow.IsReplayPreloading == false && hitObject.Judgement.Judgement != HitObjectJudgement.None)
            {
                return;
            }

            HitObjectData hitObjectData = HitObjectManager.TransformHitObjectToDataObject(hitObject);
            hitObjectData.Judgement.Judgement = (int)judgement;
            hitObjectData.Judgement.SpawnTime = hitTime;

            hitObject.Judgement.Judgement = judgement;
            hitObject.Judgement.SpawnTime = hitTime;
        }

        private static void ManiaApplyHitJudgementValuesToLongNoteTail(ManiaLongNote note, HitObjectJudgement judgement, long hitTime)
        {
            if (MainWindow.IsReplayPreloading == false && note.TailJudgement.Judgement != HitObjectJudgement.None)
            {
                return;
            }

            note.TailJudgement.Judgement = judgement;
            note.TailJudgement.SpawnTime = hitTime;

            ManiaLongNoteData lnData = (ManiaLongNoteData)HitObjectManager.TransformHitObjectToDataObject(note);
            lnData.TailJudgement.Judgement = (int)judgement;
            lnData.TailJudgement.SpawnTime = hitTime;
        }

        private static void SpawnHitJudgementVisual(HitObjectJudgement judgement, Vector2 pos, long spawnTime)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                return;
            }

            HitJudgmentUI hitJudgement = null!;
            double diameter = 0;
            if (MainWindow.replay.GameMode == GameMode.Osu)
            {
                diameter = MainWindow.OsuPlayfieldObjectDiameter;
            }
            if (MainWindow.replay.GameMode == GameMode.OsuMania)
            {
                diameter = ManiaPlayfield.ColumnWidth;
            }
            else if (MainWindow.replay.GameMode == GameMode.OsuTaiko)
            {
                diameter = 120;
            }

            JudgementCounter.Increment(judgement);
            switch (judgement)
            {
                case HitObjectJudgement.Perfect:
                    hitJudgement = Get320(diameter);
                    break;
                case HitObjectJudgement.Great:
                    hitJudgement = Get300(diameter);
                    break;
                case HitObjectJudgement.Good:
                    hitJudgement = Get200(diameter);
                    break;
                case HitObjectJudgement.Ok:
                    hitJudgement = Get100(diameter);
                    break;
                case HitObjectJudgement.Meh:
                    hitJudgement = Get50(diameter);
                    break;
                case HitObjectJudgement.Miss: // miss
                    hitJudgement = GetMiss(diameter);
                    break;
                case HitObjectJudgement.SliderTickMiss: // slider tick
                    hitJudgement = GetSliderTickMiss(diameter * 0.2); // need to reduce image size
                    break;
                case HitObjectJudgement.SliderEndMiss: // slider end
                    hitJudgement = GetSliderEndMiss(diameter * 0.2); // need to reduce image size
                    break;
            }

            hitJudgement.SpawnTime = spawnTime;
            if (MainWindow.replay.GameMode == GameMode.Osu)
            {
                hitJudgement.EndTime = spawnTime + HitMarkerData.ALIVE_TIME;
            }
            else// this doesnt count catch since catch doesnt have judgements, just mania and taiko
            {// why not use ALIVE_TIME if object will die if another one spawns in its place
                hitJudgement.EndTime = spawnTime + HitMarkerData.ALIVE_TIME;

                for (int i = 0; i < AliveHitJudgements.Count; i++)
                {
                    if (Canvas.GetLeft(AliveHitJudgements[i]) == pos.X
                    &&  Canvas.GetTop(AliveHitJudgements[i])  == pos.Y)
                    {
                        PlayfieldManager.GetActivePlayfield().Children.Remove(AliveHitJudgements[i]);
                        AliveHitJudgements.RemoveAt(i);
                    }
                }
            }

            AliveHitJudgements.Add(hitJudgement);
            PlayfieldManager.GetActivePlayfield().Children.Add(hitJudgement);

            Canvas.SetLeft(hitJudgement, pos.X);
            Canvas.SetTop(hitJudgement, pos.Y);
            Panel.SetZIndex(hitJudgement, 2);
        }

        public static void HandleAliveHitJudgements()
        {
            for (int i = 0; i < AliveHitJudgements.Count; i++)
            {
                long frameTime = PlayfieldManager.GetElapsedFrameTime();
                HitJudgmentUI hitJudgment = AliveHitJudgements[i];
                if (frameTime > hitJudgment.EndTime || frameTime < hitJudgment.SpawnTime)
                {
                    AliveHitJudgements.Remove(hitJudgment);
                    PlayfieldManager.GetActivePlayfield().Children.Remove(hitJudgment);
                }
            }
        }

        private static HitJudgmentUI Get320(double diameter)
        {
            return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.ManiaHit320), diameter, diameter);
        }

        private static HitJudgmentUI Get300(double diameter)
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit300), diameter, diameter);
                case GameMode.OsuMania:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.ManiaHit300), diameter, diameter);
                case GameMode.OsuTaiko:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.TaikoHit300), diameter, diameter);
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        private static HitJudgmentUI Get200(double diameter)
        {
            return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.ManiaHit200), diameter, diameter);
        }

        private static HitJudgmentUI Get100(double diameter)
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit100), diameter, diameter);
                case GameMode.OsuMania:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.ManiaHit100), diameter, diameter);
                case GameMode.OsuTaiko:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.TaikoHit100), diameter, diameter);
                case GameMode.OsuCatch:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit100), diameter, diameter);
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        private static HitJudgmentUI Get50(double diameter)
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit50), diameter, diameter);
                case GameMode.OsuMania:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.ManiaHit50), diameter, diameter);
                default:
                    throw new Exception("WRONG GAME MODE");
            }
            
        }

        private static HitJudgmentUI GetMiss(double diameter)
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit0), diameter, diameter);
                case GameMode.OsuMania:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.ManiaHit0), diameter, diameter);
                case GameMode.OsuTaiko:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.TaikoHit0), diameter, diameter);
                case GameMode.OsuCatch:
                    return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit0), diameter, diameter);
                default:
                    throw new Exception("WRONG GAME MODE");
            }
        }

        private static HitJudgmentUI GetSliderTickMiss(double diameter)
        {
            // increment tick misses? maybe in the future
            return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.SliderTickMiss), diameter, diameter);
        }

        private static HitJudgmentUI GetSliderEndMiss(double diameter)
        {
            // increment slider end misses? also maybe in the future
            return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.SliderEndMiss), diameter, diameter);
        }

        // separating and putting this here coz it is and will not needed anywhere else ever
        private class HitJudgmentUI : Image
        {
            public long SpawnTime { get; set; }
            public long EndTime { get; set; }

            public HitJudgmentUI(BitmapSource image, double width, double height)
            {
                Source = image;
                Width = width;
                Height = height;
            }
        }
    }

    public enum HitObjectJudgement
    {
        Perfect = 320,          // mania
        Great = 300,            // osu, mania, taiko, catch
        Good = 200,             // mania
        Ok = 100,               // osu, mania, taiko
        Meh = 50,               // osu, mania
        Miss = 0,               // osu, mania, taiko, catch
        SliderEndHit = 150,     // osu
        SliderTickMiss = -1,    // osu
        SliderEndMiss = -2,     // osu
        None = -727,            // default value
    }

    public class HitJudgement
    {
        public HitObjectJudgement Judgement { get; set; }
        public long SpawnTime { get; set; }

        public HitJudgement(HitObjectJudgement judgement, long spawnTime)
        {
            Judgement = judgement;
            SpawnTime = spawnTime;
        }
    }
}
