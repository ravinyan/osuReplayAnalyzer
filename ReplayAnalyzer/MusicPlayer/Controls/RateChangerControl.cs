using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.MusicPlayer.Controls
{
    public class RateChangerControl
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static Grid RateChangeWindow = new Grid();

        // unknown if wanted slider or just increments of 0.25x... depends on bugs i guess lol
        // but anyway min value will be 0.25x and max will be 2x
        public static Slider RateChangeSlider = new Slider();

        public static void InitializeEvents()
        {
            CreateRateChangeWindow();

            Window.rateChangeButton.Click += RateChangeButtonClick;

            Window.rateChangeButton.MouseEnter += VolumeButtonMouseEnter;
            Window.rateChangeButton.MouseLeave += VolumeButtonMouseLeave;
        }

        private static void CreateRateChangeWindow()
        {
            RateChangeWindow.Visibility = Visibility.Collapsed;
            RateChangeWindow.Width = 200;
            RateChangeWindow.Height = 50;
            RateChangeWindow.Background = new SolidColorBrush(Color.FromRgb(57, 42, 54));

            Window.ApplicationWindowUI.Children.Add(RateChangeWindow);
        }

        private static void RateChangeButtonClick(object sender, RoutedEventArgs e)
        {
            if (RateChangeWindow.Visibility == Visibility.Visible)
            {
                RateChangeWindow.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (VolumeControls.VolumeWindow.Visibility == Visibility.Visible)
                {
                    VolumeControls.VolumeWindow.Visibility = Visibility.Collapsed;
                }

                Canvas.SetTop(RateChangeWindow, Window.Height - 140);
                Canvas.SetLeft(RateChangeWindow, Window.Width - 268);

                RateChangeWindow.Visibility = Visibility.Visible;
            }
        }

        // hover effect for feedback
        private static void VolumeButtonMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Window.rateChangeText.Foreground = new SolidColorBrush(Color.FromRgb(57, 42, 54));
        }

        // hover effect for feedback
        private static void VolumeButtonMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Window.rateChangeText.Foreground = new SolidColorBrush(Colors.White);
        }
    }
}
