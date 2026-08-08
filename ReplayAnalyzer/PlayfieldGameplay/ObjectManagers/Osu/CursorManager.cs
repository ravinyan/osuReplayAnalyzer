using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Osu
{
    public class CursorManager
    {
        protected static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static ReplayFrame CursorFrame { get; set; } = null!;
        public static int CursorPositionIndex { get; private set; } = 0;

        public static void ResetFields()
        {
            CursorPositionIndex = 0;
            CursorFrame = null!;
        }

        public static void UpdateCursorPosition()
        {
            if (CursorPositionIndex < MainWindow.replay.FramesDict.Count
            &&  CursorFrame != MainWindow.replay.FramesDict[CursorPositionIndex])
            {
                CursorFrame = MainWindow.replay.FramesDict[CursorPositionIndex];
            }

            // if statement works now just fine but just in case while is better i guess
            while (CursorPositionIndex < MainWindow.replay.FramesDict.Count && GamePlayClock.TimeElapsed >= CursorFrame.Time)
            {
                double osuScale = MainWindow.OsuPlayfieldObjectScale;

                Canvas.SetLeft(OsuPlayfield.PlayfieldCursor, CursorFrame.X * osuScale - OsuPlayfield.PlayfieldCursor.Width / 2);
                Canvas.SetTop(OsuPlayfield.PlayfieldCursor, CursorFrame.Y * osuScale - OsuPlayfield.PlayfieldCursor.Width / 2);

                CursorPositionIndex++;
                CursorFrame = CursorPositionIndex < MainWindow.replay.FramesDict.Count
                    ? MainWindow.replay.FramesDict[CursorPositionIndex]
                    : MainWindow.replay.FramesDict[MainWindow.replay.FramesDict.Count - 1];
            }
        }

        public static void UpdateCursorPositionAfterSeek(ReplayFrame frame)
        {
            List<ReplayFrame> frames = MainWindow.replay.FramesDict.Values.ToList();
            CursorPositionIndex = frames.IndexOf(frame);
            frames.Clear();

            UpdateCursorPosition();
        }
    }
}
