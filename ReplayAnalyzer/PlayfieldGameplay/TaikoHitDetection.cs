using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Taiko;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Numerics;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class TaikoHitDetection
    {
        private static OsuMath math = new OsuMath();

        public static void GetHitJudgment(HitObject hitObject, long hitTime, Vector2 pos, bool isDon)
        {
            if (hitObject == null || hitObject.Visibility == System.Windows.Visibility.Collapsed)
            {
                return;
            }

            double H300 = math.GetJudgement300HitWindow();
            double H100 = math.GetJudgement100HitWindow();
            double H0 = math.GetJudgement0HitWindow();

            double diff = Math.Abs(hitObject.SpawnTime - hitTime);
            if (diff > H0)
            {
                return;
            }

            if (hitObject is TaikoHitCircle)
            {
                TaikoHitCircle circle = (TaikoHitCircle)hitObject;
                if (circle.IsDon != isDon)
                {
                    // colour hit is wrong so force miss
                    HitJudgementManager.ApplyJudgement(hitObject, pos, hitTime, HitObjectJudgement.Miss);
                    KillNote(hitObject);
                    return;
                }
            }

            if (hitObject.Judgement.Judgement == HitObjectJudgement.Great || diff <= H300)
            {
                HitJudgementManager.ApplyJudgement(hitObject, pos, hitTime, HitObjectJudgement.Great);
                URBar.ShowHit(HitObjectJudgement.Great, hitObject.SpawnTime - hitTime);
                KillNote(hitObject);
            }
            else if (hitObject.Judgement.Judgement == HitObjectJudgement.Ok || diff <= H100)
            {
                HitJudgementManager.ApplyJudgement(hitObject, pos, hitTime, HitObjectJudgement.Ok);
                URBar.ShowHit(HitObjectJudgement.Ok, hitObject.SpawnTime - hitTime);
                KillNote(hitObject);
            }
            else if (hitObject.Judgement.Judgement == HitObjectJudgement.Miss || diff <= H0)
            {
                HitJudgementManager.ApplyJudgement(hitObject, pos, hitTime, HitObjectJudgement.Miss);
                KillNote(hitObject);
            }
        }

        private static void KillNote(HitObject note)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                HitObjectManager.AnnihilateHitObject(note);
            }
            else
            {
                note.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
