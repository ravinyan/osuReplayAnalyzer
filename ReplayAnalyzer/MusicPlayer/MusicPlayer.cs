using LibVLCSharp.Shared;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.SettingsMenu;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

using NAudio.Wave;
using ReplayAnalyzer.MusicPlayer.VarispeedDemo;
using NAudio.Wave.SampleProviders;


namespace ReplayAnalyzer.MusicPlayer
{
    //https://github.com/videolan/libvlcsharp
    public static class MusicPlayer
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static bool IsInitialized = false;
        public static WasapiOut WasapiPlayer = new WasapiOut();
        public static SampleChannel sampleChannel { get; set; }
        // audio delay should be same as start delay but i have NO CLUE WHY there is some kind of
        // offset in audio so this additional number is to hopefully correct that offset
        //MainWindow.StartDelay > 100 ? MainWindow.StartDelay - 100 : -100;
        public static int AudioDelay = 100;

        public static void ResetMusicPlayer()
        {
            Window.musicPlayer.MediaPlayer!.Stop();
            Window.musicPlayer.MediaPlayer.Media = null;
            Window.musicPlayer.MediaPlayer = null;
            Window.playfieldBackground.ImageSource = null;
        }

        public static void Initialize()
        {
            // i will just enjoy comfy planning for today https://www.markheath.net/post/naudio-audio-output-devices
            // AsioOut (ok thats not gonna work) or WasapiOut or WaveOut
            // everything from here
            // https://markheath.net/post/varispeed-naudio-soundtouch
            // and https://github.com/naudio/varispeed-sample/blob/master/VarispeedDemo/SoundTouch/SoundTouchProfile.cs
            // also https://soundtouch.surina.net/download.html

            using (AudioFileReader audioFile = new AudioFileReader(FilePath.GetBeatmapAudioPath()))
            {
                VarispeedSampleProvider sampleProvider = new VarispeedSampleProvider(audioFile, 100, new SoundTouchProfile(true, false));

                VolumeSampleProvider volumeSampleProvider = new VolumeSampleProvider(sampleProvider);
                volumeSampleProvider.Volume = 0.01f;
                WasapiPlayer.Init(sampleProvider);
 
                
            }

            Window.playfieldBackground.ImageSource = LoadImage(FilePath.GetBeatmapBackgroundPath());

            /*
            //// i was looking hours for this... thank you random internet post... ... ...
            //// https://wiki.videolan.org/VLC_command-line_help/
            //LibVLC libVLC = new LibVLC();
            //Window.musicPlayer.MediaPlayer = new MediaPlayer(libVLC);
            //Window.musicPlayer.MediaPlayer.Media = new Media(libVLC, FilePath.GetBeatmapAudioPath());
            //
            //Window.musicPlayer.MediaPlayer.Media.AddOption(":start-paused");
            //Play();
            //
            //Window.playfieldBackground.ImageSource = LoadImage(FilePath.GetBeatmapBackgroundPath());
            //
            //int volume = int.Parse(SettingsOptions.GetConfigValue("MusicVolume"));
            //Window.musicPlayer.MediaPlayer.Volume = volume;
            //VolumeControls.VolumeSlider.Value = volume;
            //VolumeControls.VolumeValue.Text = $"{volume}%";
            //
            //Window.musicPlayer.MediaPlayer.Media.Parse();
            //while (Window.musicPlayer.MediaPlayer.Media.ParsedStatus != MediaParsedStatus.Done)
            //{
            //    Thread.Sleep(10);
            //}
            //
            //long duration = Window.musicPlayer.MediaPlayer.Media.Duration;
            //Window.songMaxTimer.Text = TimeSpan.FromMilliseconds(duration).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            //Window.songSlider.Maximum = duration;
            //
            //Window.musicPlayer.MediaPlayer.Media.Dispose();
            //
            //Window.musicPlayer.MediaPlayer.EndReached += MediaPlayerEndReached!;
            */
            if (IsInitialized == false)
            {
                SongSliderControls.InitializeEvents();
                VolumeControls.InitializeEvents();
                PlayPauseControls.InitializeEvents();
                RateChangerControls.InitializeEvents();

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
            return 100000;//Window.musicPlayer.MediaPlayer!.Media!.Duration;
        }

        public static void Pause()
        {
            // wait wait wait what is this... calling .Pause() pauses player... caling it SECOND time UNPAUSES it...
            // WHAT THE HELL IS WRONG WITH YOU LIKE OH MY GOD STEP ON A LEGO ARE YOU STUPID WHO HURT YOU
            //Window.musicPlayer.MediaPlayer!.Pause();

            WasapiPlayer.Stop();
        }

        public static void Play()
        {
            //if (WasapiPlayer.PlaybackState == PlaybackState.Playing)
            //{
            //    WasapiPlayer.Pause();
            //}
            //PlaybackState a  = WasapiPlayer.PlaybackState;
            WasapiPlayer.Play();
        }

        public static bool IsPlaying()
        {
            return Window.musicPlayer.MediaPlayer!.IsPlaying;
        }

        public static void ChangeVolume(int volume)
        {
            // this is system volume... ok but this is not something i should do so... how... hmmm
            var a = WasapiPlayer.Volume;

            // hm
            sampleChannel.Volume = volume;
        }

        public static void Seek(double time)
        {
            if (Window.musicPlayer.MediaPlayer != null)
            {
                //long newTime = (long)time - AudioDelay > 0 ? (long)time - AudioDelay : 0;
                //if (newTime <= 0)
                //{
                //    Window.musicPlayer.MediaPlayer.Time = 0;
                //    return;
                //}

                Window.musicPlayer.MediaPlayer.Time = (long)time + AudioDelay;
                Window.songTimer.Text = TimeSpan.FromMilliseconds(time + AudioDelay).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            }
        }
    }
}
