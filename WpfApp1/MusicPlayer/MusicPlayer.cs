using LibVLCSharp.Shared;
using ReplayParsers.Classes.Replay;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp1.GameClock;
using WpfApp1.MusicPlayer.Controls;
using WpfApp1.PlayfieldGameplay;

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
            
            Window.musicPlayer.MediaPlayer.Volume = 35;
            Window.volumeSlider.Value = 35;
            Window.musicPlayerVolume.Text = $"{35}%";   

            Window.musicPlayer.MediaPlayer.Media.Parse();
            while (Window.musicPlayer.MediaPlayer.Media.ParsedStatus != MediaParsedStatus.Done)
            {
                Thread.Sleep(10);
            }

            long duration = Window.musicPlayer.MediaPlayer.Media.Duration;
            Window.songMaxTimer.Text = TimeSpan.FromMilliseconds(duration).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            Window.songSlider.Maximum = duration;

            Window.musicPlayer.MediaPlayer.Media.Dispose();

            Window.musicPlayer.MediaPlayer.EndReached += delegate (object? sender, EventArgs e)
            {
                // this is so media player "doesnt reset" when it reached end of a song
                Window.Dispatcher.Invoke(() => 
                {
                    Window.musicPlayer.MediaPlayer = new MediaPlayer(libVLC);
                    Window.musicPlayer.MediaPlayer.Media = new Media(libVLC, FilePath.GetBeatmapAudioPath());
                    Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
                    Seek(0);

                    List<ReplayFrame> frames = MainWindow.replay.Frames;
                    ReplayFrame f = frames.First();
                    
                    Playfield.UpdateHitObjectIndexAfterSeek(f.Time);
                    Playfield.UpdateCursorPositionAfterSeek(f);
                    Playfield.UpdateHitMarkerIndexAfterSeek(f);
                });

                
                GamePlayClock.Restart();
            };
            
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
