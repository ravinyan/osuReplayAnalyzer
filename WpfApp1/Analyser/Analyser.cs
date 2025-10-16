using ReplayParsers.Classes.Replay;
using WpfApp1.Analyser.UIElements;

namespace WpfApp1.Analyser
{
    public class Analyser
    {
        public static Dictionary<int, HitMarker> HitMarkers = new Dictionary<int, HitMarker>();
        private static int Index = 0;

        public static void CreateHitMarkers()
        {
            List<ReplayFrame> frames = MainWindow.replay.Frames;

            bool isHeldL = false;
            bool isHeldR = false;
            
            foreach (ReplayFrame frame in frames)
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
                    HitMarkers.Add(Index, HitMarker.Create(frame, "left", Index));
                    Index++;
                }
                
                if (isHeldR == true && rightClick == false)
                {
                    isHeldR = false;
                }
                else if (isHeldR == false && rightClick == true)
                {
                    isHeldR = true;
                    HitMarkers.Add(Index, HitMarker.Create(frame, "right", Index));
                    Index++;
                }
            }

            Index = 0;
        }
    }
}
