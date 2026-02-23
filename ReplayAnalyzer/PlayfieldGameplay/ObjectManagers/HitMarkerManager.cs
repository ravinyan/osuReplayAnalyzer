using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.GameClock;
using System.Windows;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers
{
    public class HitMarkerManager
    {
        protected static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static HitMarkerData CurrentHitMarker = null;
        public static int CurrentHitMarkerIndex = 0;

        protected static List<HitMarker> AliveHitMarkers = new List<HitMarker>();
        protected static List<HitMarkerData> AliveHitMarkersData = new List<HitMarkerData>();

        public static void ResetFields()
        {
            CurrentHitMarker = null;
            CurrentHitMarkerIndex = 0;
            AliveHitMarkers.Clear();
            AliveHitMarkersData.Clear();
        }

        public static void UpdateHitMarkerAfterSeek(double direction, double time)
        {
            (int indx, HitMarkerData marker) foundMarker = BinarySearch(direction, (int)time);
            
            if (foundMarker.marker == null)
            {
                return;
            }

            CurrentHitMarkerIndex = foundMarker.indx;
        }

        private static (int, HitMarkerData) BinarySearch(double direction, long time)
        {
            int l = 0;
            int r = HitMarkerData.HitMarkersData.Count;

            if (r == 0)
            {
                return (0, null);
            }

            while (l < r)
            {
                int mid = l + (r - l >> 1);

                if (time >= HitMarkerData.HitMarkersData[mid].SpawnTime)
                {
                    l = mid + 1;
                }
                else if (time < HitMarkerData.HitMarkersData[mid].SpawnTime)
                {
                    r = mid;
                }
            }
            
            return l - 1 <= 0 ? (0, HitMarkerData.HitMarkersData[0]) : (l - 1, HitMarkerData.HitMarkersData[l - 1]);
        }

        protected static void SpawnHitMarker(HitMarkerData hitMarkerData, int index)
        {
            if (!AliveHitMarkersData.Contains(hitMarkerData) && index < HitMarkerData.HitMarkersData.Count)
            {
                AliveHitMarkersData.Add(hitMarkerData);
                HitMarker marker = HitMarker.Create(index);         
                Window.playfieldCanva.Children.Add(marker);
                AliveHitMarkers.Add(marker);
            }
        }

        protected static void GetCurrentHitMarker(ref HitMarkerData marker, int index)
        {
            if (index >= HitMarkerData.HitMarkersData.Count)
            {
                return;
            }

            if (index < HitMarkerData.HitMarkersData.Count && marker != HitMarkerData.HitMarkersData[index])
            {
                marker = HitMarkerData.HitMarkersData[index];
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
                    AliveHitMarkersData.Remove(AliveHitMarkersData[i]);
                }
            }
        }

        public static List<HitMarker> GetAliveHitMarkers()
        {
            return AliveHitMarkers;
        }

        public static List<HitMarkerData> GetAliveDataHitMarkers()
        {
            return AliveHitMarkersData;
        }
    }
}
