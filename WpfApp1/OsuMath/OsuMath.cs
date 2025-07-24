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
    }
}
