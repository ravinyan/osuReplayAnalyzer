using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.MusicPlayer;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers
{
    public class HitJudgementManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static List<HitJudgmentUI> AliveHitJudgements = new List<HitJudgmentUI>();

        public static void ResetFields()
        {
            AliveHitJudgements.Clear();
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

        /// <summary>
        /// For hit cicles Judgement can be 300, 100, 50 for hit and 0 for miss.
        /// For slider events: 150 = SliderEndHit, -1 = SliderTick miss, -2 = SliderEnd miss.
        /// </summary>
        public static void ApplyJudgement(HitObject hitObject, Vector2 judgementPosition, long judgementHitTime, int judgement)
        {
            switch (judgement)
            {
                case 300:
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Max, judgementHitTime);
                    SpawnHitJudgementVisual(judgement, judgementPosition, judgementHitTime);
                    break;
                case 100:
                    AddHitJudgementToTimeline(HitObjectJudgement.Ok, judgementHitTime);
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Ok, judgementHitTime);
                    SpawnHitJudgementVisual(judgement, judgementPosition, judgementHitTime);
                    break;
                case 50:
                    AddHitJudgementToTimeline(HitObjectJudgement.Meh, judgementHitTime);
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Meh, judgementHitTime);
                    SpawnHitJudgementVisual(judgement, judgementPosition, judgementHitTime);
                    break;
                case 0:
                    AddHitJudgementToTimeline(HitObjectJudgement.Miss, judgementHitTime);
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Miss, judgementHitTime);
                    SpawnHitJudgementVisual(judgement, judgementPosition, judgementHitTime);           
                    break;
                case 150:
                    ApplySliderEndJudgementToSlider((HitObjects.Slider)hitObject, HitObjectJudgement.SliderEndHit, judgementHitTime);
                    break;
                case -1: // tick miss (causes combo break)
                    AddHitJudgementToTimeline(HitObjectJudgement.Miss, judgementHitTime);
                    SpawnHitJudgementVisual(judgement, judgementPosition, judgementHitTime);
                    break;
                case -2: // end miss (no combo break) ((unless strict tracking))
                    if (StrictTrackingMod.IsStrictTrackingEnabled == true)// thats how it works in osu lazer
                    {
                        AddHitJudgementToTimeline(HitObjectJudgement.Miss, judgementHitTime);
                        ApplySliderEndJudgementToSlider((HitObjects.Slider)hitObject, HitObjectJudgement.SliderEndMiss, judgementHitTime);
                        SpawnHitJudgementVisual((int)HitObjectJudgement.SliderTickMiss, judgementPosition, judgementHitTime);
                    }
                    else
                    {
                        ApplySliderEndJudgementToSlider((HitObjects.Slider)hitObject, HitObjectJudgement.SliderEndMiss, judgementHitTime);
                        SpawnHitJudgementVisual(judgement, judgementPosition, judgementHitTime);
                    }
                    break;
                default:
                    throw new Exception(@"Judgement can be 300, 100, 50 or 0 for hit HitCircles
                                 For slider events: 150 = SliderEnd hit, -1 = SliderTick miss, -2 = SliderEnd miss");
            }
        }

        private static void AddHitJudgementToTimeline(HitObjectJudgement judgement, long hitTime)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return;
            }

            switch (judgement)
            {
                case HitObjectJudgement.Ok:
                    JudgementTimeline.AddJudgementToTimeline(new SolidColorBrush(Color.FromRgb(11, 145, 9)), hitTime, "100");
                    break;
                case HitObjectJudgement.Meh:
                    JudgementTimeline.AddJudgementToTimeline(new SolidColorBrush(Color.FromRgb(242, 146, 2)), hitTime, "50");
                    break;
                case HitObjectJudgement.Miss:
                    JudgementTimeline.AddJudgementToTimeline(new SolidColorBrush(Color.FromRgb(245, 42, 42)), hitTime, "miss");
                    break;
                default:
                    // only x100, x50 and misses should be on timeline
                    break;
            }
        }

        private static void ApplySliderEndJudgementToSlider(HitObjects.Slider slider, HitObjectJudgement judgement, long hitTime)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return;
            }

            SliderData sliderData = (SliderData)HitObjectManager.TransformHitObjectToDataObject(slider);
            sliderData.SliderEndJudgement.HitJudgement = (int)judgement;
            sliderData.SliderEndJudgement.SpawnTime = hitTime;

            slider.SliderEndJudgement.ObjectJudgement = judgement;
            slider.SliderEndJudgement.SpawnTime = hitTime;
        }

        // if it has judgement applied then let it be saved and use only saved values
        private static void ApplyHitJudgementValuesToHitObject(HitObject hitObject, HitObjectJudgement judgement, long hitTime)
        {
            if (MainWindow.IsReplayPreloading == false || hitObject.Judgement.ObjectJudgement != HitObjectJudgement.None)
            {
                return;
            }

            HitObjectData hitObjectData = HitObjectManager.TransformHitObjectToDataObject(hitObject);
            hitObjectData.Judgement.HitJudgement = (int)judgement;
            hitObjectData.Judgement.SpawnTime = hitTime;

            hitObject.Judgement.ObjectJudgement = judgement;
            hitObject.Judgement.SpawnTime = hitTime;
        }

        private static void SpawnHitJudgementVisual(int judgement, Vector2 pos, long spawnTime)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                return;
            }

            HitJudgmentUI hitJudgement = null!;
            switch (judgement)
            {
                case 300:
                    hitJudgement = Get300(MainWindow.OsuPlayfieldObjectDiameter);
                    break;
                case 100:
                    hitJudgement = Get100(MainWindow.OsuPlayfieldObjectDiameter);
                    break;
                case 50:
                    hitJudgement = Get50(MainWindow.OsuPlayfieldObjectDiameter);
                    break;
                case 0: // miss
                    hitJudgement = GetMiss(MainWindow.OsuPlayfieldObjectDiameter);
                    break;
                case -1: // slider tick
                    hitJudgement = GetSliderTickMiss(MainWindow.OsuPlayfieldObjectDiameter * 0.2); // need to reduce image size
                    break;
                case -2: // slider end
                    hitJudgement = GetSliderEndMiss(MainWindow.OsuPlayfieldObjectDiameter * 0.2); // need to reduce image size
                    break;
            }

            hitJudgement.SpawnTime = spawnTime;
            hitJudgement.EndTime = spawnTime + HitMarkerData.ALIVE_TIME;

            AliveHitJudgements.Add(hitJudgement);
            Window.playfieldCanva.Children.Add(hitJudgement);

            Canvas.SetLeft(hitJudgement, pos.X);
            Canvas.SetTop(hitJudgement, pos.Y);
        }

        private static HitJudgmentUI Get300(double diameter)
        {
            JudgementCounter.Increment300();
            return new HitJudgmentUI(SkinElement.Hit300(), diameter, diameter);
        }

        private static HitJudgmentUI Get100(double diameter)
        {
            JudgementCounter.Increment100();
            return new HitJudgmentUI(SkinElement.Hit100(), diameter, diameter);
        }

        private static HitJudgmentUI Get50(double diameter)
        {
            JudgementCounter.Increment50();
            return new HitJudgmentUI(SkinElement.Hit50(), diameter, diameter);
        }

        private static HitJudgmentUI GetMiss(double diameter)
        {
            JudgementCounter.IncrementMiss();
            return new HitJudgmentUI(SkinElement.HitMiss(), diameter, diameter);
        }

        private static HitJudgmentUI GetSliderTickMiss(double diameter)
        {
            // increment tick misses? maybe in the future
            return new HitJudgmentUI(SkinElement.SliderTickMiss(), diameter, diameter);
        }

        private static HitJudgmentUI GetSliderEndMiss(double diameter)
        {
            // increment slider end misses? also maybe in the future
            return new HitJudgmentUI(SkinElement.SliderEndMiss(), diameter, diameter);
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
        public HitObjectJudgement ObjectJudgement { get; set; }
        public long SpawnTime { get; set; }

        public HitJudgement(HitObjectJudgement judgement, long spawnTime)
        {
            ObjectJudgement = judgement;
            SpawnTime = spawnTime;
        }
    }

    public class HitJudgmentUI : Image
    {
        public long SpawnTime { get; set; }
        public long EndTime { get; set; }
        
        public HitJudgmentUI(string skinUri, double width, double height)
        {
            Source = new BitmapImage(new Uri(skinUri));
            Width = width;
            Height = height;
        }
    }
}
