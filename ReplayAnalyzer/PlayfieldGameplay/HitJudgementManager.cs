using ReplayAnalyzer.AnalyzerTools.UIElements;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.Skins;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.Objects.Slider;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class HitJudgementManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static List<HitJudgment> AliveHitJudgements = new List<HitJudgment>();

        public static void ResetFields()
        {
            AliveHitJudgements.Clear();
        }

        public static void HandleAliveHitJudgements()
        {
            for (int i = 0; i < AliveHitJudgements.Count; i++)
            {
                HitJudgment hitJudgment = AliveHitJudgements[i];
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
                    SpawnHitJudgementVisual(Get300(MainWindow.OsuPlayfieldObjectDiameter), pos, spawnTime);
                    break;
                case 100:
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Ok, spawnTime);
                    SpawnHitJudgementVisual(Get100(MainWindow.OsuPlayfieldObjectDiameter), pos, spawnTime);
                    break;
                case 50:
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Meh, spawnTime);
                    SpawnHitJudgementVisual(Get50(MainWindow.OsuPlayfieldObjectDiameter), pos, spawnTime);
                    break;
                case 0:
                    ApplyHitJudgementValuesToHitObject(hitObject, HitObjectJudgement.Miss, spawnTime);
                    SpawnHitJudgementVisual(GetMiss(MainWindow.OsuPlayfieldObjectDiameter), pos, spawnTime);
                    break;
                case -1:
                    // * 0.2 since the png should be way smaller than normal misses
                    SpawnHitJudgementVisual(GetSliderTickMiss(MainWindow.OsuPlayfieldObjectDiameter * 0.2), pos, spawnTime);
                    break;
                case -2: // maybe flag to missed slider ends since that not hard? not sure about ticks... ok nvn no ticks
                    // * 0.2 since the png should be way smaller than normal misses
                    SpawnHitJudgementVisual(GetSliderEndMiss(MainWindow.OsuPlayfieldObjectDiameter * 0.2), pos, spawnTime);
                    break;
                default:
                    throw new Exception(@"Judgement can be 300, 100, 50 for hit HitObjects
                                 For misses: 0 = HitObject miss, -1 = SliderTick miss, -2 = SliderEnd miss");
            }
        }

        // if it has judgement applied then let it be saved and use only saved values
        private static void ApplyHitJudgementValuesToHitObject(HitObject hitObject, HitObjectJudgement judgement, long hitTime)
        {
            // maybe remove IsHit since there is now Judgement.None?
            if (MainWindow.IsReplayPreloading == true)
            {
                // pre loadeding of slider ticks and slider ends
                // try UpdateLayout or use dispatcher in pre loading loop to maybe make it work
                // ^ not working and to make it work is pain and probably not worth it anyway

                hitObject.Judgement = judgement;

                if (judgement != HitObjectJudgement.Miss)
                {
                    hitObject.HitAt = hitTime;
                    hitObject.IsHit = true;
                }
            }
        }

        private static void SpawnHitJudgementVisual(HitJudgment hitJudgment, Vector2 pos, long spawnTime)
        {
            hitJudgment.SpawnTime = spawnTime;
            hitJudgment.EndTime = spawnTime + 600;

            AliveHitJudgements.Add(hitJudgment);
            Window.playfieldCanva.Children.Add(hitJudgment);

            Canvas.SetLeft(hitJudgment, pos.X);
            Canvas.SetTop(hitJudgment, pos.Y);
        }

        private static HitJudgment Get300(double diameter)
        {
            JudgementCounter.Increment300();
            return new HitJudgment(SkinElement.Hit300(), diameter, diameter);
        }

        private static HitJudgment Get100(double diameter)
        {
            JudgementCounter.Increment100();
            return new HitJudgment(SkinElement.Hit100(), diameter, diameter);
        }

        private static HitJudgment Get50(double diameter)
        {
            JudgementCounter.Increment50();
            return new HitJudgment(SkinElement.Hit50(), diameter, diameter);
        }

        private static HitJudgment GetMiss(double diameter)
        {
            JudgementCounter.IncrementMiss();
            return new HitJudgment(SkinElement.HitMiss(), diameter, diameter);
        }

        private static HitJudgment GetSliderTickMiss(double diameter)
        {
            // increment tick misses? maybe in the future
            return new HitJudgment(SkinElement.SliderTickMiss(), diameter, diameter);
        }

        private static HitJudgment GetSliderEndMiss(double diameter)
        {
            // increment slider end misses? also maybe in the future
            return new HitJudgment(SkinElement.SliderEndMiss(), diameter, diameter);
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
    }
}
