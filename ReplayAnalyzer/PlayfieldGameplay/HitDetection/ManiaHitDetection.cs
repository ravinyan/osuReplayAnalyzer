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

        public static void GetHitJudgment(HitObject note, long hitTime, float X, float Y, bool isTailJudgement = false)
        {
            // only meh/misses are somehow not correct everything else fixed yaaay
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

            // lazer does diff this way which is a bit better than putting this in every H variable... only a little bit...
            double diff = Math.Abs((judgementTime - hitTime) / (isTailJudgement == true ? 1.5 : 1));
            if (diff > H0 && isTailJudgement == false)
            {// exclusively for slider heads and normal notes
                // ?
                if (note is ManiaLongNote)
                {
                    ManiaLongNote ln = (ManiaLongNote)note;
                    if (ln.WasHoldBroken == true && ln.IsHolding == false)
                    {
                        ln.IsHolding = true;
                    }
                }

                return;
            }

            if (note is ManiaLongNote)
            {
                ManiaLongNote ln = (ManiaLongNote)note;
                if (ln.WasHoldBroken == true && isTailJudgement == true)
                {
                    if (diff <= H50)
                    {
                        KillNote(note, isTailJudgement);
                        URBar.ShowHit(HitObjectJudgement.Meh, note.SpawnTime - hitTime);
                        HitJudgementManager.ManiaApplyTailJudgement(ln, new Vector2(X, Y), hitTime, HitObjectJudgement.Meh);
                        return;
                    }
                    else
                    {
                        KillNote(note, isTailJudgement);
                        URBar.ShowHit(HitObjectJudgement.Meh, note.SpawnTime - hitTime);
                        HitJudgementManager.ManiaApplyTailJudgement(ln, new Vector2(X, Y), hitTime, HitObjectJudgement.Miss);
                        return;
                    }
                }

                if (ln.IsHolding == true && diff > H0)
                {
                    ln.WasHoldBroken = true;
                    ln.IsHolding = false;
                    // when hold is broken judgement should not be applied so return early
                    return;
                }
                else if (ln.IsHolding == false)
                {
                    ln.IsHolding = true;
                }
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
