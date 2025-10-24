using System.Numerics;

namespace OsuFileParsers.Classes.Beatmap.osu.Objects
{
    public class PathControlPoint
    {
        public CurveType? Type { get; set; }
        public Vector2 Position { get; set; }


        public PathControlPoint(Vector2 position, CurveType? type = null)
        {
            Position = position;
            Type = type;
        }
    }

    public enum CurveType
    {
        Bezier,
        Catmull,
        Linear,
        PerfectCircle,
    }
}
