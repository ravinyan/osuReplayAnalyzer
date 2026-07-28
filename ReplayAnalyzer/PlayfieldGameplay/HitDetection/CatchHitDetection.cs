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

            if (hitObject is CatchFruit)
            {
                CatchFruit fruit = (CatchFruit)hitObject;
                HitJudgementManager.ApplyCatchJudgement(new Vector2((float)(Canvas.GetLeft(CatchPlayfield.CatcherBox) + CatchPlayfield.CatcherBox.Width / 2),
                                                                    (float)(Canvas.GetTop(CatchPlayfield.CatcherBox) + 50))
                                                       , hitTime, judgement, fruit);

                if (fruit.IsMissed == false)
                {

                        KillObject(fruit);
                   
                }
            }
            else if (hitObject is JuiceStreamFruit)
            {
                JuiceStreamFruit sliderFruit = (JuiceStreamFruit)hitObject;
                HitJudgementManager.ApplyCatchJudgement(new Vector2((float)(Canvas.GetLeft(CatchPlayfield.CatcherBox) + CatchPlayfield.CatcherBox.Width / 2),
                                                                    (float)(Canvas.GetTop(CatchPlayfield.CatcherBox) + 50))
                                                       , hitTime, judgement, (CatchJuiceStream)sliderFruit.Parent);

                if (sliderFruit.IsMissed == false)
                {

                        KillObject(sliderFruit);
                    
                }
            }
        }

        private static void KillObject(FrameworkElement hitObject)
        {
            if (hitObject is CatchFruit)
            {
                if (MainWindow.IsReplayPreloading == true)
                {
                    HitObjectManager.AnnihilateHitObject((CatchFruit)hitObject);
                }
                else
                {
                    hitObject.Visibility = Visibility.Collapsed;
                }
            }
            else if (hitObject.Name == "tael")
            {
                if (MainWindow.IsReplayPreloading == true)
                {
                    HitObjectManager.AnnihilateHitObject((CatchJuiceStream)hitObject.Parent);
                }
                else
                {
                    hitObject.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                hitObject.Visibility = Visibility.Collapsed;
            }
        }
    }
}
