using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.MusicPlayer.Controls;
using System.Globalization;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class DoubleTimeMod
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
            RateChangerControls.ChangeBaseRate(1.5);
        }

        private static void ApplyLazer()
        {
            LazerMod doubleTime = MainWindow.replay.LazerMods.Where(mod => mod.Acronym == "DT").First();

            double rateChange;
            if (doubleTime.Settings.ContainsKey("speed_change") == false)
            {
                rateChange = 1.5;
            }
            else
            {
                rateChange = double.Parse((string)doubleTime.Settings["speed_change"], CultureInfo.InvariantCulture.NumberFormat);
            }

            RateChangerControls.ChangeBaseRate(rateChange);
        }
    }
    /* i just want this saved in case it will be needed also math jumpscare            
            // math taken from osu lazer... what even is this monstrocity of math
            double newAr = Math.Sign(ms - 1200) == Math.Sign(450 - 1200)
                         ? (ms - 1200) / (450 - 1200) * 5 + 5
                         : (ms - 1200) / (1200 - 1800) * 5 + 5;
            // for custom speed changes it actually breaks AR values,
            // therefore AR will be always set by speed changing method
            //newMapDifficulty.ApproachRate = (decimal)newAr;

            double greatHitWindow = math.GetOverallDifficultyHitWindow300(newMapDifficulty.OverallDifficulty);
            greatHitWindow = greatHitWindow / 1.5;

            double newOD = Math.Sign(greatHitWindow - 50) == Math.Sign(20 - 50)
                         ? (greatHitWindow - 50) / (20 - 50) * 5 + 5
                         : (greatHitWindow - 50) / (50 - 80) * 5 + 5;
            // ACTUALLY OD might also be affected but will comment it out when testing
    */
}
