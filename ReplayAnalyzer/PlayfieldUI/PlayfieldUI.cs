using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.PlayfieldUI
{
    public class PlayfieldUI
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static bool IsUpdated = false;

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
            // these UI elements need to be only created once
            if (IsUpdated == false)
            {
                Canvas UrBar = URBar.Create();
                Window.osuReplayWindow.Children.Add(UrBar);

                IsUpdated = true;
            }   
        }
    }
}
