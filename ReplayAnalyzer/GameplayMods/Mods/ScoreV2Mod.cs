using OsuFileParsers.Classes.Replay;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class ScoreV2Mod
    {
        public static bool osuEnabled { get; private set; } = false;
        public static bool maniaEnabled { get; private set; } = false;

        // idk if this is exactly how i want to do this but my head hurts too much to think about it
        public static void ApplyValues()
        {
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    ClassicMod.IsSliderHeadAccOn = true;
                    ClassicMod.IsClassicEnabled = true;
                    ClassicMod.NotelockClientType = "osu!lazer";
                    break;
                case GameMode.OsuMania:
                    maniaEnabled = true;
                    break;
            }
            
        }
    }
}
