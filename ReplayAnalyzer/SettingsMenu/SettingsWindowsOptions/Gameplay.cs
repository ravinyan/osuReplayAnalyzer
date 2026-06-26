using System.Windows.Controls;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    public class Gameplay
    {
        public static void AddOptions(StackPanel panel)
        {
            panel.Children.Add(SettingsOptions.BackgrounOpacity());
            panel.Children.Add(SettingsOptions.PlayfieldBorder());
            panel.Children.Add(SettingsOptions.HiddenModVisibility());
            panel.Children.Add(SettingsOptions.ExternalSkinFolderPath());
            panel.Children.Add(SettingsOptions.ChangeSkin());
            panel.Children.Add(SettingsOptions.ManiaChangeScrollVelocity());
        }
    }
}
