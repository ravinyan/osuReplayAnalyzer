using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;

namespace ReplayParsers.Classes.Beatmap.osu.Objects
{
    public class Slider : HitObject
    {
        public CurveType CurveType { get; set; }
        public string? CurvePoints { get; set; }
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
