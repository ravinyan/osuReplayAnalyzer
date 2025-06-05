using what.Classes.Beatmap.BeatmapClasses;

namespace what.Classes.Beatmap
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
        public List<HitObjects>? HitObjects { get; set; }
    }
}
