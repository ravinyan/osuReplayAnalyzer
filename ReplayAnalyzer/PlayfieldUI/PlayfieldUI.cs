using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using System.Windows;
using System.Windows.Controls;
using ReplayAnalyzer;

namespace ReplayAnalyzer.PlayfieldUI
{
    public class PlayfieldUI
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void CreateUIGrid()
        {
            Grid grid = new Grid();

            RowDefinition rowDefinition1 = new RowDefinition();
            rowDefinition1.MaxHeight = 100;

            ColumnDefinition columnDefinition1 = new ColumnDefinition();
            ColumnDefinition columnDefinition2 = new ColumnDefinition();
            ColumnDefinition columnDefinition3 = new ColumnDefinition();

            grid.RowDefinitions.Add(rowDefinition1);
            grid.ColumnDefinitions.Add(columnDefinition1);
            grid.ColumnDefinitions.Add(columnDefinition2);
            grid.ColumnDefinitions.Add(columnDefinition3);

            StackPanel judgementCounter = JudgementCounter.Create();
            Grid.SetRow(judgementCounter, 1);
            Grid.SetColumn(judgementCounter, 3);
            grid.Children.Add(judgementCounter);

            Grid settingsPanel = SettingsPanel.Create();
            Window.osuReplayWindow.Children.Add(settingsPanel);

            Button settingsButton = SettingsButton.Create();
            Grid.SetRow(settingsButton, 1);
            Grid.SetColumn(settingsButton, 0);
            grid.Children.Add(settingsButton);

            Window.osuReplayWindow.Children.Add(grid);
        }
    }
}
