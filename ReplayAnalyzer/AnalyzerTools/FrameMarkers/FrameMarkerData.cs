using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using System.Numerics;

namespace ReplayAnalyzer.AnalyzerTools.FrameMarkers
{
    public class FrameMarkerData
    {
        public static List<FrameMarkerData> FrameMarkersData = new List<FrameMarkerData>();

        public long SpawnTime { get; }
        public long EndTime { get; }
        public Vector2 BasePosition { get; }
        public Vector2 Position = new Vector2();

        public FrameMarkerData(long spawnTime, long endTime, Vector2 position) 
        {
            SpawnTime = spawnTime;
            EndTime = endTime;
            Position = position;
            BasePosition = position;
        }

        public static void ResetFields()
        {
            FrameMarkersData.Clear();
        }

        public static void CreateData()
        {
            foreach (ReplayFrame frame in MainWindow.replay.FramesDict.Values)
            {
                FrameMarkersData.Add(new FrameMarkerData(frame.Time, frame.Time + HitMarkerData.ALIVE_TIME, new Vector2(frame.X, frame.Y)));
            }            
        }
    }
}
