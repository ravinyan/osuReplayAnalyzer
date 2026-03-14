using ReplayAnalyzer.AnalyzerTools.KeyOverlay;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldUI
{
    public class PlayfieldUI
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static bool IsUpdated = false;

        public static void CreateUIElementsBeforeReplayLoaded()
        {
            Window.osuReplayWindow.Children.Add(JudgementCounter.Create());

            Window.ApplicationWindowUI.Children.Add(SettingsPanel.Create());

            Window.osuReplayWindow.Children.Add(SettingsButton.Create());
        }

        public static void CreateUIElementsAfterReplayLoaded()
        {
            Window.osuReplayWindow.Children.Add(URBar.Create());

            // these UI elements need to be only created once
            if (IsUpdated == false)
            {
                Window.ApplicationWindowUI.Children.Add(KeyOverlay.Create());
                
                IsUpdated = true;
            }   
        }
    }
}
