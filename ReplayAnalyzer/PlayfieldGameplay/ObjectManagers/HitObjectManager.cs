using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.HitObjects.Osu;
using ReplayAnalyzer.HitObjects.Taiko;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Numerics;
using System.Windows;
using Slider = ReplayAnalyzer.HitObjects.Osu.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers
{
    public class HitObjectManager
    {
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

            // oh no this is long... anyway
            for (int i = 0; i < AliveHitObjects.Count; i++)
            {
                HitObject toDelete = AliveHitObjects[i];

                double elapsedTime = GamePlayClock.TimeElapsed;
                if (MainWindow.replay.GameMode == OsuFileParsers.Classes.Replay.GameMode.Osu
                &&  elapsedTime < toDelete.SpawnTime - Math.GetApproachRateTiming() - 20 && elapsedTime >= 0)
                {
                    // removes objects when using seeking backwards
                    AnnihilateHitObject(toDelete);
                }
                else if (toDelete is HitCircle && toDelete.Visibility == Visibility.Visible && elapsedTime >= GetEndTime(toDelete))
                {
                    HitObjectData toDeleteData = TransformHitObjectToDataObject(toDelete);
                    if (toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.Miss
                    && toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.None)
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
                    && elapsedTime >= s.SpawnTime + Math.GetJudgement50HitWindow())
                    {
                        HitObjectData toDeleteData = TransformHitObjectToDataObject(toDelete);
                        if (toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.Miss
                        && toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.None)
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
                else if (toDelete is ManiaNote && elapsedTime >= toDelete.SpawnTime + Math.GetJudgement50HitWindow())
                {
                    ManiaNoteData n = (ManiaNoteData)TransformHitObjectToDataObject(toDelete);
                    if (n.Judgement.Judgement != (int)HitObjectJudgement.Miss
                    &&  n.Judgement.Judgement != (int)HitObjectJudgement.None)
                    {
                        // it shouldnt give miss if this occurs
                        AnnihilateHitObject(toDelete);
                        continue;
                    }

                    HitObjectDespawnMiss(toDelete, ManiaPlayfield.ColumnWidth * n.ColumnIndex, ManiaPlayfield.JudgementYPosition);
                    AnnihilateHitObject(toDelete);
                }
                else if (toDelete is ManiaLongNote)
                {
                    ManiaLongNoteData ln;
                    try
                    {
                        // every object name is its index based on when it was created and it never repeats
                        // no clue why this breaks and crashes but hopefully this wont be big problem...
                        ln = (ManiaLongNoteData)TransformHitObjectToDataObject(toDelete);
                    }
                    catch { return; }

                    if (elapsedTime > ln.EndTime + Math.GetJudgement50HitWindow())
                    {
                        if (ln.Judgement.Judgement != (int)HitObjectJudgement.Miss
                        && ln.Judgement.Judgement != (int)HitObjectJudgement.None)
                        {
                            // it shouldnt give miss if this occurs
                            AnnihilateHitObject(toDelete);
                            continue;
                        }

                        HitObjectDespawnMiss(toDelete, ManiaPlayfield.ColumnWidth * ln.ColumnIndex, ManiaPlayfield.JudgementYPosition);
                        AnnihilateHitObject(toDelete);
                    }
                }
                else if (toDelete is TaikoHitCircle && elapsedTime >= toDelete.SpawnTime + Math.GetJudgement100HitWindow())
                {
                    TaikoHitCircleData n = (TaikoHitCircleData)TransformHitObjectToDataObject(toDelete);
                    if (n.Judgement.Judgement != (int)HitObjectJudgement.Miss
                    &&  n.Judgement.Judgement != (int)HitObjectJudgement.None)
                    {
                        // it shouldnt give miss if this occurs
                        AnnihilateHitObject(toDelete);
                        continue;
                    }

                    HitObjectDespawnMiss(toDelete, TaikoPlayfield.JudgementPosition.X, TaikoPlayfield.JudgementPosition.Y);
                    AnnihilateHitObject(toDelete);
                }
                else if (toDelete is TaikoDrumRoll)
                {// this doesnt cause any misses it is just for score which i dont care about
                    TaikoDrumRoll drumRoll = (TaikoDrumRoll)toDelete;
                    if (elapsedTime >= drumRoll.EndTime)
                    {
                        AnnihilateHitObject(toDelete);
                    }
                }
                else if (toDelete is TaikoSpinner && elapsedTime >= toDelete.SpawnTime + Math.GetJudgement100HitWindow())
                {// and this is the same thing, no miss and doesnt matter
                    AnnihilateHitObject(toDelete);
                }
                else if (toDelete is CatchFruit && elapsedTime >= toDelete.SpawnTime + TaikoPlayfield.ScrollSpeed)
                {
                    AnnihilateHitObject(toDelete);
                }
                else if (toDelete is CatchJuiceStream)
                {// no way javascript???
                    CatchJuiceStream js = (CatchJuiceStream)toDelete;
                    if (elapsedTime >= js.EndTime + TaikoPlayfield.ScrollSpeed)
                    {
                        AnnihilateHitObject(toDelete);
                    }
                }
                else if (toDelete is CatchBananaShower)
                {// no judgements
                    CatchBananaShower bs = (CatchBananaShower)toDelete;
                    if (elapsedTime >= bs.EndTime + TaikoPlayfield.ScrollSpeed)
                    {
                        AnnihilateHitObject(toDelete);
                    }
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

        public static void HitObjectDespawnMiss(HitObject hitObject, float X, float Y)
        {
            HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, HitObjectJudgement.Miss);
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
            HitObjectData hitObjectData = TransformHitObjectToDataObject(toDelete);

            AliveDataObjects.Remove(hitObjectData);
            AliveHitObjects.Remove(toDelete);

            PlayfieldManager.GetActivePlayfield().Children.Remove(toDelete);
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
            else if (o is ManiaLongNote ln)
            {
                return ln.EndTime;
            }
            else if (o is TaikoDrumRoll dr)
            {
                return dr.EndTime;
            }
            else if (o is CatchJuiceStream js)
            {
                return js.EndTime;
            }
            else if (o is CatchBananaShower bs)
            {
                return bs.EndTime;
            }
            else
            {
                return o.SpawnTime + Math.GetJudgement50HitWindow();
            }
        }

        public static double GetEndTime(HitObjectData o)
        {
            if (o is OsuSliderData sl)
            {
                return sl.EndTime;
            }
            else if (o is OsuSpinnerData sp)
            {
                return sp.EndTime;
            }
            else if (o is ManiaLongNoteData ln)
            {
                return ln.EndTime;
            }
            else if (o is TaikoDrumRollData dr)
            {
                return dr.EndTime;
            }
            else if (o is CatchJuiceStreamData js)
            {
                return js.EndTime;
            }
            else if (o is CatchBananaShowerData bs)
            {
                return bs.EndTime;
            }
            else
            {
                return o.SpawnTime + Math.GetJudgement50HitWindow();
            }
        }

        public static HitObjectData TransformHitObjectToDataObject(HitObject hitObject)
        {
            string index = "";
            for (int i = 0; i < hitObject.Name.Length; i++)
            {
                if (char.IsDigit(hitObject.Name[i]))
                {
                    index = index + hitObject.Name[i];
                }
            }

            return MainWindow.map.HitObjects[int.Parse(index)];
        }
    }
}
