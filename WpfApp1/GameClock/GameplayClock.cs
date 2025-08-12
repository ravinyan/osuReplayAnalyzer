using System.Diagnostics;
using System.Windows.Threading;

namespace WpfApp1.GameClock
{
    public static class GamePlayClock
    {
        private static Stopwatch stopwatch = new Stopwatch();

        private static long Last = 0;
        public static long TimeElapsed = 0;


        private static DispatcherTimer timer = new DispatcherTimer();

        public static void Initialize()
        {
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += TimerTick!;
        }

        private static void TimerTick(object sender, EventArgs e)
        {
            GameplayClock();
        }

        private static void GameplayClock()
        {
            long now = stopwatch.ElapsedMilliseconds;
            long passed = now - Last;
            Last = now;
            TimeElapsed += passed;
        }

        public static void StartGameplayClock()
        {
            timer.Start();
            stopwatch.Start();
        }

        public static void StopGameplayClock()
        {
            timer.Stop();
            stopwatch.Stop();
        }

        public static void RestartGameplayClock()
        {
            //timer.pa();
        }

        public static long GetElapsedTime()
        { 
            return TimeElapsed;
        }
    }
}
