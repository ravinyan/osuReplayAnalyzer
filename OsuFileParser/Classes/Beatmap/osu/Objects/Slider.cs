using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.SliderPathMath;
using System.Numerics;

namespace ReplayParsers.Classes.Beatmap.osu.Objects
{
    public class Slider : HitObject
    {
        public CurveType CurveType { get; set; }
        public List<Vector2>? CurvePoints { get; set; } = new List<Vector2>();
        public PathControlPoint[] ControlPoints { get; set; } = new PathControlPoint[0];
        public SliderPath Path { get; set; } = new SliderPath();
        public Vector2 EndPosition { get; set; }
        public int RepeatCount { get; set; }
        public decimal Length { get; set; }
        public string? EdgeSounds { get; set; }
        public string? EdgeSets { get; set; }
        public double EndTime { get; set; }
        public SliderTick[] SliderTicks { get; set; } = new SliderTick[0];
    }

    public class SliderTick()
    {
        public Vector2 Position { get; set; }
    }
}
