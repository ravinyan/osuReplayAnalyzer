namespace OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses
{
    public class TimingPoint
    {
        public decimal Time { get; set; }
        public decimal BeatLength { get; set; }
        public int Meter { get; set; }
        public int SampleSet { get; set; }
        public int SampleIndex { get; set; }
        public int Volume { get; set; }
        public bool Uninherited { get; set; }
        public Effects Effects { get; set; }
    }

    public enum Effects
    {
        None = 0,
        Kiai = 1,
        FirstBarlineOmited = 1 << 3,
    }

    public enum SampleSet
    {
        Default = 0,
        Normal = 1,
        Soft = 2,
        Drum = 3,
    }
}
