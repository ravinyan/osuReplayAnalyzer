using System.Diagnostics;
using System.Timers;
using System.Windows.Threading;

namespace WpfApp1.GameClock
{
    public static class GamePlayClock
    {
        private static Stopwatch stopwatch = new Stopwatch();

        private static long Last = 0;
        public static long TimeElapsed = 0;
        private static bool IsClockPaused = true;
        private static readonly int FrameTime = 16;

        private static DispatcherTimer timer2 = new DispatcherTimer(DispatcherPriority.Render);
        private static System.Timers.Timer timer = new System.Timers.Timer();

        public static void Initialize()
        {
            //timer.Interval = TimeSpan.FromMilliseconds(1);
            //timer.Tick += TimerTick!;

            timer.Interval = 1;
            timer.Elapsed += TimerTick2!;
        }

        private static void TimerTick2(object sender, ElapsedEventArgs e)
        {
            GameplayClock();
        }

        //private static void TimerTick(object sender, EventArgs e)
        //{
        //    GameplayClock();
        //}

        private static void GameplayClock()
        {
            long now = stopwatch.ElapsedMilliseconds;
            long passed = now - Last;
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
