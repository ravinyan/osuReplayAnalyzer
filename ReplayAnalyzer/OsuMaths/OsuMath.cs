using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using System.Numerics;

namespace ReplayAnalyzer.OsuMaths
{
    public class OsuMath
    {
        private static double ApproachRate = double.MaxValue;
        private static double FadeIn       = double.MaxValue;
        private static double Judgement300 = double.MaxValue;
        private static double Judgement100 = double.MaxValue;
        private static double Judgement50  = double.MaxValue;

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

        public double GetJudgement300HitWindow()
        {
            if (Judgement300 == double.MaxValue)
            {
                Judgement300 = CalculateJudgement300HitWindow();
            }

            return Judgement300;
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

        public double GetApproachRateTiming(decimal approachRate)
        {
            if (approachRate < 5)
            {
                return (double)(1200 + 600 * (5 - approachRate) / 5);
            }
            else if (approachRate == 5)
            {
                return 1200;
            }
            else
            {
                return (double)(1200 - 750 * (approachRate - 5) / 5);
            }
        }

        public double GetFadeInTiming(decimal approachRate)
        {
            if (approachRate < 5)
            {
                return (double)(800 + 400 * (5 - approachRate) / 5);
            }
            else if (approachRate == 5)
            {
                return 800;
            }
            else
            {
                return (double)(800 - 500 * (approachRate - 5) / 5);
            }
        }

        public double GetJudgement300HitWindow(decimal overallDifficulty)
        {
            return (double)(80 - 6 * overallDifficulty) - 0.5; // -0.5 from osu lazer;
        }

        public double GetJudgement100HitWindow(decimal overallDifficulty)
        {
            return (double)(140 - 8 * overallDifficulty) - 0.5; // -0.5 from osu lazer;
        }

        public double GetJudgement50HitWindow(decimal overallDifficulty)
        {
            return (double)(200 - 10 * overallDifficulty) - 0.5; // -0.5 from osu lazer;
        }

        public float CalculateScaleFromCircleSize(decimal circleSize)
        {
            return (float)(1.0f - 0.7f * (float)circleSize) / 2;
        }

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

        private static double CalculateJudgement300HitWindow()
        {
            return (double)(80 - 6 * MainWindow.map.Difficulty!.OverallDifficulty) - 0.5; // -0.5 from osu lazer
        }

        private static double CalculateJudgement100HitWindow()
        {
            return (double)(140 - 8 * MainWindow.map.Difficulty!.OverallDifficulty) - 0.5; // -0.5 from osu lazer;
        }

        private static double CalculateJudgement50HitWindow()
        {
            return (double)(200 - 10 * MainWindow.map.Difficulty!.OverallDifficulty) - 0.5; // -0.5 from osu lazer;
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
