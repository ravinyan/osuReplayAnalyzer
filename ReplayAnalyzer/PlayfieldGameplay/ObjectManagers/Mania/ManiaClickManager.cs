using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Numerics;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Mania
{
    public class ManiaClickManager
    {
        public static ReplayFrame ManiaFrame { get; set; } = null!;
        private static int ManiaFrameIndex = 0;

        private static int StartIndex = 3;
        private static int K1Value = (int)Clicks.ManiaK1;

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

            while (ManiaFrameIndex < MainWindow.replay.FramesDict.Values.Count)
            {
                int columnCount = (int)MainWindow.map.Difficulty.CircleSize;

                HitObjectManager.GetAliveHitObjects().Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));
                List<HitObject> notes = HitObjectManager.GetAliveHitObjects();

                for (int column = 0; column < columnCount; column++)
                {
                    if (ManiaFrame.Clicks.Contains((Clicks)column + K1Value))
                    {// active clicks change needs to be AFTER judge notes functions
                        UpdateClickUI(column, columnCount, 0.5, 1);
                        JudgeNotes(notes, column);
                        ManiaPlayfield.ActiveClicks[column] = true;
                    }
                    else
                    {
                        UpdateClickUI(column, columnCount, 0, 0);
                        JudgeNoteTails(notes, column);
                        ManiaPlayfield.ActiveClicks[column] = false;
                    }
                }

                if (GamePlayClock.TimeElapsed >= ManiaFrame.Time)
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
            if (PlayfieldManager.IsReplayPlayingForward == false)
            {
                return;
            }

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
                    if (ManiaLongNote.Tail(ln).Visibility == Visibility.Collapsed)
                    {
                        continue;
                    }

                    // doto seeking and meh/misses judgement improvements head too fried to figure this out
                    if (ManiaPlayfield.ActiveClicks[column] == false && ln.ColumnIndex == column)
                    {
                        if (ManiaFrame.Time - ln.EndTime > Math.GetJudgement50HitWindow())
                        {// ln hold cannot be started on lenience release window (x50 * 1.5) so cause instant miss and continue loop
                            HitObjectManager.AnnihilateHitObject(ln);
                            HitJudgementManager.ManiaApplyTailJudgement(ln, new Vector2(ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition), ManiaFrame.Time, HitObjectJudgement.Miss);
                        }
                        else
                        {
                            bool a = false;
                            if (ManiaLongNote.Head(ln).Visibility == Visibility.Collapsed)
                            {// no need to check if you can hit head if it doesnt exist
                                ln.IsHolding = true;
                                continue;
                            }
                            ManiaHitDetection.GetHitJudgment(ln, ManiaFrame.Time, ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition);

                            //if (a == true)
                            //{
                            //    continue;
                            //}
                            
                            break;
                        }
                    }
                }
            }
        }

        private static OsuMath Math = new OsuMath();
        private static void JudgeNoteTails(List<HitObject> notes, int column)
        {
            if (PlayfieldManager.IsReplayPlayingForward == false)
            {
                return;
            }

            for (int j = 0; j < notes.Count; j++)
            {
                if (notes[j] is ManiaLongNote)
                {
                    ManiaLongNote ln = (ManiaLongNote)notes[j];
                    if (ln.ColumnIndex == column && ManiaPlayfield.ActiveClicks[column] == true && ln.IsHolding == true)
                    {
                        ManiaHitDetection.GetHitJudgment(ln, ManiaFrame.Time, ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition, true);
                        break;
                    }
                }
            }
        }

        private static void UpdateClickUI(int column, int columnCount, double keyOpacity, double lightingOpacity)
        {
            if (MainWindow.IsReplayPreloading == false)
            {// to make preloading faster
                ManiaPlayfield.Playfield.Children[StartIndex + 2 * column].Opacity = keyOpacity;
                ManiaPlayfield.Playfield.Children[(StartIndex + (2 * columnCount)) + column - 1].Opacity = lightingOpacity;
            }
        }
    }
}
