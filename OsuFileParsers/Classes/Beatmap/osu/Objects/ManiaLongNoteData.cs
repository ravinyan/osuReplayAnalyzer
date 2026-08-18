using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;

namespace OsuFileParsers.Classes.Beatmap.osu.Objects
{
    public class ManiaLongNoteData : HitObjectData
    {
        public int ColumnIndex { get; set; }
        public int EndTime { get; set; }
        public DataHitJudgement TailJudgement { get; set; } = new DataHitJudgement(-727, 0);
    }
}
