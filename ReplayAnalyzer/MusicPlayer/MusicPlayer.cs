using LibVLCSharp.Shared;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.SettingsMenu;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.MusicPlayer
{
    //https://github.com/videolan/libvlcsharp
    public static class MusicPlayer
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static bool IsInitialized = false;

        public static void Initialize()
        {
            // i was looking hours for this... thank you random internet post... ... ...
            // https://wiki.videolan.org/VLC_command-line_help/
            LibVLC libVLC = new LibVLC();
            Window.musicPlayer.MediaPlayer = new MediaPlayer(libVLC);
            Window.musicPlayer.MediaPlayer.Media = new Media(libVLC, FilePath.GetBeatmapAudioPath());
         
            Window.musicPlayer.MediaPlayer.Media.AddOption(":start-paused");
            Play();

            Window.playfieldBackground.ImageSource = LoadImage(FilePath.GetBeatmapBackgroundPath());

            int volume = int.Parse(SettingsOptions.GetConfigValue("MusicVolume"));
            Window.musicPlayer.MediaPlayer.Volume = volume;
            VolumeControls.VolumeSlider.Value = volume;
            VolumeControls.VolumeValue.Text = $"{volume}%";

            Window.musicPlayer.MediaPlayer.Media.Parse();
            while (Window.musicPlayer.MediaPlayer.Media.ParsedStatus != MediaParsedStatus.Done)
            {
                Thread.Sleep(10);
            }

            long duration = Window.musicPlayer.MediaPlayer.Media.Duration;
            Window.songMaxTimer.Text = TimeSpan.FromMilliseconds(duration).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            Window.songSlider.Maximum = duration;
            
            Window.musicPlayer.MediaPlayer.Media.Dispose();

            Window.musicPlayer.MediaPlayer.EndReached += MediaPlayerEndReached!;

            if (IsInitialized == false)
            {
                SongSliderControls.InitializeEvents();
                VolumeControls.InitializeEvents();
                PlayPauseControls.InitializeEvents();
                RateChangerControls.InitializeEvents();
                JudgementTimeline.Initialize();

                IsInitialized = true;
            }
        }

        // https://stackoverflow.com/questions/7094684/c-sharp-wpf-how-to-unreference-a-bitmapimage-so-i-can-delete-the-source-file
        // i love wpf its so annoying!!!
        private static BitmapImage LoadImage(string myImageFile)
        {
            BitmapImage? myRetVal = null;
            if (myImageFile != null)
            {
                BitmapImage image = new BitmapImage();
                using (FileStream stream = File.OpenRead(myImageFile))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                }
                myRetVal = image;
            }

            return myRetVal!;
        }

        private static void MediaPlayerEndReached(object sender, EventArgs e)
        {
            Window.Dispatcher.Invoke(() => 
            {
                Initialize();
            });
        }

        public static long SongDuration()
        {
            return Window.musicPlayer.MediaPlayer!.Media!.Duration;
        }

        public static void Pause()
        {
            // wait wait wait what is this... calling .Pause() pauses player... caling it SECOND time UNPAUSES it...
            // WHAT THE HELL IS WRONG WITH YOU LIKE OH MY GOD STEP ON A LEGO ARE YOU STUPID WHO HURT YOU
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

        public static void Seek(double time)
        {
            if (Window.musicPlayer.MediaPlayer != null)
            {
                Window.musicPlayer.MediaPlayer.Time = (long)time;
                Window.songTimer.Text = TimeSpan.FromMilliseconds(time).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            }
        }
    }
}
