namespace what.Classes.Replay
{
    public class ReplayFrame
    {
        public long TimeBetweenActions { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public MouseButtons Clicks { get; set; }
    }

    public enum MouseButtons
    {
        M1 = 1,
        M2 = 2,
        K1 = 4,
        K2 = 8,
        Smoke = 16,
    }
}
