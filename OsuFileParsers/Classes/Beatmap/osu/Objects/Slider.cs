using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.SliderPathMath;
using System.Numerics;

namespace OsuFileParsers.Classes.Beatmap.osu.Objects
{
    public class SliderData : HitObjectData
    {
        public double EndTime { get; set; }
        public double DespawnTime { get; set; }
        public Vector2 EndPosition { get; set; }
        public bool AllTicksHit { get; set; } = true;
        public DataHitJudgement SliderEndJudgement { get; set; } = new DataHitJudgement(150, 0);

        /// <summary>
        /// 1 = no repeats, 2 and up = n - 1 repeats.
        /// </summary>
        public int RepeatCount { get; set; }
        public decimal Length { get; set; }

        public SliderPath Path { get; set; } = new SliderPath();
        public List<SliderTick> SliderTicks { get; set; } = new List<SliderTick>();
        public PathControlPoint[] ControlPoints { get; set; } = new PathControlPoint[0];
        public List<Vector2>? CurvePoints { get; set; } = new List<Vector2>();

        public string? EdgeSounds { get; set; }
        public string? EdgeSets { get; set; }
        //public CurveType CurveType { get; set; }
    }

    public class SliderTick
    {
        public SliderTick()
        {

        }

        public SliderTick(Vector2 position, double positionAt, double time)
        {
            Position = position;
            PositionAt = positionAt;
            Time = time;
        }

        public Vector2 Position { get; set; }
        public double PositionAt { get; set; }
        public double Time { get; set; }
    }
}
