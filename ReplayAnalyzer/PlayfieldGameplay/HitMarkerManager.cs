using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Analyser.UIElements;
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

        public static void UpdateHitMarkerAfterSeek(ReplayFrame frame, double direction)
        {
            int idx = -1;

            bool found = false;
            bool foundFirst = false;

            int delay = direction < 0 ? 600 : 0;
            for (int i = 0; i < Analyser.Analyser.HitMarkers.Count; i++)
            {
                if (direction > 0)
                {
                    HitMarker hitMarker = Analyser.Analyser.HitMarkers[i];
                    if (hitMarker.SpawnTime >= GamePlayClock.TimeElapsed || i == Analyser.Analyser.HitMarkers.Count - 1)
                    {
                        found = true;

                        CurrentHitMarkerIndex = i;
                        break;
                    }
                }
                else
                {
                    HitMarker hitMarker = Analyser.Analyser.HitMarkers[i];

                    if ((hitMarker.SpawnTime >= GamePlayClock.TimeElapsed || i == Analyser.Analyser.HitMarkers.Count - 1)
                    && foundFirst == false)
                    {
                        foundFirst = true;
                        CurrentHitMarkerIndex = i;
                    }

                    if ((hitMarker.SpawnTime >= GamePlayClock.TimeElapsed - delay || i == Analyser.Analyser.HitMarkers.Count - 1)
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
                if (idx != -1 && !AliveHitMarkers.Contains(Analyser.Analyser.HitMarkers[idx]))
                {
                    SpawnHitMarker(Analyser.Analyser.HitMarkers[idx]);
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
            Window.playfieldCanva.Children.Add(marker);
            AliveHitMarkers.Add(marker);
        }

        protected static void GetCurrentHitMarker(ref HitMarker marker, int index)
        {
            if (index >= Analyser.Analyser.HitMarkers.Count)
            {
                return;
            }

            if (index < Analyser.Analyser.HitMarkers.Count && marker != Analyser.Analyser.HitMarkers[index])
            {
                marker = Analyser.Analyser.HitMarkers[index];
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
