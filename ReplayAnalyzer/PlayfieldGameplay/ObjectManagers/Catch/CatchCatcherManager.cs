using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldUI;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using static ReplayAnalyzer.HitObjects.Catch.CatchJuiceStream;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch
{
    public class CatchCatcherManager
    {
        public static ReplayFrame CatcherFrame { get; private set; } = MainWindow.replay.FramesDict[0];
        public static int CatcherFrameIndex { get; private set; } = 0;

        public static void ResetFields()
        {
            CatcherFrame = MainWindow.replay.FramesDict[0];
            CatcherFrameIndex = 0;
        }

        public static void UpdateCatcherMovement(bool skip)
        {
            if (skip)
            {
                return;
            }

            if (CatcherFrameIndex < MainWindow.replay.FramesDict.Count
            &&  CatcherFrame != MainWindow.replay.FramesDict[CatcherFrameIndex])
            {
                CatcherFrame = MainWindow.replay.FramesDict[CatcherFrameIndex];
            }

            while (CatcherFrameIndex < MainWindow.replay.FramesDict.Count)
            {
                Canvas.SetLeft(CatchPlayfield.CatcherBox, CatcherFrame.X * MainWindow.OsuPlayfieldObjectScale - CatchPlayfield.CatcherHitbox.Width / 2);

                if (CatcherFrameIndex > 0 && CatcherFrame.X == MainWindow.replay.FramesDict[CatcherFrameIndex - 1].X)
                {
                    CatchPlayfield.CatcherDirectionRight = false;
                    CatchPlayfield.CatcherDirectionLeft = false;
                }
                else if (CatcherFrameIndex > 0 && CatcherFrame.X <= MainWindow.replay.FramesDict[CatcherFrameIndex - 1].X)
                {
                    CatchPlayfield.CatcherDirectionLeft = true;
                    CatchPlayfield.CatcherDirectionRight = false;
                }
                else if (CatcherFrameIndex > 0 && CatcherFrame.X > MainWindow.replay.FramesDict[CatcherFrameIndex - 1].X)
                {
                    CatchPlayfield.CatcherDirectionRight = true;
                    CatchPlayfield.CatcherDirectionLeft = false;
                }

                JudgeHitObject();

                if (GamePlayClock.TimeElapsed >= CatcherFrame.Time)
                {
                    CatcherFrameIndex++;
                    CatcherFrame = CatcherFrameIndex < MainWindow.replay.FramesDict.Count
                        ? MainWindow.replay.FramesDict[CatcherFrameIndex]
                        : MainWindow.replay.FramesDict[MainWindow.replay.FramesDict.Count - 1];

                    if (MainWindow.IsReplayPreloading == false)
                    {
                        CatchPlayfield.UpdateGameplayLoop(true);
                    }
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
            CatcherFrame = MainWindow.replay.FramesDict[CatcherFrameIndex];
            frames.Clear();
        }

        private static void JudgeHitObject()
        {   
            if (PlayfieldManager.IsReplayPlayingForward == false)
            {
                return;
            }
            
            // position for the correct catches/misses
            float catcherPos = (float)(CatcherFrame.X - CatchPlayfield.CatcherBox.Width / 2.0f);

            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            aliveObjects.Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));

            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i].Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                if (aliveObjects[i] is CatchJuiceStream)
                {
                    for (int j = 0; aliveObjects.Count > 0 && j < aliveObjects[i].Children.Count; j++)
                    {
                        JuiceStreamFruit child = aliveObjects[i].Children[j] as JuiceStreamFruit;
                        if (child.Visibility == Visibility.Collapsed
                        ||  child.IsMissed == true && CatcherFrame.Time > child.SpawnTime)
                        {
                            continue;
                        }
                        
                        if ((int)child.SpawnTime <= CatcherFrame.Time)
                        {
                            if (MainWindow.IsReplayPreloading == false && child.Judgement != HitObjectJudgement.None)
                            {// use judgements from preloading since they will be as correct as possible except DT replays which are broken on
                             // osu! side and i probably can fix it but i dont feel the need for now
                             // also this only does something if app laggs or something weird happens coz normally everything is correct anyway
                                CreateHitMarker(child.XPos - catcherPos, true);
                                if (child.Name == "tael" && i - 1 >= 0)
                                {
                                    i--;
                                }
                                continue;
                            }

                            // I ACTUALLY DONT KNOW IF THERE SHOULD BE (int) CAST FOR child.XPos... time to make my own catch maps and test it
                            if (child.XPos >= catcherPos && child.XPos <= catcherPos + (float)CatchPlayfield.CatcherBox.Width)
                            {
                                CreateHitMarker(child.XPos - catcherPos, true);
                                CatchHitDetection.GetHitJudgment(child, CatcherFrame.Time, HitObjectJudgement.Great);
                                if (child.Name == "tael" && i - 1 >= 0)
                                {
                                    i--;
                                }
                            }
                            else if (child.Name == "dwoplet") // to mark missed droplets
                            {
                                if (child.IsMissed == false)
                                {
                                    CreateHitMarker(child.XPos - catcherPos, false);
                                }

                                if (child.IsMissed == false)
                                {
                                    child.IsMissed = true;
                                    CatchHitDetection.GetHitJudgment(child, CatcherFrame.Time, HitObjectJudgement.Ok);
                                }
                            }
                            else // and drops will also give misses since they break combo
                            {
                                if (child.IsMissed == false)
                                {
                                    CreateHitMarker(child.XPos - catcherPos, false);
                                }

                                if (child.IsMissed == false)
                                {
                                    child.IsMissed = true;
                                    CatchHitDetection.GetHitJudgment(child, CatcherFrame.Time, HitObjectJudgement.Miss);
                                    if (child.Name == "tael" && i - 1 >= 0)
                                    {
                                        i--;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (aliveObjects[i] is CatchFruit)
                {
                    CatchFruit fruit = (CatchFruit)aliveObjects[i];
                    if (fruit.IsMissed == true && CatcherFrame.Time > fruit.SpawnTime)
                    {
                        continue;
                    }

                    if (MainWindow.IsReplayPreloading == false && fruit.Judgement.Judgement != HitObjectJudgement.None)
                    {// use judgements from preloading since they will be as correct as possible except DT replays which are broken on
                     // osu! side and i probably can fix it but i dont feel the need for now
                     // also this only does something if app laggs or something weird happens coz normally everything is correct anyway
                        CreateHitMarker(fruit.X - catcherPos, true);
                        if (i - 1 >= 0)
                        {
                            i--;
                        }
                        continue;
                    }

                    if (fruit.SpawnTime <= CatcherFrame.Time)
                    {// is in cast correct for catcherPos? i feel like its not so i will just remove it for now and add it if its needed
                        if (fruit.X >= catcherPos && fruit.X <= catcherPos + (float)CatchPlayfield.CatcherBox.Width)
                        {
                            CreateHitMarker(fruit.X - catcherPos, true);
                            CatchHitDetection.GetHitJudgment(fruit, CatcherFrame.Time, HitObjectJudgement.Great);
                            if (i - 1 >= 0)
                            {
                                i--;
                            }
                        }
                        else
                        {
                            if (fruit.IsMissed == false)
                            {
                                CreateHitMarker(fruit.X - catcherPos, false);
                            }

                            if (fruit.IsMissed == false)
                            {
                                fruit.IsMissed = true;
                                CatchHitDetection.GetHitJudgment(fruit, CatcherFrame.Time, HitObjectJudgement.Miss);
                                if (i - 1 >= 0)
                                {
                                    i--;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void CreateHitMarker(double pos, bool isHit)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                return;
            }

            Line line = new Line();
            line.X1 = pos * MainWindow.OsuPlayfieldObjectScale;
            line.Y1 = CatchPlayfield.CatcherHitbox.Height;
            line.X2 = pos * MainWindow.OsuPlayfieldObjectScale;
            line.Y2 = -10;
            line.StrokeThickness = 1;
            line.Stroke = isHit == true ? ColourBank.CatcherCaught : ColourBank.CatcherMiss;

            CatchPlayfield.CatcherHitbox.Children.Add(line);
            line.Loaded += async delegate (object sender, RoutedEventArgs e)
            {
                await Task.Delay(2000);
                CatchPlayfield.CatcherHitbox.Children.Remove(line);
            };
        }
    }
}
