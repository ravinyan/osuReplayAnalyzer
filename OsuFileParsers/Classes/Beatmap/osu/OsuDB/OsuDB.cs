namespace OsuFileParsers.Classes.Beatmap.osu.OsuDB
{
    public class OsuDB
    {
        public int Version { get; set; }
        public int FolderCount { get; set; }
        public bool AccountUnlocked { get; set; }
        public DateTime WhenUnlocked { get; set; }
        public string? PlayerName { get; set; }
        public int NumberOfBeatmaps { get; set; }
        public List<OsuDBBeatmap>? DBBeatmaps { get; set; }
        public UserPermission UserPermission { get; set; }
    }

    public enum UserPermission
    {
        Node = 0,
        Normal = 1 << 0,
        Moderator = 1 << 1,
        Supporter = 1 << 2,
        Friend = 1 << 3,
        Peppy = 1 << 4,
        WorldCupStaff = 1 << 5,
    }
}
