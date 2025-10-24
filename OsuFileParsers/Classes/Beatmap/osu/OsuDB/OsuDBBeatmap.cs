using OsuFileParsers.Classes.Replay;

namespace OsuFileParsers.Classes.Beatmap.osu.OsuDB
{
    public class OsuDBBeatmap
    {
        public int BeatmapByteSize { get; set; }
        public string? Artist { get; set; }
        public string? ArtistUnicode { get; set; }
        public string? SongTitle { get; set; }
        public string? SongTitleUnicode { get; set; }
        public string? Creator { get; set; }
        public string? Difficulty { get; set; }
        public string? AudioFileName { get; set; }
        public string? BeatmapMD5Hash { get; set; }
        public string? BeatmapFileName { get; set; }
        public RankedStatus RankedStatus { get; set; }
        public short NumberOfCircles { get; set; }
        public short NumberOfSliders { get; set; }
        public short NumberOfSpinners { get; set; }
        public long LastModificationTimeInTicks { get; set; }
        public float ApproachRate { get; set; }
        public float CircleSize { get; set; }
        public float HpDrain { get; set; }
        public float OverallDifficulty { get; set; }
        public double SliderVelocity { get; set; }
        public List<(Mods, double)>? OsuSTDModSR { get; set; }
        public List<(Mods, double)>? OsuTaikoModSR { get; set; }
        public List<(Mods, double)>? OsuCatchModSR { get; set; }
        public List<(Mods, double)>? OsuManiaModSR { get; set; }
        public int DrainTime { get; set; }
        public int TotalTime { get; set; }
        public int AudioPreviewStartTime { get; set; }
        public List<OsuDBTimingPoint>? TimingPoints { get; set; } = new List<OsuDBTimingPoint>();
        public int DifficultyID { get; set; }
        public int BeatmapID { get; set; }
        public int ThreadID { get; set; }
        public byte OsuSTDGrade { get; set; }
        public byte OsuTaikoGrade { get; set; }
        public byte OsuCatchGrade { get; set; }
        public byte OsuManiaGrade { get; set; }
        public short LocalBeatmapOffset { get; set; }
        public float StackLeniency { get; set; }
        public GameMode GameMode { get; set; }
        public string? SongSource { get; set; }
        public string? SongTags { get; set; }
        public short OnlineOffset { get; set; }
        public string? SongTitleFont { get; set; }
        public bool IsBeatmapUnplayed { get; set; }
        public long LastTimePlayed { get; set; }
        public bool IsBeatmapOSZ2 { get; set; }
        public string? BeatmapFolderName { get; set; }
        public long LastTimeCheckedAgainsOsuRepository { get; set; }
        public bool IgnoreBeatmapSound { get; set; }
        public bool IgnoreBeatmapSkin { get; set; }
        public bool DisableStoryboard { get; set; }
        public bool DisableVideo { get; set; }
        public bool VisualOverride { get; set; }
        public short? Unknown { get; set; }
        public int LastModificationTime { get; set; }
        public byte OsuManiaScrollSpeed { get; set; }
    }

    public enum RankedStatus
    {
        Unknown = 0,
        Unsubmitted = 1,
        Pending_WIP_Graveyard = 2,
        Unused = 3,
        Ranked = 4,
        Approved = 5,
        Qualified = 6,
        Loved = 7,
    }
}
