using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using System.Numerics;

namespace ReplayAnalyzer.AnalyzerTools.CursorPath
{
    public class CursorPathData
    {
        public static List<CursorPathData> CursorPathsData = new List<CursorPathData>();

        public static long SpawnTime { get; set; }
        public static long EndTime { get; set; }
        public List<Vector2> BaseLineCoordinates { get; set; } = new List<Vector2>();
        public List<Vector2> LineCoordinates = new List<Vector2>();

        public CursorPathData(long spawnTime, long endTime, List<Vector2> position) 
        {
            SpawnTime = spawnTime;
            EndTime = endTime;
            LineCoordinates = position;
            BaseLineCoordinates = position;
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

                CursorPathsData.Add(new CursorPathData(lineStart.Time, lineStart.Time + HitMarkerData.ALIVE_TIME, coordinates));
            }
        }
    }
}
