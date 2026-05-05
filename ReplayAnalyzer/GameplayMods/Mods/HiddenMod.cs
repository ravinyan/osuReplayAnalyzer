using ReplayAnalyzer.SettingsMenu;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class HiddenMod
    {
        public static bool IsEnabled { get; private set; } = false;

        public static void ChangeHiddenModVisibility()
        {
            IsEnabled = !IsEnabled;
            if (IsEnabled)
            {
                SettingsOptions.SaveConfigOption("IsHiddenModEnabled", "true");
            }
            else
            {
                SettingsOptions.SaveConfigOption("IsHiddenModEnabled", "false");
            }
        }
    }
}
