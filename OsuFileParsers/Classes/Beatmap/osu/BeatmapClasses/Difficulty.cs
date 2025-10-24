namespace OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses
{
    public class Difficulty
    {
        public Difficulty() 
        {
        }

        public Difficulty(Difficulty diff)
        {
            HPDrainRate = diff.HPDrainRate;
            CircleSize = diff.CircleSize;
            OverallDifficulty = diff.OverallDifficulty;
            ApproachRate = diff.ApproachRate;
            SliderMultiplier = diff.SliderMultiplier;
            SliderTickRate = diff.SliderTickRate;
        }

        public decimal HPDrainRate { get; set; }
        public decimal CircleSize { get; set; }
        public decimal OverallDifficulty { get; set; }
        public decimal ApproachRate { get; set; }
        public decimal SliderMultiplier { get; set; }
        public decimal SliderTickRate { get; set; }
    }
}
