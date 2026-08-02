using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Taiko
{
    public class TaikoClickManager
    {
        public static ReplayFrame TaikoFrame = null!;
        private static int TaikoFrameIndex = 0;
        private static Clicks[] PossibleClicks = [Clicks.M1, Clicks.K1A, Clicks.M2, Clicks.K2A];


        public static void ResetFields()
        {
            TaikoFrame = null!;
            TaikoFrameIndex = 0;
        }

        public static void UpdatePlayfieldClicks()
        {
            if (TaikoFrameIndex < MainWindow.replay.FramesDict.Count
            &&  TaikoFrame != MainWindow.replay.FramesDict[TaikoFrameIndex])
            {
                TaikoFrame = MainWindow.replay.FramesDict[TaikoFrameIndex];
            }

            while (TaikoFrameIndex < MainWindow.replay.FramesDict.Count)
            {
                List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
                aliveObjects.Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));

                HitObject firstObject = null!;
                for (int i = 0; i < aliveObjects.Count; i++)
                {
                    if (aliveObjects[i].Visibility != Visibility.Collapsed)
                    {
                        firstObject = aliveObjects[i];
                        break;
                    }
                }

                for (int i = 0; i < PossibleClicks.Length; i++)
                {
                    Click(firstObject, TaikoFrame, PossibleClicks[i]);
                }

                //UpdateKeys(firstObject, frame);

                if (GamePlayClock.TimeElapsed > TaikoFrame.Time)
                {
                    TaikoFrameIndex++;
                    TaikoFrame = TaikoFrameIndex < MainWindow.replay.FramesDict.Count
                        ? MainWindow.replay.FramesDict[TaikoFrameIndex]
                        : MainWindow.replay.FramesDict[MainWindow.replay.FramesDict.Count - 1];
                }
                else
                {
                    break;
                }
            }
        }

        public static void UpdateIndexAfterSeek(ReplayFrame frame)
        {
            List<ReplayFrame> frames = MainWindow.replay.FramesDict.Values.ToList();
            TaikoFrameIndex = frames.IndexOf(frame);
            frames.Clear();

            UpdatePlayfieldClicks();
        }

        private static void UpdateKeys(HitObject hitObject, ReplayFrame f, int startIndex = 1)
        {
            // idk how else to do it i might be a bit stupid?
            // manipulating active skin elements
            // left don
            if (f.Clicks.Contains(Clicks.M1) && TaikoPlayfield.ActiveClicks[0] == false)
            {
                TaikoPlayfield.ActiveClicks[0] = true;
                ChangeClickedButtonVisibility(startIndex, 1);
                if (PlayfieldManager.IsReplayPlayingForward == true && hitObject != null)
                {
                    TaikoHitDetection.GetHitJudgment(hitObject, f.Time, TaikoPlayfield.JudgementPosition, true);
                }
            }
            else if (!f.Clicks.Contains(Clicks.M1) && TaikoPlayfield.ActiveClicks[0] == true)
            {
                TaikoPlayfield.ActiveClicks[0] = false;
                ChangeClickedButtonVisibility(startIndex, 0);
            }

            // right don
            if (f.Clicks.Contains(Clicks.K1A) && TaikoPlayfield.ActiveClicks[1] == false)
            {
                TaikoPlayfield.ActiveClicks[1] = true;
                ChangeClickedButtonVisibility(startIndex + 1, 1);
                if (PlayfieldManager.IsReplayPlayingForward == true && hitObject != null)
                {
                    TaikoHitDetection.GetHitJudgment(hitObject, f.Time, TaikoPlayfield.JudgementPosition, true);
                }
            }
            else if (!f.Clicks.Contains(Clicks.K1A) && TaikoPlayfield.ActiveClicks[1] == true)
            {
                TaikoPlayfield.ActiveClicks[1] = false;
                ChangeClickedButtonVisibility(startIndex + 1, 0);
            }

            // left kat
            if (f.Clicks.Contains(Clicks.M2) && TaikoPlayfield.ActiveClicks[2] == false)
            {
                TaikoPlayfield.ActiveClicks[2] = true;
                ChangeClickedButtonVisibility(startIndex + 2, 1);
                if (PlayfieldManager.IsReplayPlayingForward == true && hitObject != null)
                {
                    TaikoHitDetection.GetHitJudgment(hitObject, f.Time, TaikoPlayfield.JudgementPosition, false);
                }
            }
            else if (!f.Clicks.Contains(Clicks.M2) && TaikoPlayfield.ActiveClicks[2] == true)
            {
                TaikoPlayfield.ActiveClicks[2] = false;
                ChangeClickedButtonVisibility(startIndex + 2, 0);
            }

            // right cat
            if (f.Clicks.Contains(Clicks.K2A) && TaikoPlayfield.ActiveClicks[3] == false)
            {
                TaikoPlayfield.ActiveClicks[3] = true;
                ChangeClickedButtonVisibility(startIndex + 3, 1);
                if (PlayfieldManager.IsReplayPlayingForward == true && hitObject != null)
                {
                    TaikoHitDetection.GetHitJudgment(hitObject, f.Time, TaikoPlayfield.JudgementPosition, false);
                }
            }
            else if (!f.Clicks.Contains(Clicks.K2A) && TaikoPlayfield.ActiveClicks[3] == true)
            {
                TaikoPlayfield.ActiveClicks[3] = false;
                ChangeClickedButtonVisibility(startIndex + 3, 0);
            }
        }

        private static void Click(HitObject hitObject, ReplayFrame f, Clicks click)
        {
            int buttonIndex = GetButtonIndex(click);
            if (f.Clicks.Contains(click) && TaikoPlayfield.ActiveClicks[buttonIndex] == false)
            {
                TaikoPlayfield.ActiveClicks[buttonIndex] = true;
                ChangeClickedButtonVisibility(buttonIndex + 1, 1);
                if (PlayfieldManager.IsReplayPlayingForward && hitObject != null)
                {
                    bool isDon = (click == Clicks.M1 || click == Clicks.K1A);
                    TaikoHitDetection.GetHitJudgment(hitObject, f.Time, TaikoPlayfield.JudgementPosition, isDon);
                }
            }
            else if (!f.Clicks.Contains(click) && TaikoPlayfield.ActiveClicks[buttonIndex] == true)
            {
                TaikoPlayfield.ActiveClicks[buttonIndex] = false;
                ChangeClickedButtonVisibility(buttonIndex + 1, 0);
            }
        }

        private static void ChangeClickedButtonVisibility(int buttonIndex, int opacity)
        {
            TaikoPlayfield.Playfield.Children[buttonIndex].Opacity = opacity;
        }

        private static int GetButtonIndex(Clicks click)
        {
            switch (click)
            {
                case Clicks.M1:
                    return 0;
                case Clicks.K1A: 
                    return 1;
                case Clicks.M2:
                    return 2;
                case Clicks.K2A:
                    return 3;
                default:
                    throw new Exception("you are stupid and you know it");
            }
        }
    }
}
