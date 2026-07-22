using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using System.Windows.Controls;
using static ReplayAnalyzer.HitObjects.Catch.CatchJuiceStream;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch
{
    public class CatchCatcherManager
    {
        private static ReplayFrame CatcherFrame = null!;
        private static int CatcherFrameIndex = 0;

        public static void ResetFields()
        {
            CatcherFrame = null!;
            CatcherFrameIndex = 0;
        }

        public static void UpdateCatcherMovement()
        {
            if (CatcherFrameIndex < MainWindow.replay.FramesDict.Count
            &&  CatcherFrame != MainWindow.replay.FramesDict[CatcherFrameIndex])
            {
                CatcherFrame = MainWindow.replay.FramesDict[CatcherFrameIndex];
            }

            while (CatcherFrameIndex < MainWindow.replay.FramesDict.Values.Count)
            {
                Canvas.SetLeft(CatchPlayfield.Catcher, CatcherFrame.X * MainWindow.OsuPlayfieldObjectScale - CatchPlayfield.Catcher.Width / 2);

                if (CatcherFrameIndex > 0 && CatcherFrame.X <= MainWindow.replay.FramesDict[CatcherFrameIndex - 1].X)
                {
                    CatchPlayfield.CatcherDirectionLeft = true;
                }
                else
                {
                    CatchPlayfield.CatcherDirectionLeft = false;
                }

                JudgeHitObject();

                if (GamePlayClock.TimeElapsed >= CatcherFrame.Time)
                {
                    CatcherFrameIndex++;
                    CatcherFrame = CatcherFrameIndex < MainWindow.replay.FramesDict.Count
                        ? MainWindow.replay.FramesDict[CatcherFrameIndex]
                        : MainWindow.replay.FramesDict[MainWindow.replay.FramesDict.Count - 1];
                }
                else
                {
                    break;
                }
            }
        }

        public static void UpdateCatcherPositionAfterSeek(ReplayFrame frame)
        {
            List<ReplayFrame> frames = MainWindow.replay.FramesDict.Values.ToList();
            CatcherFrameIndex = frames.IndexOf(frame);
            frames.Clear();

            UpdateCatcherMovement();
        }

        private static void JudgeHitObject()
        {// position for the correct catches/misses
            float catcherPos = (float)(CatcherFrame.X - CatchPlayfield.Catcher.Width / 2.0f);

            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            aliveObjects.Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));

            HitObject firstObject = null!;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i].Visibility != Visibility.Collapsed)
                {
                    if (aliveObjects[i] is CatchFruit)
                    {
                        CatchFruit fruit = (CatchFruit)aliveObjects[i];
                        if (fruit.IsMissed == true && CatcherFrame.Time > fruit.SpawnTime)
                        {
                            continue;
                        }
                    }

                    firstObject = aliveObjects[i];
                    break;
                }
            }

            if (firstObject == null)
            {
                return;
            }

            if (firstObject is CatchJuiceStream)
            {
                for (int i = 0; i < firstObject.Children.Count; i++)
                {
                    JuiceStreamFruit child = firstObject.Children[i] as JuiceStreamFruit;
                    if (child.Visibility == Visibility.Collapsed
                    || child.IsMissed == true && CatcherFrame.Time > child.SpawnTime)
                    {
                        continue;
                    }

                    if ((int)child.SpawnTime <= CatcherFrame.Time)
                    {
                        if (child.XPos >= catcherPos && (int)child.XPos <= catcherPos + (float)CatchPlayfield.Catcher.Width)
                        {
                            CatchHitDetection.GetHitJudgment(child, CatcherFrame.Time, HitObjectJudgement.Great);
                        }
                        else if (child.Name == "dwoplet") // to mark missed droplets
                        {
                            child.IsMissed = true;
                            CatchHitDetection.GetHitJudgment(child, CatcherFrame.Time, HitObjectJudgement.Ok);
                        }
                        else // and drops will also give misses since they break combo
                        {
                            child.IsMissed = true;
                            CatchHitDetection.GetHitJudgment(child, CatcherFrame.Time, HitObjectJudgement.Miss);
                        }
                    }
                }
            }
            else if (firstObject is CatchFruit)
            {
                if (firstObject.SpawnTime <= CatcherFrame.Time)
                {
                    if (firstObject.X >= (int)catcherPos && firstObject.X <= catcherPos + (float)CatchPlayfield.Catcher.Width)
                    {
                        CatchHitDetection.GetHitJudgment(firstObject, CatcherFrame.Time, HitObjectJudgement.Great);
                    }
                    else
                    {
                        CatchFruit f = (CatchFruit)firstObject;
                        f.IsMissed = true;
                        CatchHitDetection.GetHitJudgment(firstObject, CatcherFrame.Time, HitObjectJudgement.Miss);
                    }
                }
            }
        }
    }
}
