﻿using OsuFileParsers.Classes.Replay;
using System.Diagnostics;
using System.Timers;
using ReplayAnalyzer;

namespace ReplayAnalyzer.GameClock
{
    public static class GamePlayClock
    {
        private static Stopwatch stopwatch = new Stopwatch();

        private static double Last = 0;
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
                RateChange = 1;
            }
        }

        private static void TimerTick2(object sender, ElapsedEventArgs e)
        {
            GameplayClock();
        }

        private static void GameplayClock()
        {
            double now = stopwatch.ElapsedMilliseconds;

            // this should work and i guess it works but all animations and spawns are borkded how to fix pain
            double passed = (now - Last) * MainWindow.RateChange;
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
