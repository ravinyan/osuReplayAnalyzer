using Microsoft.Win32;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.AnalyzerTools.KeyOverlay;
using ReplayAnalyzer.FileWatcher;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.MusicPlayer;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldUI;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

            // default path if nothing is set
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

                // hopefully this works only checking for replay folder
                string path = dlg.FolderName == "" ? "" : dlg.FolderName;
                if (path == "")
                {
                    // user didnt set any path no error message needed
                    return;
                }

                if (Path.Exists($"{path}\\exports") == false
                ||  Path.Exists($"{path}\\files") == false
                ||  Path.Exists($"{path}\\cielnt.realm") == false)
                {
                    // ok this is scary to test since i only play on osu lazer...
                    // ok i changed osu lazer folder location and it just created new one in appdata/roaming... i hate it here also it reset all my configs...
                    // also osu!lazer states in IMPORTANT.txt to NOT MAKE MANUAL CHANGES TO THIS FOLDER...
                    // safe to assume if one thing is missing then its incorrect folder
                    MessageBox.Show("Wrong osu!lazer folder location. Folder should contain \"exports\" folder, \"files\" folder and \"client.realm\" file.", "Wrong folder path set");
                    return;
                }

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
   
            // default path if nothing is set
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

                string path = dlg.FolderName == "" ? "" : dlg.FolderName;
                if (path == "")
                {
                    // user closed dialog and didnt chose a path so do nothing and return.
                    return;
                }

                if (Path.Exists($"{path}\\Replays") == false
                ||  Path.Exists($"{path}\\Songs") == false
                ||  Path.Exists($"{path}\\osu!.db") == false)
                {
                    // ok ban peppy
                    // special case if only Songs folder is missing but the rest is not
                    // also look if there is config file, if it is there with Replays and osu!.db then look for
                    // BeatmapDirectory = path and copy it into OsuStableSongsFolderPath
                    if (Path.Exists($"{path}\\Replays") == true
                    &&  Path.Exists($"{path}\\osu!.db") == true
                    &&  Path.Exists($"{path}\\osu!.{Environment.UserName}.cfg") == true
                    &&  Path.Exists($"{path}\\Songs") == false)
                    {
                        // if everything but Songs folder exists that means the path is set up in config file so yoink it and done
                        string[] configLines = File.ReadAllLines($"{path}\\osu!.{Environment.UserName}.cfg");
                        string[] split = new string[2];
                        for (int i = 0; i < configLines.Length; i++)
                        {
                            if (configLines[i].Contains("BeatmapDirectory"))
                            {
                                split = configLines[i].Split(" = ");
                                break;
                            }
                        }

                        SaveConfigOption("OsuStableSongsFolderPath", split[1]);

                        // update this here coz of return statement
                        SaveConfigOption("OsuStableFolderPath", dlg.FolderName == "" ? "Select Folder" : dlg.FolderName);
                        button.Content = SelectedPath("OsuStableFolderPath");
                        BeatmapFile.Load();

                        // return so the error message doesnt pop up
                        return;
                    }

                    // osu!stable automatically creates these folders/files when they are missing/deleted (tested all of them)
                    // since game automatically creates back missing files (might need to restart game if user deletes file when game is open OR file is created when game is closed... anyhow its created back when missing),
                    // its safe to assume they all should exist at once in same folder, and if not then error and return to not save this path
                    // osu!.db when missing is created when game is closed (in my scenario at least, or my folder didnt refresh instantly and it got created when game opened)
                    // Songs folder when missing is created when opening the game (once it was empty, second time it downloaded pack of songs automatically lol)
                    // Replays folder when missing is created when user saves replay in game (and exists by default when game is downloaded)
                    MessageBox.Show("Wrong osu! folder location. Folder should contain \"Replays\" folder, \"Songs\" folder and \"osu!.db\" file.", "Wrong folder path set");
                    return;
                }

                // if its set up but code goes here coz maybe user changed Songs folder path back to osu folder
                // then make it "" so it wont be used since all files are inside osu folder now
                if (GetConfigValue("OsuStableSongsFolderPath") != "")
                {
                    SaveConfigOption("OsuStableSongsFolderPath", "");
                }

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

            int backgroundOpacityValue = int.Parse(config.AppSettings.Settings["BackgroundOpacity"].Value);
            Window.playfieldBackground.Opacity = backgroundOpacityValue / 100.0;
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

                SaveConfigOption("BackgroundOpacity", $"{(int)slider.Value}");
            };

            slider.MouseEnter += delegate (object sender, MouseEventArgs e)
            {
                slider.Focusable = true;
                slider.Focus();
            };

            slider.MouseLeave += delegate (object sender, MouseEventArgs e)
            {
                slider.Focusable = false;
            };

            slider.KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Left)
                {
                    slider.Value--;
                }
                else if (e.Key == Key.Right)
                {
                    slider.Value++;
                }
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
            if (int.Parse(res[1]) == maxScreenHeight)                                                                                                //   ^ wait... ... ... it works im not touching this
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

            // what in nigthmare is this math i dont know but... it works?
            Window.Top = SystemParameters.PrimaryScreenHeight / 2 - Window.Height / 2 - ((borderWidth + topWindowHeight - 11) / 2);
            Window.Left = SystemParameters.PrimaryScreenWidth / 2 - Window.Width / 2;

            if (MainWindow.map != null)
            {
                ResizePlayfield.ResizePlayfieldCanva();
            }

            SettingsPanel.UpdatePosition();
            JudgementTimeline.ChangeTimelineSizeOnResize();
            KeyOverlay.Resize();
        }

        public static StackPanel HitmarkersVisibility()
        {
            StackPanel panel = CreateOptionPanel();
            
            TextBlock name = CreateTextBoxForPanel("Show Hit Markers: ");

            CheckBox checkbox = CreateCheckBoxForPanel();

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

            CheckBox checkbox = CreateCheckBoxForPanel();

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

            CheckBox checkbox = CreateCheckBoxForPanel();

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

            CheckBox checkbox = CreateCheckBoxForPanel();

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
                foreach (var line in JudgementTimeline.TimelineJudgements100)
                {
                    line.Visibility = Visibility.Visible;
                }

                SaveConfigOption("Show100OnTimeline", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (var line in JudgementTimeline.TimelineJudgements100)
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

            CheckBox checkbox = CreateCheckBoxForPanel();

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
                foreach (var line in JudgementTimeline.TimelineJudgements50)
                {
                    line.Visibility = Visibility.Visible;
                }

                SaveConfigOption("Show50OnTimeline", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (var line in JudgementTimeline.TimelineJudgements50)
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

            CheckBox checkbox = CreateCheckBoxForPanel();

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
                foreach (var line in JudgementTimeline.TimelineJudgementsMiss)
                {
                    line.Visibility = Visibility.Visible;
                }

                SaveConfigOption("ShowMissOnTimeline", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (var line in JudgementTimeline.TimelineJudgementsMiss)
                {
                    line.Visibility = Visibility.Collapsed;
                }

                SaveConfigOption("ShowMissOnTimeline", "false");
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel KeyOverlayVisibility()
        {
            StackPanel panel = CreateOptionPanel();

            TextBlock name = CreateTextBoxForPanel("Hit Overlay Visibility");

            CheckBox checkbox = CreateCheckBoxForPanel();

            string showKeyOverlay = config.AppSettings.Settings["ShowKeyOverlay"].Value;
            if (showKeyOverlay == "true")
            {
                checkbox.IsChecked = true;
            }
            else
            {
                checkbox.IsChecked = false;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                KeyOverlay.KeyOverlayWindow.Visibility = Visibility.Visible;
                SaveConfigOption("ShowKeyOverlay", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                KeyOverlay.KeyOverlayWindow.Visibility = Visibility.Collapsed;
                SaveConfigOption("ShowKeyOverlay", "false");
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel AudioOffset()
        {
            StackPanel panel = CreateOptionPanel();

            int audioOffsetMs = int.Parse(config.AppSettings.Settings["AudioOffset"].Value);
            TextBlock name = CreateTextBoxForPanel($"Audio Offset: {audioOffsetMs}ms");
            MusicPlayer.MusicPlayer.AudioOffset = audioOffsetMs;

            Slider slider = new Slider();
            slider.Value = 0;
            slider.Maximum = 500;
            slider.Minimum = -500;
            slider.TickFrequency = 1;
            slider.SmallChange = 1;
            slider.Width = 100;
            slider.VerticalAlignment = VerticalAlignment.Center;
            slider.HorizontalAlignment = HorizontalAlignment.Center;
            slider.Orientation = Orientation.Horizontal;
            slider.Style = Window.Resources["OptionsSliderStyle"] as Style;

            slider.ValueChanged += delegate (object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                name.Text = $"Audio Offset: {(int)slider.Value}ms";

                MusicPlayer.MusicPlayer.AudioOffset = (int)slider.Value;

                if (MusicPlayer.MusicPlayer.AudioFile != null)
                {
                    MusicPlayer.MusicPlayer.Seek(GamePlayClock.TimeElapsed);
                }
                
                SaveConfigOption("AudioOffset", $"{(int)slider.Value}");
            };

            slider.MouseEnter += delegate (object sender, MouseEventArgs e)
            {
                slider.Focusable = true;
                slider.Focus();
            };

            slider.MouseLeave += delegate (object sender, MouseEventArgs e)
            {
                slider.Focusable = false;
            };

            slider.KeyDown += delegate (object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Left)
                {
                    slider.Value--;
                }
                else if (e.Key == Key.Right)
                {
                    slider.Value++;
                }
            };

            panel.Children.Add(name);
            panel.Children.Add(slider);

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

        private static CheckBox CreateCheckBoxForPanel()
        {
            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;
            checkbox.Focusable = false;

            return checkbox;
        }
    }
}
