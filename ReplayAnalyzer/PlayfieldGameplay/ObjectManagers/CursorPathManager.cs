using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.CursorPath;
using ReplayAnalyzer.GameClock;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers
{
    public class CursorPathManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static List<CursorPath> AliveCursorPaths = new List<CursorPath>();

        // this starts from 1 and spawning paths starts from index - 1 coz otherwise visuals spawn 1 index too far
        public static int CursorPathIndex = 1;

        public static void ResetFields()
        {
            AliveCursorPaths.Clear();
            CursorPathIndex = 1;
        }

        public static void NewUpdateCursorPath()
        {
            if (CursorPathIndex + 1 >= MainWindow.replay.FramesDict.Count)
            {
                return;
            }

            // this is updated based on fps and if fps is too low then without while loop this updates too slowly
            ReplayFrame frame = MainWindow.replay.FramesDict[CursorPathIndex];
            while (GamePlayClock.TimeElapsed >= frame.Time)
            { 
                CursorPath newPath = CursorPath.Create(CursorPathIndex);
                if (newPath != null)
                {
                    Window.playfieldCanva.Children.Add(newPath);
                    AliveCursorPaths.Add(newPath);

                    CursorPathIndex++;
                    frame = MainWindow.replay.FramesDict[CursorPathIndex];
                }
            }
        }

        // i know i can use binary search from hit markers but i wont do that here coz performance here doesnt matter
        // and in hit markers it matters since preload calls it as many times as there are frames in replay
        public static void GetCursorPathAfterSeek(double direction, ReplayFrame frame)
        {
            List<ReplayFrame> frames = MainWindow.replay.FramesDict.Values.ToList();
            CursorPathIndex = frames.IndexOf(frame);
            if (direction < 0)
            {// correcting index coz otherwise there is visual bug where path doesnt appear instantly after backward seeking
                CursorPathIndex++;
            }

            frames.Clear();
        }

        public static void HandleAliveCursorPaths()
        {
            for (int i = 0; i < AliveCursorPaths.Count; i++)
            {
                CursorPath path = AliveCursorPaths[i];
                if (GamePlayClock.TimeElapsed > path.EndTime || GamePlayClock.TimeElapsed <= path.SpawnTime)
                {
                    AliveCursorPaths.Remove(path);
                    Window.playfieldCanva.Children.Remove(path);
                }
            }
        }

        public static List<CursorPath> GetAliveCursorPaths()
        {
            return AliveCursorPaths;
        }
    }
}
