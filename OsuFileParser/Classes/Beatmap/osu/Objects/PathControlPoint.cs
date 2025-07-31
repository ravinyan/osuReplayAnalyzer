using System.Numerics;

namespace ReplayParsers.Classes.Beatmap.osu.Objects
{
    public class PathControlPoint
    {
        public CurveType Type { get; set; }
        public Vector2 Position { get; set; }
    }

    public enum CurveType
    {
        Bezier,
        Catmull,
        Linear,
        PerfectCirle,
    }
}
