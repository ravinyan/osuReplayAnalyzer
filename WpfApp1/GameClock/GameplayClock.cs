using ReplayParsers.Classes.Replay;
using System.Diagnostics;
using System.Timers;

namespace WpfApp1.GameClock
{
    public static class GamePlayClock
    {
        private static Stopwatch stopwatch = new Stopwatch();

        private static long Last = 0;
        public static double TimeElapsed = 0;
        private static bool IsClockPaused = true;
        private static readonly int FrameTime = 16;

        private static System.Timers.Timer timer = new System.Timers.Timer();

        private static double RateChange = 1;

        public static void Initialize()
        {
            timer.Interval = 1;
            timer.Elapsed += TimerTick2!;

            if (MainWindow.replay.ModsUsed.HasFlag(Mods.DoubleTime))
            {
                RateChange = 1.5;
            }
        }

        private static void TimerTick2(object sender, ElapsedEventArgs e)
        {
            GameplayClock();
        }

        private static void GameplayClock()
        {
            long now = stopwatch.ElapsedMilliseconds;
            // * x.xx is DT modifier and it works but no clue how to get custom DT rates from lazer
            double passed = (now - Last) * RateChange; 
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
            if (TimeElapsed + FrameTime <= MusicPlayer.MusicPlayer.SongDuration()
            ||  TimeElapsed - FrameTime >= 0)
            {
                TimeElapsed = time;
            }
        }
    }
}
