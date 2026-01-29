using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using System.Numerics;

namespace ReplayAnalyzer.AnalyzerTools.CursorPath
{
    public class CursorPathData
    {
        public static List<CursorPathData> CursorPathsData = new List<CursorPathData>();

        public long SpawnTime { get; set; }
        public long EndTime { get; set; }

        public Vector2 BaseLineStart { get; }
        public Vector2 LineStart = new Vector2();
        public Vector2 BaseLineEnd { get; }
        public Vector2 LineEnd = new Vector2();

        public CursorPathData(long spawnTime, long endTime, Vector2 lineStart, Vector2 lineEnd) 
        {
            SpawnTime = spawnTime;
            EndTime = endTime;

            LineStart = lineStart;
            BaseLineStart = lineStart;

            LineEnd = lineEnd;
            BaseLineEnd = lineEnd;
        }

        public static void ResetFields()
        {
            CursorPathsData.Clear();
        }

        public static void CreateData()
        {
            for (int i = 1; i < MainWindow.replay.FramesDict.Count; i++)
            {
                ReplayFrame lineStart = MainWindow.replay.FramesDict[i - 1];
                ReplayFrame lineEnd = MainWindow.replay.FramesDict[i];

                CursorPathsData.Add(new CursorPathData(lineStart.Time, lineStart.Time + HitMarkerData.ALIVE_TIME, new Vector2(lineStart.X, lineStart.Y), new Vector2(lineEnd.X, lineEnd.Y)));
            }
        }
    }
}
