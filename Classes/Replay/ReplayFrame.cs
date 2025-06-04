namespace what.Classes.Replay
{
    public class ReplayFrame
    {
        public long TimeBetweenActions { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public Clicks Click { get; set; }
    }

   
    public enum Clicks
    {
        M1 = 1,
        M2 = 2,
        K1 = 4 + M1,
        K2 = 8 + M2,
        Smoke = 16,
    }
}
