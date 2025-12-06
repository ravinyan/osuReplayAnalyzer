using System.Windows.Controls;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    public class Files
    {
        public static void AddOptions(StackPanel panel)
        {
            panel.Orientation = Orientation.Vertical;
            panel.Children.Add(SettingsOptions.OsuStableSourceFolderLocation());
            panel.Children.Add(SettingsOptions.OsuLazerSourceFolderLocation());
        }
    }
}
