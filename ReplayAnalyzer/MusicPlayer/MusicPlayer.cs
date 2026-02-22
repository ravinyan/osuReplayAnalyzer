using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.MusicPlayer.VarispeedDemo;
using ReplayAnalyzer.SettingsMenu;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

#nullable disable

namespace ReplayAnalyzer.MusicPlayer
{
    public static class MusicPlayer
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static bool IsInitialized = false;

        public static Mp3FileReader AudioFile { get; set; }
        private static SampleChannel AudioFileVolume { get; set; }

        // bigger delay = more latency and lower cpu
        // lower delay = less latency and A LOT of cpu
        // 20ms looks like nice spot to not use too much cpu and i cant hear delay anyway
        private static WasapiOut WasapiPlayer = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 20);
        private static VarispeedSampleProvider VarispeedSampleProvider { get; set; }

        public static int AudioOffset;

        public static void ResetMusicPlayer()
        {
            WasapiPlayer.Stop();
            WasapiPlayer.Dispose();
            WasapiPlayer = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 20);
            AudioFile.Dispose();
            AudioFileVolume = null;
            VarispeedSampleProvider.Dispose();
            Window.playfieldBackground.ImageSource = null;
        }

        // before great audio change https://github.com/ravinyan/osuReplayAnalyzer/tree/ab9d1131d4fa15e0b3c02ee7f81a7058e12e8ebc
        public static void Initialize()
        {
            // i will just enjoy comfy planning for today https://www.markheath.net/post/naudio-audio-output-devices
            // AsioOut (ok thats not gonna work) or WasapiOut or WaveOut
            // everything from here (DO NOT USE AUDIOFILEREADER JUST CONVERT TO MP3 AND USE MP3FILEREADER)
            // https://markheath.net/post/varispeed-naudio-soundtouch < FUCK YOU
            // and https://github.com/naudio/varispeed-sample/blob/master/VarispeedDemo/SoundTouch/SoundTouchProfile.cs
            // also https://soundtouch.surina.net/download.html

            // fuck you fuck you fuck you fuck you FUCKYOU
            //AudioFile = new AudioFileReader(FilePath.GetBeatmapAudioPath());

            // everything is converted to mp3 files now... issue with audio was mp3 has different file formats and some of them
            // were not correctly supported(?) in AudioFileReader... so now everything should work correctly (please work correctly)
            AudioFile = new Mp3FileReader(FilePath.GetBeatmapAudioPath());
            AudioFileVolume = new SampleChannel(AudioFile);

            int volume = int.Parse(SettingsOptions.GetConfigValue("MusicVolume"));
            AudioFileVolume.Volume = volume / 100.0f;
            VolumeControls.VolumeValue.Text = $"{volume}%";
            VolumeControls.VolumeSlider.Value = volume;

            double duration = AudioFile.TotalTime.TotalMilliseconds;
            Window.songMaxTimer.Text = TimeSpan.FromMilliseconds(duration).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            Window.songSlider.Maximum = duration;

            Window.playfieldBackground.ImageSource = LoadImage(FilePath.GetBeatmapBackgroundPath());

            VarispeedSampleProvider = new VarispeedSampleProvider(AudioFileVolume, 100, new SoundTouchProfile(true, false));
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
            BitmapImage myRetVal = null;
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
            AudioFileVolume.Volume = volume;
        }

        public static float GetVolume()
        {
            return AudioFileVolume.Volume;
        }

        public static void Seek(double time, double diff = 0)
        {
            if (AudioFile == null)
            {
                return;
            }

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

            TimeSpan currentTime = TimeSpan.FromMilliseconds(time);
            if (AudioOffset > 0)
            {
                currentTime += TimeSpan.FromMilliseconds(AudioOffset);
            }
            else if (AudioOffset < 0)
            {
                currentTime -= TimeSpan.FromMilliseconds(AudioOffset);
            }

            // prevent crash until i unscuff
            if (currentTime < TimeSpan.Zero || time < 0)
            {
                currentTime = TimeSpan.Zero;
            }

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
