using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.MusicPlayer.Controls;
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

        // slownly learning how mania judgement work one thing at a time (and losing my mind)
        public static void GetHitJudgment(HitObject note, long hitTime, float X, float Y, bool isTailJudgement = false)
        {
            double H320 = math.GetJudgement320HitWindow();
            double H300 = math.GetJudgement300HitWindow();
            double H200 = math.GetJudgement200HitWindow();
            double H100 = math.GetJudgement100HitWindow();
            double H50 = math.GetJudgement50HitWindow();
            double H0 = math.GetJudgement0HitWindow();

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
                shouldSkipJudgement = JudgeNotes((ManiaNote)note, diff, H0);
            }
            else // its long note!
            {
                // if stable/classic mode then reeding https://osu.ppy.sh/wiki/en/Gameplay/Judgement/osu%21mania#hold-notes
                if (MainWindow.replay.IsLazer == false || ClassicMod.IsClassicEnabled == true)
                {
                    //idk where to put https://github.com/ppy/osu/issues/21659
                }

                diff = Math.Abs((judgementTime - hitTime) / (isTailJudgement == true ? 1.5 : 1));
                shouldSkipJudgement = JudgeLongNotes((ManiaLongNote)note, diff, hitTime, isTailJudgement, new Vector2(X, Y), H0, H50);
            }

            if (shouldSkipJudgement == true)
            {
                return;
            }

            if (judgement == HitObjectJudgement.Perfect || diff <= H320)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Perfect);
                URBar.ShowHit(HitObjectJudgement.Perfect, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Great || diff <= H300)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Great);
                URBar.ShowHit(HitObjectJudgement.Great, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Good || diff <= H200)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Good);
                URBar.ShowHit(HitObjectJudgement.Good, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Ok || diff <= H100)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Ok);
                URBar.ShowHit(HitObjectJudgement.Ok, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Meh || diff <= H50)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Meh);
                URBar.ShowHit(HitObjectJudgement.Meh, judgementTime - hitTime);
            }
            else if (judgement == HitObjectJudgement.Miss || diff <= H0)
            {
                KillNote(note, isTailJudgement);
                ApplyJudgement(note, isTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Miss);
            }
        }

        // its so simple...
        private static bool JudgeNotes(ManiaNote n, double diff, double H0)
        {
            if (diff > H0)
            {
                return true;
            }

            return false;
        }

        // ...what the-
        private static bool JudgeLongNotes(ManiaLongNote ln, double diff, long hitTime, bool isTailJudgement, Vector2 pos, double H0, double H50)
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
