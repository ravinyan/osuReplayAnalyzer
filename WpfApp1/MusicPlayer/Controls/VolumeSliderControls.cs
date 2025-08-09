using System.Windows;
using System.Windows.Media;

namespace WpfApp1.MusicPlayer.Controls
{
    public static class VolumeSliderControls
    {
        private static MainWindow Window = (MainWindow)Application.Current.MainWindow;
        
        public static void InitializeEvents()
        {
            Window.volumeSlider.ValueChanged += VolumeSliderValueChanged;
        }

        private static void VolumeSliderValueChanged(object sender, RoutedEventArgs e)
        {
            if (Window.musicPlayerVolume != null && Window.musicPlayer.MediaPlayer != null)
            {
                Window.musicPlayerVolume.Text = $"{Window.volumeSlider.Value}%";
                Window.musicPlayer.MediaPlayer.Volume = (int)Window.volumeSlider.Value;

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
