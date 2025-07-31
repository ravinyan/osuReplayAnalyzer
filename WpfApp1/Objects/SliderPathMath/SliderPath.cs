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


        public SliderPath(Slider sliderr)
        {
            slider = sliderr;
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
            CalculatedLength();

            valid = true;
        }

        private void CalculatePath()
        {
            calculatedPath.Clear();
            segmentedEnds.Clear();
            optimisedLength = 0;

            if (slider.CurvePoints.Count == 0)
            {
                return;
            }

            Vector2[] vertices = new Vector2[slider.CurvePoints.Count];
            for (int i = 0; i < slider.CurvePoints.Count; i++)
            {
                vertices[i].X = slider.CurvePoints[i].X;
                vertices[i].Y = slider.CurvePoints[i].Y;
            }

            int start = 0;

            CurveType segmentType = slider.CurveType;

            for (int i = 0; i < slider.CurvePoints.Count; i++)
            {
                if (i < slider.CurvePoints.Count - 1)
                {
                    continue;
                }

                var segmentedVertices = vertices.AsSpan().Slice(start, i - start + 1);
                
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

        private void CalculatedLength()
        {

        }

        private List<Vector2> CalculateSubPath(ReadOnlySpan<Vector2> subControlPoints, CurveType type)
        {
            switch (type)
            {
                case CurveType.Linear:
                    return PathApproximator.LinearToPiecewiseLinear(subControlPoints);

                case CurveType.PerfectCirle:
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

            return PathApproximator.BSplineToPiecewiseLinear(subControlPoints, 0);
        }
    }
}
