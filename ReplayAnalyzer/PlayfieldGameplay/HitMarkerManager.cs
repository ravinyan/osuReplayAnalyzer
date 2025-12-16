using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.GameClock;
using System.Windows;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
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

        public static void UpdateHitMarkerAfterSeek(double direction, long time = 0, bool isReverseSeeking = false)
        {
            if (time == 0)
            {
                time = (long)GamePlayClock.TimeElapsed;
            }

            int idx = -1;

            bool found = false;
            bool foundFirst = false;

            int delay = direction < 0 ? HitMarkerData.ALIVE_TIME : 0;
            for (int i = 0; i < HitMarkerData.HitMarkersData.Count; i++)
            {
                if (direction >= 0)
                {
                    HitMarkerData hitMarker = HitMarkerData.HitMarkersData[i];
                    if (hitMarker.SpawnTime >= time || i == HitMarkerData.HitMarkersData.Count - 1)
                    {
                        found = true;

                        idx = i;
                        CurrentHitMarkerIndex = i;
                        break;
                    }
                }
                else
                {
                    HitMarkerData hitMarker = HitMarkerData.HitMarkersData[i];
                    if ((hitMarker.SpawnTime > time || i == HitMarkerData.HitMarkersData.Count - 1)
                    && foundFirst == false)
                    {
                        foundFirst = true;
                        CurrentHitMarkerIndex = i;
                    }

                    if ((hitMarker.SpawnTime >= time - delay || i == HitMarkerData.HitMarkersData.Count - 1)
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

            if (isReverseSeeking == true)
            {
                //if (idx != -1 && !AliveHitMarkersData.Contains(AliveHitMarkersData[idx])
                //&&  GamePlayClock.TimeElapsed > HitMarkerData.HitMarkersData[idx].SpawnTime
                //&&  GamePlayClock.TimeElapsed < HitMarkerData.HitMarkersData[idx].EndTime)
                //{
                //    // idk
                //    HitMarker.Create(idx);
                //}
            }
        }

        public static void UpdateHitMarker()
        {
            GetCurrentHitMarker(ref CurrentHitMarker, CurrentHitMarkerIndex);
            SpawnHitMarker(CurrentHitMarker);
        }

        protected static void SpawnHitMarker(HitMarkerData idx)
        {
            if (!AliveHitMarkersData.Contains(idx) && CurrentHitMarkerIndex < HitMarkerData.HitMarkersData.Count)
            {
                AliveHitMarkersData.Add(idx);
                HitMarker marker = HitMarker.Create(CurrentHitMarkerIndex);
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
