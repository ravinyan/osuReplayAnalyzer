namespace OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses
{
    public class General
    {
        public string? AudioFileName { get; set; }
        public int AudioLeadIn { get; set; } = 0;
        public string? AudioHash { get; set; }
        public int PreviewTime { get; set; } = -1;
        public int Countdown { get; set; } = 1;
        public string? SampleSet { get; set; } = new string("Normal");
        public decimal StackLeniency { get; set; } = new decimal(0.7);
        public int Mode { get; set; } = 0;
        public bool LetterboxInBreaks { get; set; } = false;
        public bool StoryFireInFront { get; set; } = true;
        public bool UseSkinSprites { get; set; } = false;
        public bool AlwaysShowPlayfield { get; set; } = false;
        public string? OverlayPosition { get; set; } = new string("NoChange");
        public string? SkinPreference { get; set; }
        public bool EpilepsyWarning { get; set; } = false;
        public int CountdownOffset { get; set; } = 0;
        public bool SpecialStyle { get; set; } = false;
        public bool WidescreenStoryboard { get; set; } = false;
        public bool SamplesMatchPlaybackRate { get; set; } = false;

    }
}
