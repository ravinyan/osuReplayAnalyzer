using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers
{
    public class HitObjectManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static OsuMath Math = new OsuMath();

        private static List<HitObject> AliveHitObjects = new List<HitObject>();
        private static List<HitObjectData> AliveDataObjects = new List<HitObjectData>();

        public static void ResetFields()
        {
            AliveHitObjects.Clear();
            AliveDataObjects.Clear();
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

                    if (elapsedTime < toDelete.SpawnTime - endTime - 20 && elapsedTime >= 0)
                    {
                        // removes objects when using seeking backwards
                        AnnihilateHitObject(toDelete);
                    }
                    else if (toDelete is HitCircle && toDelete.Visibility == Visibility.Visible && elapsedTime >= Slider.GetEndTime(toDelete))
                    {
                        HitObjectData toDeleteData = TransformHitObjectToDataObject(toDelete);
                        if (toDeleteData.Judgement.HitJudgement != (int)HitObjectJudgement.Miss
                        && toDeleteData.Judgement.HitJudgement != (int)HitObjectJudgement.None)
                        {
                            // it shouldnt give miss if this occurs
                            AnnihilateHitObject(toDelete);
                            continue;
                        }

                        HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter);
                        AnnihilateHitObject(toDelete);
                    }
                    else if (toDelete is Slider)
                    {
                        Slider s = toDelete as Slider;
                        Canvas sliderHead = toDelete.Children[1] as Canvas;
                        if (SliderEndJudgement.IsSliderEndHit == false 
                        &&  elapsedTime >= (s.Judgement.ObjectJudgement > HitObjectJudgement.Miss ? s.EndTime : s.DespawnTime))
                        {
                            HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter * 0.2, true);
                            AnnihilateHitObject(toDelete);
                        }
                        else if (SliderEndJudgement.IsSliderEndHit == true 
                        &&       elapsedTime >= (sliderHead.Children[0].Visibility == Visibility.Visible ? s.DespawnTime : s.EndTime))
                        {
                            // if visibility is visible then it wasnt it... if its anything but visible it is hit
                            // bug  elapsedTime >= (s.IsHit == false ? s.DespawnTime : s.EndTime) IsHit is preset from preloading
                            AnnihilateHitObject(toDelete);
                        }

                        if (sliderHead.Children[0].Visibility == Visibility.Visible && s.Judgement.ObjectJudgement <= HitObjectJudgement.Miss
                        &&  elapsedTime >= s.SpawnTime + Math.GetOverallDifficultyHitWindow50())
                        {
                            HitObjectData toDeleteData = TransformHitObjectToDataObject(toDelete);
                            if (toDeleteData.Judgement.HitJudgement != (int)HitObjectJudgement.Miss
                            &&  toDeleteData.Judgement.HitJudgement != (int)HitObjectJudgement.None)
                            {
                                // it shouldnt give miss if this occurs
                                continue;
                            }

                            HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter);
                            if (toDelete.Children.Count != 0)
                            {
                                Slider.RemoveSliderHead(toDelete.Children[1] as Canvas);
                            }
                        }
                    }
                    else if (toDelete is Spinner && elapsedTime >= Slider.GetEndTime(toDelete))
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
                missPosition = slider.RepeatCount % 2 == 0 ? slider.BaseSpawnPosition 
                                                           : slider.EndPosition;
            }
            else
            {
                missPosition = hitObject.BaseSpawnPosition;
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
            HitObjectData hitObjectData = AliveDataObjects.FirstOrDefault(h => h.SpawnTime == toDelete.SpawnTime) ?? null;
            if (hitObjectData == null)
            {
                return;
            }

            AliveDataObjects.Remove(hitObjectData);
            AliveHitObjects.Remove(toDelete);

            Window.playfieldCanva.Children.Remove(toDelete);
            toDelete.Visibility = Visibility.Collapsed;

            if (MainWindow.IsReplayPreloading == false)
            {
                if (toDelete is Slider)
                {
                    HitObjectAnimations.RemoveEventCompleted(toDelete as Slider);
                }
                HitObjectAnimations.Remove(toDelete);
                HitObjectAnimations.RemoveStoryboardFromDict(toDelete);
            }
            
            //toDelete.Dispose();
        }

        public static List<HitObject> GetAliveHitObjects()
        {
            return AliveHitObjects;
        }

        public static List<HitObjectData> GetAliveDataObjects()
        {
            return AliveDataObjects;
        }

        public static void ClearAliveObjects()
        {
            for (int i = AliveHitObjects.Count - 1; i >= 0; i--)
            {
                AnnihilateHitObject(AliveHitObjects[i]);
            }

            // only data object clear is needed but i will just use both coz why not
            AliveHitObjects.Clear();
            AliveDataObjects.Clear();
        }

        public static HitObjectData TransformHitObjectToDataObject(HitObject hitObject)
        {
            // all object names are the same exact length, and this extracts only numbers at the end which start from 0
            return MainWindow.map.HitObjects[int.Parse(hitObject.Name.Substring(15))];
        }
    }
}
