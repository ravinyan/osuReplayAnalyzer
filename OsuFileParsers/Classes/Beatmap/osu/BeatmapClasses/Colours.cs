using System.Drawing;

namespace OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses
{
    public class Colours
    {
        public List<Color>? ComboColour { get; set; } = new List<Color>();
        public Color SliderTrackOverride { get; set; }
        public Color SliderBorder { get; set; } 
    }
}
