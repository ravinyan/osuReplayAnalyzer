using LibVLCSharp.Shared;
using System.Printing;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp1.MusicPlayer.Controls;

namespace WpfApp1.MusicPlayer
{
    public static class MusicPlayer
    {
        private static MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static LibVLC LibVLC;

        public static void InitializeMusicPlayer323()
        {

        }

        public static void InitializeMusicPlayer()
        {
            LibVLC libVLC = new LibVLC();
            Window.musicPlayer.MediaPlayer = new MediaPlayer(libVLC);
            Window.musicPlayer.MediaPlayer.Media = new Media(libVLC, $@"{AppDomain.CurrentDomain.BaseDirectory}\osu\Audio\audio.mp3");

            Window.playfieldBackground.ImageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Background\\bg.jpg"));

            Window.musicPlayer.MediaPlayer.Volume = 0;
            Window.musicPlayerVolume.Text = $"{Window.musicPlayer.MediaPlayer.Volume}%";   

            Window.musicPlayer.MediaPlayer.Media.Parse();
            while (Window.musicPlayer.MediaPlayer.Media.ParsedStatus != MediaParsedStatus.Done)
            {
                Thread.Sleep(10);
            }

            Window.songMaxTimer.Text = TimeSpan.FromMilliseconds(Window.musicPlayer.MediaPlayer.Media.Duration).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            Window.songSlider.Maximum = Window.musicPlayer.MediaPlayer.Media.Duration;

            SongSliderControls.InitializeEvents();
            VolumeSliderControls.InitializeEvents();
            PlayPauseControls.InitializeEvents();
        }
    }
}
