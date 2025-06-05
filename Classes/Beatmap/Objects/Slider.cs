namespace what.Classes.Beatmap.Objects
{
    public class Slider
    {
        public CurveType CurveType { get; set; }
        public string? CurvePoints { get; set; }
        public int Slides { get; set; }
        public decimal Length { get; set; }
        public string? EdgeSounds { get; set; }
        public string? EdgeSets { get; set; }
    }

    // dont know how in what way its best to get the chars... just in case maybe will do ASCII
    // B = 66
    // C = 67
    // L = 76
    // P = 80
    public enum CurveType
    {
        Bezier,
        Centripetal,
        Linear,
        PerfectCirle,
    }
}
