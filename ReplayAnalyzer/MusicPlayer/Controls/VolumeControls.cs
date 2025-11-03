using ReplayAnalyzer.SettingsMenu;
using System.Configuration;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace ReplayAnalyzer.MusicPlayer.Controls
{
    public static class VolumeControls
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Grid VolumeWindow = new Grid();

        public static Slider VolumeSlider = new Slider();
        public static TextBlock VolumeValue = new TextBlock();

        public static void InitializeEvents()
        {
            CreateVolumeSliderWindow();

            Window.volumeButton.Click += VolumeButtonClick;
            VolumeSlider.ValueChanged += VolumeSliderValueChanged;

            Window.volumeButton.MouseEnter += VolumeButtonMouseEnter;
            Window.volumeButton.MouseLeave += VolumeButtonMouseLeave;
        }

        private static void CreateVolumeSliderWindow()
        {
            VolumeWindow.Height = 100;
            VolumeWindow.Width = 40;
            VolumeWindow.Visibility = Visibility.Collapsed;
            VolumeWindow.Background = new SolidColorBrush(Color.FromRgb(57, 42, 54));

            RowDefinition vol = new RowDefinition();
            vol.MaxHeight = 15;
            
            VolumeValue.Width = VolumeWindow.Width;
            VolumeValue.Height = vol.MaxHeight;
            VolumeValue.Foreground = new SolidColorBrush(Colors.White);
            VolumeValue.TextAlignment = TextAlignment.Center;

            VolumeWindow.RowDefinitions.Add(vol);
            VolumeWindow.Children.Add(VolumeValue);
            Grid.SetRow(VolumeValue, 0);

            RowDefinition sl = new RowDefinition();
            sl.MaxHeight = VolumeWindow.Height - vol.MaxHeight;

            VolumeSlider.Orientation = Orientation.Vertical;
            VolumeSlider.Height = sl.MaxHeight;
            VolumeSlider.VerticalAlignment = VerticalAlignment.Center;
            VolumeSlider.Width = VolumeWindow.Width;
            VolumeSlider.Margin = new Thickness((VolumeWindow.Width / 4) + 1, 0, 0, 2);
            VolumeSlider.Minimum = 0;
            VolumeSlider.Maximum = 100;
            VolumeSlider.TickFrequency = 1;
            VolumeSlider.IsSnapToTickEnabled = true;

            VolumeWindow.RowDefinitions.Add(sl);
            VolumeWindow.Children.Add(VolumeSlider);
            Grid.SetRow(VolumeSlider, 1);

            Window.ApplicationWindowUI.Children.Add(VolumeWindow);
        }

        private static void VolumeButtonClick(object sender, RoutedEventArgs e)
        {
            if (VolumeWindow.Visibility == Visibility.Visible)
            {
                VolumeWindow.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (RateChangerControls.RateChangeWindow.Visibility == Visibility.Visible)
                {
                    RateChangerControls.RateChangeWindow.Visibility = Visibility.Collapsed;
                }

                Canvas.SetTop(VolumeWindow, Window.Height - 190);
                Canvas.SetLeft(VolumeWindow, Window.Width - 230);

                VolumeWindow.Visibility = Visibility.Visible;
            }
        }

        private static void VolumeSliderValueChanged(object sender, RoutedEventArgs e)
        {
            if (Window.musicPlayer.MediaPlayer != null)
            {
                VolumeValue.Text = $"{VolumeSlider.Value}%";
                Window.musicPlayer.MediaPlayer.Volume = (int)VolumeSlider.Value;
            
                SettingsOptions.config.AppSettings.Settings["MusicVolume"].Value = $"{(int)VolumeSlider.Value}";
                SettingsOptions.config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(SettingsOptions.config.AppSettings.SectionInformation.Name);
            
                if (Window.musicPlayer.MediaPlayer.Volume == 0)
                {
                    Window.volumeIcon.Data = Geometry.Parse("m5 7 4.146-4.146a.5.5 0 0 1 .854.353v13.586a.5.5 0 0 1-.854.353L5 13H4a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h1zm7 1.414L13.414 7l1.623 1.623L16.66 7l1.414 1.414-1.623 1.623 1.623 1.623-1.414 1.414-1.623-1.623-1.623 1.623L12 11.66l1.623-1.623L12 8.414z");
                }
                else if (Window.musicPlayer.MediaPlayer.Volume > 0 && Window.musicPlayer.MediaPlayer.Volume < 50)
                {
                    Window.volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z");
                }
                else if (Window.musicPlayer.MediaPlayer.Volume >= 50)
                {
                    Window.volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z M12 6a4 4 0 0 1 0 8v2a6 6 0 0 0 0-12v2z");
                }
            }
        }

        // hover effect for feedback
        private static void VolumeButtonMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Window.volumeIcon.Fill = new SolidColorBrush(Color.FromRgb(57, 42, 54));
        }

        // hover effect for feedback
        private static void VolumeButtonMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Window.volumeIcon.Fill = new SolidColorBrush(Colors.White);
        }
    }
}
