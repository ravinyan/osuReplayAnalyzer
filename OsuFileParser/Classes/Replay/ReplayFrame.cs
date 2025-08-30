namespace ReplayParsers.Classes.Replay
{
    public class ReplayFrame
    {
        public long Time { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public Clicks Click { get; set; }
    }

    public enum Clicks
    {
        M1 = 1,
        M2 = 2,
        M12 = M1 + M2,
        K1 = 4 + M1,
        K2 = 8 + M2,
        K12 = K1 + K2,
        Smoke = 16,
    }
}
