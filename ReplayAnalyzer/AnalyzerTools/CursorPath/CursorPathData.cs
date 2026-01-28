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
        public List<Vector2> BaseLineCoordinates { get; }
        public List<Vector2> LineCoordinates = new List<Vector2>();
        public Vector2 BasePosition { get; }
        public Vector2 Position = new Vector2();

        public CursorPathData(long spawnTime, long endTime, Vector2 position, List<Vector2> linePosition) 
        {
            SpawnTime = spawnTime;
            EndTime = endTime;

            Position = position;
            BasePosition = position;

            LineCoordinates = linePosition;
            BaseLineCoordinates = linePosition;
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
                List<Vector2> coordinates = new List<Vector2>
                {
                    new Vector2(lineStart.X, lineStart.Y),
                    new Vector2(lineEnd.X, lineEnd.Y),
                };

                CursorPathsData.Add(new CursorPathData(lineStart.Time, lineStart.Time + HitMarkerData.ALIVE_TIME, new Vector2(lineStart.X, lineStart.Y), coordinates));
            }
        }
    }
}
