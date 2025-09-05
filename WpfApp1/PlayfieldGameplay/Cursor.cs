using ReplayParsers.Classes.Replay;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.GameClock;

namespace WpfApp1.PlayfieldGameplay
{
    public class Cursor
    {
        private static int CursorPositionIndex = 0;
        private static ReplayFrame CurrentFrame = null;
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void UpdateCursor()
        {
            if (CursorPositionIndex < MainWindow.replay.FramesDict.Count
            && CurrentFrame != MainWindow.replay.FramesDict[CursorPositionIndex])
            {
                CurrentFrame = MainWindow.replay.FramesDict[CursorPositionIndex];
            }

            while (CursorPositionIndex < MainWindow.replay.FramesDict.Count && GamePlayClock.TimeElapsed >= CurrentFrame.Time)
            {
                double osuScale = Math.Min(Window.playfieldCanva.Width / 512, Window.playfieldCanva.Height / 384);

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
            CursorPositionIndex = MainWindow.replay.Frames.IndexOf(frame);
        }

    }
}
