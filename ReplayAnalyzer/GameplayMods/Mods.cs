using OsuFileParsers.Classes.Replay;

namespace ReplayAnalyzer.GameplayMods
{
    public class Mods
    {
        public static void ApplyMods()
        {
            // using this instead of config to automatically "apply" Classic mod if user takes osu!stable Replay from osu!lazer
            if (MainWindow.replay.IsLazer == false)
            {
                ApplyStableMods(MainWindow.replay.StableMods);
                return;
            }

            ApplyLazerMods(MainWindow.replay.LazerMods);
        }

        private static void ApplyStableMods(OsuFileParsers.Classes.Replay.Mods mods)
        {

        }

        private static void ApplyLazerMods(List<LazerMod> mods)
        {

        }
    }
}
