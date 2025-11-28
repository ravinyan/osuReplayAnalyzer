using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using System.Numerics;

namespace ReplayAnalyzer.OsuMaths
{
    public class OsuMath
    {
        public double GetApproachRateTiming()
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

        public double GetFadeInTiming()
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

        public double GetOverallDifficultyHitWindow300()
        {
            return (double)(80 - 6 * MainWindow.map.Difficulty!.OverallDifficulty);
        }

        public double GetOverallDifficultyHitWindow300(decimal overallDifficulty)
        {
            return (double)(80 - 6 * overallDifficulty);
        }

        public double GetOverallDifficultyHitWindow100()
        {
            return (double)(140 - 8 * MainWindow.map.Difficulty!.OverallDifficulty);
        }

        public double GetOverallDifficultyHitWindow100(decimal overallDifficulty)
        {
            return (double)(140 - 8 * overallDifficulty);
        }

        public double GetOverallDifficultyHitWindow50()
        {
            return (double)(200 - 10 * MainWindow.map.Difficulty!.OverallDifficulty);
        }

        public double GetOverallDifficultyHitWindow50(decimal overallDifficulty)
        {
            return (double)(200 - 10 * overallDifficulty);
        }

        public float CalculateScaleFromCircleSize(decimal circleSize)
        {
            return (float)(1.0f - 0.7f * (float)circleSize) / 2;
        }

        public double GetSliderEndTime(HitObjectData hitObject, decimal sliderMultiplayer)
        {
            if (hitObject is SliderData)
            {
                SliderData a = hitObject as SliderData;
                int repeats = a.RepeatCount + 1;
                return (double)(a.SpawnTime + repeats * a.Length / sliderMultiplayer);
            }

            return hitObject.SpawnTime;
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
