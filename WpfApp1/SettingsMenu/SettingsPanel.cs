using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1.SettingsMenu
{
    public class SettingsPanel
    {
        public static Grid SettingPanel = new Grid();
        public static ScrollViewer ScrollViewer = new ScrollViewer();
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static ScrollViewer Create()
        {
            SettingPanel.Background = new SolidColorBrush(Colors.Black) { Opacity = 0.6 };
            SettingPanel.Width = 400;
            SettingPanel.Height = 700;
            SettingPanel.Visibility = System.Windows.Visibility.Hidden;
            SettingPanel.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            SettingPanel.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            SettingPanel.FlowDirection = System.Windows.FlowDirection.LeftToRight;

            SettingsOptions settingsOptions = new SettingsOptions();
            settingsOptions.CreateOptions();

            SettingPanel.ColumnDefinitions.Add(new ColumnDefinition());
            
            RowDefinition padddingRow = new RowDefinition();
            padddingRow.MaxHeight = 50;
            SettingPanel.RowDefinitions.Add(padddingRow);

            for (int i = 1; i <= settingsOptions.Options.Count; i++)
            {
                RowDefinition row = new RowDefinition();
                row.MaxHeight = 50;
                SettingPanel.RowDefinitions.Add(row);

                StackPanel option = settingsOptions.Options[i - 1];
                SettingPanel.Children.Add(option);
                Grid.SetRow(option, i);
            }


            ScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            ScrollViewer.Content = SettingPanel;
            ScrollViewer.FlowDirection = System.Windows.FlowDirection.RightToLeft;

            ScrollViewer.Style = Window.FindResource("ScrollViewWithoutArrows") as Style;
            

        

            return ScrollViewer;
        }
    }
}
