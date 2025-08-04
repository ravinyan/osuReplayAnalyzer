using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.SliderPathMath;
using System.Numerics;

namespace ReplayParsers.Classes.Beatmap.osu.Objects
{
    public class Slider : HitObject
    {
        public CurveType CurveType { get; set; }
        public List<Vector2>? CurvePoints { get; set; } = new List<Vector2>();
        public List<PathControlPoint> ControlPoints { get; set; } = new List<PathControlPoint>();
        public SliderPath Path { get; set; } = new SliderPath();
        public Vector2 EndPosition { get; set; }
        public int RepeatCount { get; set; }
        public decimal Length { get; set; }
        public string? EdgeSounds { get; set; }
        public string? EdgeSets { get; set; }
    }
}
