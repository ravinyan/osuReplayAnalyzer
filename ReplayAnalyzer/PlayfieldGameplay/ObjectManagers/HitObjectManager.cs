using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using System.Numerics;
using System.Windows;
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

                    double elapsedTime = GamePlayClock.TimeElapsed;
                    if (elapsedTime < toDelete.SpawnTime - Math.GetApproachRateTiming() - 20 && elapsedTime >= 0)
                    {
                        // removes objects when using seeking backwards
                        AnnihilateHitObject(toDelete);
                    }
                    else if (toDelete is HitCircle && toDelete.Visibility == Visibility.Visible && elapsedTime >= GetEndTime(toDelete))
                    {
                        HitObjectData toDeleteData = TransformHitObjectToDataObject(toDelete);
                        if (toDeleteData.Judgement.HitJudgement != (int)HitObjectJudgement.Miss
                        &&  toDeleteData.Judgement.HitJudgement != (int)HitObjectJudgement.None)
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

                        double endTime = Slider.HeadHitCircle(s).Visibility == Visibility.Visible
                                       ? s.DespawnTime
                                       : s.EndTime;
                        if (elapsedTime >= endTime)
                        {   // this is not clear but HitObjectDespawnMiss wont give slider end (here) miss if slider was
                            // correctly tracked... need to change it... also now it gives SliderEndHit judgement... lol
                            HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter * 0.2, true);
                            AnnihilateHitObject(toDelete);
                        }

                        if (Slider.HeadApproachCircle(s).Visibility == Visibility.Visible && s.Judgement.ObjectJudgement <= HitObjectJudgement.Miss
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
                                Slider.RemoveSliderHead(s);
                            }
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
                if (StrictTrackingMod.IsStrictTrackingEnabled == true 
                &&  SliderEndJudgement.IsJudged == false && SliderEndJudgement.IsTracking == false)
                {
                    HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, -1);
                }
                else if (StrictTrackingMod.IsStrictTrackingEnabled == false && SliderEndJudgement.IsTracking == false)
                {
                    HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, -2);
                }
                else if (StrictTrackingMod.IsStrictTrackingEnabled == false && SliderEndJudgement.IsTracking == true)
                {
                    HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, 150);
                }
            }
            else
            {
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, 0);
            }
        }

        public static void AnnihilateHitObject(HitObject toDelete)
        {
            HitObjectData hitObjectData;
            if (toDelete is Spinner)
            {
                hitObjectData = AliveDataObjects.FirstOrDefault(h => h.SpawnTime - Spinner.SpawnOffset == toDelete.SpawnTime) ?? null;
            }
            else
            {
                hitObjectData = AliveDataObjects.FirstOrDefault(h => h.SpawnTime == toDelete.SpawnTime) ?? null;
            }
                
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

        public static double GetEndTime(HitObjectData o)
        {
            if (o is SliderData sl)
            {
                return sl.EndTime;
            }
            else if (o is SpinnerData sp)
            {
                return sp.EndTime;
            }
            else
            {
                return o.SpawnTime + Math.GetOverallDifficultyHitWindow50();
            }
        }

        public static HitObjectData TransformHitObjectToDataObject(HitObject hitObject)
        {
            // all object names are the same exact length, and this extracts only numbers at the end which start from 0
            return MainWindow.map.HitObjects[int.Parse(hitObject.Name.Substring(15))];
        }
    }
}
