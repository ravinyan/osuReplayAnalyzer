namespace ReplayParsers.Classes.Beatmap.osu.BeatmapClasses
{
    public class Events
    {
        public string? Backgrounds { get; set; }
        public string? Videos { get; set; }
        public List<string>? Breaks { get; set; } = new List<string>();

        // maybe one day or never
        // public Storyboard Storyboard { get; set; }
    }
}
