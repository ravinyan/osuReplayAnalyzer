using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Numerics;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldGameplay.HitDetection
{
    public class ManiaHitDetection
    {
        private static OsuMath math = new OsuMath();
        private static double H320 => math.GetJudgement320HitWindow();
        private static double H300 => math.GetJudgement300HitWindow();
        private static double H200 => math.GetJudgement200HitWindow();
        private static double H100 => math.GetJudgement100HitWindow();
        private static double H50  => math.GetJudgement50HitWindow();
        private static double H0   => math.GetJudgement0HitWindow();

        // slownly learning how mania judgement work one thing at a time (and losing my mind)
        public static void GetHitJudgment(HitObject note, long hitTime, Vector2 pos, bool isTailJudgement = false)
        {
            int judgementTime = 0;
            HitObjectJudgement judgement;
            if (isTailJudgement == false)
            {
                judgementTime = note.SpawnTime;
                judgement = note.Judgement.Judgement;
            }
            else
            {
                ManiaLongNote ln = (ManiaLongNote)note;
                judgementTime = ln.EndTime;
                judgement = ln.TailJudgement.Judgement;
            }

            bool shouldSkipJudgement;
            double diff;
            if (note is ManiaNote)
            {
                diff = Math.Abs(judgementTime - hitTime);
                shouldSkipJudgement = JudgeNotes((ManiaNote)note, diff);
            }
            else // its long note! ITS NOT WORKING I HATE LONG NOTES
            {
                // if stable/classic mode then reeding https://osu.ppy.sh/wiki/en/Gameplay/Judgement/osu%21mania#hold-notes
                if (MainWindow.replay.IsLazer == false || ClassicMod.IsClassicEnabled == true)
                {
                    //idk where to put https://github.com/ppy/osu/issues/21659
                }

                diff = Math.Abs((judgementTime - hitTime) / (isTailJudgement == true ? 1.5 : 1));
                shouldSkipJudgement = JudgeLongNotes((ManiaLongNote)note, diff, hitTime, isTailJudgement, pos);
            }

            if (shouldSkipJudgement == true)
            {
                return;
            }

            // LN tail is not yet missed AND head is not hittable, but also is not in hold state > player tries to click it >
            // > there is note/LN head that CAN be hit above LN tail > force miss the LN tail player tried to hit >
            // > and lastly judge the note/LN head that was above the tail
            (bool forceKill, ManiaLongNote objectToMiss) result;
            if (note is ManiaNote && isTailJudgement == false)
            {
                //result = ShouldForceKillLNTail((ManiaNote)note, HitObjectManager.GetAliveHitObjects(), new Vector2(X, Y), hitTime, diff, H0, H50);
                ShouldForceKillLNTail((ManiaNote)note, HitObjectManager.GetAliveHitObjects(), pos, hitTime, ref diff);
            }
            else if (note is ManiaLongNote && isTailJudgement == false)
            {
                //note = ForceMissIfNotHittable((ManiaLongNote)note, HitObjectManager.GetAliveHitObjects(), pos, hitTime, ref diff);
                //result = 
                ShouldForceKillLNTail((ManiaLongNote)note, HitObjectManager.GetAliveHitObjects(), pos, hitTime, ref diff);
            }

            //if (result.forceKill == true)
            //{
            //    if (ManiaLongNote.Head(result.objectToMiss).Visibility == Visibility.Visible)
            //    {
            //        KillNote(result.objectToMiss, false);
            //        ApplyJudgement(result.objectToMiss, false, pos, hitTime, HitObjectJudgement.Miss);
            //    }
            //    KillNote(result.objectToMiss, true);
            //    ApplyJudgement(result.objectToMiss, true, pos, hitTime, HitObjectJudgement.Miss);
            //}
            // 11 25
            // 15 33
            // 4 8
            // 21 52

            // 42
            // 4

            if (judgement == HitObjectJudgement.Perfect || diff <= H320)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, pos, hitTime, HitObjectJudgement.Perfect);
                URBar.ShowHit(HitObjectJudgement.Perfect, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Great || diff <= H300)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, pos, hitTime, HitObjectJudgement.Great);
                URBar.ShowHit(HitObjectJudgement.Great, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Good || diff <= H200)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, pos, hitTime, HitObjectJudgement.Good);
                URBar.ShowHit(HitObjectJudgement.Good, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Ok || diff <= H100)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, pos, hitTime, HitObjectJudgement.Ok);
                URBar.ShowHit(HitObjectJudgement.Ok, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Meh || diff <= H50)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, pos, hitTime, HitObjectJudgement.Meh);
                URBar.ShowHit(HitObjectJudgement.Meh, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Miss || diff <= H0)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, pos, hitTime, HitObjectJudgement.Miss);
            }
        }

        // its so simple...
        private static bool JudgeNotes(ManiaNote n, double diff)
        {
            if (diff > H0)
            {
                return true;
            }

            return false;
        }

        // ...what the-
        private static bool JudgeLongNotes(ManiaLongNote ln, double diff, long hitTime, bool isTailJudgement, Vector2 pos)
        {
            if (diff > H50 && diff <= H0 && isTailJudgement == false)
            {
                ln.WasHoldBroken = true;
            }

            if (isTailJudgement == true && ManiaLongNote.Head(ln).Visibility == Visibility.Visible)
            {// tail cannot be judged before head is judged
                return true;
            }

            if (isTailJudgement == false)
            {
                ln.IsHolding = true;
            }
            else
            {
                if (ln.IsHolding == true && diff > H0)
                {// if time is too far to judge tail then break hold and skip judging this long note
                    ln.WasHoldBroken = true;
                    ln.IsHolding = false;
                    return true;
                }
                else if (ln.WasHoldBroken == true && ln.IsHolding == false)
                {// this occurs if head was missed by despawning, which breaks hold state of ln... this ensures that
                 // this release will ONLY SET IsHolding as true, then NEXT release will be able to correctly judge ln tail
                 // and if next release never occurs then tail will just be missed when ln gets despawned
                    ln.IsHolding = true;
                    return true;
                }

                ln.IsHolding = false;
            }

            if (diff > H0 && isTailJudgement == false)
            {// only for heads + this is after assignment of ln.IsHolding to true/false since that is how osu does things 
                return true;
            }

            if (ln.WasHoldBroken == true && isTailJudgement == true)
            {
                KillNote(ln, isTailJudgement);
                URBar.ShowHit(HitObjectJudgement.Meh, ln.SpawnTime - hitTime);

                if (diff <= H50)
                {
                    HitJudgementManager.ManiaApplyTailJudgement(ln, pos, hitTime, HitObjectJudgement.Meh);
                }
                else
                {
                    HitJudgementManager.ManiaApplyTailJudgement(ln, pos, hitTime, HitObjectJudgement.Miss);
                }

                return true;
            }

            if (isTailJudgement == false && ManiaLongNote.Head(ln).Visibility == Visibility.Collapsed)
            {// if head doesnt exists then dont judge
                return true;
            }

            return false;
        }

        // how to write it nicely... this code makes me want to vomit
        //private static (bool forceKill, ManiaLongNote objectToMiss) ShouldForceKillLNTail(ManiaNote clickedNote, List<HitObject> list, Vector2 pos, long hitTime, double diff, double H0, double H50)
        private static void ShouldForceKillLNTail(ManiaNote clickedNote, List<HitObject> list, Vector2 pos, long hitTime, ref double diff)
        {
            foreach (HitObject o in list)
            {
                if (o is ManiaLongNote)
                {
                    ManiaLongNote listLN = (ManiaLongNote)o;

                    if (ManiaLongNote.Tail(listLN).Visibility == Visibility.Visible
                    &&  listLN.ColumnIndex == clickedNote.ColumnIndex && listLN.EndTime < clickedNote.SpawnTime && diff <= H50)
                    {
                        //if (ManiaLongNote.Head(listLN).Visibility == Visibility.Visible
                        //&&  clickedNote.Visibility == Visibility.Visible 
                        //&&  diff > H50 && diff <= H0)
                        //{
                        //    HitJudgementManager.ApplyJudgement(listLN, pos, hitTime, HitObjectJudgement.Miss);
                        //}

                        //KillNote(listLN, true);
                        //ApplyJudgement(listLN, true, pos, hitTime, HitObjectJudgement.Miss);
                        //return;
                    }
                }
            }
        }

        //private static (bool forceKill, ManiaLongNote objectToMiss) ShouldForceKillLNTail(ManiaLongNote clickedLN, List<HitObject> list, Vector2 pos, long hitTime, double diff, double H0, double H50)
        private static ManiaLongNote ShouldForceKillLNTail(ManiaLongNote clickedLN, List<HitObject> list, Vector2 pos, long hitTime, ref double diff)
        {
            foreach (HitObject o in list)
            {
                if (o is ManiaLongNote)
                {
                    ManiaLongNote listLN = (ManiaLongNote)o;

                    // condition to kill CLICKED note head + tail
                    // this feels so stupid and overcomplicated i feel like but it still doesnt work... i hate ln
                    //if (ManiaLongNote.Tail(clickedLN).Visibility == Visibility.Visible && ManiaLongNote.Head(clickedLN).Visibility == Visibility.Visible
                    //&&  ManiaLongNote.Tail(listLN).Visibility == Visibility.Visible
                    //&&  diff >= (int)H50 && diff <= H0 // hit would result in force miss
                    //&&  listLN.ColumnIndex == clickedLN.ColumnIndex && listLN.SpawnTime > clickedLN.EndTime
                    //&&  listLN.SpawnTime - hitTime <= H0)
                    //{
                    //    KillNote(clickedLN, true);
                    //    HitJudgementManager.ManiaApplyTailJudgement(clickedLN, pos, hitTime, HitObjectJudgement.Miss);
                    //    HitJudgementManager.ApplyJudgement(clickedLN, pos, hitTime, HitObjectJudgement.Miss);
                    //
                    //    clickedLN = listLN;
                    //    clickedLN.IsHolding = true;
                    //    diff = Math.Abs(listLN.SpawnTime - hitTime);
                    //
                    //    return clickedLN;
                    //}

                    // condition to kill PREVIOUS tail
                    if (ManiaLongNote.Tail(listLN).Visibility == Visibility.Visible
                    &&  ManiaLongNote.Head(clickedLN).Visibility == Visibility.Visible
                    &&  listLN.ColumnIndex == clickedLN.ColumnIndex && listLN.EndTime < clickedLN.SpawnTime && diff <= H50)
                    {
                        KillNote(listLN, true);
                        ApplyJudgement(listLN, true, pos, hitTime, HitObjectJudgement.Miss);
                        return clickedLN;
                    }
                }
            }

            return clickedLN;
        }

        private static void ApplyJudgement(HitObject note, bool isTailJudgement, Vector2 pos, long hitTime, HitObjectJudgement judgement)
        {
            if (note is ManiaLongNote && isTailJudgement == true)
            {
                HitJudgementManager.ManiaApplyTailJudgement((ManiaLongNote)note, pos, hitTime, judgement);
            }
            else
            {
                HitJudgementManager.ApplyJudgement(note, pos, hitTime, judgement);
            }
        }

        private static void KillNote(HitObject note, bool isTailJudgement)
        {
            // ok this needs to make objects collapsed coz otherwise seeking doesnt work coz of how notes work
            if (note is ManiaNote)
            {
                if (MainWindow.IsReplayPreloading == true)
                {
                    HitObjectManager.AnnihilateHitObject(note);
                }
                else
                {
                    note.Visibility = Visibility.Collapsed;
                }
            }
            else if (note is ManiaLongNote)
            {
                ManiaLongNote ln = (ManiaLongNote)note;
                if (MainWindow.IsReplayPreloading == true)
                {
                    if (isTailJudgement == true)
                    {
                        HitObjectManager.AnnihilateHitObject(ln);
                    }
                    else
                    {
                        ManiaLongNote.Head(ln).Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    if (isTailJudgement == true)
                    {
                        ManiaLongNote.Body(ln).Visibility = Visibility.Collapsed;
                        ManiaLongNote.Tail(ln).Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ManiaLongNote.Head(ln).Visibility = Visibility.Collapsed;
                    }
                }
            }
        }
    }
}
