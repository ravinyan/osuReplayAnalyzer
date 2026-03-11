using System.Diagnostics;
using System.Timers;
using ReplayAnalyzer.MusicPlayer.Controls;

namespace ReplayAnalyzer.GameClock
{
    public static class GamePlayClock
    {
        private static Stopwatch stopwatch = new Stopwatch();

        private static double Last = 0;
        
        public static double TimeElapsed = 0;

        private static bool IsClockPaused = true;

        private static System.Timers.Timer timer = new System.Timers.Timer();

        public static void Initialize()
        {
            timer.Interval = 1;
            timer.Elapsed += TimerTick2!;
        }

        private static void TimerTick2(object sender, ElapsedEventArgs e)
        {
            GameplayClock();
        }

        private static void GameplayClock()
        {
            double now = stopwatch.ElapsedMilliseconds;
            double passed = (now - Last) * RateChangerControls.RateChange;

            Last = now;
            TimeElapsed += passed;
        }

        public static void Start()
        {
            timer.Start();
            stopwatch.Start();
            IsClockPaused = false;
        }

        public static void Pause()
        {
            timer.Stop();
            stopwatch.Stop();
            IsClockPaused = true;
        }

        public static void Restart()
        {
            stopwatch.Stop();
            TimeElapsed = 0;
            IsClockPaused = true;
        }

        public static bool IsPaused()
        {
            return IsClockPaused;
        }

        public static void Seek(long time)
        {
            if (MainWindow.IsReplayPreloading == false && time < 0 && MusicPlayer.MusicPlayer.AudioOffset < 0
            ||  time <= 500 && MusicPlayer.MusicPlayer.AudioOffset > 0 && MusicPlayer.MusicPlayer.AudioOffset <= 500)
            {
                time = -MusicPlayer.MusicPlayer.AudioOffset;
            }
            else if (time < 0)
            {
                time = 0;
            }

            TimeElapsed = time;
        }
    }
}
