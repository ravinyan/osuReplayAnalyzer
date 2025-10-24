namespace OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses
{
    public class Metadata
    {
        public string? Title { get; set; }
        public string? TitleUnicode { get; set; }
        public string? Artist { get; set; }
        public string? ArtistUnicode { get; set; }
        public string? Creator { get; set; }
        public string? Version { get; set; }
        public string? Source { get; set; }
        public string? Tags { get; set; }
        public int BeatmapId { get; set; }
        public int BeatmapSetId { get; set; }
    }
}
