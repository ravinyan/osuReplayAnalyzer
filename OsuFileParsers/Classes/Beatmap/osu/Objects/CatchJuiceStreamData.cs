using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;

namespace OsuFileParsers.Classes.Beatmap.osu.Objects
{
    public class CatchJuiceStreamData : HitObjectData
    {
        public int EndXPosition { get; set; }
        public int EndTime { get; set; }
        public List<SliderTick> Drops { get; set; } = new List<SliderTick>();
    }
}
