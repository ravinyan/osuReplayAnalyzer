using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;

namespace ReplayParsers.Classes.Beatmap.osu
{
    public class Beatmap
    {
        public const int File = 14;

        public General? General { get; set; }
        public Editor? Editor { get; set; }
        public Metadata? Metadata { get; set; }
        public Difficulty? Difficulty { get; set; }
        public Events? Events { get; set; }
        public List<TimingPoints>? TimingPoints { get; set; }
        public Colours? Colours { get; set; }
        public List<HitObject>? HitObjects { get; set; }
    }
}
