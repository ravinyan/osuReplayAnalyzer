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

        //public static void GetHitJudgement()
        //{
        //
        //}

        //??
        private static void GetHitJudgementV1(ManiaLongNote note, long hitTime, Vector2 pos, bool isTailJudgement = false)
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
                judgementTime = note.EndTime;
                judgement = note.TailJudgement.Judgement;
            }

            bool shouldSkipJudgement = false;
            double diff;
            if (note is ManiaNote)
            {
                diff = Math.Abs(judgementTime - hitTime);
                //shouldSkipJudgement = JudgeNotes((ManiaNote)note, diff);
            }
            else // its long note!
            {
                diff = Math.Abs((judgementTime - hitTime) / (isTailJudgement == true ? 1.5 : 1));
                //shouldSkipJudgement = JudgeLongNotes((ManiaLongNote)note, diff, hitTime, isTailJudgement, pos);
            }

            if (shouldSkipJudgement == true)
            {
                return;
            }

            if (MainWindow.replay.IsLazer == true || MainWindow.replay.StableMods == OsuFileParsers.Classes.Replay.Mods.ScoreV2)
            {
                if (note is ManiaNote && isTailJudgement == false)
                {
                    //ManiaNote a = (ManiaNote)note;
                    //note = CheckIfLongNoteCanBeJudged(a, a.ColumnIndex, pos, hitTime, ref diff);
                }
                else if (note is ManiaLongNote && isTailJudgement == false)
                {
                    //ManiaLongNote a = (ManiaLongNote)note;
                    //note = CheckIfLongNoteCanBeJudged(a, a.ColumnIndex, pos, hitTime, ref diff);
                }
            }

            // notes for classic mod
            // apparently heads never disappear when clicked? which means what? do head hit judgement offset changes when spam
            // clicking multiple times? < it looks like a no... it saves first hit then waits for tail judgement... i think?
            // nothing makes sense bruh spam click head to get barely x50 judgement and release of said click causes judgement
            // to be x200 even tho tail release was like 500ms before any hit/miss window...
            // NVM head judgement was x100 and tail was x50 somewhere in the other side of galaxy
            // window for combined x200 judgement is 153ms but the combined judgements are >700ms
            // max head hit error is also higher than allowed for x200 yet it still got x200...
            // i might just not do it this shit is annoying how am i supposed do guess how this works if wiki info is incorrect
            // the tail and head judgements must get overwritten and have some notelock rules otherwise this makes no sense 

            if (MainWindow.replay.IsLazer == false && isTailJudgement == true && note is ManiaLongNote && diff <= H0)
            {
                GetClassicLNJudgement((ManiaLongNote)note, pos, judgementTime, hitTime);
                return;
            }
            else if (MainWindow.replay.IsLazer == false && isTailJudgement == false && note is ManiaLongNote)
            {
                // if stable mode then reeding https://osu.ppy.sh/wiki/en/Gameplay/Judgement/osu%21mania#hold-notes
                // not looking for classic mod since classic mod in lazer still has scorev2 judgements
                // save head hit error and return since there is nothing else to do
                // DO NOT REMOVE HEAD coz that is what stable does
                ManiaLongNote ln = (ManiaLongNote)note;

                if (diff <= H50)
                {
                    ln.ClassicHeadHitError = Math.Abs(judgementTime - hitTime);
                    ln.IsHolding = true;
                }

                return;
            }

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

            bool shouldSkipJudgement = false;
            double diff;
            if (note is ManiaNote)
            {
                diff = Math.Abs(judgementTime - hitTime);
                //shouldSkipJudgement = JudgeNotes((ManiaNote)note, diff);
            }
            else // its long note!
            {
                diff = Math.Abs((judgementTime - hitTime) / (isTailJudgement == true ? 1.5 : 1));
                //shouldSkipJudgement = JudgeLongNotes((ManiaLongNote)note, diff, hitTime, isTailJudgement, pos);
            }

            if (shouldSkipJudgement == true)
            {
                return;
            }

            if (MainWindow.replay.IsLazer == true || MainWindow.replay.StableMods == OsuFileParsers.Classes.Replay.Mods.ScoreV2)
            {
                if (note is ManiaNote && isTailJudgement == false)
                {
                    ManiaNote a = (ManiaNote)note;
                    note = CheckIfLongNoteCanBeJudged(a, a.ColumnIndex, pos, hitTime, ref diff);
                }
                else if (note is ManiaLongNote && isTailJudgement == false)
                {
                    ManiaLongNote a = (ManiaLongNote)note;
                    note = CheckIfLongNoteCanBeJudged(a, a.ColumnIndex, pos, hitTime, ref diff);
                }
            }

            // from one test of score v1 long note judgement
            // apparently heads never disappear when clicked? which means what? do head hit judgement offset changes when spam
            // clicking multiple times? < it looks like a no... it saves first hit then waits for tail judgement... i think?
            // nothing makes sense bruh spam click head to get barely x50 judgement and release of said click causes judgement
            // to be x200 even tho tail release was like 500ms before any hit/miss window...
            // NVM head judgement was x100 and tail was x50 somewhere in the other side of galaxy
            // window for combined x200 judgement is 153ms but the combined judgements are >700ms
            // max head hit error is also higher than allowed for x200 yet it still got x200...
            // i might just not do it this shit is annoying how am i supposed do guess how this works if wiki info is incorrect
            // the tail and head judgements must get overwritten and have some notelock rules otherwise this makes no sense 

            if (MainWindow.replay.IsLazer == false && isTailJudgement == true && note is ManiaLongNote && diff <= H50)
            {
                GetClassicLNJudgement((ManiaLongNote)note, pos, judgementTime, hitTime);
                return;
            }
            else if (MainWindow.replay.IsLazer == false && isTailJudgement == false && note is ManiaLongNote)
            {
                // if stable mode then reeding https://osu.ppy.sh/wiki/en/Gameplay/Judgement/osu%21mania#hold-notes
                // not looking for classic mod since classic mod in lazer still has scorev2 judgements
                // save head hit error and return since there is nothing else to do
                // DO NOT REMOVE HEAD coz that is what stable does
                ManiaLongNote ln = (ManiaLongNote)note;

                if (diff <= H50)
                {
                    ln.ClassicHeadHitError = diff;
                    ln.IsHolding = true;
                }

                return;
            }

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

        private static HitObject CheckIfLongNoteCanBeJudged(HitObject clickedNote, int columnIndex, Vector2 pos, long hitTime, ref double diff)
        {
            // DOES THIS REALLY WORK AM I FREE????????? OH MY GOD THIS WORKS (on 2 extremely hard maps) AAAAAAAAAAA FREEDOM!!!
            List<HitObject> list = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < list.Count; i++)
            {
                if (clickedNote == list[i])
                {
                    continue;
                }

                if ((list[i] is ManiaNote n && n.ColumnIndex == columnIndex && n.Visibility == Visibility.Visible)
                ||  (list[i] is ManiaLongNote ln && ln.ColumnIndex == columnIndex && ln.Visibility == Visibility.Visible))
                {
                    HitObject note = list[i];

                    // if normal note is clickable, ln head doesnt exists (hit or missed), then force miss ln tail
                    // and return normal note which will now be judged instead
                    if (note is ManiaLongNote && ManiaLongNote.Head((ManiaLongNote)note).Visibility == Visibility.Collapsed
                    &&  Math.Abs(clickedNote.SpawnTime - hitTime) <= H50)
                    {
                        KillNote(note, true);
                        ApplyJudgement(note, true, pos, hitTime, HitObjectJudgement.Miss);

                        return clickedNote;
                    }

                    // if long note is about to be hit and head + tail are still not hit, BUT there is normal/long note which
                    // is so far in the gameplay now that its spawn time is lower than current hit time
                    // then fully miss head + tail of long note and focus on this normal/long note
                    if (clickedNote is ManiaLongNote)
                    {
                        if (ManiaLongNote.Head((ManiaLongNote)clickedNote).Visibility == Visibility.Visible
                        &&  note.SpawnTime < hitTime)
                        {
                            KillNote(clickedNote, false); // to mark head as hit as well
                            KillNote(clickedNote, true);
                            ApplyJudgement(clickedNote, false, pos, hitTime, HitObjectJudgement.Miss);
                            ApplyJudgement(clickedNote, true, pos, hitTime, HitObjectJudgement.Miss);

                            if (note is ManiaLongNote)
                            {
                                ManiaLongNote wha = (ManiaLongNote)note;
                                wha.IsHolding = true;
                            }

                            // also only here update diff to be accurate for newly focused note
                            diff = Math.Abs(note.SpawnTime - hitTime);

                            return note;
                        }
                    }
                }
            }

            return clickedNote;
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
            // or does it... it would be ideal if the collapsed thing wasnt needed
            // THIS works... somehow it is also faster than what i previously had so cant complain i guess
            if (note is ManiaNote)
            {
                note.Visibility = Visibility.Collapsed;
            }
            else if (note is ManiaLongNote)
            {
                if (isTailJudgement == true)
                {
                    ManiaLongNote.Body((ManiaLongNote)note).Visibility = Visibility.Collapsed;
                    ManiaLongNote.Tail((ManiaLongNote)note).Visibility = Visibility.Collapsed;
                    note.Visibility = Visibility.Collapsed;
            
                }
                else
                {
                    ManiaLongNote.Head((ManiaLongNote)note).Visibility = Visibility.Collapsed;
                }
            }
        }

        // so this is logic for judging mania notes... yea... wat now coz it doesnt really work
        private static void GetClassicLNJudgement(ManiaLongNote ln, Vector2 pos, int judgementTime, long hitTime)
        {
            if (ln.WasHoldBroken == true)
            {
                KillNote(ln, true);
                ApplyJudgement(ln, false, pos, hitTime, HitObjectJudgement.Meh);
                URBar.ShowHit(HitObjectJudgement.Meh, judgementTime - hitTime);
            }

            ln.ClassicTailHitError = Math.Abs(judgementTime - hitTime);
            if (ln.ClassicHeadHitError <= H320 * 1.2 && ln.ClassicHeadHitError + ln.ClassicTailHitError < H320 * 2.4)
            {
                KillNote(ln, true);
                ApplyJudgement(ln, false, pos, hitTime, HitObjectJudgement.Perfect);
                URBar.ShowHit(HitObjectJudgement.Perfect, judgementTime - hitTime);
            }
            else if (ln.ClassicHeadHitError <= H300 * 1.1 && ln.ClassicHeadHitError + ln.ClassicTailHitError < H300 * 2.2)
            {
                KillNote(ln, true);
                ApplyJudgement(ln, false, pos, hitTime, HitObjectJudgement.Great);
                URBar.ShowHit(HitObjectJudgement.Great, judgementTime - hitTime);
            }
            else if (ln.ClassicHeadHitError <= H200 && ln.ClassicHeadHitError + ln.ClassicTailHitError < H200 * 2)
            {
                KillNote(ln, true);
                ApplyJudgement(ln, false, pos, hitTime, HitObjectJudgement.Good);
                URBar.ShowHit(HitObjectJudgement.Good, judgementTime - hitTime);
            }
            else if (ln.ClassicHeadHitError <= H100 && ln.ClassicHeadHitError + ln.ClassicTailHitError < H100 * 2)
            {
                KillNote(ln, true);
                ApplyJudgement(ln, false, pos, hitTime, HitObjectJudgement.Ok);
                URBar.ShowHit(HitObjectJudgement.Ok, judgementTime - hitTime);
            }
            else if (ln.ClassicHeadHitError > H50 || ln.ClassicTailHitError > H100)
            {
                KillNote(ln, true);
                ApplyJudgement(ln, false, pos, hitTime, HitObjectJudgement.Miss);
            }
            else// if (Math.Abs(ln.ClassicHeadHitError) <= H50 && Math.Abs(ln.ClassicHeadHitError + ln.ClassicTailHitError) < H50)
            {// "Anything else that is not a miss" wiki says then this should be correct no? or am i stupid
                KillNote(ln, true);
                ApplyJudgement(ln, false, pos, hitTime, HitObjectJudgement.Meh);
                URBar.ShowHit(HitObjectJudgement.Meh, judgementTime - hitTime);
            }
        }
    }
}
