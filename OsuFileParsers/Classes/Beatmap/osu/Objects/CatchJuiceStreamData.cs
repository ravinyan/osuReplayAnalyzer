using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.SliderPathMath;

namespace OsuFileParsers.Classes.Beatmap.osu.Objects
{
    public class CatchJuiceStreamData : HitObjectData
    {
        public int EndXPosition { get; set; }
        public int EndYPosition { get; set; }
        public double EndTime { get; set; }
        public int RepeatCount { get; set; } = 1;
        public SliderPath Path { get; set; } = new SliderPath();
        public List<SliderTick> Drops { get; set; } = new List<SliderTick>();
        public List<object> Droplets { get; set; } = new List<object>();
    }
}
