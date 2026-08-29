using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI
{
    // i should do this class before i did improvements in each class separately... but eh why think when you can not think
    /// <summary>
    /// Here are only colours that are getting re-used multiple times
    /// </summary>
    // ... for now?
    public class ColourBank
    {
        public static SolidColorBrush URBarGreat { get; private set; } = null!;
        public static SolidColorBrush URBarOk { get; private set; } = null!;
        public static SolidColorBrush URBarMeh { get; private set; } = null!;

        public static SolidColorBrush KeyOverlayClick { get; private set; } = null!;
        public static SolidColorBrush KeyOverlayButtonInactive { get; private set; } = null!;

        public static SolidColorBrush JudgementTimelineOk { get; private set; } = null!;
        public static SolidColorBrush JudgementTimelineMeh { get; private set; } = null!;
        public static SolidColorBrush JudgementTimelineMiss { get; private set; } = null!;

        public static SolidColorBrush SliderBody { get; private set; } = null!;
        public static SolidColorBrush SliderBorder { get; private set; } = null!;

        public static SolidColorBrush HitMarkerHit { get; private set; } = null!;
        public static SolidColorBrush HitMarkerPassive { get; private set; } = null!;

        public static SolidColorBrush CursorPathLine { get; private set; } = null!;
        public static SolidColorBrush FrameMarkerDot { get; private set; } = null!;

        public static SolidColorBrush CatcherCaught { get; private set; } = null!;
        public static SolidColorBrush CatcherMiss { get; private set; } = null!;

        public static SolidColorBrush HitMapHit { get; private set; } = null!;

        // this will be a foken CHONKER
        public static void CacheColours()
        {
            URBar();
            KeyOverlay();
            JudgementTimeline();
            SliderHitObject();
            OsuHitMarker();
            CursorPath();
            FrameMarker();
            CatchCatcher();
            HitMap();
        }

        private static void HitMap()
        {
            HitMapHit = Brushes.Cyan;
            HitMapHit.Freeze();
        }

        private static void CatchCatcher()
        {
            CatcherCaught = Brushes.Cyan;
            CatcherCaught.Freeze();

            CatcherMiss = Brushes.Red;
            CatcherMiss.Freeze();
        }

        private static void FrameMarker()
        {
            FrameMarkerDot = new SolidColorBrush(Colors.Pink);
            FrameMarkerDot.Freeze();
        }

        private static void CursorPath()
        {
            CursorPathLine = new SolidColorBrush(Colors.Pink);
            CursorPathLine.Freeze();
        }

        private static void OsuHitMarker()
        {
            HitMarkerHit = Brushes.HotPink;
            HitMarkerHit.Freeze();

            HitMarkerPassive = Brushes.LightGray;
            HitMarkerPassive.Freeze();
        }

        private static void SliderHitObject()
        {
            SliderBody = new SolidColorBrush(Color.FromRgb(3, 3, 12));
            SliderBody.Freeze();

            SliderBorder = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            SliderBorder.Freeze();
        }

        private static void JudgementTimeline()
        {
            JudgementTimelineOk = new SolidColorBrush(Color.FromRgb(11, 145, 9));
            JudgementTimelineOk.Freeze();

            JudgementTimelineMeh = new SolidColorBrush(Color.FromRgb(242, 146, 2));
            JudgementTimelineMeh.Freeze();

            JudgementTimelineMiss = new SolidColorBrush(Color.FromRgb(245, 42, 42));
            JudgementTimelineMiss.Freeze();
        }

        private static void KeyOverlay()
        {
            KeyOverlayClick = new SolidColorBrush(Color.FromRgb(63, 190, 221));
            KeyOverlayClick.Freeze();

            KeyOverlayButtonInactive = new SolidColorBrush(Colors.Transparent);
            KeyOverlayButtonInactive.Freeze();
        }

        private static void URBar()
        {
            URBarGreat = new SolidColorBrush(Color.FromRgb(138, 216, 255));
            URBarGreat.Freeze();

            URBarOk = new SolidColorBrush(Color.FromRgb(176, 192, 25));
            URBarOk.Freeze();

            URBarMeh = new SolidColorBrush(Color.FromRgb(255, 217, 61));
            URBarMeh.Freeze();
        }
    }
}
