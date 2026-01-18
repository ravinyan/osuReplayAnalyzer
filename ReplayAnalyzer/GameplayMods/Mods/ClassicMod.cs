using OsuFileParsers.Classes.Replay;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class ClassicMod
    {
        public static bool IsSliderHeadAccOn = true;
        public static string NotelockClientType = "";

        public static void ApplyValues(bool isLazer)
        {
            if (isLazer == true)
            {
                ApplyLazer();
            }
        }

        private static void ApplyLazer()
        {
            LazerMod classic = MainWindow.replay.LazerMods.Where(mod => mod.Acronym == "CL").First();

            // only implementing head acc and notelock
            // options: no_slider_head_accuracy, classc_note_lock, always_play_tail_sample, fade_hit_circle_early, classic_health
            foreach (KeyValuePair<string, object> setting in classic.Settings)
            {
                switch (setting.Key)
                {
                    case "no_slider_head_accuracy":
                        IsSliderHeadAccOn = false;
                        break;
                    case "classc_note_lock":
                        NotelockClientType = "osu!";
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
