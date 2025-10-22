using Microsoft.Win32;
using System.Configuration;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WpfApp1.FileWatcher;
using WpfApp1.PlayfieldUI;

namespace WpfApp1.SettingsMenu
{
    public class SettingsOptions
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public List<StackPanel> Options = new List<StackPanel>();
        public static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

        public void CreateOptions()
        {
            Grid panel = SettingsPanel.SettingPanelBox;

            OsuVersion();
            OsuStableSourceFolderLocation();
            OsuLazerSourceFolderLocation();
            BackgrounOpacity();
            ScreenResolution();
            HitmarkersVisibility();
            FrameMarkersVisibility();
            CursorPathVisibility();
        }

        private void OsuLazerSourceFolderLocation()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Text = "Osu lazer folder path: ";
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;

            Button button = new Button();
            button.Width = 100;
            button.Height = 25;

            if (config.AppSettings.Settings["OsuLazerFolderPath"].Value == "")
            {
                button.Content = "Select Folder";
            }
            else
            {
                button.Content = SelectedPath("OsuLazerFolderPath");
            } 

            button.Click += delegate (object sender, RoutedEventArgs e)
            {
                OpenFolderDialog dlg = new OpenFolderDialog();
                dlg.Title = "Hello... please select your osu lazer folder";
                dlg.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dlg.ShowDialog();

                config.AppSettings.Settings["OsuLazerFolderPath"].Value = dlg.FolderName;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);

                button.Content = SelectedPath("OsuLazerFolderPath");
            };

            panel.Children.Add(name);
            panel.Children.Add(button);

            Options.Add(panel);
        }

        private void OsuStableSourceFolderLocation()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Text = "Osu stable folder path: ";
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;

            Button button = new Button();
            button.Width = 100;
            button.Height = 25;

            if (config.AppSettings.Settings["OsuStableFolderPath"].Value == "")
            {
                button.Content = "Select Folder";
            }
            else
            {
                button.Content = SelectedPath("OsuStableFolderPath");
            }

            button.Click += delegate (object sender, RoutedEventArgs e)
            {
                OpenFolderDialog dlg = new OpenFolderDialog();
                dlg.Title = "Hello... please select your osu stable folder";
                dlg.DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                dlg.ShowDialog();

                config.AppSettings.Settings["OsuStableFolderPath"].Value = dlg.FolderName;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);

                button.Content = SelectedPath("OsuStableFolderPath");
            };

            panel.Children.Add(name);
            panel.Children.Add(button);

            Options.Add(panel);
        }

        private string SelectedPath(string folderPathSetting)
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

        private void OsuVersion()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Text = "Osu client Replay is from: ";
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;

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
                config.AppSettings.Settings["OsuClient"].Value = comboBox.SelectedItem.ToString();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);

                BeatmapFile.Load();
            };

            panel.Children.Add(name);
            panel.Children.Add(comboBox);

            Options.Add(panel);
        }

        private void BackgrounOpacity()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;

            Window.playfieldBackground.Opacity = Math.Floor(double.Parse(config.AppSettings.Settings["BackgroundOpacity"].Value)) / 100;
            name.Text = $"Background Opacity: {Math.Floor(double.Parse(config.AppSettings.Settings["BackgroundOpacity"].Value))}%";

            Slider slider = new Slider();
            slider.Value = Window.playfieldBackground.Opacity * 100;
            slider.Maximum = 100;
            slider.TickFrequency = 1;
            slider.SmallChange = 1;
            slider.Width = 100;

            slider.ValueChanged += delegate (object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                Window.playfieldBackground.Opacity = slider.Value / 100;
                name.Text = $"Background Opacity: {(int)slider.Value}%";

                config.AppSettings.Settings["BackgroundOpacity"].Value = slider.Value.ToString();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            };

            panel.Children.Add(name);
            panel.Children.Add(slider);

            Options.Add(panel);
        }

        private void ScreenResolution()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Text = "Resolution: ";
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;

            string[] resolutionOptions = new string[] 
            { 
               "800x600", "1280x800", "1360x786", "1440x1080", "1600x1050", "1980x1080", "2560x1440", "2560x1600" 
            };

            ComboBox comboBox = new ComboBox();
            comboBox.Width = 100;
            comboBox.Height = 25;
            comboBox.SelectedIndex = 0;
            comboBox.ItemsSource = resolutionOptions;

            comboBox.SelectedItem = config.AppSettings.Settings["ScreenResolution"].Value;
            ChangeResolution(comboBox);

            // i hate math
            comboBox.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                config.AppSettings.Settings["ScreenResolution"].Value = comboBox.SelectedItem.ToString();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);

                ChangeResolution(comboBox);
            };

            panel.Children.Add(name);
            panel.Children.Add(comboBox);

            Options.Add(panel);
        }

        private void ChangeResolution(ComboBox comboBox)
        {
            string[] res = comboBox.SelectedItem.ToString()!.Split('x');

            PropertyInfo? dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            PropertyInfo? dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);

            double dpiScaleWidht = (int)dpiXProperty!.GetValue(null, null)! / 96.0;
            double dpiScaleHeight = (int)dpiYProperty!.GetValue(null, null)! / 96.0;

            double borderWidth = (SystemParameters.BorderWidth * dpiScaleHeight) * 2;

            double width = (int.Parse(res[0]) + borderWidth) / dpiScaleWidht;
            double height = (int.Parse(res[1]) + borderWidth) / dpiScaleHeight;

            double maxScreenHeight = SystemParameters.PrimaryScreenHeight * dpiScaleHeight;

            double topWindowHeight = System.Windows.Forms.SystemInformation.ToolWindowCaptionHeight * dpiScaleHeight;
            double toolbarHeight = (SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight) + dpiScaleHeight;
            if (int.Parse(res[1]) == maxScreenHeight)
            {
                Window.Height = height + borderWidth - toolbarHeight;
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

        }

        private void HitmarkersVisibility()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;
            name.Text = $"Show Hitmarkers:";

            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;

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
                foreach (var marker in Analyser.Analyser.HitMarkers)
                {
                    marker.Value.Visibility = Visibility.Visible;
                }

                config.AppSettings.Settings["ShowHitMarkers"].Value = "true";
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            };
           
            checkbox.Unchecked += delegate (object sender, RoutedEventArgs e)
            {
                foreach (var marker in Analyser.Analyser.HitMarkers)
                {
                    marker.Value.Visibility = Visibility.Collapsed;
                }

                config.AppSettings.Settings["ShowHitMarkers"].Value = "false";
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            };

            panel.Children.Add(name);
            panel.Children.Add(checkbox);

            Options.Add(panel);
        }

        private void FrameMarkersVisibility()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;
            name.Text = $"Show Frame Markers:";

            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;

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

            Options.Add(panel);
        }

        private void CursorPathVisibility()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;
            name.Text = $"Show Cursor Path:";

            CheckBox checkbox = new CheckBox();
            checkbox.Style = Window.Resources["SwitchBox"] as Style;

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

            Options.Add(panel);
        }
    }
}
