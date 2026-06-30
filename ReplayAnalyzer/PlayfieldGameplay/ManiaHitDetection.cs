using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Numerics;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class ManiaHitDetection
    {
        private static OsuMath math = new OsuMath();

        public static void GetHitJudgment(HitObject note, long hitTime, float X, float Y, bool isLongNoteTailJudgement = false)
        {
            if (note.Visibility == System.Windows.Visibility.Collapsed)
            {
                return;
            }

            double H320 = math.GetJudgement320HitWindow() * (isLongNoteTailJudgement == true ? 1.5 : 1);
            double H300 = math.GetJudgement300HitWindow() * (isLongNoteTailJudgement == true ? 1.5 : 1);
            double H200 = math.GetJudgement200HitWindow() * (isLongNoteTailJudgement == true ? 1.5 : 1);
            double H100 = math.GetJudgement100HitWindow() * (isLongNoteTailJudgement == true ? 1.5 : 1);
            double H50 = math.GetJudgement50HitWindow() * (isLongNoteTailJudgement == true ? 1.5 : 1);
            double H0 = math.GetJudgement0HitWindow() * (isLongNoteTailJudgement == true ? 1.5 : 1);

            int judgementTime = 0;
            if (isLongNoteTailJudgement == false)
            {
                judgementTime = note.SpawnTime;
            }
            else
            {
                ManiaLongNote ln = (ManiaLongNote)note;
                judgementTime = ln.EndTime;
            }

            double diff = Math.Abs(judgementTime - hitTime);
            if (diff > H0)
            {
                return;
            }

            if (note is ManiaLongNote)
            {
                ManiaLongNote ln = (ManiaLongNote)note;
                if (ln.WasHoldBroken == true && isLongNoteTailJudgement == true)
                {
                    if (diff <= H50 && diff >= -H50)
                    {
                        KillNote(note, isLongNoteTailJudgement);
                        URBar.ShowHit(HitObjectJudgement.Meh, note.SpawnTime - hitTime);
                        HitJudgementManager.ManiaApplyTailJudgement(ln, new Vector2(X, Y), hitTime, HitObjectJudgement.Meh);
                    }
                    else
                    {
                        KillNote(note, isLongNoteTailJudgement);
                        URBar.ShowHit(HitObjectJudgement.Miss, note.SpawnTime - hitTime);
                        HitJudgementManager.ManiaApplyTailJudgement(ln, new Vector2(X, Y), hitTime, HitObjectJudgement.Meh);
                    }
                }

                if (diff > H0 && isLongNoteTailJudgement == true)
                {
                    ln.WasHoldBroken = true;
                }
            }

            if (note.Judgement.Judgement == HitObjectJudgement.Perfect || diff <= H320)
            {
                KillNote(note, isLongNoteTailJudgement);
                ApplyJudgement(note, isLongNoteTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Perfect);
                URBar.ShowHit(HitObjectJudgement.Perfect, judgementTime - hitTime);
            }
            else if (note.Judgement.Judgement == HitObjectJudgement.Great || diff <= H300)
            {
                KillNote(note, isLongNoteTailJudgement);
                ApplyJudgement(note, isLongNoteTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Great);
                URBar.ShowHit(HitObjectJudgement.Great, judgementTime - hitTime);
            }
            else if (note.Judgement.Judgement == HitObjectJudgement.Good || diff <= H200)
            {
                KillNote(note, isLongNoteTailJudgement);
                ApplyJudgement(note, isLongNoteTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Good);
                URBar.ShowHit(HitObjectJudgement.Good, judgementTime - hitTime);
            }
            else if (note.Judgement.Judgement == HitObjectJudgement.Ok || diff <= H100)
            {
                KillNote(note, isLongNoteTailJudgement);
                ApplyJudgement(note, isLongNoteTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Ok);
                URBar.ShowHit(HitObjectJudgement.Ok, judgementTime - hitTime);
            }
            else if (note.Judgement.Judgement == HitObjectJudgement.Meh || diff <= H50)
            {
                KillNote(note, isLongNoteTailJudgement);
                ApplyJudgement(note, isLongNoteTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Meh);
                URBar.ShowHit(HitObjectJudgement.Meh, judgementTime - hitTime);
            }
            else if (note.Judgement.Judgement == HitObjectJudgement.Miss || diff <= H0)
            {
                KillNote(note, isLongNoteTailJudgement);
                ApplyJudgement(note, isLongNoteTailJudgement, new Vector2(X, Y), hitTime, HitObjectJudgement.Miss);
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
            if (note is ManiaNote || (note is ManiaLongNote && isTailJudgement == true))
            {
                if (MainWindow.IsReplayPreloading == true)
                {
                    HitObjectManager.AnnihilateHitObject(note);
                }
                else
                {
                    note.Visibility = System.Windows.Visibility.Collapsed;
                }

                if (note is ManiaLongNote)
                {
                    ManiaLongNote ln = (ManiaLongNote)note;
                    ln.TailJudged = true;
                }
            }
        }
    }
}
