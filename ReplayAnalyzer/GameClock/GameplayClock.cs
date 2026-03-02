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
            // maybe audio offset can be changed using this? it would make gameplay look scuffed when changing it but it would work
            // if so then
            //if (MainWindow.IsReplayPreloading == false)
            //{
            //    TimeElapsed = time + MusicPlayer.MusicPlayer.AudioOffset;  
            //}
            //else // if preloading the time needs to be exact so dont use offset
            //{
            //    TimeElapsed = time;
            //}

            if (time >= 0)
            {
                TimeElapsed = time;
            }
        }
    }
}
