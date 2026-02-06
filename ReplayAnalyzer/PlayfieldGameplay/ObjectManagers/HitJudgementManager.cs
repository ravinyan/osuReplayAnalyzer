using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.GameClock;
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
        /// Judgement can be 300, 100, 50 for hit HitObjects.
        /// For misses: 0 = HitObject miss, -1 = SliderTick miss, -2 = SliderEnd miss.
        /// </summary>
        public static void ApplyJudgement(HitObject hitObject, Vector2 pos, long spawnTime, int judgement)
        {
            switch (judgement)
            {
                case 300:
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Max, spawnTime);
                    SpawnHitJudgementVisual(judgement, pos, spawnTime);
                    break;
                case 100:
                    AddHitJudgementToTimeline(HitObjectJudgement.Ok, spawnTime);
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Ok, spawnTime);
                    SpawnHitJudgementVisual(judgement, pos, spawnTime);
                    break;
                case 50:
                    AddHitJudgementToTimeline(HitObjectJudgement.Meh, spawnTime);
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Meh, spawnTime);
                    SpawnHitJudgementVisual(judgement, pos, spawnTime);
                    break;
                case 0:
                    AddHitJudgementToTimeline(HitObjectJudgement.Miss, spawnTime);
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Miss, spawnTime);
                    SpawnHitJudgementVisual(judgement, pos, spawnTime);           
                    break;
                case -1: // tick miss (causes combo break)
                    AddHitJudgementToTimeline(HitObjectJudgement.Miss, spawnTime);
                    SpawnHitJudgementVisual(judgement, pos, spawnTime); 
                    break;
                case -2: // end miss
                    SpawnHitJudgementVisual(judgement, pos, spawnTime);
                    break;
                default:
                    throw new Exception(@"Judgement can be 300, 100, 50 for hit HitObjects
                                 For misses: 0 = HitObject miss, -1 = SliderTick miss, -2 = SliderEnd miss");
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

        // if it has judgement applied then let it be saved and use only saved values
        private static void ApplyHitJudgementValuesToHitObject(HitObject hitObject, HitObjectJudgement judgement, long hitTime)
        {
            // maybe remove IsHit since there is now Judgement.None?
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
