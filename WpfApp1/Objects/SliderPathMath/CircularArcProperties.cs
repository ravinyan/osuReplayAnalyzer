using System.Numerics;
using System.Windows;

namespace WpfApp1.Objects.SliderPathMath
{
    public class CircularArcProperties
    {
        public readonly bool IsValid;
        public readonly double ThetaStart;
        public readonly double ThetaRange;
        public readonly double Direction;
        public readonly float Radius;
        public readonly Vector2 Centre;

        public double ThetaEnd => ThetaStart + ThetaRange * Direction;

        public CircularArcProperties(ReadOnlySpan<Vector2> controlPoints)
        {
            Vector2 a = controlPoints[0];
            Vector2 b = controlPoints[1];
            Vector2 c = controlPoints[2];

            if (AlmostEquals(0, (b.Y - a.Y) * (c.X - a.X) - (b.X - a.X) * (c.Y - a.Y)))
            {
                IsValid = false;
                ThetaStart = default;
                ThetaRange = default;
                Direction = default;
                Radius = default;
                Centre = default;

                return;
            }

            float d = 2 * (a.X * (b - c).Y + b.X * (c - a).Y + c.X * (a - b).Y);
            float aSq = a.LengthSquared();
            float bSq = b.LengthSquared();
            float cSq = c.LengthSquared();

            Centre = new Vector2(
                aSq * (b - c).Y + bSq * (c - a).Y + cSq * (a - b).Y,
                aSq * (c - b).X + bSq * (a - c).X + cSq * (b - a).X) / d;

            Vector2 dA = a - Centre;
            Vector2 dC = c - Centre;

            Radius = dA.Length();

            ThetaStart = Math.Atan2(dA.Y, dA.X);
            double thetaEnd = Math.Atan2(dC.Y, dC.X);

            while (thetaEnd < ThetaStart)
                thetaEnd += 2 * Math.PI;

            Direction = 1;
            ThetaRange = thetaEnd - ThetaStart;

            Vector2 orthoAtoC = c - a;
            orthoAtoC = new Vector2(orthoAtoC.Y, -orthoAtoC.X);

            if (Vector2.Dot(orthoAtoC, b - a) < 0)
            {
                Direction = -Direction;
                ThetaRange = 2 * Math.PI - ThetaRange;
            }

            IsValid = true;
        }

        // maybe move to somewhere else and make public if something else will need this
        private const float FLOAT_EPSILON = 1e-3f;

        private const double DOUBLE_EPSILON = 1e-7;

        private static bool AlmostEquals(float value1, float value2, float acceptableDifference = FLOAT_EPSILON)
        { 
            return Math.Abs(value1 - value2) <= acceptableDifference; 
        }

        private static bool AlmostEquals(Vector2 value1, Vector2 value2, float acceptableDifference = FLOAT_EPSILON)
        { 
            return AlmostEquals(value1.X, value2.X, acceptableDifference) && AlmostEquals(value1.Y, value2.Y, acceptableDifference);
        }

        private static bool AlmostEquals(double value1, double value2, double acceptableDifference = DOUBLE_EPSILON)
        {
            return Math.Abs(value1 - value2) <= acceptableDifference;
        }
    }
}
