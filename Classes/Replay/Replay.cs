namespace what.Classes.Replay
{
    public class Replay
    {
        public byte GameMode { get; set; }
        public int GameVersion { get; set; }
        public string? BeatmapMD5Hash { get; set; }
        public string? PlayerName { get; set; }
        public string? ReplayMD5Hash { get; set; }
        public short Hit300 { get; set; }
        public short Hit100 { get; set; }
        public short Hit50 { get; set; }
        public short Gekis { get; set; }
        public short Katus { get; set; }
        public short MissCount { get; set; }
        public int TotalScore { get; set; }
        public short MaxComboAchieved { get; set; }
        public bool IsFullCombo { get; set; }
        public Mods ModsUsed { get; set; }
        public string? LifeBarGraph { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ReplayDataLength { get; set; }
        public byte[]? CompressedReplayDataLength { get; set; }
        public List<ReplayFrame>? Frames { get; set; } = new List<ReplayFrame>();

        // not needed but its here anyway... also not working oops
        public long ScoreId { get; set; }
        public double AdditionalModInfo { get; set; }
    }

    [Flags]
    public enum Mods
    {
        None           = 0,
        NoFail         = 1 << 0,
        Easy           = 1 << 1,
        TouchDevice    = 1 << 2,
        Hidden         = 1 << 3, // best mod
        HardRock       = 1 << 4,
        SuddenDeath    = 1 << 5,
        DoubleTime     = 1 << 6,
        Relax          = 1 << 7,
        HalfTime       = 1 << 8,
        Nightcore      = (1 << 9) + DoubleTime,
        Flashlight     = 1 << 10,
        Autoplay       = 1 << 11,
        SpunOut        = 1 << 12,
        Autopilot      = 1 << 13,
        Perfect        = 1 << 14,
        Key4           = 1 << 15,
        Key5           = 1 << 16,
        Key6           = 1 << 17,
        Key7           = 1 << 18,
        Key8           = 1 << 19,
        FadeIn         = 1 << 20,
        Random         = 1 << 21,
        Cinema         = 1 << 22,
        TargetPractice = 1 << 23,
        Key9           = 1 << 24,
        Coop           = 1 << 25,
        Key1           = 1 << 26,
        Key3           = 1 << 27,
        Key2           = 1 << 28,
        ScoreV2        = 1 << 29,
        Mirror         = 1 << 30,
    }
}
