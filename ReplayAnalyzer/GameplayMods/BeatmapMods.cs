using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameplayMods.Mods;

namespace ReplayAnalyzer.GameplayMods
{
    public class BeatmapMods
    {
        public static void Apply()
        {
            // using this instead of config to automatically "apply" Classic mod if user takes osu!stable Replay from osu!lazer
            if (MainWindow.replay.IsLazer == false)
            {
                ApplyStableMods(MainWindow.replay.StableMods, MainWindow.replay.IsLazer);
                return;
            }

            ApplyLazerMods(MainWindow.replay.LazerMods, MainWindow.replay.IsLazer);
        }

        private static void ApplyStableMods(OsuFileParsers.Classes.Replay.Mods mods, bool isLazer)
        {
            ClassicMod.NotelockClientType = "osu!";
            ClassicMod.IsSliderHeadAccOn = false;

            string[] stableMods = mods.ToString().Split(", ");
            foreach (string mod in stableMods)
            {
                switch (mod)
                {
                    case "DoubleTime":
                    case "Nightcore":
                        DoubleTimeMod.ApplyValues(isLazer);
                        break;
                    case "HalfTime":
                    case "Daycore":
                        HalfTimeMod.ApplyValues(isLazer);
                        break;
                    case "HardRock":
                        HardRockMod.ApplyValues(isLazer);
                        break;
                    case "Easy":
                        EasyMod.ApplyValues(isLazer);
                        break;
                }
            }
        }

        private static void ApplyLazerMods(List<LazerMod> mods, bool isLazer)
        {
            ClassicMod.NotelockClientType = "osu!lazer";
            ClassicMod.IsSliderHeadAccOn = true;

            // im not implementing anything other than what is here unless somehow someone asks me to
            // implementing Fun mods is gonna be pain in the ass and i dont feel like doing it if i dont need to (also not doing TP if not needed)
            foreach (LazerMod mod in mods)
            {
                switch (mod.Acronym)
                {
                    case "DT":
                    case "NC":
                        DoubleTimeMod.ApplyValues(isLazer);
                        break;
                    case "HT":
                    case "DC":
                        HalfTimeMod.ApplyValues(isLazer);
                        break;
                    case "HR":
                        HardRockMod.ApplyValues(isLazer);
                        break;
                    case "EZ":
                        EasyMod.ApplyValues(isLazer);
                        break;
                    case "DA":
                        DifficultyAdjustMod.ApplyValues(isLazer);
                        break;
                    case "MR":
                        MirrorMod.ApplyValues(isLazer);
                        break;
                    case "CL":
                        ClassicMod.ApplyValues(isLazer);
                        break;
                    case "ST": // not done
                        //StrictTrackingMod.ApplyValues(isLazer);
                        break;
                    case "RD": // trying this wont hurt... hopefully (it did hurt)
                        //RandomMod.ApplyValues(isLazer);
                        break;
                }
            }
        }
    }
}
