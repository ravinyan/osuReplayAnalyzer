using ReplayAnalyzer.KeyboardShortcuts;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    public class Shortcuts
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static (string, TextBlock) KeybindData = ("", null!);

        public static void AddOptions(StackPanel panel)
        {
            panel.Children.Add(CreatePanel("make shortcuts clickable and", "changeable"));
            panel.Children.Add(CreatePanel("Rate Change +0.25x", "Up Arrow"));
            panel.Children.Add(CreatePanel("Rate Change -0.25x", "Down Arrow"));
            panel.Children.Add(CreatePanel("Jump to closest past miss", "Left Arrow"));
            panel.Children.Add(CreatePanel("Jump to closest future miss", "Right Arrow"));
            panel.Children.Add(CreatePanel("Jump 1 Frame to Left", ","));
            panel.Children.Add(CreatePanel("Jump 1 Frame to Right", "."));
            panel.Children.Add(CreatePanel("Open Menu", "Escape"));
            panel.Children.Add(CreatePanel("Pause", "Space"));
        }

        private static StackPanel CreatePanel(string shortcutName, string shortcut)
        {
            StackPanel panel = Panel();
            TextBlock shortcutDescription = ShortcutDescription(shortcutName);
            TextBlock shortcutKeybind = ShortcutKeybind(shortcut);
            ApplyShortcutKeybindEvents(shortcutName, shortcutKeybind);

            panel.Children.Add(shortcutDescription);
            panel.Children.Add(shortcutKeybind);

            return panel;
        }

        private static StackPanel Panel()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;
            panel.Margin = new Thickness(5);
            panel.Height = 25;

            return panel;
        }

        private static TextBlock ShortcutDescription(string name)
        {
            TextBlock shortcutDescription = new TextBlock();
            shortcutDescription.Text = name + ": ";
            shortcutDescription.Foreground = new SolidColorBrush(Colors.White);
            shortcutDescription.Width = 170;
            shortcutDescription.VerticalAlignment = VerticalAlignment.Center;

            return shortcutDescription;
        }

        private static TextBlock ShortcutKeybind(string name)
        {
            TextBlock shortcutKeybind = new TextBlock();
            shortcutKeybind.Text = name;
            shortcutKeybind.Foreground = new SolidColorBrush(Colors.White);
            shortcutKeybind.Width = 80;
            shortcutKeybind.FontWeight = FontWeights.Bold;
            shortcutKeybind.TextAlignment = TextAlignment.Center;
            shortcutKeybind.VerticalAlignment = VerticalAlignment.Center;

            return shortcutKeybind;
        }

        private static void ApplyShortcutKeybindEvents(string shortcutName, TextBlock keybind)
        {
            keybind.MouseEnter += delegate (object sender, MouseEventArgs e)
            {
                keybind.Foreground = new SolidColorBrush(Color.FromRgb(255, 102, 198));
            };

            keybind.MouseLeave += delegate (object sender, MouseEventArgs e)
            {
                keybind.Foreground = new SolidColorBrush(Colors.White);
            };

            keybind.MouseDown += delegate (object sender, MouseButtonEventArgs e)
            {
                keybind.Text = "Press Key";
                KeybindData = (shortcutName, keybind);
                Window.KeyDown -= ShortcutManager.ShortcutPicker;
                Window.KeyDown += ListenForKeyDown;
            };
        }

        private static void ListenForKeyDown(object sender, KeyEventArgs e)
        {
            // just for better clarity
            string text = e.Key.ToString();
            if (text == "Left" || text == "Right" || text == "Up" || text == "Down")
            {
                string newText = text + " " + "Arrow";
                text = newText;
            }

            if (KeybindData.Item2 == null)
            {
                Window.KeyDown -= ListenForKeyDown;
                Window.KeyDown += ShortcutManager.ShortcutPicker;
                return;
            }

            //ChangeConfigKeybind(KeybindData.Item1, KeybindData.Item2.Text);

            KeybindData.Item2.Text = text;
            Window.KeyDown -= ListenForKeyDown;
            Window.KeyDown += ShortcutManager.ShortcutPicker;

            // begone to the garbage land you stinky xaml
            if (KeybindData.Item2 != null)
            {
                KeybindData = (null!, null!);
            }
        }

        private static void ChangeConfigKeybind(string keybindFunction, string keybind)
        {
            SettingsOptions.config.AppSettings.Settings["a"].Value = "b";
            SettingsOptions.config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(SettingsOptions.config.AppSettings.SectionInformation.Name);
        }
    }
}
