using ReplayAnalyzer.SettingsMenu;
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

            UpdateVolumeIcon();
        }

        private static void CreateVolumeSliderWindow()
        {
            VolumeWindow.Height = 100;
            VolumeWindow.Width = 40;
            VolumeWindow.Visibility = Visibility.Collapsed;
            VolumeWindow.Background = new SolidColorBrush(Color.FromRgb(57, 42, 54));

            Canvas.SetZIndex(VolumeWindow, 10000);

            ApplyPropertiesToVolumeValue();
            ApplyPropertiesToSlider();

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
            if (MusicPlayer.AudioFile != null)
            {
                VolumeValue.Text = $"{VolumeSlider.Value}%";
                MusicPlayer.ChangeVolume((float)(VolumeSlider.Value / 100));

                SettingsOptions.SaveConfigOption("MusicVolume", $"{(int)VolumeSlider.Value}");

                UpdateVolumeIcon();
            }
        }

        private static void UpdateVolumeIcon()
        {
            if (MusicPlayer.AudioFileVolume.Volume == 0)
            {
                Window.volumeIcon.Data = Geometry.Parse("m5 7 4.146-4.146a.5.5 0 0 1 .854.353v13.586a.5.5 0 0 1-.854.353L5 13H4a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h1zm7 1.414L13.414 7l1.623 1.623L16.66 7l1.414 1.414-1.623 1.623 1.623 1.623-1.414 1.414-1.623-1.623-1.623 1.623L12 11.66l1.623-1.623L12 8.414z");
            }
            else if (MusicPlayer.AudioFileVolume.Volume > 0 && MusicPlayer.AudioFileVolume.Volume < 0.50)
            {
                Window.volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z");
            }
            else if (MusicPlayer.AudioFileVolume.Volume >= 0.50)
            {
                Window.volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z M12 6a4 4 0 0 1 0 8v2a6 6 0 0 0 0-12v2z");
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

        private static void ApplyPropertiesToVolumeValue()
        {
            RowDefinition vol = new RowDefinition();
            vol.MaxHeight = 15;

            VolumeValue.Foreground = new SolidColorBrush(Colors.White);
            VolumeValue.TextAlignment = TextAlignment.Center;

            VolumeWindow.RowDefinitions.Add(vol);
            VolumeWindow.Children.Add(VolumeValue);
            Grid.SetRow(VolumeValue, 0);
        }

        private static void ApplyPropertiesToSlider()
        {
            RowDefinition slider = new RowDefinition();
            slider.MaxHeight = VolumeWindow.Height - 15;

            VolumeSlider.Orientation = Orientation.Vertical;
            VolumeSlider.Height = 70;
            VolumeSlider.Width = 20;
            VolumeSlider.Margin = new Thickness((VolumeWindow.Width / 4) - 1, 0, 0, 0);
            VolumeSlider.Minimum = 0;
            VolumeSlider.Maximum = 100;
            VolumeSlider.TickFrequency = 1;
            VolumeSlider.IsSnapToTickEnabled = true;
            VolumeSlider.HorizontalAlignment = HorizontalAlignment.Center;
            VolumeSlider.VerticalAlignment = VerticalAlignment.Center;
            VolumeSlider.Style = Window.Resources["OptionsSliderStyle"] as Style;

            VolumeWindow.RowDefinitions.Add(slider);
            VolumeWindow.Children.Add(VolumeSlider);
            Grid.SetRow(VolumeSlider, 1);
        }
    }
}
