using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.AnalyzerTools.Cursor;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers
{
    public class HitJudgementManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static List<HitJudgmentUI> AliveHitJudgements = new List<HitJudgmentUI>();

        public static void ResetFields()
        {
            AliveHitJudgements.Clear();
        }

        public static void ApplyJudgement(HitObject hitObject, Vector2 position, long hitTime, HitObjectJudgement judgement)
        {
            switch (judgement)
            {
                case HitObjectJudgement.Max:
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
                    ApplySliderEndJudgementToSlider((HitObjects.Slider)hitObject, judgement, hitTime);
                    break;
                case HitObjectJudgement.SliderTickMiss:
                    AddHitJudgementToTimeline(HitObjectJudgement.Miss, hitTime);
                    SpawnHitJudgementVisual(judgement, position, hitTime);
                    break;
                case HitObjectJudgement.SliderEndMiss:
                    if (StrictTrackingMod.IsStrictTrackingEnabled == true)// thats how it works in osu lazer
                    {
                        AddHitJudgementToTimeline(HitObjectJudgement.Miss, hitTime);
                        ApplySliderEndJudgementToSlider((HitObjects.Slider)hitObject, judgement, hitTime);
                        SpawnHitJudgementVisual(HitObjectJudgement.SliderTickMiss, position, hitTime);
                    }
                    else
                    {
                        ApplySliderEndJudgementToSlider((HitObjects.Slider)hitObject, judgement, hitTime);
                        SpawnHitJudgementVisual(judgement, position, hitTime);
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

        private static void ApplySliderEndJudgementToSlider(HitObjects.Slider slider, HitObjectJudgement judgement, long hitTime)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return;
            }

            SliderData sliderData = (SliderData)HitObjectManager.TransformHitObjectToDataObject(slider);
            sliderData.SliderEndJudgement.Judgement = (int)judgement;
            sliderData.SliderEndJudgement.SpawnTime = hitTime;

            slider.SliderEndJudgement.Judgement = judgement;
            slider.SliderEndJudgement.SpawnTime = hitTime;
        }

        // if it has judgement applied then let it be saved and use only saved values
        private static void ApplyHitJudgementValuesToHitObject(HitObject hitObject, HitObjectJudgement judgement, long hitTime)
        {
            if (MainWindow.IsReplayPreloading == false || hitObject.Judgement.Judgement != HitObjectJudgement.None)
            {
                return;
            }

            HitObjectData hitObjectData = HitObjectManager.TransformHitObjectToDataObject(hitObject);
            hitObjectData.Judgement.Judgement = (int)judgement;
            hitObjectData.Judgement.SpawnTime = hitTime;

            hitObject.Judgement.Judgement = judgement;
            hitObject.Judgement.SpawnTime = hitTime;
        }

        private static void SpawnHitJudgementVisual(HitObjectJudgement judgement, Vector2 pos, long spawnTime)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                return;
            }

            HitJudgmentUI hitJudgement = null!;
            double diameter = MainWindow.OsuPlayfieldObjectDiameter;
            switch (judgement)
            {
                case HitObjectJudgement.Max:
                    Increment(HitObjectJudgement.Max);
                    hitJudgement = Get300(diameter);
                    break;
                case HitObjectJudgement.Ok:
                    Increment(HitObjectJudgement.Ok);
                    hitJudgement = Get100(diameter);
                    break;
                case HitObjectJudgement.Meh:
                    Increment(HitObjectJudgement.Meh);
                    hitJudgement = Get50(diameter);
                    break;
                case HitObjectJudgement.Miss: // miss
                    Increment(HitObjectJudgement.Miss);
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
            hitJudgement.EndTime = spawnTime + HitMarkerData.ALIVE_TIME;

            AliveHitJudgements.Add(hitJudgement);
            Window.playfieldCanva.Children.Add(hitJudgement);

            Canvas.SetLeft(hitJudgement, pos.X);
            Canvas.SetTop(hitJudgement, pos.Y);
            Canvas.SetZIndex(hitJudgement, 2);
        }

        public static void HandleAliveHitJudgements()
        {
            for (int i = 0; i < AliveHitJudgements.Count; i++)
            {
                HitJudgmentUI hitJudgment = AliveHitJudgements[i];
                if (GamePlayClock.TimeElapsed > hitJudgment.EndTime || GamePlayClock.TimeElapsed < hitJudgment.SpawnTime)
                {
                    AliveHitJudgements.Remove(hitJudgment);
                    Window.playfieldCanva.Children.Remove(hitJudgment);
                }
            }
        }

        private static void Increment(HitObjectJudgement judgement)
        {
            if (GamePlayClock.IsPaused() == true)
            {
                return; // disable incrementing when paused coz then seeking takes care of this in JudgementCounter class
            }
            JudgementCounter.Increment(judgement);
        }

        private static HitJudgmentUI Get300(double diameter)
        {
            return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit300), diameter, diameter);
        }

        private static HitJudgmentUI Get100(double diameter)
        {
            return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit100), diameter, diameter);
        }

        private static HitJudgmentUI Get50(double diameter)
        {
            return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit50), diameter, diameter);
        }

        private static HitJudgmentUI GetMiss(double diameter)
        {
            return new HitJudgmentUI(SkinElement.GetElement(SkinElement.SkinElements.Hit0), diameter, diameter);
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
        Max = 300,
        Ok = 100,
        Meh = 50,
        Miss = 0,
        SliderEndHit = 150,
        SliderTickMiss = -1,
        SliderEndMiss = -2,
        None = -727,
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
