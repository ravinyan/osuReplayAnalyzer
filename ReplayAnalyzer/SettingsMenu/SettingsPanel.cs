using ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReplayAnalyzer.SettingsMenu
{
    public class SettingsPanel
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Grid SettingsPanelBox = new Grid();

        public static Grid Create()
        {
            ApplyPropertiesToSettingsPanelBox();

            StackPanel buttonsPanel = CreateButtonsPanel();

            ColumnDefinition buttonsPanelCol = new ColumnDefinition();
            buttonsPanelCol.MaxWidth = 200;

            Grid.SetColumn(buttonsPanel, 0);
            SettingsPanelBox.ColumnDefinitions.Add(buttonsPanelCol);
            SettingsPanelBox.Children.Add(buttonsPanel);

            ColumnDefinition optionsPanelCol = new ColumnDefinition();
            optionsPanelCol.MaxWidth = 300;

            SettingsPanelBox.ColumnDefinitions.Add(optionsPanelCol);
            string[] settingsOptionsa = ["General", "Gameplay", "Analyzer", "Files", "Shortcuts", "Updates"];
            for (int i = 0; i < settingsOptionsa.Length; i++)
            {
                StackPanel panel = CreateSettingsPanel(settingsOptionsa[i], i == 0);
                Grid.SetColumn(panel, 1);
                SettingsPanelBox.Children.Add(panel);

                CreateSettings(settingsOptionsa[i], panel);

                TextBlock button = CreateButton(settingsOptionsa[i], buttonsPanel.Width);
                CreateButtonEvents(button, panel, SettingsPanelBox);
                buttonsPanel.Children.Add(button);
            }

            Canvas.SetZIndex(SettingsPanelBox, 9999);

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

        private static void ApplyPropertiesToSettingsPanelBox()
        {
            SolidColorBrush panelBoxBgColour = new SolidColorBrush(Color.FromRgb(57, 42, 54));
            panelBoxBgColour.Opacity = 0.9;
            SettingsPanelBox.Background = panelBoxBgColour;
            SettingsPanelBox.Name = "SettingsMenu";
            SettingsPanelBox.Width = 500;
            SettingsPanelBox.Height = 400;
            SettingsPanelBox.HorizontalAlignment = HorizontalAlignment.Center;
            SettingsPanelBox.VerticalAlignment = VerticalAlignment.Center;
            SettingsPanelBox.Visibility = Visibility.Collapsed;
        }

        private static StackPanel CreateButtonsPanel()
        {
            StackPanel buttonsPanel = new StackPanel();
            buttonsPanel.Name = "ButtonPanel";
            SolidColorBrush buttonsPanelBgColour = new SolidColorBrush(Color.FromRgb(41, 30, 38));
            buttonsPanelBgColour.Opacity = 0.6;
            buttonsPanel.Background = buttonsPanelBgColour;
            buttonsPanel.Margin = new Thickness(20);
            buttonsPanel.Width = 160;
            buttonsPanel.Orientation = Orientation.Vertical;
            buttonsPanel.HorizontalAlignment = HorizontalAlignment.Left;

            return buttonsPanel;
        }

        private static TextBlock CreateButton(string name, double width)
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

        private static void CreateButtonEvents(TextBlock button, StackPanel panel, Grid settingsWindow)
        {
            button.MouseEnter += delegate (object sender, MouseEventArgs e)
            {
                button.Foreground = new SolidColorBrush(Color.FromRgb(255, 102, 198));
            };

            button.MouseLeave += delegate (object sender, MouseEventArgs e)
            {
                button.Foreground = new SolidColorBrush(Colors.White);
            };

            button.MouseDown += delegate (object sender, MouseButtonEventArgs e)
            {
                if (panel.Visibility == Visibility.Visible)
                {
                    return;
                }

                // i = 1 coz index 0 are buttons and everything else after that are actual options panels
                // this makes all panels collapsed so the only visible panel will be the clicked panel (could check for which one is visible but doesnt matter)
                for (int i = 1; i < settingsWindow.Children.Count; i++)
                {
                    settingsWindow.Children[i].Visibility = Visibility.Collapsed;
                }

                panel.Visibility = Visibility.Visible;
            };
        }

        private static StackPanel CreateSettingsPanel(string name, bool shouldBeVisible)
        {
            StackPanel settingsPanel = new StackPanel();
            settingsPanel.Name = name;
            SolidColorBrush optionsPanelBgColour = new SolidColorBrush(Color.FromRgb(41, 30, 38));
            optionsPanelBgColour.Opacity = 0.6;
            settingsPanel.Background = optionsPanelBgColour;
            settingsPanel.Margin = new Thickness(20);
            settingsPanel.Width = 260;
            settingsPanel.Orientation = Orientation.Vertical;
            settingsPanel.HorizontalAlignment = HorizontalAlignment.Left;

            if (shouldBeVisible == false)
            {
                settingsPanel.Visibility = Visibility.Collapsed;
            }

            return settingsPanel;
        }

        private static void CreateSettings(string settingName, StackPanel panel)
        {
            switch (settingName)
            {
                case "General":
                    General.AddOptions(panel);
                    break;
                case "Gameplay":
                    Gameplay.AddOptions(panel);
                    break;
                case "Skin":
                    Skin.AddOptions(panel); // one day surely the POTENTIAL the V I S I O N
                    break;
                case "Analyzer":
                    Analyzer.AddOptions(panel);
                    break;
                case "Files":
                    Files.AddOptions(panel);
                    break;
                case "Shortcuts":
                    Shortcuts.AddOptions(panel);
                    break;
                case "Updates":
                    Updates.AddOptions(panel);
                    break;
                default:
                    throw new Exception("Wrong panel name");
            }
        }
    }
}
