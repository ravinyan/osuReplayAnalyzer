using System.Windows.Controls;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    public class General
    {
        public static void AddOptions(StackPanel panel)
        {
            panel.Children.Add(SettingsOptions.OsuVersion());
            panel.Children.Add(SettingsOptions.ScreenResolution());
        }
    }
}
