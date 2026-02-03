using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers
{
    public class CursorManager
    {
        protected static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static int CursorPositionIndex = 0;
        private static ReplayFrame CurrentFrame = null!;

        public static void ResetFields()
        {
            CursorPositionIndex = 0;
            CurrentFrame = null!;
        }

        public static void UpdateCursor()
        {
            if (CursorPositionIndex < MainWindow.replay.FramesDict.Count
            && CurrentFrame != MainWindow.replay.FramesDict[CursorPositionIndex])
            {
                CurrentFrame = MainWindow.replay.FramesDict[CursorPositionIndex];
            }

            // if statement works now just fine but just in case while is better i guess
            while (CursorPositionIndex < MainWindow.replay.FramesDict.Count && GamePlayClock.TimeElapsed >= CurrentFrame.Time)
            {
                double osuScale = MainWindow.OsuPlayfieldObjectScale;

                Canvas.SetLeft(Window.playfieldCursor, CurrentFrame.X * osuScale - Window.playfieldCursor.Width / 2);
                Canvas.SetTop(Window.playfieldCursor, CurrentFrame.Y * osuScale - Window.playfieldCursor.Width / 2);

                CursorPositionIndex++;
                CurrentFrame = CursorPositionIndex < MainWindow.replay.FramesDict.Count
                    ? MainWindow.replay.FramesDict[CursorPositionIndex]
                    : MainWindow.replay.FramesDict[MainWindow.replay.FramesDict.Count - 1];
            }
        }

        public static void UpdateCursorPositionAfterSeek(ReplayFrame frame)
        {
            List<ReplayFrame> frames = MainWindow.replay.FramesDict.Values.ToList();
            CursorPositionIndex = frames.IndexOf(frame);
            frames.Clear();
        }
    }
}
