using Microsoft.Win32;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.FileWatcher;
using ReplayAnalyzer.MusicPlayer;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldUI;
using System.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.SettingsMenu
{
    public class SettingsOptions
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public static StackPanel OsuLazerSourceFolderLocation()
        {
            StackPanel panel = CreateOptionPanel();

            TextBlock name = CreateTextBoxForPanel("osu!lazer folder path: ");

            Button button = new Button();
            button.Width = 100;
            button.Height = 25;

            if (config.AppSettings.Settings["OsuLazerFolderPath"].Value == "")
            {
                SaveConfigOption("OsuLazerFolderPath", $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu");
            }
            button.Content = SelectedPath("OsuLazerFolderPath");

            button.Click += delegate (object sender, RoutedEventArgs e)
            {
                OpenFolderDialog dlg = new OpenFolderDialog();
                dlg.Title = "Hello... please select your osu lazer folder";
                dlg.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dlg.ShowDialog();

                SaveConfigOption("OsuLazerFolderPath", dlg.FolderName == "" ? "Select Folder" : dlg.FolderName);

                button.Content = SelectedPath("OsuLazerFolderPath");
                BeatmapFile.Load();
            };

            panel.Children.Add(name);
            panel.Children.Add(button);

            return panel;
        }

        public static StackPanel OsuStableSourceFolderLocation()
        {
            StackPanel panel = CreateOptionPanel();

            TextBlock name = CreateTextBoxForPanel("osu!stable folder path: ");

            Button button = new Button();
            button.Width = 100;
            button.Height = 25;
   
            if (config.AppSettings.Settings["OsuStableFolderPath"].Value == "")
            {
                SaveConfigOption("OsuStableFolderPath", $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!");
            }
            button.Content = SelectedPath("OsuStableFolderPath");

            button.Click += delegate (object sender, RoutedEventArgs e)
            {
                OpenFolderDialog dlg = new OpenFolderDialog();
                dlg.Title = "Hello... please select your osu stable folder";
                dlg.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dlg.ShowDialog();

                SaveConfigOption("OsuStableFolderPath", dlg.FolderName == "" ? "Select Folder" : dlg.FolderName);
 
                button.Content = SelectedPath("OsuStableFolderPath");
                BeatmapFile.Load();
            };

            panel.Children.Add(name);
            panel.Children.Add(button);

            return panel;
        }

        private static string SelectedPath(string folderPathSetting)
        {
            string[] path = config.AppSettings.Settings[folderPathSetting].Value.Split("\\");

            if (path.Length >= 2 
            && (path[path.Length - 2] == Environment.UserDomainName || path[path.Length - 2] == Environment.UserName))
            {
                path[path.Length - 2] = "User";
            }
            else if (path[path.Length - 1] == Environment.UserDomainName || path[path.Length - 1] == Environment.UserName)
            {
                path[path.Length - 1] = "User";
            }

            // i dont know if its possible to have path length of 1 but just did this just in case its possible lol
            return path.Length >= 2 
                ? $"{path[path.Length - 2]}\\{path[path.Length - 1]}" 
                : $"{path[path.Length - 1]}";
        }

        public static StackPanel OsuVersion()
        {
            StackPanel panel = CreateOptionPanel();

            TextBlock name = CreateTextBoxForPanel("osu client Replay is from: ");

            string[] clientOptions = new string[]
            {
                "osu!", "osu!lazer"
            };

            ComboBox comboBox = new ComboBox();
            comboBox.Width = 100;
            comboBox.Height = 25;
            comboBox.SelectedIndex = 0;
            comboBox.ItemsSource = clientOptions;

            comboBox.SelectedItem = config.AppSettings.Settings["OsuClient"].Value;

            comboBox.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                SaveConfigOption("OsuClient", comboBox.SelectedItem.ToString()!);
                BeatmapFile.Load();
            };

            panel.Children.Add(name);
            panel.Children.Add(comboBox);

            return panel;
        }

        public static StackPanel BackgrounOpacity()
        {
            StackPanel panel = CreateOptionPanel();

            double backgroundOpacityValue = Math.Floor(double.Parse(config.AppSettings.Settings["BackgroundOpacity"].Value));
            Window.playfieldBackground.Opacity = backgroundOpacityValue / 100;
            TextBlock name = CreateTextBoxForPanel($"Background Opacity: {backgroundOpacityValue}%");

            Slider slider = new Slider();
            slider.Value = Window.playfieldBackground.Opacity * 100;
            slider.Maximum = 100;
            slider.TickFrequency = 1;
            slider.SmallChange = 1;
            slider.Width = 100;
            slider.VerticalAlignment = VerticalAlignment.Center;
            slider.HorizontalAlignment = HorizontalAlignment.Center;
            slider.Orientation = Orientation.Horizontal;
            slider.Style = Window.Resources["OptionsSliderStyle"] as Style;

            slider.ValueChanged += delegate (object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                Window.playfieldBackground.Opacity = slider.Value / 100;
                name.Text = $"Background Opacity: {(int)slider.Value}%";

                SaveConfigOption("BackgroundOpacity", slider.Value.ToString());
            };

            panel.Children.Add(name);
            panel.Children.Add(slider);

            return panel;
        }

        public static StackPanel ScreenResolution()
        {
            StackPanel panel = CreateOptionPanel();
            
            TextBlock name = CreateTextBoxForPanel("Resolution: ");

            string[] resolutionOptions = new string[] 
            { 
               "800x600", "1280x800", "1360x786", "1440x1080", "1600x1050", "1980x1080", "2560x1440", "2560x1600" 
            };

            ComboBox comboBox = new ComboBox();
            comboBox.Width = 100;
            comboBox.Height = 25;
            comboBox.SelectedIndex = 0;
            comboBox.ItemsSource = resolutionOptions;
            comboBox.Focusable = false;

            comboBox.SelectedItem = config.AppSettings.Settings["ScreenResolution"].Value;
            ChangeResolution(comboBox);

            // i hate math
            comboBox.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                // im lazy so this hides music UI windows when changing resolutions so there is no flying window on screen
                VolumeControls.VolumeWindow.Visibility = Visibility.Collapsed;
                RateChangerControls.RateChangeWindow.Visibility = Visibility.Collapsed;

                SaveConfigOption("ScreenResolution", comboBox.SelectedItem.ToString()!);

                ChangeResolution(comboBox);
            };

            panel.Children.Add(name);
            panel.Children.Add(comboBox);

            return panel;
        }

        private static void ChangeResolution(ComboBox comboBox)
        {
            string[] res = comboBox.SelectedItem.ToString()!.Split('x');

            PropertyInfo? dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            PropertyInfo? dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            double dpiScaleWidth = (int)dpiXProperty!.GetValue(null, null)! / 96.0;
            double dpiScaleHeight = (int)dpiYProperty!.GetValue(null, null)! / 96.0;

            Window.osuReplayWindow.Width = int.Parse(config.AppSettings.Settings["ScreenResolution"].Value.Split('x')[0]) / dpiScaleWidth;
            Window.osuReplayWindow.Height = int.Parse(config.AppSettings.Settings["ScreenResolution"].Value.Split('x')[1]) / dpiScaleHeight;

            double borderWidth = SystemParameters.BorderWidth * dpiScaleHeight * 2;

            double width = (int.Parse(res[0]) + borderWidth) / dpiScaleWidth;
            double height = (int.Parse(res[1]) + borderWidth) / dpiScaleHeight;

            double maxScreenHeight = SystemParameters.PrimaryScreenHeight * dpiScaleHeight;

            double topWindowHeight = System.Windows.Forms.SystemInformation.ToolWindowCaptionHeight * dpiScaleHeight;
            double toolbarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight + dpiScaleHeight;
            if (int.Parse(res[1]) == maxScreenHeight)
            {
                Window.Height = height + borderWidth - toolbarHeight;
                Window.osuReplayWindow.Height -= toolbarHeight * dpiScaleHeight;
            }
            else
            {
                Window.Height = height + borderWidth + topWindowHeight - 22;
            }

            // -22 and + 10 are idk what even adjusted it by hand and it works for pixel perfect screen size
            Window.Width = width + 10;

            if (MainWindow.map != null)
            {
                ResizePlayfield.ResizePlayfieldCanva();
            }

            SettingsPanel.UpdatePosition();
            JudgementTimeline.ChangeTimelineSizeOnResize();
        }

        public static StackPanel HitmarkersVisibility()
        {
            StackPanel panel = CreateOptionPanel();
            
            TextBlock name = CreateTextBoxForPanel("Show Hit Markers: ");

            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;
            checkbox.Focusable = false;

            string showMarkers = config.AppSettings.Settings["ShowHitMarkers"].Value;
            if (showMarkers == "true")
            {
                checkbox.IsChecked = true;
            }
            else
            {
                checkbox.IsChecked = false;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (HitMarker marker in HitMarkerManager.GetAliveHitMarkers())
                {
                    marker.Visibility = Visibility.Visible;
                }

                SaveConfigOption("ShowHitMarkers", "true");
            };
           
            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (HitMarker marker in HitMarkerManager.GetAliveHitMarkers())
                {
                    marker.Visibility = Visibility.Collapsed;
                }

                SaveConfigOption("ShowHitMarkers", "false");
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel FrameMarkersVisibility()
        {
            StackPanel panel = CreateOptionPanel();
            
            TextBlock name = CreateTextBoxForPanel("Show Frame Markers:");

            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;
            checkbox.Focusable = false;

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                //foreach (var marker in Analyser.Analyser.PathMarkers)
                //{
                //    marker.Value.Visibility = Visibility.Visible;
                //}
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                //foreach (var marker in Analyser.Analyser.PathMarkers)
                //{
                //    marker.Value.Visibility = Visibility.Collapsed;
                //}
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel CursorPathVisibility()
        {
            StackPanel panel = CreateOptionPanel();
            
            TextBlock name = CreateTextBoxForPanel("Show Cursor Path:");

            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;
            checkbox.Focusable = false;

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                //foreach (var marker in Analyser.Analyser.CursorPath)
                //{
                //    marker.Value.Visibility = Visibility.Visible;
                //}
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                //foreach (var marker in Analyser.Analyser.CursorPath)
                //{
                //    marker.Value.Visibility = Visibility.Collapsed;
                //}
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel JudgementTimelineVisible100()
        {
            StackPanel panel = CreateOptionPanel();

            TextBlock name = CreateTextBoxForPanel("Timeline x100 Visibility:");

            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;
            checkbox.Focusable = false;

            string showJudgement = config.AppSettings.Settings["Show100OnTimeline"].Value;
            if (showJudgement == "true")
            {
                checkbox.IsChecked = true;
            }
            else
            {
                checkbox.IsChecked = false;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (Line line in JudgementTimeline.TimelineJudgements100)
                {
                    line.Visibility = Visibility.Visible;
                }

                SaveConfigOption("Show100OnTimeline", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (Line line in JudgementTimeline.TimelineJudgements100)
                {
                    line.Visibility = Visibility.Collapsed;
                }

                SaveConfigOption("Show100OnTimeline", "false");
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel JudgementTimelineVisible50()
        {
            StackPanel panel = CreateOptionPanel();

            TextBlock name = CreateTextBoxForPanel("Timeline x50 Visibility:");

            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;
            checkbox.Focusable = false;

            string showJudgement = config.AppSettings.Settings["Show50OnTimeline"].Value;
            if (showJudgement == "true")
            {
                checkbox.IsChecked = true;
            }
            else
            {
                checkbox.IsChecked = false;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (Line line in JudgementTimeline.TimelineJudgements50)
                {
                    line.Visibility = Visibility.Visible;
                }

                SaveConfigOption("Show50OnTimeline", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (Line line in JudgementTimeline.TimelineJudgements50)
                {
                    line.Visibility = Visibility.Collapsed;
                }

                SaveConfigOption("Show50OnTimeline", "false");
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel JudgementTimelineVisibleMiss()
        {
            StackPanel panel = CreateOptionPanel();

            TextBlock name = CreateTextBoxForPanel("Timeline Miss Visibility:");

            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;
            checkbox.Focusable = false;

            string showJudgement = config.AppSettings.Settings["ShowMissOnTimeline"].Value;
            if (showJudgement == "true")
            {
                checkbox.IsChecked = true;
            }
            else
            {
                checkbox.IsChecked = false;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (Line line in JudgementTimeline.TimelineJudgementsMiss)
                {
                    line.Visibility = Visibility.Visible;
                }

                SaveConfigOption("ShowMissOnTimeline", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (Line line in JudgementTimeline.TimelineJudgementsMiss)
                {
                    line.Visibility = Visibility.Collapsed;
                }

                SaveConfigOption("ShowMissOnTimeline", "false");
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static void SaveConfigOption(string key, string value)
        {
            config.AppSettings.Settings[key].Value = value;
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        public static string GetConfigValue(string key)
        {
            return config.AppSettings.Settings[key].Value;
        }

        public static string GetConfigKey(string key)
        {
            return config.AppSettings.Settings[key].Key;
        }

        public static KeyValueConfigurationCollection GetAllSettingsKeyValue()
        {
            return config.AppSettings.Settings;
        }

        private static StackPanel CreateOptionPanel()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;
            panel.Margin = new Thickness(5);

            return panel;
        }

        private static TextBlock CreateTextBoxForPanel(string name)
        {
            TextBlock textBox = new TextBlock();
            textBox.Text = name;
            textBox.Foreground = new SolidColorBrush(Colors.White);
            textBox.Width = 150;
            textBox.VerticalAlignment = VerticalAlignment.Center;

            return textBox;
        }
    }
}
