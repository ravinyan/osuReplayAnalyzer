using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.PlayfieldGameplay.HitDetection
{
    public class CatchHitDetection
    {
        public static void GetHitJudgment(FrameworkElement hitObject, long hitTime, HitObjectJudgement judgement)
        {
            if (hitObject == null || hitObject.Visibility == Visibility.Collapsed)
            {
                return;
            }
            hitObject.Visibility = Visibility.Collapsed;
            // this is either miss or not miss for big fruits and ticks, and x100 or nothing for droplets
            // KillObject(hitObject);

            if (judgement == HitObjectJudgement.Ok)
            {
                HitJudgementManager.ApplyCatchJudgement(new Vector2((float)(Canvas.GetLeft(CatchPlayfield.Catcher) + CatchPlayfield.Catcher.Width / 2),
                                                                    (float)(Canvas.GetTop(CatchPlayfield.Catcher) + 50))
                                                       ,hitTime, HitObjectJudgement.Ok);

                KillObject2(hitObject);
            }
            else if (judgement == HitObjectJudgement.Miss)
            {
                HitJudgementManager.ApplyCatchJudgement(new Vector2((float)(Canvas.GetLeft(CatchPlayfield.Catcher) + CatchPlayfield.Catcher.Width / 2),
                                                                    (float)(Canvas.GetTop(CatchPlayfield.Catcher) + 50))
                                                       ,hitTime, HitObjectJudgement.Miss);

                KillObject2(hitObject);
            }
            else
            {
                KillObject2(hitObject);
            }
        }

        private static void KillObject2(FrameworkElement hitObject)
        {
            if (hitObject is CatchFruit)
            {
                KillObject((CatchFruit)hitObject);
            }
            else if (hitObject.Name == "tael")
            {
                KillObject((CatchJuiceStream)hitObject.Parent);
            }
            else
            {
                hitObject.Visibility = Visibility.Collapsed;
            }
        }

        private static void KillObject(HitObject fruit)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                HitObjectManager.AnnihilateHitObject(fruit);
            }
            else
            {
                fruit.Visibility = Visibility.Collapsed;
            }
        }
    }
}
