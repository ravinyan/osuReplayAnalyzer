using ReplayAnalyzer.AnalyzerTools;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.SettingsMenu;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

namespace ReplayAnalyzer.KeyboardShortcuts
{
    public class ShortcutManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void Initialize()
        {
            Window.KeyDown += ShortcutPicker;
        }

        public static void ShortcutPicker(object sender, KeyEventArgs e)
        {
            string optionName = "";
            foreach (KeyValueConfigurationElement keyValue in SettingsOptions.GetAllSettingsKeyValue())
            {
                if (keyValue.Value == e.Key.ToString())
                {
                    optionName = keyValue.Key;
                    break;
                }
            }

            switch (optionName)
            {
                case "Open Menu":
                    SettingsButton.OpenClose();
                    break;
                case "Pause":
                    PlayPauseControls.PlayPauseButton(null!, null!);
                    break;
                case "Jump 1 Frame to Left":
                    SongSliderControls.SeekByFrame(-727);
                    break;
                case "Jump 1 Frame to Right":
                    SongSliderControls.SeekByFrame(727);
                    break;
                case "Jump to closest past miss":
                    MissFinder.FindClosestMiss(-727);
                    break;
                case "Jump to closest future miss":
                    MissFinder.FindClosestMiss(727);
                    break;
                case "Rate Change -0.25x":
                    RateChangerControls.ChangeRateShortcut(-727);
                    break;
                case "Rate Change +0.25x":
                    RateChangerControls.ChangeRateShortcut(727);
                    break;
                default:
                    break;
            }
        }
    }
}
