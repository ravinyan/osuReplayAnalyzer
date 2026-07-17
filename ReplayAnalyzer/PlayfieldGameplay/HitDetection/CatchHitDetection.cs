using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using static ReplayAnalyzer.HitObjects.Catch.CatchJuiceStream;

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

            HitJudgementManager.ApplyCatchJudgement(new Vector2((float)(Canvas.GetLeft(CatchPlayfield.Catcher) + CatchPlayfield.Catcher.Width / 2),
                                                                (float)(Canvas.GetTop(CatchPlayfield.Catcher) + 50))
                                                   , hitTime, judgement);

            if (hitObject is CatchFruit)
            {
                CatchFruit fruit = (CatchFruit)hitObject;
                if (fruit.IsMissed == false)
                {
                    KillObject2(hitObject);
                }
            }
            else if (hitObject is JuiceStreamFruit)
            {
                JuiceStreamFruit sliderFruit = (JuiceStreamFruit)hitObject;
                if (sliderFruit.IsMissed == false)
                {
                    KillObject2(hitObject);
                }
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
