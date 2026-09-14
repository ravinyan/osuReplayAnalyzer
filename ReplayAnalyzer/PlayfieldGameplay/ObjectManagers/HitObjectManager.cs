using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.HitObjects.Osu;
using ReplayAnalyzer.HitObjects.Taiko;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
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

                long elapsedTime = PlayfieldManager.GetElapsedFrameTime();
                // to ensure objects NEVER despawn too early there is additional - 25ms (catch replay frames have >16ms gaps)
                if (elapsedTime < toDelete.SpawnTime - AdditionalVisualSpawnTime() - 25)
                {
                    // removes objects when using seeking backwards
                    AnnihilateHitObject(toDelete);
                    i--;
                }
                else if (toDelete is HitCircle && toDelete.Visibility == Visibility.Visible
                &&       elapsedTime >= toDelete.SpawnTime + Math.GetJudgement50HitWindow())
                {
                    HitObjectData toDeleteData = TransformHitObjectToDataObject(toDelete);
                    if (toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.Miss
                    &&  toDeleteData.Judgement.Judgement != (int)HitObjectJudgement.None)
                    {
                        // it shouldnt give miss if this occurs
                        AnnihilateHitObject(toDelete);
                        i--;
                        continue;
                    }

                    HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter, elapsedTime);
                    AnnihilateHitObject(toDelete);
                    i--;
                }
                else if (toDelete is Slider)
                {
                    Slider s = toDelete as Slider;

                    double endTime = Slider.HeadHitCircleContainer(s).Visibility == Visibility.Visible
                                   ? s.DespawnTime
                                   : s.EndTime;
                    if (elapsedTime >= endTime)
                    {
                        SliderEndDespawnJudgement(s, MainWindow.OsuPlayfieldObjectDiameter * 0.2, elapsedTime);
                        AnnihilateHitObject(toDelete);
                        i--;
                        continue;
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

                        HitObjectDespawnMiss(toDelete, MainWindow.OsuPlayfieldObjectDiameter, elapsedTime);
                        Slider.RemoveSliderHead(s);
                    }
                }
                else if (toDelete is Spinner && elapsedTime >= GetEndTime(toDelete))
                {
                    AnnihilateHitObject(toDelete);
                    i--;
                }
                else if (toDelete is ManiaNote)
                {
                    bool canContinue = false;
                    // i got this case where elapsedTime == 281926 and delete is at 281926.5... in scorev1 it gives miss tho
                    // so i assume i can delete 0.5 from here... idk if i should do this anywhere else tho
                    // i assume -0.5 should be everywhere but... not going to do that for now coz maybe scorev1 is just special
                    if (ScoreV2Mod.ManiaEnabled == false && elapsedTime >= toDelete.SpawnTime + (Math.GetJudgement100HitWindow() - 0.5))
                    {// in scoreV1 ONLY LATE x50 judgements are impossible and notes get despawn miss after LATE x100 judgement window passes
                        canContinue = true;
                    }
                    else if (ScoreV2Mod.ManiaEnabled == true && elapsedTime >= toDelete.SpawnTime + Math.GetJudgement50HitWindow())
                    {// in scoreV2 late x50 judgements are possible tho
                        canContinue = true;
                    }

                    if (canContinue == false)
                    {
                        continue;
                    }

                    if (toDelete.Visibility == Visibility.Collapsed)
                    {
                        AnnihilateHitObject(toDelete);
                        i--;
                        continue;
                    }
                    else
                    {
                        ManiaNoteData n = (ManiaNoteData)TransformHitObjectToDataObject(toDelete);
                        if (n.Judgement.Judgement != (int)HitObjectJudgement.Miss
                        &&  n.Judgement.Judgement != (int)HitObjectJudgement.None)
                        {
                            // it shouldnt give miss if this occurs
                            AnnihilateHitObject(toDelete);
                            i--;
                            continue;
                        }

                        HitObjectDespawnMiss(toDelete, ManiaPlayfield.JudgementPos[n.ColumnIndex], elapsedTime);
                        AnnihilateHitObject(toDelete);
                        i--;
                    }
                }
                else if (toDelete is ManiaLongNote)
                {
                    ManiaLongNote ln = (ManiaLongNote)toDelete;
                    if (elapsedTime > toDelete.SpawnTime + Math.GetJudgement50HitWindow())
                    {
                        if (ManiaLongNote.Head(ln).Visibility == Visibility.Visible
                        && (MainWindow.replay.IsLazer == true || MainWindow.replay.StableMods.HasFlag(OsuFileParsers.Classes.Replay.Mods.ScoreV2)))
                        {
                            ln.WasHoldBroken = true;
                            HitObjectDespawnMiss(toDelete, ManiaPlayfield.JudgementPos[ln.ColumnIndex], elapsedTime);
                            ManiaLongNote.Head((ManiaLongNote)toDelete).Visibility = Visibility.Collapsed;
                        }
                    
                        if (ManiaLongNote.Tail(ln).Visibility == Visibility.Collapsed
                        ||  (ln.CanBeJudged == false && ln.SpawnTime > Math.GetJudgement0HitWindow()))
                        {
                            AnnihilateHitObject(toDelete);
                            i--;
                            continue; // continue since this means long note died rip
                        }
                    }
                    
                    if (ScoreV2Mod.ManiaEnabled == true)
                    {
                        // long note tail have more lenient judgements, which is base judgement window * 1.5
                        if (elapsedTime > ln.EndTime + (Math.GetJudgement50HitWindow() * 1.5))
                        {
                            ManiaLongNoteData lnd = (ManiaLongNoteData)TransformHitObjectToDataObject(toDelete);
                            if (lnd.TailJudgement.Judgement != (int)HitObjectJudgement.Miss
                            &&  lnd.TailJudgement.Judgement != (int)HitObjectJudgement.None)
                            {
                                // it shouldnt give miss if this occurs
                                AnnihilateHitObject(toDelete);
                                i--;
                                continue;
                            }

                            HitJudgementManager.ManiaApplyTailJudgement((ManiaLongNote)toDelete, ManiaPlayfield.JudgementPos[ln.ColumnIndex], elapsedTime, HitObjectJudgement.Miss);
                            AnnihilateHitObject(toDelete);
                            i--;
                        }
                    }
                    else // replay was played on stable with scoreV1
                    {
                        //if (ln.CanBeJudged == false && ln.SpawnTime > elapsedTime)
                        //{
                        //    AnnihilateHitObject(toDelete);
                        //    i--;
                        //    continue;
                        //}
                        //if (elapsedTime > ln.SpawnTime + Math.GetJudgement100HitWindow())// && ln.ClassicHeadHitError == -1)
                        //{
                        if (ln.EndTime - ln.SpawnTime > Math.GetJudgement0HitWindow() && ln.ClassicHeadHitError != -1)
                        {
                            if (ln.IsHolding == true && ln.EndTime - ln.SpawnTime < Math.GetJudgement50HitWindow())
                            {// in specifically scoreV1, if you hit head and never release tail, you can still get even x200 lol
                                ManiaHitDetection.GetHitJudgment(ln, elapsedTime, ManiaPlayfield.JudgementPos[ln.ColumnIndex], true);
                                AnnihilateHitObject(toDelete);
                                i--;
                                continue;
                            }
                            else if (ln.WasHoldBroken == true && ln.IsHolding == false 
                                 &&  (elapsedTime > ln.EndTime - Math.GetJudgement50HitWindow()
                                 ||   elapsedTime > ln.SpawnTime && ln.ClassicHeadHitError > Math.GetJudgement50HitWindow()))
                            {
                            
                                HitJudgementManager.ApplyJudgement((ManiaLongNote)toDelete, ManiaPlayfield.JudgementPos[ln.ColumnIndex], elapsedTime, HitObjectJudgement.Miss);
                                AnnihilateHitObject(toDelete);
                                i--;
                                continue;
                            }
                        }
                        else if (elapsedTime > ln.SpawnTime && ln.WasHoldBroken == true 
                             &&  ln.ClassicHeadHitError != -1 && ln.ClassicTailHitError == -1)
                        {
                            //HitJudgementManager.ApplyJudgement((ManiaLongNote)toDelete, ManiaPlayfield.JudgementPos[ln.ColumnIndex], elapsedTime, HitObjectJudgement.Miss);
                            //AnnihilateHitObject(toDelete);
                            //i--;
                            //continue;
                        }
                        //}
                        // miss counts in groups: 1, 1, 1, 8, 1, 1, 1, 5 for a total of 19
                        // ^ ok this replay work correctly... now onto another replay
                        // replay2 ok tosu is op: 1, 1, 1, 2, 1, 1, (1, 6), 1, 6, 3, 2 for a total of 26
                        // first 2 misses i feel like are too early in first 6

                        // oh my god i hate it here zenith ln dan my eyes hurt
                        // replay3: (group of 3)2, 1, (group of 2)1, BREAK, 1, (fake miss), 25..., BREAK
                        // , part 3 no misses, BREAK, 1, 2, 1, 1, 1 for a total of 36
                        // so many misses in wrong places... maybe i should give up LOL

                        bool canBeRemoved = false;
                        if (ln.ClassicHeadHitError == -1 && ln.IsHolding == false 
                        &&  elapsedTime > ln.SpawnTime + Math.GetJudgement50HitWindow())
                        {
                            canBeRemoved = true;
                        }
                        else if (ln.IsHolding == true && elapsedTime > ln.EndTime + Math.GetJudgement50HitWindow())
                        {
                            canBeRemoved = true;
                        }

                        if (canBeRemoved)
                        {
                            ManiaLongNoteData lnd = (ManiaLongNoteData)TransformHitObjectToDataObject(toDelete);
                            if (lnd.Judgement.Judgement != (int)HitObjectJudgement.Miss
                            &&  lnd.Judgement.Judgement != (int)HitObjectJudgement.None)
                            {
                                // it shouldnt give miss if this occurs
                                //AnnihilateHitObject(toDelete);
                                //i--;
                                //continue;
                            }

                           
                            if (ln.IsHolding == true)
                            {// in specifically scoreV1, if you hit head and never release tail, you can still get even x200 lol
                                ManiaHitDetection.GetHitJudgment(ln, elapsedTime, ManiaPlayfield.JudgementPos[ln.ColumnIndex], true);
                            }
                            else
                            {
                                HitJudgementManager.ApplyJudgement((ManiaLongNote)toDelete, ManiaPlayfield.JudgementPos[ln.ColumnIndex], elapsedTime, HitObjectJudgement.Miss);
                            }

                            AnnihilateHitObject(toDelete);
                            i--;
                        }
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
                        i--;
                        continue;
                    }

                    HitObjectDespawnMiss(toDelete, TaikoPlayfield.JudgementPosition, elapsedTime);
                    AnnihilateHitObject(toDelete);
                    i--;
                }
                else if (toDelete is TaikoDrumRoll)
                {// this doesnt cause any misses it is just for score which i dont care about
                    TaikoDrumRoll drumRoll = (TaikoDrumRoll)toDelete;
                    if (elapsedTime >= drumRoll.EndTime)
                    {
                        AnnihilateHitObject(toDelete);
                        i--;
                    }
                }
                else if (toDelete is TaikoSpinner && elapsedTime >= toDelete.SpawnTime + Math.GetJudgement100HitWindow())
                {// and this is the same thing, no miss and doesnt matter
                    AnnihilateHitObject(toDelete);
                    i--;
                }
                else if (toDelete is CatchFruit && Canvas.GetTop(toDelete) > CatchPlayfield.Playfield.Height)
                {
                    AnnihilateHitObject(toDelete);
                    i--;
                }
                else if (toDelete is CatchJuiceStream)
                {// no way javascript???
                    CatchJuiceStream js = (CatchJuiceStream)toDelete;
                    if (Canvas.GetTop(js) + Canvas.GetTop(CatchJuiceStream.Tail(js)) > CatchPlayfield.Playfield.Height)
                    {
                        AnnihilateHitObject(toDelete);
                        i--;
                    }
                }
                else if (toDelete is CatchBananaShower)
                {// no judgements
                    CatchBananaShower bs = (CatchBananaShower)toDelete;
                    if (elapsedTime >= bs.EndTime)
                    {
                        AnnihilateHitObject(toDelete);
                        i--;
                    }
                }
            }
        }

        public static void HitObjectDespawnMiss(HitObject hitObject, double diameter, long time)
        {
            Vector2 missPosition = hitObject.BaseSpawnPosition;

            float X = (float)(missPosition.X * MainWindow.OsuPlayfieldObjectScale - diameter / 2);
            float Y = (float)(missPosition.Y * MainWindow.OsuPlayfieldObjectScale - diameter);

            HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), time, 0);
        }

        public static void HitObjectDespawnMiss(HitObject hitObject, Vector2 pos, long time)
        {
            HitJudgementManager.ApplyJudgement(hitObject, pos, time, HitObjectJudgement.Miss);
        }

        private static void SliderEndDespawnJudgement(Slider s, double diameter, long time)
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

            if ((StrictTrackingMod.IsStrictTrackingEnabled == true && SliderEndJudgement.IsJudged == false
            ||   StrictTrackingMod.IsStrictTrackingEnabled == false) && SliderEndJudgement.IsTracking == false)
            {
                if (MainWindow.IsReplayPreloading == true)
                {
                    OsuSliderData sd = (OsuSliderData)TransformHitObjectToDataObject(s);
                    sd.MissedEventsCount++;
                }
                HitJudgementManager.ApplyJudgement(s, new Vector2(X, Y), time, HitObjectJudgement.SliderEndMiss);
            }
            else if (SliderEndJudgement.IsTracking == true)
            {
                HitJudgementManager.ApplyJudgement(s, new Vector2(X, Y), time, HitObjectJudgement.SliderEndHit);
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
                return o.SpawnTime;
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
                return o.SpawnTime;
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

        private static double AdditionalVisualSpawnTime()
        {
            switch (MainWindow.replay.GameMode)
            {
                case OsuFileParsers.Classes.Replay.GameMode.Osu:
                    return Math.GetApproachRateTiming();
                case OsuFileParsers.Classes.Replay.GameMode.OsuMania:
                    return ManiaPlayfield.ScrollSpeed;
                case OsuFileParsers.Classes.Replay.GameMode.OsuTaiko:
                    return TaikoPlayfield.ScrollSpeed;
                case OsuFileParsers.Classes.Replay.GameMode.OsuCatch:
                    return CatchPlayfield.ScrollSpeed;
                default:
                    throw new Exception("how in the fuxk");
            }
        }
    }
}
