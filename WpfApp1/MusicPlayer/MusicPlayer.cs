using LibVLCSharp.Shared;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp1.GameClock;
using WpfApp1.MusicPlayer.Controls;

namespace WpfApp1.MusicPlayer
{
    public static class MusicPlayer
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static readonly int FrameTime = 16;

        public static void Initialize()
        {
            LibVLC libVLC = new LibVLC();
            Window.musicPlayer.MediaPlayer = new MediaPlayer(libVLC);
            Window.musicPlayer.MediaPlayer.Media = new Media(libVLC, FilePath.GetBeatmapAudioPath());

            Window.playfieldBackground.ImageSource = new BitmapImage(new Uri(FilePath.GetBeatmapBackgroundPath()));
            
            Window.musicPlayer.MediaPlayer.Volume = 0;
            Window.musicPlayerVolume.Text = $"{Window.musicPlayer.MediaPlayer.Volume}%";   

            Window.musicPlayer.MediaPlayer.Media.Parse();
            while (Window.musicPlayer.MediaPlayer.Media.ParsedStatus != MediaParsedStatus.Done)
            {
                Thread.Sleep(10);
            }

            long duration = Window.musicPlayer.MediaPlayer.Media.Duration;
            Window.songMaxTimer.Text = TimeSpan.FromMilliseconds(duration).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            Window.songSlider.Maximum = duration;
            
            Window.musicPlayer.MediaPlayer.Media.Dispose();

            SongSliderControls.InitializeEvents();
            VolumeSliderControls.InitializeEvents();
            PlayPauseControls.InitializeEvents();
        }

        public static long SongDuration()
        {
            return Window.musicPlayer.MediaPlayer!.Media!.Duration;
        }

        public static void Pause()
        {
            Window.musicPlayer.MediaPlayer!.Pause();
        }

        public static void Play()
        {
            Window.musicPlayer.MediaPlayer!.Play();
        }

        public static bool IsPlaying()
        {
            return Window.musicPlayer.MediaPlayer!.IsPlaying;
        }

        public static void Seek(long time)
        {
            if (Window.musicPlayer.MediaPlayer != null)
            {
                Window.musicPlayer.MediaPlayer.Time = time;
                Window.songTimer.Text = TimeSpan.FromMilliseconds(time).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            }
        }
    }
}
