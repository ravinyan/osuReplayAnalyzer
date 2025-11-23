using ReplayAnalyzer.AnalyzerTools.UIElements;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.Skins;
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

                    double elapsedTime = GamePlayClock.TimeElapsed;

                    double endTime = Math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
                    if (elapsedTime < toDelete.SpawnTime - endTime - 20)
                    {
                        // there is bug that deletes object too ^ early so here maybe temporary fix

                        // here is for backwards seeking so it doesnt show misses
                        // nvm right now this is for backwards AND forward seeking
                        AnnihilateHitObject(toDelete);

                        if (toDelete is Slider)
                        {
                            Slider.ResetToDefault(toDelete);
                        }
                    }
                    else if (toDelete is HitCircle && toDelete.Visibility == Visibility.Visible && elapsedTime >= GetEndTime(toDelete))
                    {
                        HitObjectDespawnMiss(toDelete, SkinElement.HitMiss(), MainWindow.OsuPlayfieldObjectDiameter);

                        //HitObjectSpawner.FindObjectIndexAfterSeek((long)elapsedTime, -1);

                        AnnihilateHitObject(toDelete);
                    }
                    else if (toDelete is Slider)
                    {
                        Slider s = toDelete as Slider;
                        if (SliderEndJudgement.IsSliderEndHit == false && elapsedTime >= (s.IsHit == true ? s.EndTime : s.DespawnTime))
                        {
                            HitObjectDespawnMiss(toDelete, SkinElement.SliderEndMiss(), MainWindow.OsuPlayfieldObjectDiameter * 0.2, true);
                            AnnihilateHitObject(toDelete);
                        }
                        else if (SliderEndJudgement.IsSliderEndHit == true && elapsedTime >= (s.IsHit == true ? s.EndTime : s.DespawnTime))
                        {
                            AnnihilateHitObject(toDelete);
                        }

                        Canvas sliderHead = toDelete.Children[1] as Canvas;
                        if (sliderHead.Children[0].Visibility == Visibility.Visible && s.IsHit == false
                        && elapsedTime >= s.SpawnTime + Math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty))
                        {

                            HitObjectDespawnMiss(toDelete, SkinElement.HitMiss(), MainWindow.OsuPlayfieldObjectDiameter);
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

        public static void HitObjectDespawnMiss(HitObject hitObject, string missImageUri, double diameter, bool sliderEndMiss = false)
        {
            HitJudgment hitJudgment = new HitJudgment(missImageUri, diameter, diameter);
            hitJudgment.SpawnTime = (long)GamePlayClock.TimeElapsed;
            hitJudgment.EndTime = hitJudgment.SpawnTime + 600;

            Vector2 missPosition;
            if (hitObject is HitCircle)
            {
                missPosition = hitObject.SpawnPosition;
            }
            else
            {
                Slider slider = hitObject as Slider;
                if (sliderEndMiss == true)
                {
                    missPosition = slider.RepeatCount % 2 == 0 ? slider.SpawnPosition : slider.EndPosition;
                }
                else
                {
                    missPosition = slider.SpawnPosition;
                }
            }

            double X = missPosition.X * MainWindow.OsuPlayfieldObjectScale - hitJudgment.Width / 2;
            double Y = missPosition.Y * MainWindow.OsuPlayfieldObjectScale - hitJudgment.Height;

            Canvas.SetLeft(hitJudgment, X);
            Canvas.SetTop(hitJudgment, Y);

            if (sliderEndMiss == false)
            {
               GamePlayClock.Pause();
               MusicPlayer.MusicPlayer.Pause();
               
               HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
               
               // this one line just correct very small offset when pausing...
               // from testing it doesnt cause any audio problems or any delay anymore so yaaay
               MusicPlayer.MusicPlayer.Seek(GamePlayClock.TimeElapsed);
               
               Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
               
                JudgementCounter.IncrementMiss();
            }

            Window.playfieldCanva.Children.Add(hitJudgment);
            HitJudgementManager.AliveHitJudgements.Add(hitJudgment);
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
                return o.SpawnTime + Math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);
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
