using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.OsuMaths;

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

            newMapDifficulty.ApproachRate = newMapDifficulty.ApproachRate * 0.5m;
            newMapDifficulty.HPDrainRate = newMapDifficulty.HPDrainRate * 0.5m;

            if (MainWindow.replay.GameMode == OsuFileParsers.Classes.Replay.GameMode.OsuMania)
            {// mania is special
                OsuMath.ManiaDifficultyMultiplier = 1 / 1.4;
            }
            else
            {// mania doesnt like this
                newMapDifficulty.CircleSize = newMapDifficulty.CircleSize * 0.5m;
                newMapDifficulty.OverallDifficulty = newMapDifficulty.OverallDifficulty * 0.5m;
            }

            MainWindow.map.Difficulty = newMapDifficulty;
        }

        private static void ApplyLazer()
        {
            // even tho it has config of extra lives it doesnt matter in the case of this app so its same as stable
            ApplyStable();
        }
    }
}
