using System.Numerics;

namespace ReplayParsers.Classes.Beatmap.osu.BeatmapClasses
{
    public class HitObject
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public int SpawnTime { get; set; }
        public ObjectType Type { get; set; }
        public HitSound HitSound { get; set; }
        public string? ObjectParams { get; set; }
        public string? HitSample { get; set; }

        public int StackHeight { get; set; }
        public float Scale { get; set; } = 1;
    }

    [Flags]
    public enum ObjectType
    {
        HitCircle = 1 << 0,
        Slider = 1 << 1,
        StartNewCombo = 1 << 2,
        Spinner = 1 << 3,
        ComboColourSkip = 1 << 4 | 1 << 5 | 1 << 6,
        HoldNote = 1 << 7,
    }

    public enum HitSound
    {
        Normal = 0,
        Whistle = 1,
        Finish = 2,
        Clap = 3,
    }
}
