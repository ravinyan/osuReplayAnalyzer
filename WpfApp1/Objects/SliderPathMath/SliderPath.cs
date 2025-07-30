using System.Numerics;

namespace WpfApp1.Objects.SliderPathMath
{
    public class SliderPath
    {
        private static bool valid = false;
        private static readonly List<Vector2> calculatedPath = new List<Vector2>();
        private static readonly List<double> cumulativeLength = new List<double>();
        private static readonly List<int> segmentedEnds = new List<int>();

        public static void CalculatedPath()
        {

        }

        private static void Invalidate()
        {
            valid = false;
        }

        private static void EnsureValid()
        {
            if (valid)
            {
                return;
            }

            CalculatedPath();
            CalculatedLength();

            valid = true;
        }

        private static void CalculatePath()
        {

        }

        private static void CalculatedLength()
        {

        }
    }
}
