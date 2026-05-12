using Microsoft.Win32;
using ReplayAnalyzer.AnalyzerTools;
using ReplayAnalyzer.AnalyzerTools.Cursor;
using ReplayAnalyzer.FileWatcher;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.MusicPlayer;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
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
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("osu!lazer folder path: ");

            Button button = CreateButton();

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

                if (dlg.FolderName == "")
                {
                    // user didnt set any path no error message needed
                    return;
                }

                if (Path.Exists($"{dlg.FolderName}\\exports") == false
                ||  Path.Exists($"{dlg.FolderName}\\files") == false
                ||  Path.Exists($"{dlg.FolderName}\\client.realm") == false)
                {
                    // ok this is scary to test since i only play on osu lazer...
                    // ok i changed osu lazer folder location and it just created new one in appdata/roaming... i hate it here also it reset all my configs...
                    // also osu!lazer states in IMPORTANT.txt to NOT MAKE MANUAL CHANGES TO THIS FOLDER...
                    // safe to assume if one thing is missing then its incorrect folder
                    MessageBox.Show("Wrong osu!lazer folder location. Folder should contain \"exports\" folder, \"files\" folder and \"client.realm\" file.", "Wrong folder path set");
                    return;
                }

                SaveConfigOption("OsuLazerFolderPath", dlg.FolderName);

                button.Content = SelectedPath("OsuLazerFolderPath");
                BeatmapFile.Load();
            };

            panel.Children.Add(name);
            panel.Children.Add(button);

            return panel;
        }

        public static StackPanel OsuStableSourceFolderLocation()
        {
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("osu!stable folder path: ");

            Button button = CreateButton();

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

                if (dlg.FolderName == "")
                {
                    // user closed dialog and didnt chose a path so do nothing and return.
                    return;
                }

                if (Path.Exists($"{dlg.FolderName}\\Replays") == false
                ||  Path.Exists($"{dlg.FolderName}\\Songs") == false
                ||  Path.Exists($"{dlg.FolderName}\\osu!.db") == false)
                {
                    // ok ban peppy
                    // special case if only Songs folder is missing but the rest is not
                    // also look if there is config file, if it is there with Replays and osu!.db then look for
                    // BeatmapDirectory = path and copy it into OsuStableSongsFolderPath
                    if (Path.Exists($"{dlg.FolderName}\\Replays") == true
                    &&  Path.Exists($"{dlg.FolderName}\\osu!.db") == true
                    &&  Path.Exists($"{dlg.FolderName}\\osu!.{Environment.UserName}.cfg") == true
                    &&  Path.Exists($"{dlg.FolderName}\\Songs") == false)
                    {
                        // if everything but Songs folder exists that means the path is set up in config file so yoink it and done
                        string[] configLines = File.ReadAllLines($"{dlg.FolderName}\\osu!.{Environment.UserName}.cfg");
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
                        SaveConfigOption("OsuStableFolderPath", dlg.FolderName);
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

                SaveConfigOption("OsuStableFolderPath", dlg.FolderName);
 
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
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("osu! client Replay is from: ");

            string[] clientOptions = new string[]
            {
                "osu!", "osu!lazer"
            };

            ComboBox comboBox = CreateComboBox(clientOptions);

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
            StackPanel panel = CreatePanel();

            int backgroundOpacityValue = int.Parse(config.AppSettings.Settings["BackgroundOpacity"].Value);
            Window.playfieldBackground.Opacity = backgroundOpacityValue / 100.0;
            TextBlock name = CreateTextBlock($"Background Opacity: {backgroundOpacityValue}%");

            Slider slider = CreateSlider(0, 100, backgroundOpacityValue);

            slider.ValueChanged += delegate (object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                Window.playfieldBackground.Opacity = slider.Value / 100;
                name.Text = $"Background Opacity: {(int)slider.Value}%";

                SaveConfigOption("BackgroundOpacity", $"{(int)slider.Value}");
            };

            panel.Children.Add(name);
            panel.Children.Add(slider);

            return panel;
        }

        public static StackPanel ScreenResolution()
        {
            StackPanel panel = CreatePanel();
            
            TextBlock name = CreateTextBlock("Resolution: ");

            string[] resolutionOptions = new string[] 
            { 
               "800x600", "1280x800", "1360x786", "1440x1080", "1600x1050", "1980x1080", "2560x1440", "2560x1600" 
            };

            ComboBox comboBox = CreateComboBox(resolutionOptions);

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

            if (MainWindow.map.FileVersion != -1)
            {
                ResizePlayfield.ResizePlayfieldCanva();
            }

            SettingsPanel.UpdatePosition();
            JudgementTimeline.ChangeTimelineSizeOnResize();
            KeyOverlay.Resize();
            HitMap.Resize();
        }

        public static StackPanel HitmarkersVisibility()
        {
            StackPanel panel = CreatePanel();
            
            TextBlock name = CreateTextBlock("Show Hit Markers: ");

            CheckBox checkbox = CreateCheckBox();

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
            StackPanel panel = CreatePanel();
            
            TextBlock name = CreateTextBlock("Show Frame Markers:");

            CheckBox checkbox = CreateCheckBox();

            string showMarkers = config.AppSettings.Settings["ShowFrameMarkers"].Value;
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
                foreach (FrameMarker marker in FrameMarkerManager.GetAliveFrameMarkers())
                {
                    marker.Visibility = Visibility.Visible;
                }

                SaveConfigOption("ShowFrameMarkers", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (FrameMarker marker in FrameMarkerManager.GetAliveFrameMarkers())
                {
                    marker.Visibility = Visibility.Collapsed;
                }

                SaveConfigOption("ShowFrameMarkers", "false");
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel CursorPathVisibility()
        {
            StackPanel panel = CreatePanel();
            
            TextBlock name = CreateTextBlock("Show Cursor Path:");

            CheckBox checkbox = CreateCheckBox();

            string showPaths = config.AppSettings.Settings["ShowCursorPath"].Value;
            if (showPaths == "true")
            {
                checkbox.IsChecked = true;
            }
            else
            {
                checkbox.IsChecked = false;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (CursorPath path in CursorPathManager.GetAliveCursorPaths())
                {
                    path.Visibility = Visibility.Visible;
                }

                SaveConfigOption("ShowCursorPath", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (CursorPath path in CursorPathManager.GetAliveCursorPaths())
                {
                    path.Visibility = Visibility.Collapsed;
                }

                SaveConfigOption("ShowCursorPath", "false");
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel JudgementTimelineVisible100()
        {
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("Timeline x100 Visibility:");

            CheckBox checkbox = CreateCheckBox();

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
                SaveConfigOption("Show100OnTimeline", "true");
                JudgementTimeline.ShowJudgements(JudgementTimeline.TimelineJudgements100);
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {

                SaveConfigOption("Show100OnTimeline", "false");
                JudgementTimeline.HideJudgements(JudgementTimeline.TimelineJudgements100);
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel JudgementTimelineVisible50()
        {
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("Timeline x50 Visibility:");

            CheckBox checkbox = CreateCheckBox();

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
                SaveConfigOption("Show50OnTimeline", "true");
                JudgementTimeline.ShowJudgements(JudgementTimeline.TimelineJudgements50);
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {

                SaveConfigOption("Show50OnTimeline", "false");
                JudgementTimeline.HideJudgements(JudgementTimeline.TimelineJudgements50);
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel JudgementTimelineVisibleMiss()
        {
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("Timeline Miss Visibility:");

            CheckBox checkbox = CreateCheckBox();

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
                SaveConfigOption("ShowMissOnTimeline", "true");
                JudgementTimeline.ShowJudgements(JudgementTimeline.TimelineJudgementsMiss);
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                SaveConfigOption("ShowMissOnTimeline", "false");
                JudgementTimeline.HideJudgements(JudgementTimeline.TimelineJudgementsMiss);
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel KeyOverlayVisibility()
        {
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("Hit Overlay Visibility");

            CheckBox checkbox = CreateCheckBox();

            string showKeyOverlay = config.AppSettings.Settings["ShowKeyOverlay"].Value;
            if (showKeyOverlay == "true")
            {
                checkbox.IsChecked = true;
                KeyOverlay.KeyOverlayWindow.Visibility = Visibility.Visible;
            }
            else
            {
                checkbox.IsChecked = false;
                KeyOverlay.KeyOverlayWindow.Visibility = Visibility.Collapsed;
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
            StackPanel panel = CreatePanel();

            int audioOffsetMs = int.Parse(config.AppSettings.Settings["AudioOffset"].Value);
            TextBlock name = CreateTextBlock($"Audio Offset: {audioOffsetMs}ms");
            MusicPlayer.MusicPlayer.AudioOffset = audioOffsetMs;

            Slider slider = CreateSlider(-500, 500, audioOffsetMs);

            // audio offset my ass its whole gameplay offset lol
            slider.ValueChanged += delegate (object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                name.Text = $"Audio Offset: {(int)slider.Value}ms";

                MusicPlayer.MusicPlayer.AudioOffset = (int)slider.Value;

                int currentOffset = int.Parse(GetConfigValue("AudioOffset"));
                int newOffset = (int)slider.Value;

                long offsetTimeElapsed = (long)GamePlayClock.TimeElapsed + (newOffset - currentOffset);
                GamePlayClock.Seek(offsetTimeElapsed);
                Window.songSlider.Value = offsetTimeElapsed;

                int direction = newOffset - currentOffset < 0 ? -1 : 1;
                SongSliderControls.SeekGameplayToCurrentFrame(direction);
                HitObjects.Slider.UpdateAliveSliderEvents();

                SaveConfigOption("AudioOffset", $"{(int)slider.Value}");
            };

            panel.Children.Add(name);
            panel.Children.Add(slider);

            return panel;
        }

        public static StackPanel FpsLimiter()
        {
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("FPS Limit");

            // my "counter" never shows wrong numbers (<200 in 240) but we dont talk about it my counter is scuffed and wrong yes
            string[] fpsOptions = new string[]
            {
                "60", "144", "240", "Unlimited"
            };

            ComboBox comboBox = CreateComboBox(fpsOptions);

            comboBox.SelectedItem = config.AppSettings.Settings["FPSLimit"].Value;

            // i hate math
            comboBox.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                SaveConfigOption("FPSLimit", comboBox.SelectedItem.ToString()!);
                ChangeFps();
            };

            panel.Children.Add(name);
            panel.Children.Add(comboBox);

            return panel;

            void ChangeFps()
            {
                if ((string)comboBox.SelectedItem == "Unlimited")
                {
                    Window.ChangeGameplayLoopFrameRate(1);
                }
                else
                {
                    Window.ChangeGameplayLoopFrameRate(1000 / double.Parse(comboBox.SelectedItem.ToString()!));
                }
            }
        }

        public static StackPanel PlayfieldBorder()
        {
            StackPanel panel = CreatePanel();

            TextBlock text = CreateTextBlock("Disable playfield border: ");

            CheckBox checkbox = CreateCheckBox();

            Thickness defaultThickness = new Thickness(4);
            Thickness disabledThickness = new Thickness(0);
            string showBorder = config.AppSettings.Settings["ShowPlayfieldBorder"].Value;
            if (showBorder == "true")
            {
                checkbox.IsChecked = true;
                Window.playfieldBorder.BorderThickness = defaultThickness;
            }
            else
            {
                checkbox.IsChecked = false;
                Window.playfieldBorder.BorderThickness = disabledThickness;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                Window.playfieldBorder.BorderThickness = defaultThickness;
                SaveConfigOption("ShowPlayfieldBorder", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                Window.playfieldBorder.BorderThickness = disabledThickness;
                SaveConfigOption("ShowPlayfieldBorder", "false");
            };

            panel.Children.Add(text);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel HiddenModVisibility()
        {
            StackPanel panel = CreatePanel();

            TextBlock text = CreateTextBlock("Enable Hidden Mod: ");

            CheckBox checkbox = CreateCheckBox();

            string enableHidden = config.AppSettings.Settings["IsHiddenModEnabled"].Value;
            if (enableHidden == "true")
            {
                checkbox.IsChecked = true;
            }
            else
            {
                checkbox.IsChecked = false;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                HiddenMod.ChangeHiddenModVisibility();
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                HiddenMod.ChangeHiddenModVisibility();
            };

            panel.Children.Add(text);
            panel.Children.Add(checkbox);

            return panel;
        }

        public static StackPanel HitMapVisibility()
        {
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("Hit Map Visibility");

            CheckBox checkbox = CreateCheckBox();

            string visibility = config.AppSettings.Settings["ShowHitMap"].Value;
            if (visibility == "true")
            {
                checkbox.IsChecked = true;
                HitMap.HitMapUI.Visibility = Visibility.Visible;
            }
            else
            {
                checkbox.IsChecked = false;
                HitMap.HitMapUI.Visibility = Visibility.Collapsed;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                HitMap.HitMapUI.Visibility = Visibility.Visible;
                SaveConfigOption("ShowHitMap", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                HitMap.HitMapUI.Visibility = Visibility.Collapsed;
                SaveConfigOption("ShowHitMap", "false");
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            return panel;
        }

        // dropdown with skins inside replay analyzer Skins folder
        // uh i have no better idea how to do that i spent 1.5h of thinking/trying to find better way and i dont want to think anymore
        private static string[] FullStringPaths;
        public static StackPanel ChangeSkin()
        {
            StackPanel panel = CreatePanel();

            TextBlock text = CreateTextBlock("Current Skin: ");

            string[] options = GetAnalyzerSkins();
            ComboBox comboBox = CreateComboBox(options);

            comboBox.SelectedItem = config.AppSettings.Settings["CurrentSkin"].Value;

            comboBox.DropDownOpened += delegate (object? sender, EventArgs e)
            {
                // temporary for refreshing list
                comboBox.ItemsSource = GetAnalyzerSkins();
            };

            comboBox.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {

                SkinElement.UpdateSkinPath(FullStringPaths[comboBox.SelectedIndex]);
                SaveConfigOption("CurrentSkin", comboBox.SelectedItem.ToString()!);
            };

            panel.Children.Add(text);
            panel.Children.Add(comboBox);

            return panel;

            string[] GetAnalyzerSkins()
            {
                DirectoryInfo directoryInfo = new DirectoryInfo($"{AppContext.BaseDirectory}\\Skins");
                DirectoryInfo[] directories = directoryInfo.GetDirectories();

                string[] skinsInternal = new string[directories.Length];
                for (int i = 0; i < directories.Length; i++)
                {
                    skinsInternal[i] = directories[i].FullName;
                }

                string[] skinsExternal = new string[0];
                if (Directory.Exists(GetConfigValue("ExternalSkinFolderPath")))
                {
                    DirectoryInfo directoryInfoExternal = new DirectoryInfo(GetConfigValue("ExternalSkinFolderPath"));
                    DirectoryInfo[] directoriesExternal = directoryInfoExternal.GetDirectories();

                    skinsExternal = new string[directoriesExternal.Length];
                    for (int i = 0; i < directoriesExternal.Length; i++)
                    {
                        skinsExternal[i] = directoriesExternal[i].FullName;
                    }
                }

                string[] skins = new string[skinsInternal.Length + skinsExternal.Length];
                FullStringPaths = new string[skinsInternal.Length + skinsExternal.Length];
                for (int i = 0; i < skins.Length; i++)
                {
                    if (i < skinsInternal.Length)
                    {
                        FullStringPaths[i] = skinsInternal[i]; 
                        skins[i] = CutUserNameFromPath(skinsInternal[i]);
                    }
                    else
                    {
                        FullStringPaths[i] = skinsExternal[i - skinsInternal.Length];
                        skins[i] = CutUserNameFromPath(skinsExternal[i - skinsInternal.Length]);
                    }
                }

                return skins;
            }
        }

        // button that will open file explorer where user can pick path to any skin folder from anywhere
        public static StackPanel ExternalSkinFolderPath()
        {
            StackPanel panel = CreatePanel();

            TextBlock text = CreateTextBlock("External Skin Folder Path: ");

            Button button = CreateButton();

            button.Content = "Select Folder";

            button.Click += delegate (object sender, RoutedEventArgs e)
            {
                OpenFolderDialog dlg = new OpenFolderDialog();
                dlg.Title = "Hello... please select Skin folder";
                dlg.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dlg.ShowDialog();

                if (dlg.FolderName == "")
                {
                    return;
                }

                SaveConfigOption("ExternalSkinFolderPath", dlg.FolderName);
                
                //button.Content = SelectedPath("");
            };

            panel.Children.Add(text);
            panel.Children.Add(button);

            return panel;
        }

        // i dont want pc user name to show but at the same time this is making things annoying... sigh
        // ok this is the only thing that makes me not being able to finish this and i cant find any solution to this... AAAAA
        private static string CutUserNameFromPath(string path)
        {
            string[] splitPath = path.Split("\\");
            string newPath = "";
            for (int i = 0; i < splitPath.Length; i++)
            {
                if (splitPath[i] == Environment.UserDomainName || splitPath[i] == Environment.UserName)
                {// 
                    splitPath[i] = "User";
                }

                string temp = splitPath[i];
                newPath = newPath + temp + "\\";
            }

            return newPath;
        }

        // tested on circle only map with all elements on SD and difference was whatever so no point...
        // also user can delete all HD skin elements and it will use SD elements anyway so meh
        public static StackPanel SkinTextureFilePriority()
        {
            StackPanel panel = CreatePanel();

            TextBlock name = CreateTextBlock("Prioritize HD skin elements");

            CheckBox checkbox = CreateCheckBox();

            string prioritizeHDSkin = config.AppSettings.Settings["PrioritizeHDSkinElements"].Value;
            if (prioritizeHDSkin == "true")
            {
                checkbox.IsChecked = true;
            }
            else
            {
                checkbox.IsChecked = false;
            }

            checkbox.Checked += delegate (object sender, RoutedEventArgs e)
            {
                SaveConfigOption("PrioritizeHDSkinElements", "true");
            };

            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                SaveConfigOption("PrioritizeHDSkinElements", "false");
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

        private static StackPanel CreatePanel()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;
            panel.Margin = new Thickness(5);

            return panel;
        }

        private static TextBlock CreateTextBlock(string name)
        {
            TextBlock textBox = new TextBlock();
            textBox.Text = name;
            textBox.Foreground = new SolidColorBrush(Colors.White);
            textBox.Width = 170;
            textBox.VerticalAlignment = VerticalAlignment.Center;

            return textBox;
        }

        private static CheckBox CreateCheckBox()
        {
            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;
            checkbox.Focusable = false;

            return checkbox;
        }

        private static Slider CreateSlider(int minValue, int maxValue, double value)
        {
            Slider slider = new Slider();
            slider.Value = value;
            slider.Maximum = maxValue;
            slider.Minimum = minValue;
            slider.TickFrequency = 1;
            slider.SmallChange = 1;
            slider.Width = 100;
            slider.VerticalAlignment = VerticalAlignment.Center;
            slider.HorizontalAlignment = HorizontalAlignment.Center;
            slider.Orientation = Orientation.Horizontal;
            slider.Style = Window.Resources["OptionsSliderStyle"] as Style;

            slider.MouseEnter += delegate (object sender, MouseEventArgs e)
            {
                slider.Focusable = true;
                slider.Focus();
            };

            slider.MouseLeave += delegate (object sender, MouseEventArgs e)
            {
                slider.Focusable = false;
            };

            return slider;
        }

        // why not call this dropdown like normal person im gonna snap what the hell is combo box even
        private static ComboBox CreateComboBox(string[] options)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Width = 100;
            comboBox.Height = 25;
            comboBox.SelectedIndex = 0;
            comboBox.ItemsSource = options; 
            comboBox.Style = Window.Resources["ComboBoxSTYLE"] as Style;

            return comboBox;
        }

        private static Button CreateButton()
        {
            Button button = new Button();
            button.Width = 100;
            button.Height = 25;
            button.Style = Window.Resources["OptionsMenuButton"] as Style;

            return button;
        }
    }
}
