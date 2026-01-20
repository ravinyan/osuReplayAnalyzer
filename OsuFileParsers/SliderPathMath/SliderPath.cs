using OsuFileParsers.Classes.Beatmap.osu.Objects;
using System.Numerics;

namespace OsuFileParsers.SliderPathMath
{
    public class SliderPath
    {
        private bool valid = false;
        private readonly List<Vector2> calculatedPath = new List<Vector2>();
        private readonly List<double> cumulativeLength = new List<double>();
        private readonly List<int> segmentedEnds = new List<int>();
        private double optimisedLength;
        private double calculatedLength;
        private double[] segmentedEndDistances = Array.Empty<double>();

        public double ExpectedDistance = 0;
        public readonly List<PathControlPoint> ControlPoints = new List<PathControlPoint>();

        public double Distance
        {
            get
            {
                EnsureValid();
                return cumulativeLength.Count == 0 ? 0 : cumulativeLength[^1];
            }
        }

        public SliderPath()
        {

        }

        public SliderPath(SliderData sliderr)
        {
            ControlPoints.AddRange(sliderr.ControlPoints);
            ExpectedDistance = (double)sliderr.Length;
        }

        public SliderPath(PathControlPoint[] controlPoints, double expectedDistance)
        {
            ControlPoints.AddRange(controlPoints);
            ExpectedDistance = expectedDistance;
        }

        public List<Vector2> CalculatedPath()
        {
            EnsureValid();

            return calculatedPath;
        }

        public void GetPathToProgress(List<Vector2> path, double p0, double p1)
        {
            EnsureValid();

            double d0 = ProgressToDistance(p0);
            double d1 = ProgressToDistance(p1);

            path.Clear();

            int i = 0;
            // what the this is so smart
            for (; i < calculatedPath.Count && cumulativeLength[i] < d0; ++i)
            {
            }

            path.Add(InterpolateVertices(i, d0));

            for (; i < calculatedPath.Count && cumulativeLength[i] <= d1; ++i)
            {
                path.Add(calculatedPath[i]);
            }

            path.Add(InterpolateVertices(i, d1));
        }

        public Vector2 PositionAt(double progress)
        {
            EnsureValid();

            double d = ProgressToDistance(progress);
            return InterpolateVertices(IndexOfDistance(d), d);
        }

        private int IndexOfDistance(double d)
        {
            int i = cumulativeLength.BinarySearch(d);
            if (i < 0)
            {
                i = ~i;
            }

            return i;
        }

        private Vector2 InterpolateVertices(int i, double d)
        {
            if (calculatedPath.Count == 0)
            {
                return Vector2.Zero;
            }

            if (i <= 0)
            {
                return calculatedPath.First();
            }

            if (i >= calculatedPath.Count)
            {
                return calculatedPath.Last();
            }

            Vector2 p0 = calculatedPath[i - 1];
            Vector2 p1 = calculatedPath[i];

            double d0 = cumulativeLength[i - 1];
            double d1 = cumulativeLength[i];

            if (Precision.AlmostEquals(d0, d1))
            {
                return p0;
            }

            double w = (d - d0) / (d1 - d0);
            return p0 + (p1 - p0) * (float)w;
        }

        private double ProgressToDistance(double progress)
        {
            // NOTED clamping is limiting value either progress if its between 0 and 1 or 0/1 if value is lower/higher
            return Math.Clamp(progress, 0, 1) * Distance;
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

            if (ControlPoints.Count == 0)  
            {
                return;
            }

            Vector2[] vertices = new Vector2[ControlPoints.Count];
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                vertices[i] = ControlPoints[i].Position;
            }

            int start = 0;

            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (ControlPoints[i].Type == null && i < ControlPoints.Count - 1)
                {
                    continue;
                }

                Span<Vector2> segmentedVertices = vertices.AsSpan().Slice(start, i - start + 1);
                CurveType segmentType = ControlPoints[start].Type ?? CurveType.Linear;

                if (segmentedVertices.Length == 1)
                {
                    calculatedPath.Add(segmentedVertices[0]);
                }
                else if (segmentedVertices.Length > 1)
                {
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

                    int subPoints = 2f * properties.Radius <= 0.1f ? 2 : Math.Max(2, (int)Math.Ceiling(properties.ThetaRange / (2.0 * Math.Acos(1f - 0.1f / properties.Radius))));

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

                Vector2 normalized = calculatedPath[pathEndIndex] - calculatedPath[pathEndIndex - 1];
                float num = 1f / normalized.Length();
                normalized.X *= num;
                normalized.Y *= num;

                Vector2 dir = normalized;

                calculatedPath[pathEndIndex] = calculatedPath[pathEndIndex - 1] + dir * (float)(expectedDistance - cumulativeLength[^1]);
                cumulativeLength.Add(expectedDistance);
            }
        }
    }
}
