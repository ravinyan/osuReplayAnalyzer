using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Mania
{
    public class ManiaClickManager
    {
        public static ReplayFrame ManiaFrame = null!;
        private static int ManiaFrameIndex = 0;

        public static void ResetFields()
        {
            ManiaFrame = null!;
            ManiaFrameIndex = 0;
        }

        public static void UpdatePlayfieldClicks()
        {
            if (ManiaFrameIndex < MainWindow.replay.FramesDict.Count
            &&  ManiaFrame != MainWindow.replay.FramesDict[ManiaFrameIndex])
            {
                ManiaFrame = MainWindow.replay.FramesDict[ManiaFrameIndex];
            }

            while (ManiaFrameIndex < MainWindow.replay.FramesDict.Count)
            {
                int startIndex = 3;
                int k1Value = (int)Clicks.ManiaK1;
                int columnCount = (int)MainWindow.map.Difficulty.CircleSize;

                HitObjectManager.GetAliveHitObjects().Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));
                List<HitObject> notes = HitObjectManager.GetAliveHitObjects();
                for (int column = 0; column < columnCount; column++)
                {
                    if (ManiaFrame.Clicks.Contains((Clicks)column + k1Value))
                    {
                        if (MainWindow.IsReplayPreloading == false)
                        {// to make preloading faster
                            ManiaPlayfield.Playfield.Children[startIndex + 2 * column].Opacity = 0.5; // active click UI
                            ManiaPlayfield.Playfield.Children[(startIndex + (2 * columnCount)) + column - 1].Opacity = 1; // lightning UI
                        }

                        if (PlayfieldManager.IsReplayPlayingForward == true)
                        {
                            JudgeNotes(notes, column);
                        }
                        ManiaPlayfield.ActiveClicks[column] = true;
                    }
                    else
                    {
                        if (MainWindow.IsReplayPreloading == false)
                        {// to make preloading faster
                            ManiaPlayfield.Playfield.Children[startIndex + 2 * column].Opacity = 0; // active click UI
                            ManiaPlayfield.Playfield.Children[(startIndex + (2 * columnCount)) + column - 1].Opacity = 0; // lightning UI
                        }

                        if (PlayfieldManager.IsReplayPlayingForward == true)
                        {
                            JudgeNoteTails(notes, column);
                        }
                        ManiaPlayfield.ActiveClicks[column] = false;
                    }
                }

                if (GamePlayClock.TimeElapsed > ManiaFrame.Time)
                {
                    ManiaFrameIndex++;
                    ManiaFrame = ManiaFrameIndex < MainWindow.replay.FramesDict.Count
                        ? MainWindow.replay.FramesDict[ManiaFrameIndex]
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
            ManiaFrameIndex = frames.IndexOf(frame);
            frames.Clear();

            UpdatePlayfieldClicks();
        }

        private static void JudgeNotes(List<HitObject> notes, int column)
        {
            for (int j = 0; j < notes.Count; j++)
            {
                if (notes[j].Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                if (notes[j] is ManiaNote)
                {
                    ManiaNote n = (ManiaNote)notes[j];
                    if (n.ColumnIndex == column && ManiaPlayfield.ActiveClicks[column] == false)
                    {
                        ManiaHitDetection.GetHitJudgment(n, ManiaFrame.Time, ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition);
                        break;
                    }
                }
                else
                {
                    ManiaLongNote ln = (ManiaLongNote)notes[j];
                    if (ln.ColumnIndex == column && ManiaPlayfield.ActiveClicks[column] == false)
                    {
                        ln.HoldStarted = true;
                        ManiaHitDetection.GetHitJudgment(ln, ManiaFrame.Time, ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition);
                        break;
                    }
                }
            }
        }

        private static void JudgeNoteTails(List<HitObject> notes, int column)
        {
            for (int j = 0; j < notes.Count; j++)
            {
                if (notes[j].Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                if (notes[j] is ManiaLongNote)
                {
                    ManiaLongNote ln = (ManiaLongNote)notes[j];
                    if (ln.ColumnIndex == column && ManiaPlayfield.ActiveClicks[column] == true && ln.HoldStarted == true)
                    {
                        ManiaHitDetection.GetHitJudgment(ln, ManiaFrame.Time, ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition, true);
                        break;
                    }
                }
            }
        }
    }
}
