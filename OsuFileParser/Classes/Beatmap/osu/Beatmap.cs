using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;

namespace ReplayParsers.Classes.Beatmap.osu
{
    public class Beatmap
    {
        public int FileVersion {get; set;}
        public General? General { get; set; }
        public Editor? Editor { get; set; }
        public Metadata? Metadata { get; set; }
        public Difficulty? Difficulty { get; set; }
        public Events? Events { get; set; }
        public List<TimingPoint>? TimingPoints { get; set; }
        public Colours? Colours { get; set; }
        public List<HitObjectData>? HitObjects { get; set; }
    }
}
