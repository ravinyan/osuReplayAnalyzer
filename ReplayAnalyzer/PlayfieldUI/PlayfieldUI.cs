using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.PlayfieldUI
{
    public class PlayfieldUI
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void CreateUIElementsBeforeReplayLoaded()
        {
            StackPanel judgementCounter = JudgementCounter.Create();
            Window.osuReplayWindow.Children.Add(judgementCounter);

            Grid settingsPanel = SettingsPanel.Create();
            Window.osuReplayWindow.Children.Add(settingsPanel);

            Button settingsButton = SettingsButton.Create();
            Window.osuReplayWindow.Children.Add(settingsButton);
        }

        public static void CreateUIElementsAfterReplayLoaded()
        {
            Canvas UrBar = URBar.Create();
            Window.osuReplayWindow.Children.Add(UrBar);
        }
    }
}
