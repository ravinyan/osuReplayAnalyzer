using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Numerics;

namespace WpfApp1.Objects.SliderPathMath
{
    public class SliderPath
    {
        private bool valid = false;
        private readonly List<Vector2> calculatedPath = new List<Vector2>();
        private readonly List<double> cumulativeLength = new List<double>();
        private readonly List<int> segmentedEnds = new List<int>();
        private double optimisedLength;
        private Slider? slider;
        private double calculatedLength;
        private double[] segmentedEndDistances = Array.Empty<double>();
        private double ExpectedDistance = 0;

        public SliderPath(Slider sliderr)
        {
            slider = sliderr;
            ExpectedDistance = (double)sliderr.Length;
        }

        public List<Vector2> CalculatedPath()
        {
            EnsureValid();
            return calculatedPath;
        }

        private void Invalidate()
        {
            valid = false;
        }

        private void EnsureValid()
        {
            if (valid)
            {
                return;
            }

            CalculatePath();
            CalculateLength();

            valid = true;
        }

        private void CalculatePath()
        {
            calculatedPath.Clear();
            segmentedEnds.Clear();
            optimisedLength = 0;

            if (slider.ControlPoints.Count == 0)  
            {
                return;
            }

            Vector2[] vertices = new Vector2[slider.ControlPoints.Count];
            for (int i = 0; i < slider.ControlPoints.Count; i++)
            {
                vertices[i] = slider.ControlPoints[i].Position;
            }

            int start = 0;

            for (int i = 0; i < slider.ControlPoints.Count; i++)
            {
                if (slider.ControlPoints[i].Type == null && i < slider.ControlPoints.Count - 1)
                {
                    continue;
                }

                var segmentedVertices = vertices.AsSpan().Slice(start, i - start + 1);
                var segmentType = slider.ControlPoints[start].Type ?? CurveType.Linear;

                if (segmentedVertices.Length == 1)
                {
                    calculatedPath.Add(segmentedVertices[0]);
                }
                else if (segmentedVertices.Length > 1)
                {
                    // i know every path point should have its own type in osu lazer encoding but i wont do it until its problem
                    // never in my life saw map that has 2 curve types
                    List<Vector2> subPath = CalculateSubPath(segmentedVertices, segmentType);

                    bool skipFirst = calculatedPath.Count > 0 && subPath.Count > 0 && calculatedPath.Last() == subPath[0];

                    for (int j = skipFirst ? 1 : 0; j < subPath.Count; j++)
                    {
                        calculatedPath.Add(subPath[j]);
                    }
                }

                if (i > 0)
                {
                    segmentedEnds.Add(calculatedPath.Count - 1);
                }

                start = i;
            }
        }

        private List<Vector2> CalculateSubPath(ReadOnlySpan<Vector2> subControlPoints, CurveType type)
        {
            switch (type)
            {
                case CurveType.Linear:
                    return PathApproximator.LinearToPiecewiseLinear(subControlPoints);

                case CurveType.PerfectCircle:
                {
                    if (subControlPoints.Length != 3)
                    {
                        break;
                    }

                    CircularArcProperties properties = new CircularArcProperties(subControlPoints);

                    if (!properties.IsValid)
                    {
                        break;
                    }

                    int subPoints = (2f * properties.Radius <= 0.1f) ? 2 : Math.Max(2, (int)Math.Ceiling(properties.ThetaRange / (2.0 * Math.Acos(1f - (0.1f / properties.Radius)))));

                    if (subPoints >= 1000)
                    {
                        break;
                    }

                    List<Vector2> subPath = PathApproximator.CircularArcToPiecewiseLinear(subControlPoints);

                    if (subPath.Count == 0)
                    {
                        break;
                    }

                    return subPath;
                } 
                case CurveType.Catmull:
                {
                    List<Vector2> subPath = PathApproximator.CatmullToPiecewiseLinear(subControlPoints);

                    List<Vector2> optimisedPath = new List<Vector2>(subPath.Count);

                    Vector2? lastStart = null;
                    double lengthRemovedSinceStart = 0;

                    for (int i = 0; i < subPath.Count; i++)
                    {
                        if (lastStart == null)
                        {
                            optimisedPath.Add(subPath[i]);
                            lastStart = subPath[i];
                            continue;
                        }

                        double distFromStart = Vector2.Distance(lastStart.Value, subPath[i]);
                        lengthRemovedSinceStart += Vector2.Distance(subPath[i - 1], subPath[i]);

                        const int catmullDetail = 50;
                        const int catmullSegmentLength = catmullDetail * 2;

                        if (distFromStart > 6 || (i + 1) % catmullSegmentLength == 0 || i == subPath.Count - 1)
                        {
                            optimisedPath.Add(subPath[i]);
                            optimisedLength += lengthRemovedSinceStart - distFromStart;

                            lastStart = null;
                            lengthRemovedSinceStart = 0;
                        }
                    }

                    return optimisedPath;
                }
            }

            return PathApproximator.BSplineToPiecewiseLinear(subControlPoints, subControlPoints.Length);
        }

        private void CalculateLength()
        {
            calculatedLength = optimisedLength;
            cumulativeLength.Clear();
            cumulativeLength.Add(0);

            for (int i = 0; i < calculatedPath.Count - 1; i++)
            {
                Vector2 diff = calculatedPath[i + 1] - calculatedPath[i];
                calculatedLength += diff.Length();
                cumulativeLength.Add(calculatedLength);
            }

            segmentedEndDistances = new double[segmentedEnds.Count];

            for (int i = 0; i < segmentedEnds.Count; i++)
            {
                segmentedEndDistances[i] = cumulativeLength[segmentedEnds[i]];
            }

            if (ExpectedDistance is double expectedDistance && calculatedLength != expectedDistance)
            {
                if (calculatedPath.Count >= 2 && calculatedPath[^1] == calculatedPath[^2] && expectedDistance > calculatedLength)
                {
                    cumulativeLength.Add(calculatedLength);
                    return;
                }

                cumulativeLength.RemoveAt(cumulativeLength.Count - 1);

                int pathEndIndex = calculatedPath.Count - 1;

                if (calculatedLength > expectedDistance)
                {
                    while (cumulativeLength.Count > 0 && cumulativeLength[^1] >= expectedDistance)
                    {
                        cumulativeLength.RemoveAt(cumulativeLength.Count - 1);
                        calculatedPath.RemoveAt(pathEndIndex--);
                    }
                }

                if (pathEndIndex <= 0)
                {
                    cumulativeLength.Add(0);
                    return;
                }

                Vector2 normalized = (calculatedPath[pathEndIndex] - calculatedPath[pathEndIndex - 1]);
                float num = 1f / normalized.Length();
                normalized.X += num;
                normalized.Y += num;

                Vector2 dir = normalized;

                calculatedPath[pathEndIndex] = calculatedPath[pathEndIndex - 1] + dir * (float)(expectedDistance - cumulativeLength[^1]);
                cumulativeLength.Add(expectedDistance);
            }
        }
    }
}
