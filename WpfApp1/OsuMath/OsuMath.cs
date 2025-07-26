namespace WpfApp1.OsuMaths
{
    public class OsuMath
    {
        public decimal GetApproachRateTiming(decimal approachRate)
        {
            if (approachRate < 5)
            {
                return 1200 + 600 * (5 - approachRate) / 5;
            }
            else if (approachRate == 5)
            {
                return 1200;
            }
            else
            {
                return 1200 - 750 * (approachRate - 5) / 5;
            }
        }

        public decimal GetFadeInTiming(decimal approachRate)
        {
            if (approachRate < 5)
            {
                return 800 + 400 * (5 - approachRate) / 5;
            }
            else if (approachRate == 5)
            {
                return 800;
            }
            else
            {
                return 800 - 500 * (approachRate - 5) / 5;
            }
        }

        public decimal GetOverallDifficultyHitWindow300(decimal overallDifficulty)
        {
            return 80 - 6 * overallDifficulty;
        }

        public decimal GetOverallDifficultyHitWindow100(decimal overallDifficulty)
        {
            return 140 - 8 * overallDifficulty;
        }

        public decimal GetOverallDifficultyHitWindow50(decimal overallDifficulty)
        {
            return 200 - 10 * overallDifficulty;
        }

        public float CalculateScaleFromCircleSize(decimal circleSize)
        {
            return (float)(1.0f - 0.7f * (float)circleSize) / 2;
        }
    }
}
