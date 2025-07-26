using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Numerics;

namespace ReplayParsers.Classes.Beatmap.osu.Objects
{
    public class Slider : HitObject
    {
        public CurveType CurveType { get; set; }
        public List<Vector2>? CurvePoints { get; set; } = new List<Vector2>();
        public int Slides { get; set; }
        public decimal Length { get; set; }
        public string? EdgeSounds { get; set; }
        public string? EdgeSets { get; set; }
    }

    public enum CurveType
    {
        Bezier,
        Centripetal,
        Linear,
        PerfectCirle,
    }
}
