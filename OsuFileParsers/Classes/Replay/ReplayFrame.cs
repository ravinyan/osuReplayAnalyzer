namespace OsuFileParsers.Classes.Replay
{
    public class ReplayFrame
    {
        public long Time { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public List<Clicks> Clicks { get; set; } = new List<Clicks>();
    }

    [Flags]
    public enum Clicks
    {
        // taiko and catch might be able to use these keys, if not taiko/catch will start from number 60
        M1    = 1,
        M2    = 2,
        M12   = M1 + M2,
        K1A   = 4,
        K2A   = 8,
        K1    = 4 + M1,
        K2    = 8 + M2,
        K12   = K1 + K2,
        Smoke = 16,

        // for the mania keys with 10k for now and maybe more if i finish all gamemodes
        ManiaK1  = 20,
        ManiaK2  = 21,
        ManiaK3  = 22,
        ManiaK4  = 23,
        ManiaK5  = 24,
        ManiaK6  = 25,
        ManiaK7  = 26,
        ManiaK8  = 27,
        ManiaK9  = 28,
        ManiaK10 = 29,
    }
}
