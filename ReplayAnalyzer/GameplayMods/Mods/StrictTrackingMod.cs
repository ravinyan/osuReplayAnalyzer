using OsuFileParsers.Classes.Replay;
using System.Globalization;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class StrictTrackingMod
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
            // change how sliders work here... somehow
        }
    }
}
