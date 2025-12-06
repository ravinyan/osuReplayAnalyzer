using ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace ReplayAnalyzer.SettingsMenu
{
    public class SettingsPanel
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Grid SettingsPanelBox = new Grid();

        public static Grid Create()
        {
            // new one
            // [grid] 2x col > 2 [grid] on left (buttons) and right (options) > left and right are stack panels > if needed give right ability to scroll > right panel has stack panels with options
            SettingsPanelBox.Background = new SolidColorBrush(Color.FromRgb(57, 42, 54));
            SettingsPanelBox.Opacity = 0.9;
            SettingsPanelBox.Width = 500;
            SettingsPanelBox.Height = 400;
            SettingsPanelBox.HorizontalAlignment = HorizontalAlignment.Center;
            SettingsPanelBox.VerticalAlignment = VerticalAlignment.Center;
            SettingsPanelBox.Visibility = Visibility.Collapsed;

            ColumnDefinition settingsButtonsCol = new ColumnDefinition();
            settingsButtonsCol.MaxWidth = 200;
            ColumnDefinition settingsOptionsCol = new ColumnDefinition();
            settingsOptionsCol.MaxWidth = 300;

            StackPanel buttonsPanel = CreateButtonsPanel();
            Grid.SetColumn(buttonsPanel, 0);
            SettingsPanelBox.ColumnDefinitions.Add(settingsButtonsCol);
            SettingsPanelBox.Children.Add(buttonsPanel);

            SettingsPanelBox.ColumnDefinitions.Add(settingsOptionsCol);
            string[] settingsOptionsa = ["General", "Gameplay", "Analyzer", "Files"];
            for (int i = 0; i < settingsOptionsa.Length; i++)
            {
                StackPanel panel = CreateOptionPanel(settingsOptionsa[i]);
                CreatePanelOptions(settingsOptionsa[i], panel);

                if (i != 0)
                {
                    panel.Visibility = Visibility.Collapsed;
                }

                Grid.SetColumn(panel, 1);
                SettingsPanelBox.Children.Add(panel);

                TextBlock optionText = CreateOptionText(settingsOptionsa[i], buttonsPanel.Width);
                CreateOptionTextEvent(optionText, panel, SettingsPanelBox);
                buttonsPanel.Children.Add(optionText);
            }

            // just in case coz setting this up was pain in the ass to find
            // main window has child scroll and scroll has child (.Content) panel box
            //ScrollViewer scrollViewer = new ScrollViewer();
            //scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            //scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //scrollViewer.Visibility = Visibility.Collapsed;
            //scrollViewer.CanContentScroll = true;
            //
            //mainWindow.Children.Add(scrollViewer);
            //scrollViewer.Content = SettingPanelBox;

            return SettingsPanelBox;
        }

        public static void UpdatePosition()
        {
            // numbers at the end adjusted by hand to make box in the middle of the screen
            Canvas.SetTop(SettingsPanelBox, (Window.Height / 2 - SettingsPanelBox.Height / 2) - 19);
            Canvas.SetLeft(SettingsPanelBox, Window.Width / 2 - SettingsPanelBox.Width / 2 - 7.5);
        }

        private static StackPanel CreateButtonsPanel()
        {
            StackPanel buttonsPanel = new StackPanel();
            buttonsPanel.Name = "ButtonPanel";
            buttonsPanel.Background = new SolidColorBrush(Color.FromRgb(41, 30, 38));
            buttonsPanel.Margin = new Thickness(20);
            buttonsPanel.Width = 160;
            buttonsPanel.Orientation = Orientation.Vertical;
            buttonsPanel.HorizontalAlignment = HorizontalAlignment.Left;

            return buttonsPanel;
        }

        private static TextBlock CreateOptionText(string name, double width)
        {
            TextBlock imTiredOfStylizingButtons = new TextBlock();
            imTiredOfStylizingButtons.Name = name;
            imTiredOfStylizingButtons.Text = name;
            imTiredOfStylizingButtons.Width = width;
            imTiredOfStylizingButtons.Height = 40;
            imTiredOfStylizingButtons.Foreground = new SolidColorBrush(Colors.White);
            imTiredOfStylizingButtons.TextAlignment = TextAlignment.Center;
            imTiredOfStylizingButtons.HorizontalAlignment = HorizontalAlignment.Center;
            imTiredOfStylizingButtons.FontSize = 25;

            return imTiredOfStylizingButtons;
        }

        private static void CreateOptionTextEvent(TextBlock text, StackPanel panel, Grid mainWindow)
        {
            text.MouseEnter += delegate (object sender, MouseEventArgs e)
            {
                text.Foreground = new SolidColorBrush(Color.FromRgb(255, 102, 198));
            };

            text.MouseLeave += delegate (object sender, MouseEventArgs e)
            {
                text.Foreground = new SolidColorBrush(Colors.White);
            };

            text.MouseDown += delegate (object sender, MouseButtonEventArgs e)
            {
                if (panel.Visibility == Visibility.Visible)
                {
                    return;
                }

                // i = 1 coz index 0 are buttons and everything else after that are actual options panels
                for (int i = 1; i < mainWindow.Children.Count; i++)
                {
                    mainWindow.Children[i].Visibility = Visibility.Collapsed;
                }

                panel.Visibility = Visibility.Visible;
            };
        }

        private static StackPanel CreateOptionPanel(string name)
        {
            StackPanel optionsPanel = new StackPanel();
            optionsPanel.Name = name;
            optionsPanel.Background = new SolidColorBrush(Color.FromRgb(41, 30, 38));
            optionsPanel.Margin = new Thickness(20);
            optionsPanel.Width = 260;
            optionsPanel.Orientation = Orientation.Vertical;
            optionsPanel.HorizontalAlignment = HorizontalAlignment.Left;

            return optionsPanel;
        }

        private static void CreatePanelOptions(string settingName, StackPanel panel)
        {
            switch (settingName)
            {
                case "General":
                    General.AddOptions(panel);
                    break;
                case "Gameplay":
                    Gameplay.AddOptions(panel);
                    break;
                case "Analyzer":
                    Analyzer.AddOptions(panel);
                    break;
                case "Files":
                    Files.AddOptions(panel);
                    break;
                default:
                    throw new Exception("Wrong panel name");
            }
        }
    }
}
