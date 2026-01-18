using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.MusicPlayer.Controls;
using System.Globalization;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class HalfTimeMod
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
            RateChangerControls.ChangeBaseRate(0.75);
        }

        private static void ApplyLazer()
        {
            LazerMod halfTime = MainWindow.replay.LazerMods.Where(mod => mod.Acronym == "HT").First();

            double rateChange;
            if (halfTime.Settings.ContainsKey("speed_change") == false)
            {
                rateChange = 0.75;
            }
            else
            {
                rateChange = double.Parse((string)halfTime.Settings["speed_change"], CultureInfo.InvariantCulture.NumberFormat);
            }

            RateChangerControls.ChangeBaseRate(rateChange);
        }
    }
}
