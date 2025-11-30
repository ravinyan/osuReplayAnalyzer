using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.Objects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class HitObjectManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static OsuMath Math = new OsuMath();

        private static List<HitObject> AliveHitObjects = new List<HitObject>();

        public static void ResetFields()
        {
            AliveHitObjects.Clear();
        }

        public static void HandleVisibleHitObjects()
        {
            if (AliveHitObjects.Count > 0)
            {
                // need to check everything coz of sliders edge cases
                for (int i = 0; i < AliveHitObjects.Count; i++)
                {
                    HitObject toDelete = AliveHitObjects[i];

                    double endTime = Math.GetApproachRateTiming();
                    double elapsedTime = GamePlayClock.TimeElapsed;
                    if (toDelete is HitCircle && toDelete.Visibility == Visibility.Visible && elapsedTime >= GetEndTime(toDelete))
                    {
                        if (toDelete.Judgement != HitJudgementManager.HitObjectJudgement.Miss
                        &&  toDelete.Judgement != HitJudgementManager.HitObjectJudgement.None)
                        {
                            // it shouldnt give miss if this occurs so just despawn the object
                            AnnihilateHitObject(toDelete);
                            continue;
                        }

                        HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter);
                        AnnihilateHitObject(toDelete);
                    }
                    else if (toDelete is Slider)
                    {
                        Slider s = toDelete as Slider;
                        if (SliderEndJudgement.IsSliderEndHit == false && elapsedTime >= (s.IsHit == true ? s.EndTime : s.DespawnTime))
                        {
                            HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter * 0.2, true);
                            AnnihilateHitObject(toDelete);
                        }
                        else if (SliderEndJudgement.IsSliderEndHit == true && elapsedTime >= (s.IsHit == true ? s.EndTime : s.DespawnTime))
                        {
                            AnnihilateHitObject(toDelete);
                        }

                        Canvas sliderHead = toDelete.Children[1] as Canvas;
                        if (sliderHead.Children[0].Visibility == Visibility.Visible && s.IsHit == false
                        && elapsedTime >= s.SpawnTime + Math.GetOverallDifficultyHitWindow50())
                        {
                            if (toDelete.Judgement != HitJudgementManager.HitObjectJudgement.Miss
                            &&  toDelete.Judgement != HitJudgementManager.HitObjectJudgement.None)
                            {
                                // it shouldnt give miss if this occurs so just despawn the object
                                AnnihilateHitObject(toDelete);
                                continue;
                            }

                            HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter);
                            RemoveSliderHead(toDelete.Children[1] as Canvas);
                        }
                    }
                    else if (toDelete is Spinner && elapsedTime >= GetEndTime(toDelete))
                    {
                        AnnihilateHitObject(toDelete);
                    }
                }
            }
        }

        public static void HitObjectDespawnMiss(HitObject hitObject, double diameter, bool sliderEndMiss = false)
        {
            Vector2 missPosition;
            if (sliderEndMiss == true)
            {
                Slider slider = hitObject as Slider;
                missPosition = slider.RepeatCount % 2 == 0 ? slider.SpawnPosition : slider.EndPosition;
            }
            else
            {
                missPosition = hitObject.SpawnPosition;
            }

            float X = (float)(missPosition.X * MainWindow.OsuPlayfieldObjectScale - diameter / 2);
            float Y = (float)(missPosition.Y * MainWindow.OsuPlayfieldObjectScale - diameter);

            if (sliderEndMiss == true)
            {
                HitJudgementManager.ApplyJudgement(null, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, -2);
            }
            else
            {
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, 0);
            }
        }

        public static void AnnihilateHitObject(HitObject toDelete)
        {
            Window.playfieldCanva.Children.Remove(toDelete);
            toDelete.Visibility = Visibility.Collapsed;
            AliveHitObjects.Remove(toDelete);
            HitObjectAnimations.Remove(toDelete);
        }

        public static void RemoveSliderHead(Canvas sliderHead)
        {
            // all slider head children that are hit circle
            for (int i = 0; i <= 3; i++)
            {
                sliderHead.Children[i].Visibility = Visibility.Collapsed;
            }

            // reverse arrow if exists will now be visible
            if (sliderHead.Children.Count > 4)
            {
                sliderHead.Children[4].Visibility = Visibility.Visible;
            }
        }

        public static void UpdateCurrentSliderValues(Slider s)
        {
            SliderTick.ResetFields();
            SliderReverseArrow.ResetFields();

            // to check
            for (int i = 0; i < s.RepeatCount - 1; i++)
            {
                SliderReverseArrow.UpdateSliderRepeats();
            }

            SliderTick.HidePastTicks(s);

            RemoveSliderHead(s.Children[1] as Canvas);
        }

        public static List<HitObject> GetAliveHitObjects()
        {
            return AliveHitObjects;
        }

        public static double GetEndTime(HitObject o)
        {
            if (o is Slider sl)
            {
                return sl.EndTime;
            }
            else if (o is Spinner sp)
            {
                return sp.EndTime;
            }
            else
            {
                return o.SpawnTime + Math.GetOverallDifficultyHitWindow50();
            }
        }

        public static Slider GetFirstSliderDataBySpawnTime()
        {
            Slider slider = null;

            foreach (HitObject obj in GetAliveHitObjects())
            {
                if (obj is not Slider)
                {
                    continue;
                }

                Slider s = obj as Slider;
                if (slider == null || slider.SpawnTime > s.SpawnTime)
                {
                    slider = s;
                }
            }

            return slider;
        }
    }
}
