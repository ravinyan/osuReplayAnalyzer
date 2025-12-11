using System.Numerics;

namespace OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses
{
    public class HitObjectData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public int SpawnTime { get; set; }
        public ObjectType Type { get; set; }
        public HitSound HitSound { get; set; }
        public string? ObjectParams { get; set; }
        public string? HitSample { get; set; }

        public int StackHeight { get; set; }
        public float Scale { get; set; } = 1;

        public double HitAt { get; set; } = -1;
        public bool IsHit { get; set; } = false;

        // update that in resize playfield and everywhere and use that for XAML (could just use Diameter but meh its fine)
        public double Width { get; set; }
        public double Height { get; set; }
        //public Judgement Judgement { get; set; }

        /// <summary>
        /// 300 = MAX, 100 = OK, 50 = MEH, 0 = MISS, -1 = SLIDER TICK MISS, -2 = SLIDER END MISS.
        /// </summary>
        public int Judgement { get; set; } = -727;
        public int ComboNumber { get; set; }
    }

    public enum Judgement
    {
        Max = 300,
        Ok = 100,
        Meh = 50,
        Miss = 0,
        SliderTickMiss = -1,
        SliderEndMiss = -2,
        None = -727,
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
