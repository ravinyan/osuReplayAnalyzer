using NAudio.Wave;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.MusicPlayer.VarispeedDemo;
using ReplayAnalyzer.SettingsMenu;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;


namespace ReplayAnalyzer.MusicPlayer
{
    public static class MusicPlayer
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static bool IsInitialized = false;

        public static AudioFileReader? AudioFile { get; set; }
        private static WasapiOut WasapiPlayer = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 0);
        private static VarispeedSampleProvider? VarispeedSampleProvider { get; set; }

        // audio delay should be same as start delay but i have NO CLUE WHY there is some kind of
        // offset in audio so this additional number is to hopefully correct that offset
        //MainWindow.StartDelay > 100 ? MainWindow.StartDelay - 100 : -100;
        public static int AudioDelay = 100;

        public static int AudioOffset;

        public static void ResetMusicPlayer()
        {
            WasapiPlayer.Stop();
            WasapiPlayer.Dispose();
            WasapiPlayer = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 0);
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

        public static void Seek(double time, double diff = 0)
        {
            /* im tired of this audio bullshit i will either find that i cant fix it or i find a way to fix it i dont care anymore         
            on most maps everything works perfectly
            on r3m everlastin eternity if seek was never used audio is correct but when it is used then the further map is from the start
            the worse audio offset is (audio plays too early)
            on kotoha map seeking randomly anywhere makes audio (it was too late when i tested)
            on kotoha map i also deleted BPM (no timing points) and issue still persists so that is not the problem
            on kotoha map i checked out 2 different mapsets from what i played (cut ver and full ver) and both had correct audio but my version saved here somehow doesnt
            on kotoha map audio files in order replay saved here > cut ver > full ver is mp3 > mp3 > mp3 so all are the same
            there is literally N O T H I N G in these mapsets that could make ANY difference even changing bpm timing point does nothing anywhere (other than just changing offset and making my acc cry)
            also changed audio from beatmap saved here to cut version is osu lazer and it had different offset in game but whatever
            but in analyzer there was again this audio delay bug... therefore conclusion of THIS AUDIO FILE IS THE PROBLEM 2 OTHER SAME SONG AUDIO FILES
            (even tho 1 was cut version) HAD NO PROBLEMS AT ALL AAAAAAAA i give up
            */
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

            var positioon = AudioFile.Position;
            WasapiPlayer.Stop();

            var positione = AudioFile.Position;

            // if i understand this Reposition requests reposition (duh) and clears SoundTouch song data like processed and to be processed? idk
            // but it helps with removing audio delay and removes delay when seeking when audio reached the end
            VarispeedSampleProvider.Reposition();


            // i shall fix this or i die
            // when playing normally there will never be audio problems BUT
            // when pausing/unpausing the time of AudioFile is higher than gameplay clock (which is the correct one)
            // with difference of gameplay clock = 111318, AudioFile = 111700
            // in r3m map the longer it goes the higher the delay ONLY WHEN USING SEEKING otherwise there is no delay
            var a = AudioFile.CurrentTime.TotalMilliseconds;
            
            Dictionary<int, ReplayFrame>.ValueCollection? frames = MainWindow.replay.FramesDict.Values;
            ReplayFrame f = frames.FirstOrDefault(f => f.Time > Window.songSlider.Value) ?? frames.Last();

           // AudioFile.CurrentTime = TimeSpan.FromMilliseconds(time);
            var difference = AudioFile.CurrentTime.TotalMilliseconds - time;

            // sometimes it happens in very specific scenario and it also should never be 0 coz it will break music player timing
            if (f.Time < 0)
            {
                f = frames.First(f => f.Time >= 0);
            }

            
            //TimeSpan currentTime = time + AudioDelay > 0 ? TimeSpan.FromMilliseconds(time) : TimeSpan.Zero;
            //TimeSpan currentTime = TimeSpan.FromMilliseconds(time + difference);
            TimeSpan currentTime = time - 200 > 0 ? TimeSpan.FromMilliseconds(time) : TimeSpan.Zero;
            
            if (AudioOffset > 0)
            {
                currentTime += TimeSpan.FromMilliseconds(AudioOffset);
                time += AudioOffset;
            }
            else if (AudioOffset < 0)
            {
                currentTime -= TimeSpan.FromMilliseconds(AudioOffset);
                time -= AudioOffset;
            }

            // prevent crash until i unscuff
            if (currentTime < TimeSpan.Zero || time < 0)
            {
                currentTime = TimeSpan.Zero;
                time = 0;
            }

            var help = AudioFile.WaveFormat.ConvertLatencyToByteSize(200);
            var b = GamePlayClock.TimeElapsed;
            var asd = Window.songSlider.Value;
            var aaa = (long)((time / 1000) * AudioFile.WaveFormat.AverageBytesPerSecond);
            aaa -= help;
            //var aaa = (long)((time / 1000) * help);


            //AudioFile.Seek(aaa, SeekOrigin.Begin);
            AudioFile.CurrentTime = currentTime;
            //AudioFile.Seek((long)time * 4, SeekOrigin.Begin);

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
