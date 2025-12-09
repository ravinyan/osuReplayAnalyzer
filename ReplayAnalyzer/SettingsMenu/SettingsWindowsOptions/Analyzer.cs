using System.Windows.Controls;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    public class Analyzer
    {
        public static void AddOptions(StackPanel panel)
        {
            panel.Children.Add(SettingsOptions.HitmarkersVisibility());
            panel.Children.Add(SettingsOptions.FrameMarkersVisibility());
            panel.Children.Add(SettingsOptions.CursorPathVisibility());
        }
    }
}
