using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.SettingsMenu
{
    public class SettingsPanel
    {
        public static Grid SettingPanelBox = new Grid();

        public static Grid Create()
        {
            // [main] grid (widht and height of the visible control
            // [2nd] scroll
            // [3rd] grid with options (no height, width same as [main])
            SettingPanelBox.Name = "SettingsPanelBox";
            SettingPanelBox.Width = 400;
            SettingPanelBox.Height = 400;
            SettingPanelBox.VerticalAlignment = VerticalAlignment.Top;
            SettingPanelBox.HorizontalAlignment = HorizontalAlignment.Left;
            SettingPanelBox.Visibility = Visibility.Hidden;
            SettingPanelBox.Background = new SolidColorBrush(Colors.Black) { Opacity = 0.6 };

            Grid settingPanel = new Grid();
            settingPanel.Name = "SettingsPanel";
            settingPanel.Width = 300;
            settingPanel.VerticalAlignment = VerticalAlignment.Top;
            settingPanel.HorizontalAlignment = HorizontalAlignment.Left;

            SettingsOptions settingsOptions = new SettingsOptions();
            settingsOptions.CreateOptions();

            SettingPanelBox.ColumnDefinitions.Add(new ColumnDefinition());
            
            RowDefinition padddingRow = new RowDefinition();
            padddingRow.MaxHeight = 50;
            SettingPanelBox.RowDefinitions.Add(padddingRow);

            for (int i = 1; i <= settingsOptions.Options.Count; i++)
            {
                RowDefinition row = new RowDefinition();
                row.MaxHeight = 50;
                SettingPanelBox.RowDefinitions.Add(row);

                StackPanel option = settingsOptions.Options[i - 1];
                option.Margin = new Thickness(10);
                SettingPanelBox.Children.Add(option);
                Grid.SetRow(option, i);
            }

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Visibility = Visibility.Collapsed;
            scrollViewer.CanContentScroll = true;

            settingPanel.Children.Add(scrollViewer);
            scrollViewer.Content = SettingPanelBox;

            return settingPanel;
        }
    }
}
