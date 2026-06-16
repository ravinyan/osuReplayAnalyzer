using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;

namespace OsuFileParsers.Classes.Beatmap.osu.Objects
{
    public class ManiaHoldNoteData : HitObjectData
    {
        public int ColumnIndex { get; set; }
        public int EndTime { get; set; }
    }
}
