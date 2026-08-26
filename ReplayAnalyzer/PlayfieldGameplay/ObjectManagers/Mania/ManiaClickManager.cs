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
        public static ReplayFrame ManiaFrame { get; private set; } = MainWindow.replay.FramesDict[0];
        private static int ManiaFrameIndex = 0;

        private static int StartIndex = 3;
        private static int K1Value = (int)Clicks.ManiaK1;

        public static void ResetFields()
        {
            ManiaFrame = MainWindow.replay.FramesDict[0];
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
                if (ManiaFrame.Time > GamePlayClock.TimeElapsed)
                {// to prevent 1 frame inaccuracy with the clicks when seeking by frame
                    break;
                }

                int columnCount = (int)MainWindow.map.Difficulty.CircleSize;

                HitObjectManager.GetAliveHitObjects().Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));
                List<HitObject> notes = HitObjectManager.GetAliveHitObjects();
                
                // vibro doesnt work... why?
                // it LOOKS like there is sometimes ONE click too early that will snowball into non stop misses
                // ok i found it the note could EASILY be clicked and in replay column key WAS clicked but note wasnt judged at all
                // there was not any note below... it just didnt hit it... when judgement would be x200 almost x300
                // nani the fuck... well if osu stable can play it correctly then i can do it too... somehow
                // actually i have no clue how does this even work since it goes against all of logic of this game systems
                // maybe it is secret notelock mechanic which how do i even guess how it works coz i cant see osu stable code
                
                // maybe i will publish this release of all game mode compatibility and then slowly try and hit my head
                // against the wall until i figure it out or i just die
                for (int column = 0; column < columnCount; column++)
                {
                    if (ManiaFrame.Clicks.Contains((Clicks)column + K1Value))
                    {// active clicks change needs to be AFTER judge notes functions
                        UpdateClickUI(column, columnCount, Visibility.Visible);
                        JudgeNotes(notes, column);
                        ManiaPlayfield.ActiveClicks[column] = true;
                    }
                    else
                    {
                        UpdateClickUI(column, columnCount, Visibility.Collapsed);
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
            ManiaFrame = MainWindow.replay.FramesDict[ManiaFrameIndex];
            frames.Clear();
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

        private static void UpdateClickUI(int column, int columnCount, Visibility v)
        {
            if (MainWindow.IsReplayPreloading == false)
            {// to make preloading faster
                ManiaPlayfield.Playfield.Children[StartIndex + 2 * column].Visibility = v;
                ManiaPlayfield.Playfield.Children[(StartIndex + (2 * columnCount)) + column - 1].Visibility = v;
            }
        }
    }
}
