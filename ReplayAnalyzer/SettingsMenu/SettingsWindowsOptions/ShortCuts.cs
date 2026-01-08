using ReplayAnalyzer.KeyboardShortcuts;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    // Yes im using TextBlock as a Button. No i will not use and stylize Button coz its such pain in the ass i rather eat plastic
    public class Shortcuts
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static (string keybindDescription, TextBlock textBlock) KeybindData = ("", null!);
        public static bool IsConfiguring = false;

        private static bool IsKeybindChanging = false;

        public static void AddOptions(StackPanel panel)
        {
            bool startChecking = false;
            foreach (KeyValueConfigurationElement keyValue in SettingsOptions.GetAllSettingsKeyValue())
            {
                // Open Menu is the start of all options...
                // since i iterate over every option confing it should only look at values when Open Menu is reached
                if (keyValue.Key == "Open Menu")
                {
                    startChecking = true;
                }

                if (startChecking == true)
                {
                    panel.Children.Add(CreatePanel(keyValue.Key, KeyToString(keyValue.Value)));
                }
            }
        }

        private static StackPanel CreatePanel(string keybindDescription, string keybind)
        {
            StackPanel panel = Panel();
            TextBlock shortcutDescription = ShortcutDescription(keybindDescription);
            TextBlock shortcutKeybind = ShortcutKeybind(keybind);
            ApplyShortcutKeybindEvents(keybindDescription, shortcutKeybind);

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

        private static void ApplyShortcutKeybindEvents(string keybindDescription, TextBlock keybind)
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
                // from small testing it works but idk
                if (IsKeybindChanging == true)
                {
                    // funny thing i found this coz 90% of clicks in my mouse are double clicks...
                    // if keybind is already being changed then return coz otherwise the event is going to be subscribed
                    // multiple times causing keybinds to work multiple times
                    return;
                }

                IsKeybindChanging = true;

                keybind.Text = "Press Key";
                KeybindData = (keybindDescription, keybind);
                Window.KeyDown -= ShortcutManager.ShortcutPicker;
                Window.KeyDown += ListenForKeyDown;
                IsConfiguring = true;
            };
        }

        private static void ListenForKeyDown(object sender, KeyEventArgs e)
        {
            Window.KeyDown -= ListenForKeyDown;
            Window.KeyDown += ShortcutManager.ShortcutPicker;

            if (KeybindData.textBlock == null)
            {
                IsKeybindChanging = false;
                return;
            }

            string newKeybind = KeyToString(e.Key.ToString());
            ChangeConfigKeybind(KeybindData.keybindDescription, newKeybind);

            // begone to the garbage land you stinky xaml
            if (KeybindData.textBlock != null)
            {
                KeybindData = (null!, null!);
            }

            IsKeybindChanging = false;
        }

        private static void ChangeConfigKeybind(string keybindDescription, string keybind)
        {
            string[] keys = SettingsOptions.GetAllSettingsKeyValue().AllKeys;
            foreach (string key in keys)
            {
                if (SettingsOptions.GetConfigValue(key) == StringToKey(keybind))
                {
                    if (SettingsOptions.GetConfigKey(key) == keybindDescription)
                    {
                        KeybindData.textBlock.Text = KeyToString(SettingsOptions.GetConfigValue(keybindDescription));
                        IsConfiguring = false;
                        return;
                    }

                    KeybindData.textBlock.Text = KeyToString(SettingsOptions.GetConfigValue(keybindDescription));
                    IsConfiguring = false;
                    MessageBox.Show("Can't have duplicate keybindings");
                    return;
                }
            }

            KeybindData.textBlock.Text = keybind;

            SettingsOptions.SaveConfigOption(keybindDescription, keybind);
            IsConfiguring = false;
        }

        // for things to string functions i think i should also do stuff for ; ' ] [ and all that... will fix if it becomes issue
        // idk if its better but i prefer having it this way
        private static string KeyToString(string key)
        {
            if (key == "Left" || key == "Right" || key == "Up" || key == "Down")
            {
                string newText = key + " " + "Arrow";
                key = newText;
            }

            if (key == "OemComma")
            {
                key = ",";
            }

            if (key == "OemPeriod")
            {
                key = ".";
            }

            return key;
        }


        // idk if its better but i prefer having it this way
        private static string StringToKey(string s)
        {
            if (s == "Left Arrow" || s == "Right Arrow" || s == "Up Arrow" || s == "Down Arrow")
            {
                string newText = s.Remove(s.Length - 6);
                s = newText;
            }

            if (s == ",")
            {
                s = "OemComma";
            }

            if (s == ".")
            {
                s = "OemPeriod";
            }

            return s;
        }
    }
}
