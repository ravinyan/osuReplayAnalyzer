using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class EasyMod
    {
        public static void ApplyValues(bool isLazer)
        {
            if (isLazer == false)
            {
                ApplyStable();
            }
            else
            {
                ApplyLazer();
            }
        }
        private static void ApplyStable()
        {
            Difficulty newMapDifficulty = MainWindow.map.Difficulty!;

            newMapDifficulty.CircleSize = newMapDifficulty.CircleSize * 0.5m;
            newMapDifficulty.ApproachRate = newMapDifficulty.ApproachRate * 0.5m;
            newMapDifficulty.OverallDifficulty = newMapDifficulty.OverallDifficulty * 0.5m;
            newMapDifficulty.HPDrainRate = newMapDifficulty.HPDrainRate * 0.5m;

            MainWindow.map.Difficulty = newMapDifficulty;
        }

        private static void ApplyLazer()
        {
            // even tho it has config of extra lives it doesnt matter in the case of this app so its same as stable
            ApplyStable();
        }
    }
}
