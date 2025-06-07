namespace what.Classes.Beatmap.osu.BeatmapClasses
{
    public class HitObjects
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Time { get; set; }
        public Typee Type { get; set; }
        public HitSound HitSound { get; set; }
        public string? ObjectParams { get; set; }
        public string? HitSample { get; set; }
    }

    public enum Typee
    {
        HitCircle = 0,
        Slider = 1 << 0,
        StartNewCombo = 1 << 1,
        Spinner = 1 << 2,
        ComboColourSkip = 1 << 3 | 1 << 4 | 1 << 5,
        HoldNote = 1 << 6,
    }

    public enum HitSound
    {
        Normal = 0,
        Whistle = 1,
        Finish = 2,
        Clap = 3,
    }
}
