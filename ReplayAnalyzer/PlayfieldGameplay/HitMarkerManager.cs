using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools;
using ReplayAnalyzer.AnalyzerTools.UIElements;
using ReplayAnalyzer.GameClock;
using System.Windows;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class HitMarkerManager
    {
        protected static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static HitMarker CurrentHitMarker = null;
        public static int CurrentHitMarkerIndex = 0;

        protected static List<HitMarker> AliveHitMarkers = new List<HitMarker>();

        public static void ResetFields()
        {
            CurrentHitMarker = null;
            CurrentHitMarkerIndex = 0;
            AliveHitMarkers.Clear();
        }

        public static void UpdateHitMarkerAfterSeek(double direction, long time = 0)
        {
            if (time == 0)
            {
                time = (long)GamePlayClock.TimeElapsed;
            }

            int idx = -1;

            bool found = false;
            bool foundFirst = false;

            int delay = direction < 0 ? 600 : 0;
            for (int i = 0; i < Analyzer.HitMarkers.Count; i++)
            {
                if (direction >= 0)
                {
                    HitMarker hitMarker = Analyzer.HitMarkers[i];
                    if (hitMarker.SpawnTime >= time || i == Analyzer.HitMarkers.Count - 1)
                    {
                        found = true;

                        CurrentHitMarkerIndex = i;
                        break;
                    }
                }
                else
                {
                    HitMarker hitMarker = Analyzer.HitMarkers[i];
                    if ((hitMarker.SpawnTime > time || i == Analyzer.HitMarkers.Count - 1)
                    && foundFirst == false)
                    {
                        foundFirst = true;
                        CurrentHitMarkerIndex = i;
                    }

                    if ((hitMarker.SpawnTime >= time - delay || i == Analyzer.HitMarkers.Count - 1)
                    && found == false)
                    {
                        idx = i;
                        found = true;
                    }

                    if (found == true && foundFirst == true)
                    {
                        break;
                    }
                }
            }

            if (found == true)
            {
                if (idx != -1 && !AliveHitMarkers.Contains(Analyzer.HitMarkers[idx]))
                {
                    SpawnHitMarker(Analyzer.HitMarkers[idx]);
                }
            }
        }

        public static void UpdateHitMarker()
        {
            GetCurrentHitMarker(ref CurrentHitMarker, CurrentHitMarkerIndex);
            SpawnHitMarker(CurrentHitMarker);
        }

        protected static void SpawnHitMarker(HitMarker marker)
        {
            if (!AliveHitMarkers.Contains(marker))
            {
                Window.playfieldCanva.Children.Add(marker);
                AliveHitMarkers.Add(marker);
            }
        }

        protected static void GetCurrentHitMarker(ref HitMarker marker, int index)
        {
            if (index >= Analyzer.HitMarkers.Count)
            {
                return;
            }

            if (index < Analyzer.HitMarkers.Count && marker != Analyzer.HitMarkers[index])
            {
                marker = Analyzer.HitMarkers[index];
            }
        }

        public static void HandleAliveHitMarkers()
        {
            for (int i = 0; i < AliveHitMarkers.Count; i++)
            {
                HitMarker marker = AliveHitMarkers[i];
                if (GamePlayClock.TimeElapsed > marker.EndTime || GamePlayClock.TimeElapsed < marker.SpawnTime)
                {
                    AliveHitMarkers.Remove(marker);
                    Window.playfieldCanva.Children.Remove(marker);
                }
            }
        }
    }
}
