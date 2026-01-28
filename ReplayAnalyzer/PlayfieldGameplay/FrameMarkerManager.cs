using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.FrameMarkers;
using ReplayAnalyzer.GameClock;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class FrameMarkerManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static List<FrameMarker> AliveFrameMarkers = new List<FrameMarker>();
        private static List<FrameMarkerData> AliveFrameMarkersData = new List<FrameMarkerData>();

        public static FrameMarkerData CurrentFrameMarker = null;
        public static int FrameMarkerIndex = 0;

        public static void ResetFields()
        {
            AliveFrameMarkers.Clear();
            AliveFrameMarkersData.Clear();
            CurrentFrameMarker = null;
            FrameMarkerIndex = 0;
        }

        public static void UpdateFrameMarker()
        {
            GetCurrentFrameMarker(ref CurrentFrameMarker, FrameMarkerIndex);
            SpawnFrameMarker(CurrentFrameMarker, FrameMarkerIndex);
        }

        private static void SpawnFrameMarker(FrameMarkerData frameMarkerData, int index)
        {
            if (!AliveFrameMarkersData.Contains(frameMarkerData) && index < FrameMarkerData.FrameMarkersData.Count
            &&  GamePlayClock.TimeElapsed >= frameMarkerData.SpawnTime)
            {
                FrameMarker marker = FrameMarker.Create(index);
                if (marker != null)
                {
                    AliveFrameMarkersData.Add(frameMarkerData);
                    Window.playfieldCanva.Children.Add(marker);
                    AliveFrameMarkers.Add(marker);
                    FrameMarkerIndex++;
                }
            }
        }

        private static void GetCurrentFrameMarker(ref FrameMarkerData marker, int index)
        {
            if (index >= FrameMarkerData.FrameMarkersData.Count)
            {
                return;
            }

            if (index < FrameMarkerData.FrameMarkersData.Count && marker != FrameMarkerData.FrameMarkersData[index])
            {
                marker = FrameMarkerData.FrameMarkersData[index];
            }
        }

        // i know i can use binary search from hit markers but i wont do that here coz performance here doesnt matter
        // and in hit markers it matters since preload calls it as many times as there are frames in replay
        public static void GetFrameMarkerAfterSeek(ReplayFrame frame)
        {
            List<ReplayFrame> frames = MainWindow.replay.FramesDict.Values.ToList();
            FrameMarkerIndex = frames.IndexOf(frame);
            frames.Clear();
        }

        public static void HandleAliveFrameMarkers()
        {
            for (int i = 0; i < AliveFrameMarkers.Count; i++)
            {
                FrameMarker marker = AliveFrameMarkers[i];
                if (GamePlayClock.TimeElapsed > marker.EndTime || GamePlayClock.TimeElapsed < marker.SpawnTime)
                {
                    AliveFrameMarkers.Remove(marker);
                    Window.playfieldCanva.Children.Remove(marker);
                    AliveFrameMarkersData.Remove(AliveFrameMarkersData[i]);
                }
            }
        }

        public static List<FrameMarker> GetAliveFrameMarkers()
        {
            return AliveFrameMarkers;
        }

        public static List<FrameMarkerData> GetAliveDataFrameMarkers()
        {
            return AliveFrameMarkersData;
        }
    }
}
