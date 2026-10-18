using ReplayAnalyzer.AnalyzerTools.Cursor;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Osu
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

        public static void UpdateIndexAfterSeek(double time, bool seekByFrame = false)
        {
            (int indx, HitMarkerData marker) foundMarker = BinarySearch((int)time);
            
            if (foundMarker.marker == null)
            {
                return;
            }

            // when seeking values are == so this needs to exists when index changes to spawn newest hit marker while seeking
            if (seekByFrame == false && CurrentHitMarkerIndex <= foundMarker.indx && MainWindow.IsReplayPreloading == false)
            {// if slider seeking and its not preload then do this coz otherwise while loop is endless
                OsuHitDetection.CheckIfObjectWasHit();
            }
            else
            {// when preloading this loops through all hit markers so judgement times are correct
             // when by frame seeking this spawns all hit markers
                int prevIndex = 0;
                while (CurrentHitMarkerIndex <= foundMarker.indx)
                {
                    OsuHitDetection.CheckIfObjectWasHit();

                    if (prevIndex == CurrentHitMarkerIndex)
                    {// this prevents endless looping
                        CurrentHitMarkerIndex++;
                    }

                    if (CurrentHitMarkerIndex == foundMarker.indx)
                    {
                        break;
                    }

                    prevIndex = CurrentHitMarkerIndex;
                }
            }
                
            CurrentHitMarkerIndex = foundMarker.indx;
        }

        private static (int, HitMarkerData) BinarySearch(long time)
        {
            int l = 0;
            int r = HitMarkerData.HitMarkersData.Count - 1;

            if (r < 0)
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

            return (l, HitMarkerData.HitMarkersData[l]);
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

        protected static void SpawnHitMarker(HitMarkerData hitMarkerData, int index)
        {
            if (!AliveHitMarkersData.Contains(hitMarkerData) && index < HitMarkerData.HitMarkersData.Count)
            {
                AliveHitMarkersData.Add(hitMarkerData);
                HitMarker marker = HitMarker.Create(index);
                OsuPlayfield.Playfield.Children.Add(marker);
                AliveHitMarkers.Add(marker);
            }
        }

        public static void HandleAliveHitMarkers()
        {
            for (int i = 0; i < AliveHitMarkers.Count; i++)
            {
                HitMarker marker = AliveHitMarkers[i];
                if (GamePlayClock.TimeElapsed > marker.EndTime || GamePlayClock.TimeElapsed < marker.SpawnTime)
                {
                    OsuPlayfield.Playfield.Children.Remove(marker);
                    AliveHitMarkers.Remove(marker);
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
