using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.MusicPlayer.VarispeedDemo;
using ReplayAnalyzer.SettingsMenu;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

#nullable disable

namespace ReplayAnalyzer.MusicPlayer
{
    public static class MusicPlayer
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static bool IsInitialized = false;

        private static Mp3FileReader AudioFileMP3 { get; set; }
        private static AudioFileReader AudioFileGeneral { get; set; }
        private static VorbisWaveReader AudioFileOGG { get; set; }
        private static FileExtension CurrentFileExtension { get; set; }

        private static SampleChannel AudioSampleChannel { get; set; }

        // bigger delay = more latency and lower cpu
        // lower delay = less latency and A LOT of cpu
        // 20ms looks like nice spot to not use too much cpu and i cant hear delay anyway
        private static WasapiOut WasapiPlayer = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 20);
        private static VarispeedSampleProvider VarispeedSampleProvider { get; set; }

        public static int AudioOffset { get; set; }

        public static void ResetMusicPlayer()
        {
            WasapiPlayer.Stop();
            WasapiPlayer.Dispose();
            WasapiPlayer = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 20);

            if (AudioFileMP3 != null)
            {
                AudioFileMP3.Dispose();
                AudioFileMP3 = null;
            }
            else if (AudioFileOGG != null)
            {
                AudioFileOGG.Dispose();
                AudioFileOGG = null;
            }
            else
            {
                AudioFileGeneral.Dispose();
                AudioFileGeneral = null;
            }

            AudioSampleChannel = null;
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

            // i found out this eats 25MB of ram for 2k sized(?) image... maybe i should unload this when bg opacity = 0%?
            // ^ only if it has 0(ZERO) impact on performance
            //   ^ this is useless WPF and BitmapImage is so bad its literally better to just always have this loaded... somehow
            Window.playfieldBackground.ImageSource = LoadImage(FilePath.GetBeatmapBackgroundPath());

            string path = FilePath.GetBeatmapAudioPath();
            if (path.Substring(path.Length - 4).Contains(".mp3"))
            {
                CurrentFileExtension = FileExtension.MP3;
                AudioFileMP3 = new Mp3FileReader(path);
            }
            else if (path.Substring(path.Length - 4).Contains(".ogg"))
            {
                CurrentFileExtension = FileExtension.OGG;
                AudioFileOGG = new VorbisWaveReader(path);
            }
            else
            {
                CurrentFileExtension = FileExtension.General;
                AudioFileGeneral = new AudioFileReader(path);
            }

            double duration = GetCurrentFileReader().TotalTime.TotalMilliseconds; 
            Window.songMaxTimer.Text = TimeSpan.FromMilliseconds(duration).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            Window.songSlider.Maximum = duration;

            AudioSampleChannel = new SampleChannel(GetCurrentFileReader());

            int volume = int.Parse(SettingsOptions.GetConfigValue("MusicVolume"));
            AudioSampleChannel.Volume = volume / 100.0f;
            VolumeControls.VolumeValue.Text = $"{volume}%";
            VolumeControls.VolumeSlider.Value = volume;

            VarispeedSampleProvider = new VarispeedSampleProvider(AudioSampleChannel, 100, new SoundTouchProfile(true, false));
            
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

        public static bool AudioFileExists()
        {
            return GetCurrentFileReader() != null;
        }

        public static void ChangeVolume(float volume)
        {
            AudioSampleChannel.Volume = volume;
        }

        public static float GetVolume()
        {
            return AudioSampleChannel.Volume;
        }

        public static void Seek(double time)
        {
            if (GetCurrentFileReader() == null)
            {
                return;
            }

            if (time >= GetCurrentFileReader().TotalTime.TotalMilliseconds)
            {
                GetCurrentFileReader().CurrentTime = GetCurrentFileReader().TotalTime;
                Window.songTimer.Text = GetCurrentFileReader().TotalTime.ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
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
            if (time > 500 && AudioOffset > 0)
            {
                currentTime = currentTime + TimeSpan.FromMilliseconds(AudioOffset);
            }
            else if (time > 500 && AudioOffset < 0)
            {
                currentTime = currentTime - TimeSpan.FromMilliseconds(-AudioOffset);
            }

            GetCurrentFileReader().CurrentTime = currentTime;
            Window.songTimer.Text = currentTime.ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);

            if (continuePlay == true)
            {
                WasapiPlayer.Play();
            }
        }

        public static void ChangeMusicRate(float rate)
        {
            if (AudioFileExists() == true)
            {
                VarispeedSampleProvider.PlaybackRate = rate;
            }
        }

        // wait this works just like that LMAO i didnt expect that
        private static WaveStream GetCurrentFileReader()
        {
            switch (CurrentFileExtension)
            {
                case FileExtension.MP3:
                    return AudioFileMP3;
                case FileExtension.OGG:
                    return AudioFileOGG;
                case FileExtension.General:
                    return AudioFileGeneral;
                default:
                    throw new Exception("wrong file extension");
            }
        }

        private enum FileExtension
        {
            MP3 = 0,
            OGG = 1,
            General = 2,
        }
    }
}
