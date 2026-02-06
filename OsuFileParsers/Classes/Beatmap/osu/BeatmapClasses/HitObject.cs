using System.Drawing;
using System.Numerics;

namespace OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses
{
    public class HitObjectData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double BaseX { get; set; }
        public double BaseY { get; set; }
        public Vector2 BaseSpawnPosition { get; set; }
        public int SpawnTime { get; set; }
        public ObjectType Type { get; set; }
        public HitSound HitSound { get; set; }
        public string? HitSample { get; set; }

        public int StackHeight { get; set; }
        public float StackOffset { get; set; }

        /// <summary>
        /// 300 = MAX, 100 = OK, 50 = MEH, 0 = MISS, -1 = SLIDER TICK MISS, -2 = SLIDER END MISS, -727 = NONE.
        /// </summary>
        public DataHitJudgement? Judgement { get; set; } = new DataHitJudgement(-727, 0);
        public int ComboNumber { get; set; } = 0;
        public Color RGBValue { get; set; } = new Color();
    }

    public class DataHitJudgement
    {
        public int HitJudgement { get; set; }
        public long SpawnTime { get; set; }

        public DataHitJudgement(int judgement, long spawnTime)
        {
            HitJudgement = judgement;
            SpawnTime = spawnTime;
        }
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
