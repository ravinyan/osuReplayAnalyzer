using ReplayAnalyzer.SettingsMenu;
using System.Configuration;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace ReplayAnalyzer.MusicPlayer.Controls
{
    public static class VolumeSliderControls
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static Grid VolumeWindow = new Grid();

        public static void InitializeEvents()
        {
            CreateVolumeSliderWindow();
            Window.volumeSlider.ValueChanged += VolumeSliderValueChanged;
            Window.volumeButton.Click += VolumeButtonClick;
        }

        private static void CreateVolumeSliderWindow()
        {
            VolumeWindow.Height = 100;
            VolumeWindow.Width = 40;
            VolumeWindow.Visibility = Visibility.Collapsed;
            VolumeWindow.Background = new SolidColorBrush(Color.FromRgb(57, 42, 54));

            // add volume slider and at the top volume percentage
            RowDefinition vol = new RowDefinition();
            vol.MaxHeight = 30;
            VolumeWindow.RowDefinitions.Add(vol);

            TextBlock volumePercentage = new TextBlock();
            volumePercentage.Width = 30;
            volumePercentage.Height = 30;
            volumePercentage.Foreground = new SolidColorBrush(Colors.White);
            volumePercentage.Text = "100%";

            RowDefinition sl = new RowDefinition();
            sl.MaxHeight = VolumeWindow.Height - vol.MaxHeight;

            Slider slider = new Slider();
            slider.Orientation = Orientation.Vertical;
            slider.Height = VolumeWindow.Height - vol.MaxHeight;
            slider.VerticalAlignment = VerticalAlignment.Center;
            slider.Width = 30;
            slider.Margin = new Thickness(10);

            VolumeWindow.ColumnDefinitions.Add(new ColumnDefinition());

            VolumeWindow.Children.Add(volumePercentage);
            Grid.SetRow(volumePercentage, 0);

            VolumeWindow.RowDefinitions.Add(sl);
            VolumeWindow.Children.Add(slider);
            Grid.SetRow(slider, 1);


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
                Canvas.SetTop(VolumeWindow, Window.Height - 200);
                Canvas.SetLeft(VolumeWindow, Window.Width - 195);

                var a = Canvas.GetTop(Window.volumeButton);
                VolumeWindow.Visibility = Visibility.Visible;
            }
        }

        private static void VolumeSliderValueChanged(object sender, RoutedEventArgs e)
        {
            if (Window.musicPlayerVolume != null && Window.musicPlayer.MediaPlayer != null)
            {
                Window.musicPlayerVolume.Text = $"{Window.volumeSlider.Value}%";
                Window.musicPlayer.MediaPlayer.Volume = (int)Window.volumeSlider.Value;

                SettingsOptions.config.AppSettings.Settings["MusicVolume"].Value = $"{(int)Window.volumeSlider.Value}";
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
    }
}
