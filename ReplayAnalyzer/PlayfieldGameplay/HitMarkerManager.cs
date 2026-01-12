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

        public static void UpdateHitMarkerAfterSeek(double direction, long time = 0)
        {
            if (time == 0)
            {
                time = (long)GamePlayClock.TimeElapsed;
            }

            int idx = -1;

            bool foundLast = false;
            bool foundFirst = false;

            int delay = direction < 0 ? HitMarkerData.ALIVE_TIME : 0;

            // not ideal but one step closer to greatness
            //if (time > Window.songSlider.Maximum / 2)
            //{
            //    for (int i = HitMarkerData.HitMarkersData.Count - 1; i > 0; i--)
            //    {
            //        HitMarkerData hitMarker = HitMarkerData.HitMarkersData[i];
            //        if (direction >= 0)
            //        {
            //            if (hitMarker.SpawnTime >= time || i == HitMarkerData.HitMarkersData.Count - 1)
            //            {
            //                foundLast = true;
            //
            //                idx = i;
            //                CurrentHitMarkerIndex = i;
            //                break;
            //            }
            //        }
            //        else
            //        {
            //            if ((hitMarker.SpawnTime > time || i == HitMarkerData.HitMarkersData.Count - 1)
            //            && foundFirst == false)
            //            {
            //                foundFirst = true;
            //                CurrentHitMarkerIndex = i;
            //            }
            //
            //            if ((hitMarker.SpawnTime >= time - delay || i == HitMarkerData.HitMarkersData.Count - 1)
            //            && foundLast == false)
            //            {
            //                idx = i;
            //                foundLast = true;
            //            }
            //
            //            if (foundLast == true && foundFirst == true)
            //            {
            //                if (GamePlayClock.TimeElapsed > HitMarkerData.HitMarkersData[idx].SpawnTime
            //                && GamePlayClock.TimeElapsed < HitMarkerData.HitMarkersData[idx].EndTime)
            //                {
            //                    SpawnHitMarker(HitMarkerData.HitMarkersData[idx], idx);
            //                }
            //
            //                break;
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < HitMarkerData.HitMarkersData.Count; i++)
            //    {
            //        HitMarkerData hitMarker = HitMarkerData.HitMarkersData[i];
            //        if (direction >= 0)
            //        {
            //            if (hitMarker.SpawnTime >= time || i == HitMarkerData.HitMarkersData.Count - 1)
            //            {
            //                foundLast = true;
            //
            //                idx = i;
            //                CurrentHitMarkerIndex = i;
            //                break;
            //            }
            //        }
            //        else
            //        {
            //            if ((hitMarker.SpawnTime > time || i == HitMarkerData.HitMarkersData.Count - 1)
            //            && foundFirst == false)
            //            {
            //                foundFirst = true;
            //                CurrentHitMarkerIndex = i;
            //            }
            //
            //            if ((hitMarker.SpawnTime >= time - delay || i == HitMarkerData.HitMarkersData.Count - 1)
            //            && foundLast == false)
            //            {
            //                idx = i;
            //                foundLast = true;
            //            }
            //
            //            if (foundLast == true && foundFirst == true)
            //            {
            //                if (GamePlayClock.TimeElapsed > HitMarkerData.HitMarkersData[idx].SpawnTime
            //                && GamePlayClock.TimeElapsed < HitMarkerData.HitMarkersData[idx].EndTime)
            //                {
            //                    SpawnHitMarker(HitMarkerData.HitMarkersData[idx], idx);
            //                }
            //
            //                break;
            //            }
            //        }
            //    }
            //}

            for (int i = 0; i < HitMarkerData.HitMarkersData.Count; i++)
            {
                HitMarkerData hitMarker = HitMarkerData.HitMarkersData[i];
                if (direction >= 0)
                {
                    if (hitMarker.SpawnTime >= time || i == HitMarkerData.HitMarkersData.Count - 1)
                    {
                        foundLast = true;

                        idx = i;
                        CurrentHitMarkerIndex = i;
                        break;
                    }
                }
                else
                {
                    if ((hitMarker.SpawnTime > time || i == HitMarkerData.HitMarkersData.Count - 1)
                    &&  foundFirst == false)
                    {
                        foundFirst = true;
                        CurrentHitMarkerIndex = i;
                    }

                    if ((hitMarker.SpawnTime >= time - delay || i == HitMarkerData.HitMarkersData.Count - 1)
                    &&  foundLast == false)
                    {
                        idx = i;
                        foundLast = true;
                    }

                    if (foundLast == true && foundFirst == true)
                    {
                        if (GamePlayClock.TimeElapsed > HitMarkerData.HitMarkersData[idx].SpawnTime
                        &&  GamePlayClock.TimeElapsed < HitMarkerData.HitMarkersData[idx].EndTime)
                        {
                            SpawnHitMarker(HitMarkerData.HitMarkersData[idx], idx);
                        }

                        break;
                    }
                }
            }
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
