using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
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
            if (AliveHitObjects.Count == 0)
            {
                return;
            }

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
                    if (toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.Miss
                    &&  toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.None)
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
                    
                    double endTime = Slider.HeadHitCircleContainer(s).Visibility == Visibility.Visible
                                   ? s.DespawnTime
                                   : s.EndTime;
                    if (elapsedTime >= endTime)
                    {
                        SliderEndDespawnJudgement(s, MainWindow.OsuPlayfieldObjectDiameter * 0.2);
                        AnnihilateHitObject(toDelete);
                    }

                    if (Slider.HeadHitCircleContainer(s).Visibility == Visibility.Visible && s.Judgement.Judgement <= HitObjectJudgement.Miss
                    &&  elapsedTime >= s.SpawnTime + Math.GetJudgement50HitWindow())
                    {
                        HitObjectData toDeleteData = TransformHitObjectToDataObject(toDelete);
                        if (toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.Miss
                        &&  toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.None)
                        {
                            // it shouldnt give miss if this occurs
                            continue;
                        }

                        HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter);
                        Slider.RemoveSliderHead(s);
                    }
                }
                else if (toDelete is Spinner && elapsedTime >= GetEndTime(toDelete))
                {             
                    AnnihilateHitObject(toDelete);
                }
            }
        }

        public static void HitObjectDespawnMiss(HitObject hitObject, double diameter)
        {
            Vector2 missPosition = hitObject.BaseSpawnPosition;

            float X = (float)(missPosition.X * MainWindow.OsuPlayfieldObjectScale - diameter / 2);
            float Y = (float)(missPosition.Y * MainWindow.OsuPlayfieldObjectScale - diameter);

            HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, 0);
        }

        private static void SliderEndDespawnJudgement(Slider s, double diameter)
        {
            // if it was hit then there is possible edge case where cursor is far from the end, BUT got max judgement
            // coz of slider tail leniency which allows locking getting max judgement 36ms before slider ends...
            // when you backwards seek and cursor is outside of the ball hitbox, but in preload it still got max judgement
            // it would give miss coz of lack of slider leniency, so if it got max judgement like that then just return here
            if (s.SliderEndJudgement.Judgement == HitObjectJudgement.SliderEndHit)
            {
                return;
            }

            Vector2 missPosition = s.RepeatCount % 2 == 0
                                 ? s.BaseSpawnPosition
                                 : s.EndPosition;

            float X = (float)(missPosition.X * MainWindow.OsuPlayfieldObjectScale - diameter / 2);
            float Y = (float)(missPosition.Y * MainWindow.OsuPlayfieldObjectScale - diameter);

            if (((StrictTrackingMod.IsStrictTrackingEnabled == true && SliderEndJudgement.IsJudged == false)
            ||    StrictTrackingMod.IsStrictTrackingEnabled == false) && SliderEndJudgement.IsTracking == false)
            {
                HitJudgementManager.ApplyJudgement(s, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, HitObjectJudgement.SliderEndMiss);
            }
            else if (SliderEndJudgement.IsTracking == true)
            {
                HitJudgementManager.ApplyJudgement(s, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, HitObjectJudgement.SliderEndHit);
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
                return o.SpawnTime + Math.GetJudgement50HitWindow();
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
                return o.SpawnTime + Math.GetJudgement50HitWindow();
            }
        }

        public static HitObjectData TransformHitObjectToDataObject(HitObject hitObject)
        {
            // all object names are the same exact length, and this extracts only numbers at the end which start from 0
            return MainWindow.map.HitObjects[int.Parse(hitObject.Name.Substring(15))];
        }
    }
}
