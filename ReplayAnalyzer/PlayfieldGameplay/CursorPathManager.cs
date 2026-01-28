using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.CursorPath;
using ReplayAnalyzer.GameClock;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class CursorPathManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static List<CursorPath> AliveCursorPaths = new List<CursorPath>();
        private static List<CursorPathData> AliveCursorPathsData = new List<CursorPathData>();

        public static CursorPathData CurrentCursorPath = null;
        public static int CursorPathIndex = 0;

        public static void ResetFields()
        {
            AliveCursorPaths.Clear();
            AliveCursorPathsData.Clear();
            CurrentCursorPath = null;
            CursorPathIndex = 0;
        }

        public static void UpdateCursorPath()
        {
            GetCurrentCursorPath(ref CurrentCursorPath, CursorPathIndex);
            SpawnCursorPath(CurrentCursorPath, CursorPathIndex);
        }

        private static void SpawnCursorPath(CursorPathData cursorPathData, int index)
        {
            if (!AliveCursorPathsData.Contains(cursorPathData) && index < CursorPathData.CursorPathsData.Count
            &&  GamePlayClock.TimeElapsed >= cursorPathData.SpawnTime)
            {
                CursorPath path = CursorPath.Create(index);
                if (path != null)
                {
                    AliveCursorPathsData.Add(cursorPathData);
                    Window.playfieldCanva.Children.Add(path);
                    AliveCursorPaths.Add(path);
                    CursorPathIndex++;
                }
            }
        }

        private static void GetCurrentCursorPath(ref CursorPathData path, int index)
        {
            if (index >= CursorPathData.CursorPathsData.Count)
            {
                return;
            }

            if (index < CursorPathData.CursorPathsData.Count && path != CursorPathData.CursorPathsData[index])
            {
                path = CursorPathData.CursorPathsData[index];
            }
        }

        // i know i can use binary search from hit markers but i wont do that here coz performance here doesnt matter
        // and in hit markers it matters since preload calls it as many times as there are frames in replay
        public static void GetCursorPathAfterSeek(ReplayFrame frame)
        {
            List<ReplayFrame> frames = MainWindow.replay.FramesDict.Values.ToList();
            CursorPathIndex = frames.IndexOf(frame);
            frames.Clear();
        }

        public static void HandleAliveCursorPaths()
        {
            for (int i = 0; i < AliveCursorPaths.Count; i++)
            {
                CursorPath path = AliveCursorPaths[i];
                if (GamePlayClock.TimeElapsed > path.EndTime || GamePlayClock.TimeElapsed < path.SpawnTime)
                {
                    AliveCursorPaths.Remove(path);
                    Window.playfieldCanva.Children.Remove(path);
                    AliveCursorPathsData.Remove(AliveCursorPathsData[i]);
                }
            }
        }

        public static List<CursorPath> GetAliveCursorPaths()
        {
            return AliveCursorPaths;
        }

        public static List<CursorPathData> GetAliveDataCursorPaths()
        {
            return AliveCursorPathsData;
        }
    }
}
