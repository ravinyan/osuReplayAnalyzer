using System.CodeDom;
using System.Numerics;
using System.Windows.Forms;

namespace WpfApp1.Objects.SliderPathMath
{
    public class PathApproximator
    {
        // im too stupid to understand but will try anyway
        // will take long coz i want to type out everything coz i hate brainless copy pasting
        // without even trying to understand how things work... even if in the end i wont understand 99% of this LOL
        // https://github.com/ppy/osu/blob/master/osu.Game/Rulesets/Objects/SliderPath.cs
        // https://github.com/ppy/osu-framework/blob/master/osu.Framework/Utils/PathApproximator.cs

        internal const float BezierTolerance = 0.25f;
        private const int CatmullDetail = 50;
        private const float CircularArcTolerance = 0.1f;

        public static List<Vector2> BSplineToPiecewiseLinear(ReadOnlySpan<Vector2> controlPoints, int degree)
        {
            if (controlPoints.Length < 2)
            {
                return controlPoints.Length == 0 ? new List<Vector2>() : new List<Vector2> { controlPoints[0] };
            }
                
            degree = Math.Min(degree, controlPoints.Length - 1);

            List<Vector2> output = new List<Vector2>();
            int pointCount = controlPoints.Length - 1;

            Stack<Vector2[]> toFlatten = BSplineToBezierInternal(controlPoints, ref degree);
            Stack<Vector2[]> freeBuffers = new Stack<Vector2[]>();

            var subdivisionBuffer1 = new Vector2[degree + 1];
            var subdivisionBuffer2 = new Vector2[degree * 2 + 1];

            Vector2[] leftChild = subdivisionBuffer2;

            while (toFlatten.Count > 0)
            {
                Vector2[] parent = toFlatten.Pop();

                if (BezierIsFlatEnough(parent))
                {
                    BezierApproximate(parent, output, subdivisionBuffer1, subdivisionBuffer2, degree + 1);

                    freeBuffers.Push(parent);
                    continue;
                }

                Vector2[] rightChild = freeBuffers.Count > 0 ? freeBuffers.Pop() : new Vector2[degree + 1];
                BezierSubdivide(parent, leftChild, rightChild, subdivisionBuffer1, degree + 1);

                for (int i = 0; i < degree + 1; ++i)
                {
                    parent[i] = leftChild[i];
                }

                toFlatten.Push(rightChild);
                toFlatten.Push(parent);
            }

            output.Add(controlPoints[pointCount]);
            return output;
        }

        public static List<Vector2> CatmullToPiecewiseLinear(ReadOnlySpan<Vector2> controlPoints)
        {
            var result = new List<Vector2>((controlPoints.Length - 1) * CatmullDetail * 2);

            for (int i = 0; i < controlPoints.Length - 1; i++)
            {
                var v1 = i > 0 ? controlPoints[i - 1] : controlPoints[i];
                var v2 = controlPoints[i];
                var v3 = i < controlPoints.Length - 1 ? controlPoints[i + 1] : v2 + v2 - v1;
                var v4 = i < controlPoints.Length - 2 ? controlPoints[i + 2] : v3 + v3 - v2;

                for (int c = 0; c < CatmullDetail; c++)
                {
                    result.Add(CatmullFindPoint(ref v1, ref v2, ref v3, ref v4, (float)c / CatmullDetail));
                    result.Add(CatmullFindPoint(ref v1, ref v2, ref v3, ref v4, (float)(c + 1) / CatmullDetail));
                }
            }

            return result;
        }

        public static List<Vector2> CircularArcToPiecewiseLinear(ReadOnlySpan<Vector2> controlPoints)
        {
            CircularArcProperties pr = new CircularArcProperties(controlPoints);

            if (pr.IsValid == false)
            {
                return BezierToPiecewiseLinear(controlPoints);
            }

            int amountPoints = 2 * pr.Radius <= CircularArcTolerance ? 2 : Math.Max(2, (int)Math.Ceiling(pr.ThetaRange / (2 * Math.Acos(1 - CircularArcTolerance / pr.Radius))));
            
            List<Vector2> output = new List<Vector2>(amountPoints);

            for (int i = 0; i < amountPoints; ++i)
            {
                double fract = (double)i / (amountPoints - 1);
                double theta = pr.ThetaStart + pr.Direction * fract * pr.ThetaRange;
                Vector2 o = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)) * pr.Radius;
                output.Add(o);
            }

            return output;
        }

        public static List<Vector2> LinearToPiecewiseLinear(ReadOnlySpan<Vector2> controlPoints)
        {
            var result = new List<Vector2>(controlPoints.Length);

            foreach (Vector2 cp in controlPoints)
            {
                result.Add(cp);
            }

            return result;
        }

        private static List<Vector2> BezierToPiecewiseLinear(ReadOnlySpan<Vector2> controlPoints)
        {
            return BSplineToPiecewiseLinear(controlPoints, Math.Max(1, controlPoints.Length - 1));
        }

        private static Vector2 CatmullFindPoint(ref Vector2 vec1, ref Vector2 vec2, ref Vector2 vec3, ref Vector2 vec4, float t)
        {
            float t2 = t * t;
            float t3 = t * t2;

            // what the hell is this black magic i hate math
            Vector2 result;
            result.X = 0.5f * (2f * vec2.X + (-vec1.X + vec3.X) * t + (2f * vec1.X - 5f * vec2.X + 4f * vec3.X - vec4.X) * t2 + (-vec1.X + 3f * vec2.X - 3f * vec3.X + vec4.X) * t3);
            result.Y = 0.5f * (2f * vec2.Y + (-vec1.Y + vec3.Y) * t + (2f * vec1.Y - 5f * vec2.Y + 4f * vec3.Y - vec4.Y) * t2 + (-vec1.Y + 3f * vec2.Y - 3f * vec3.Y + vec4.Y) * t3);

            return result;
        }

        private static void BezierSubdivide(Vector2[] controlPoints, Vector2[] l, Vector2[] r, Vector2[] subDivisionBuffer, int count)
        {
            Vector2[] midPoints = subDivisionBuffer;

            for (int i = 0; i < count; ++i)
            {
                midPoints[i] = controlPoints[i];
            }
                
            for (int i = 0; i < count; i++)
            {
                l[i] = midPoints[0];
                r[count - i - 1] = midPoints[count - i - 1];

                for (int j = 0; j < count - i - 1; j++)
                {
                    midPoints[j] = (midPoints[j] + midPoints[j + 1]) / 2;
                }
            }
        }

        private static void BezierApproximate(Vector2[] controlPoints, List<Vector2> output, Vector2[] subDivisionBuffer1, Vector2[] subDivisionBuffer2, int count)
        {
            Vector2[] l = subDivisionBuffer2;
            Vector2[] r = subDivisionBuffer1;

            BezierSubdivide(controlPoints, l, r, subDivisionBuffer1, count);

            for (int i = 0; i < count - 1; ++i)
            {
                l[count + i] = r[i + 1];
            }

            output.Add(controlPoints[0]);

            for (int i = 1; i < count - 1; ++i)
            {
                int index = 2 * i;
                Vector2 p = 0.25f * (l[index - 1] + 2 * l[index] + l[index + 1]);
                output.Add(p);
            }
        }

        private static bool BezierIsFlatEnough(Vector2[] controlPoints)
        {
            for (int i = 1; i < controlPoints.Length - 1; i++)
            {
                if ((controlPoints[i - 1] - 2 * controlPoints[i] + controlPoints[i + 1]).LengthSquared() > BezierTolerance * BezierTolerance * 4)
                {
                    return false;
                }
            }

            return true;
        }

        private static Stack<Vector2[]> BSplineToBezierInternal(ReadOnlySpan<Vector2> controlPoints, ref int degree)
        {
            Stack<Vector2[]> result = new Stack<Vector2[]>();

            degree = Math.Min(degree, controlPoints.Length - 1);

            int pointCount = controlPoints.Length - 1;
            var points = controlPoints.ToArray();

            if (degree == pointCount)
            {
                result.Push(points);
            }
            else
            {
                for (int i = 0; i < pointCount - degree; i++)
                {
                    var subBezier = new Vector2[degree + 1];
                    subBezier[0] = points[i];

                    for (int j = 0; j < degree - 1; j++)
                    {
                        subBezier[j + 1] = points[i + 1];

                        for (int k = 1; k < degree - j; k++)
                        {
                            int l = Math.Min(k, pointCount - degree - i);
                            points[i + k] = (l * points[i + k] + points[i + k + 1]) / (l + 1);
                        }
                    }

                    subBezier[degree] = points[i + 1];
                    result.Push(subBezier);
                }

                result.Push(points[(pointCount - degree)..]);
                result = new Stack<Vector2[]>(result);
            }

            return result;
        }
    }
}
