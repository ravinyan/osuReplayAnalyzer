using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.SliderPathMath;
using System.Numerics;

namespace ReplayParsers.Classes.Beatmap.osu.Objects
{
    public class SliderData : HitObjectData
    {
        public CurveType CurveType { get; set; }
        public List<Vector2>? CurvePoints { get; set; } = new List<Vector2>();
        public PathControlPoint[] ControlPoints { get; set; } = new PathControlPoint[0];
        public SliderPath Path { get; set; } = new SliderPath();
        public Vector2 EndPosition { get; set; }

        /// <summary>
        /// 1 = no repeats, 2 and up = n - 1 repeats.
        /// </summary>
        public int RepeatCount { get; set; }

        public decimal Length { get; set; }
        public string? EdgeSounds { get; set; }
        public string? EdgeSets { get; set; }
        public double EndTime { get; set; }
        public double DespawnTime { get; set; }
        public SliderTick[] SliderTicks { get; set; } = new SliderTick[0];
    }

    public class SliderTick()
    {
        public Vector2 Position { get; set; }
        public double PositionAt { get; set; }
        public double Time { get; set; }
    }
}
