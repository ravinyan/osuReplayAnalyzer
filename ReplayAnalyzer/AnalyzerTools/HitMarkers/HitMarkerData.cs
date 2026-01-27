using OsuFileParsers.Classes.Replay;
using System.Numerics;

namespace ReplayAnalyzer.AnalyzerTools.HitMarkers
{
    public class HitMarkerData
    {
        public static List<HitMarkerData> HitMarkersData = new List<HitMarkerData>();

        public const int ALIVE_TIME = 600;

        public long SpawnTime { get; }
        public long EndTime { get; }
        public Vector2 BasePosition { get; }
        public Vector2 Position = new Vector2();
        public string ClickPos { get; }

        public HitMarkerData(long spawnTime, long endTime, Vector2 position, string click)
        {
            SpawnTime = spawnTime;
            EndTime = endTime;
            BasePosition = position;
            position = BasePosition;
            ClickPos = click;
        }

        public static void ResetFields()
        {
            HitMarkersData.Clear();
        }

        public static void CreateData()
        {
            bool isHeldL = false;
            bool isHeldR = false;

            foreach (ReplayFrame frame in MainWindow.replay.FramesDict.Values)
            {
                bool leftClick = false;
                bool rightClick = false;

                if (frame.Click == Clicks.M1 || frame.Click == Clicks.K1)
                {
                    leftClick = true;
                }
                else if (frame.Click == Clicks.M2 || frame.Click == Clicks.K2)
                {
                    rightClick = true;
                }
                else if (frame.Click == Clicks.M12 || frame.Click == Clicks.K12)
                {
                    leftClick = true;
                    rightClick = true;
                }

                if (isHeldL == true && leftClick == false)
                {
                    isHeldL = false;
                }
                else if (isHeldL == false && leftClick == true)
                {
                    isHeldL = true;
                    HitMarkersData.Add(new HitMarkerData(frame.Time, frame.Time + ALIVE_TIME, new Vector2(frame.X, frame.Y), "left"));
                }

                if (isHeldR == true && rightClick == false)
                {
                    isHeldR = false;
                }
                else if (isHeldR == false && rightClick == true)
                {
                    isHeldR = true;
                    HitMarkersData.Add(new HitMarkerData(frame.Time, frame.Time + ALIVE_TIME, new Vector2(frame.X, frame.Y), "right"));
                }
            }
        }
    }
}
