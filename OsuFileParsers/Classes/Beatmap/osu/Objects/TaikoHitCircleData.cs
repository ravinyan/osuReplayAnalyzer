using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;

namespace OsuFileParsers.Classes.Beatmap.osu.Objects
{
    public class TaikoHitCircleData : HitObjectData
    {
        public bool IsBig { get; set; }

        /// <summary>
        /// true = Don (red), false = Kat (blue)
        /// </summary>
        public bool IsDon { get; set; }
    }
}
