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
                        ManiaHitDetection.GetHitJudgment(n, ManiaFrame.Time, new Vector2(ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition));
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

                    HitObject nextObjectInColumn = null;
                    for (int k = 0; k < notes.Count; k++)
                    {
                        if (notes[k] is ManiaLongNote)
                        {
                            ManiaLongNote ln2 = (ManiaLongNote)notes[k];
                            if (ln2.ColumnIndex == ln.ColumnIndex && ln2.SpawnTime < ManiaFrame.Time
                            &&  column == ln.ColumnIndex)
                            {
                                //ln = ln2;
                                //break;
                            }
                        }
                        else if (notes[k] is ManiaNote)
                        {
                            // fuck me
                        }
                    }

                    //for (int k = j + 1; k < notes.Count; k++)
                    //{
                    //    if (notes[k] is not ManiaLongNote)
                    //    {
                    //        continue;
                    //    }
                    //
                    //    if (ln.SpawnTime > ManiaFrame.Time)
                    //    {
                    //        break;
                    //    }
                    //
                    //    // force miss like that??? at this point im guessing coz lazer code is so annoying to go through
                    //    //ManiaLongNote ln2 = (ManiaLongNote)notes[k];
                    //    //if (ln2.ColumnIndex == ln.ColumnIndex && ln2.SpawnTime < ManiaFrame.Time && ln.IsHolding == false)
                    //    //{
                    //    //    //HitObjectManager.AnnihilateHitObject(ln);
                    //    //    HitJudgementManager.ManiaApplyTailJudgement(ln, new Vector2(ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition), ManiaFrame.Time, HitObjectJudgement.Miss);
                    //    //    if (ManiaLongNote.Head(ln).Visibility == Visibility.Visible)
                    //    //    {
                    //    //        HitJudgementManager.ApplyJudgement(ln, new Vector2(ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition), ManiaFrame.Time, HitObjectJudgement.Miss);
                    //    //    }
                    //    //
                    //    //    ln = ln2;
                    //    //}
                    //}

                    // BLA BLA BLA I DO EVERYTHING HOW LAZER DOES BUT IT NO WORK BLA BLA BLA IM GOING TO LOSE MY MIND
                    // I DO NOT UNDERSTAND HOW LONG NOTES WORK FUCK YOU LONG NOTES
                    //if (column == 0)
                    //{
                    //
                    //    var a = (ln.SpawnTime - ManiaFrame.Time) * -1;
                    //    for (int k = j + 1; k < notes.Count; k++)
                    //    {
                    //        if (notes[k] is not ManiaLongNote)
                    //        {
                    //            continue;
                    //        }
                    //        ManiaLongNote ln2 = (ManiaLongNote)notes[k];
                    //        if (ln2.ColumnIndex == ln.ColumnIndex)
                    //        {// im only assuming here based on lazer code: if there are 2 notes (both head and tail is alive)
                    //            // N1 spawntime = 1000 | N2 spawntime = 1100
                    //            // and the time of click is > 1100 so minimum 1101 then the N1 note will get fully missed
                    //            // since N2 is now the clickable note even if judgement for meh is 130 which should qualify
                    //            // N1 to have MEH judgement... am i going insane?
                    //            if (ln2.SpawnTime - ManiaFrame.Time < 0 && ManiaLongNote.Head(ln2).Visibility == Visibility.Visible)
                    //            {
                    //
                    //                ln = ln2;
                    //            }
                    //
                    //            //break;
                    //        }
                    //    }
                    //}
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
                            if (ManiaLongNote.Head(ln).Visibility == Visibility.Collapsed)
                            {// no need to check if you can hit head if it doesnt exist
                                ln.IsHolding = true;
                                continue;
                            }

                            ManiaHitDetection.GetHitJudgment(ln, ManiaFrame.Time, new Vector2(ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition));
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
                        ManiaHitDetection.GetHitJudgment(ln, ManiaFrame.Time, new Vector2(ManiaPlayfield.ColumnWidth * column, ManiaPlayfield.JudgementYPosition), true);
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
