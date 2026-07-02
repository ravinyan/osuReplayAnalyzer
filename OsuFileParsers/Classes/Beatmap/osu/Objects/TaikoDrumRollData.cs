using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;

namespace OsuFileParsers.Classes.Beatmap.osu.Objects
{
    public class TaikoDrumRollData : HitObjectData
    {
        public int EndTime { get; set; }
        public bool IsBig { get; set; }
        public double Length { get; set; }
    }
}
