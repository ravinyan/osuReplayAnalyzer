using System.Diagnostics;

namespace WpfApp1.GameClock
{
    public static class GamePlayClock
    {
        private static Stopwatch stopwatch = new Stopwatch();

        private static long Last = 0;
        private static long TimeElapsed = 0;

        public static void GameplayClockTest()
        {
            long now = stopwatch.ElapsedMilliseconds;
            long passed = now - Last;
            Last = now;
            TimeElapsed += passed;
        }

        public static void StartGameplayClock()
        {
            stopwatch.Start();
        }

        public static void StopGameplayClock()
        {
            stopwatch.Stop();
        }

        public static void RestartGameplayClock()
        {
            stopwatch.Restart();
        }

        public static long GetElapsedTime()
        { 
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
