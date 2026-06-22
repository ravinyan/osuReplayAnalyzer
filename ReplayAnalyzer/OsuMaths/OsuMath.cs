using OsuFileParsers.Classes.Replay;
using System.Numerics;

namespace ReplayAnalyzer.OsuMaths
{
    public class OsuMath
    {
        private static double ApproachRate = double.MaxValue;
        private static double FadeIn       = double.MaxValue;
        private static double Judgement320 = double.MaxValue; // mania
        private static double Judgement300 = double.MaxValue; // osu, mania, taiko
        private static double Judgement200 = double.MaxValue; // mania
        private static double Judgement100 = double.MaxValue; // osu, mania, taiko
        private static double Judgement50  = double.MaxValue; // osu, mania

        public static void ResetFields()
        {
            ApproachRate = double.MaxValue;
            FadeIn = double.MaxValue;
            Judgement300 = double.MaxValue;
            Judgement100 = double.MaxValue;
            Judgement50 = double.MaxValue;
        }

        public double GetApproachRateTiming()
        {
            if (ApproachRate == double.MaxValue)
            {
                ApproachRate = CalculateApproachRate();
            }

            return ApproachRate;
        }

        public double GetFadeInTiming()
        {
            if (FadeIn == double.MaxValue)
            {
                FadeIn = CalculateFadeDuration();
            }

            return FadeIn;
        }

        public double GetJudgement320HitWindow()
        {
            if (Judgement320 == double.MaxValue)
            {
                Judgement320 = CalculateJudgement320HitWindow();
            }

            return Judgement320;
        }

        public double GetJudgement300HitWindow()
        {
            if (Judgement300 == double.MaxValue)
            {
                Judgement300 = CalculateJudgement300HitWindow();
            }

            return Judgement300;
        }

        public double GetJudgement200HitWindow()
        {
            if (Judgement200 == double.MaxValue)
            {
                Judgement200 = CalculateJudgement200HitWindow();
            }

            return Judgement200;
        }

        public double GetJudgement100HitWindow()
        {
            if (Judgement100 == double.MaxValue)
            {
                Judgement100 = CalculateJudgement100HitWindow();
            }

            return Judgement100;
        }

        public double GetJudgement50HitWindow()
        {
            if (Judgement50 == double.MaxValue)
            {
                Judgement50 = CalculateJudgement50HitWindow();
            }

            return Judgement50;
        }

        //public double GetApproachRateTiming(decimal approachRate)
        //{
        //    if (approachRate < 5)
        //    {
        //        return (double)(1200 + 600 * (5 - approachRate) / 5);
        //    }
        //    else if (approachRate == 5)
        //    {
        //        return 1200;
        //    }
        //    else
        //    {
        //        return (double)(1200 - 750 * (approachRate - 5) / 5);
        //    }
        //}

        //public double GetFadeInTiming(decimal approachRate)
        //{
        //    if (approachRate < 5)
        //    {
        //        return (double)(800 + 400 * (5 - approachRate) / 5);
        //    }
        //    else if (approachRate == 5)
        //    {
        //        return 800;
        //    }
        //    else
        //    {
        //        return (double)(800 - 500 * (approachRate - 5) / 5);
        //    }
        //}

        //public double GetJudgement300HitWindow(decimal overallDifficulty)
        //{
        //    return (double)(80 - 6 * overallDifficulty) - 0.5; // -0.5 from osu lazer;
        //}

        //public double GetJudgement100HitWindow(decimal overallDifficulty)
        //{
        //    return (double)(140 - 8 * overallDifficulty) - 0.5; // -0.5 from osu lazer;
        //}

        //public double GetJudgement50HitWindow(decimal overallDifficulty)
        //{
        //    return (double)(200 - 10 * overallDifficulty) - 0.5; // -0.5 from osu lazer;
        //}

        //public float CalculateScaleFromCircleSize(decimal circleSize)
        //{
        //    return (float)(1.0f - 0.7f * (float)circleSize) / 2;
        //}

        private static double CalculateApproachRate()
        {
            decimal AR = MainWindow.map.Difficulty!.ApproachRate;
            if (AR < 5)
            {
                return (double)(1200 + 600 * (5 - AR) / 5);
            }
            else if (AR == 5)
            {
                return 1200;
            }
            else
            {
                return (double)(1200 - 750 * (AR - 5) / 5);
            }
        }

        private static double CalculateFadeDuration()
        {
            decimal AR = MainWindow.map.Difficulty!.ApproachRate;
            if (AR < 5)
            {
                return (double)(800 + 400 * (5 - AR) / 5);
            }
            else if (AR == 5)
            {
                return 800;
            }
            else
            {
                return (double)(800 - 500 * (AR - 5) / 5);
            }
        }

        // stable and lazer have different judgements... ffs
        // HR changes this value on stable from 16 to way less... WHY CANT THIS BE SIMPLE MAN
        // CONVERTS USE DIFFERENT JUDGEMENTS ARE YOU KIDDING ME AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
        // "Rate-changing mods (Double Time, Half Time, and Nightcore) do not affect hit window durations in osu!mania."
        // IT CLEARLY DOES dt 16.5 > 16.33, ht 16.5 > 16.67... i hate it here AND ITS IN STABLE ONLY... im not doing this lol
        // long notes have their own judgement system too...
        // https://osu.ppy.sh/wiki/en/Gameplay/Judgement/osu%21mania
        // not doing any converts the beatmap data from osu map had osu hit objects im not touching that nope goodbye
        private static double CalculateJudgement320HitWindow()
        {
            if (MainWindow.replay.IsLazer == false && MainWindow.replay.StableMods != Mods.ScoreV2)
            {
                return 16 + 0.5; // mania adds + 0.5
            }
            else
            {
                return (22.4 - 0.6 * (double)MainWindow.map.Difficulty.OverallDifficulty) + 0.5; // mania adds + 0.5
            }
        }

        private static double CalculateJudgement300HitWindow()
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    return (80 - 6 * (double)MainWindow.map.Difficulty!.OverallDifficulty) - 0.5; // -0.5 from osu lazer
                case GameMode.OsuMania:
                    return (64 - 3 * (double)MainWindow.map.Difficulty!.OverallDifficulty) + 0.5; // mania adds + 0.5
                case GameMode.OsuTaiko:
                    return 1;
            }

            return -1;
        }

        private static double CalculateJudgement200HitWindow()
        {
            return (97 - 3 * (double)MainWindow.map.Difficulty!.OverallDifficulty) + 0.5; // mania adds + 0.5
        }

        private static double CalculateJudgement100HitWindow()
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    return (140 - 8 * (double)MainWindow.map.Difficulty!.OverallDifficulty) - 0.5; // -0.5 from osu lazer;
                case GameMode.OsuMania:
                    return (127 - 3 * (double)MainWindow.map.Difficulty!.OverallDifficulty) + 0.5; // mania adds + 0.5
                case GameMode.OsuTaiko:
                    return 1;
            }

            return -1;
        }

        private static double CalculateJudgement50HitWindow()
        {
            GameMode mode = MainWindow.replay.GameMode;
            switch (mode)
            {
                case GameMode.Osu:
                    return (200 - 10 * (double)MainWindow.map.Difficulty!.OverallDifficulty) - 0.5; // -0.5 from osu lazer;
                case GameMode.OsuMania:
                    return (151 - 3 * (double)MainWindow.map.Difficulty!.OverallDifficulty) + 0.5; // mania adds + 0.5
                case GameMode.OsuTaiko:
                    return 1;
            }

            return -1;
        }

        // from osu lazer code
        private const float FLOAT_EPSILON = 1e-3f;

        private const double DOUBLE_EPSILON = 1e-7;

        public static bool AlmostEquals(float value1, float value2, float acceptableDifference = FLOAT_EPSILON)
        {
            return Math.Abs(value1 - value2) <= acceptableDifference;
        }

        public static bool AlmostEquals(Vector2 value1, Vector2 value2, float acceptableDifference = FLOAT_EPSILON)
        {
            return AlmostEquals(value1.X, value2.X, acceptableDifference) && AlmostEquals(value1.Y, value2.Y, acceptableDifference);
        }

        public static bool AlmostEquals(double value1, double value2, double acceptableDifference = DOUBLE_EPSILON)
        {
            return Math.Abs(value1 - value2) <= acceptableDifference;
        }
    }
}
