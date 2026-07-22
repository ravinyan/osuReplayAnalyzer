using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.Cursor;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Osu
{
    public class FrameMarkerManager
    {
        private static List<FrameMarker> AliveFrameMarkers = new List<FrameMarker>();

        public static int FrameMarkerIndex = 0;

        public static void ResetFields()
        {
            AliveFrameMarkers.Clear();
            FrameMarkerIndex = 0;
        }

        public static void UpdateFrameMarker()
        {
            if (FrameMarkerIndex + 1 >= MainWindow.replay.FramesDict.Count)
            {
                return;
            }

            ReplayFrame frame = MainWindow.replay.FramesDict[FrameMarkerIndex];
            // this is updated based on fps and if fps is too low then without while loop this updates too slowly
            while (GamePlayClock.TimeElapsed >= frame.Time)
            {
                FrameMarker newMarker = FrameMarker.Create(FrameMarkerIndex);
                if (newMarker != null)
                {
                    OsuPlayfield.Playfield.Children.Add(newMarker);
                    AliveFrameMarkers.Add(newMarker);

                    FrameMarkerIndex++;

                    if (FrameMarkerIndex >= MainWindow.replay.FramesDict.Count)
                    {
                        return;
                    }

                    frame = MainWindow.replay.FramesDict[FrameMarkerIndex];
                }
            }
        } 

        public static void HandleAliveFrameMarkers()
        {
            for (int i = 0; i < AliveFrameMarkers.Count; i++)
            {
                FrameMarker marker = AliveFrameMarkers[i];
                if (GamePlayClock.TimeElapsed > marker.EndTime || GamePlayClock.TimeElapsed < marker.SpawnTime)
                {
                    AliveFrameMarkers.Remove(marker);
                    OsuPlayfield.Playfield.Children.Remove(marker);
                }
            }
        }

        // i know i can use binary search from hit markers but i wont do that here coz performance here doesnt matter
        // and in hit markers it matters since preload calls it as many times as there are frames in replay
        public static void UpdateIndexAfterSeek(double direction, ReplayFrame frame)
        {
            List<ReplayFrame> frames = MainWindow.replay.FramesDict.Values.ToList();
            FrameMarkerIndex = frames.IndexOf(frame);
            if (direction < 0)
            {// correcting index coz otherwise there is visual bug where dots dont appear instantly after backward seeking
                FrameMarkerIndex++;
            }

            frames.Clear();
        }

        public static List<FrameMarker> GetAliveFrameMarkers()
        {
            return AliveFrameMarkers;
        }
    }
}
