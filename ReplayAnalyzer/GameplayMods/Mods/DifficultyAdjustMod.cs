using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Replay;
using System.Globalization;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class DifficultyAdjustMod
    {
        public static void ApplyValues(bool isLazer)
        {
            if (isLazer == true)
            {
                ApplyLazer();
            }
        }

        private static void ApplyLazer()
        {
            LazerMod difficultyAdjust = MainWindow.replay.LazerMods.Where(mod => mod.Acronym == "DA").First();
            
            // drain rate and extended limits wont be used but putting them here just in case
            //string[] settings = ["circle_size", "approach_rate", "drain_rate", "overall_difficulty", "extended_limits"];

            Difficulty newDifficulty = MainWindow.map.Difficulty!;
            foreach (KeyValuePair<string, object> setting in difficultyAdjust.Settings)
            {
                switch (setting.Key)
                {
                    case "circle_size":
                        decimal cs = decimal.Parse((string)setting.Value, CultureInfo.InvariantCulture.NumberFormat);
                        newDifficulty.CircleSize = cs;
                        break;
                    case "approach_rate":
                        decimal ar = decimal.Parse((string)setting.Value, CultureInfo.InvariantCulture.NumberFormat);
                        newDifficulty.ApproachRate = ar;
                        break;
                    case "overall_difficulty":
                        decimal od = decimal.Parse((string)setting.Value, CultureInfo.InvariantCulture.NumberFormat);
                        newDifficulty.OverallDifficulty = od;
                        break;
                    default:
                        break;
                }
            }

            MainWindow.map.Difficulty = newDifficulty;
        }
    }
}
