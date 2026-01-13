using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.SettingsMenu;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using NAudio.Wave;
using ReplayAnalyzer.MusicPlayer.VarispeedDemo;


namespace ReplayAnalyzer.MusicPlayer
{
    //https://github.com/videolan/libvlcsharp
    public static class MusicPlayer
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static bool IsInitialized = false;

        public static AudioFileReader? AudioFile { get; set; }
        private static WasapiOut WasapiPlayer = new WasapiOut();
        private static VarispeedSampleProvider? VarispeedSampleProvider { get; set; }

        // audio delay should be same as start delay but i have NO CLUE WHY there is some kind of
        // offset in audio so this additional number is to hopefully correct that offset
        //MainWindow.StartDelay > 100 ? MainWindow.StartDelay - 100 : -100;
        public static int AudioDelay = 100;

        public static void ResetMusicPlayer()
        {
            WasapiPlayer.Stop();
            WasapiPlayer.Dispose();
            WasapiPlayer = new WasapiOut();
            AudioFile.Dispose();
            VarispeedSampleProvider.Dispose();
            Window.playfieldBackground.ImageSource = null;
        }

        // before great audio change https://github.com/ravinyan/osuReplayAnalyzer/tree/ab9d1131d4fa15e0b3c02ee7f81a7058e12e8ebc
        public static void Initialize()
        {
            // i will just enjoy comfy planning for today https://www.markheath.net/post/naudio-audio-output-devices
            // AsioOut (ok thats not gonna work) or WasapiOut or WaveOut
            // everything from here
            // https://markheath.net/post/varispeed-naudio-soundtouch
            // and https://github.com/naudio/varispeed-sample/blob/master/VarispeedDemo/SoundTouch/SoundTouchProfile.cs
            // also https://soundtouch.surina.net/download.html

            AudioFile = new AudioFileReader(FilePath.GetBeatmapAudioPath());

            int volume = int.Parse(SettingsOptions.GetConfigValue("MusicVolume"));
            AudioFile.Volume = volume / 100.0f;
            VolumeControls.VolumeValue.Text = $"{volume}%";
            VolumeControls.VolumeSlider.Value = volume;

            double duration = AudioFile.TotalTime.TotalMilliseconds;
            Window.songMaxTimer.Text = TimeSpan.FromMilliseconds(duration).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            Window.songSlider.Maximum = duration;

            Window.playfieldBackground.ImageSource = LoadImage(FilePath.GetBeatmapBackgroundPath());

            VarispeedSampleProvider = new VarispeedSampleProvider(AudioFile, 100, new SoundTouchProfile(true, false));
            WasapiPlayer.Init(VarispeedSampleProvider);
            
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

        // to test later
        private static void MediaPlayerEndReached(object sender, EventArgs e)
        {
            Window.Dispatcher.Invoke(() => 
            {
                Initialize();
            });
        }

        public static long SongDuration()
        {
            return (long)AudioFile.TotalTime.TotalMilliseconds;
        }

        public static void Pause()
        {
            // Stop() instead of Pause() coz it stops music instantly while Pause() stops after ~500ms
            WasapiPlayer.Stop();
        }

        public static void Play()
        {
            WasapiPlayer.Play();
        }

        public static bool IsPlaying()
        {
            return WasapiPlayer.PlaybackState == PlaybackState.Playing ? true : false;
        }

        public static void ChangeVolume(float volume)
        {
            AudioFile!.Volume = volume;
        }

        public static void Seek(double time)
        {
            if (time >= AudioFile.TotalTime.TotalMilliseconds)
            {
                AudioFile.CurrentTime = AudioFile.TotalTime;
                Window.songTimer.Text = AudioFile.TotalTime.ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
                return;
            }

            // using Stop() to stop the audio and then change current time
            // then Play() starts from the correct point
            // doing so without Stop() and Play() causes audio to play ~500ms delayed
            bool continuePlay = false;
            if (IsPlaying())
            {
                continuePlay = true;
            }

            WasapiPlayer.Stop();
            
            // if i understand this Reposition requests reposition (duh) and clears SoundTouch song data like processed and to be processed? idk
            // but it helps with removing audio delay and removes delay when seeking when audio reached the end
            VarispeedSampleProvider.Reposition();

            TimeSpan currentTime = time + AudioDelay > 0 ? TimeSpan.FromMilliseconds(time + AudioDelay) : TimeSpan.Zero;
            AudioFile.CurrentTime = currentTime;
            Window.songTimer.Text = currentTime.ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            
            if (continuePlay == true)
            {
                WasapiPlayer.Play();
            }
        }

        public static void ChangeMusicRate(float rate)
        {
            VarispeedSampleProvider.PlaybackRate = rate;
        }
    }
}
