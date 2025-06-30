using Microsoft.Win32;
using Realms.Exceptions;
using ReplayParsers;
using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Classes.Beatmap.osuLazer;
using ReplayParsers.Decoders;
using ReplayParsers.FileWatchers;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Unosquare.FFME;
using static System.Net.Mime.MediaTypeNames;
using Beatmap = ReplayParsers.Classes.Beatmap.osu.Beatmap;

// https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Beatmap? map;
   

        public MainWindow()
        {
            InitializeComponent();

            playfieldBackground.Opacity = 0.5;
            //var a = Task.Run(() => BeatmapDecoder.GetOsuLazerBeatmap());
            
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick!;
            timer.Start();
            InitializeMusicPlayer();
            //GetReplayFile();
        }

        void ResizePlayfieldCanva(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine(e.NewSize);
            //playfieldViewbox.Height = playfieldViewbox.Height - 100;
        }

        private void GetReplayFile()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\";
            watcher.EnableRaisingEvents = true;
            watcher.Created += OnCreated;
        
            void OnCreated(object sender, FileSystemEventArgs e)
            {
                if (Dispatcher.Invoke(() => musicPlayer.Source) != null)
                {
                    Dispatcher.Invoke(() => musicPlayer.Close());
                    Dispatcher.Invoke(() => playfieldBackground.ImageSource = null);
                    Thread.Sleep(2000);
                }
        
                string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\{e.Name!.Substring(1, e.Name.Length - 38)}";
                map = BeatmapDecoder.GetOsuLazerBeatmap(file);
        
                Dispatcher.Invoke(() => InitializeMusicPlayer());
            }
        }
        
        private async void InitializeMusicPlayer()
        {
            musicPlayer.Volume = 0.05;
            await musicPlayer.Open(new Uri($@"{AppDomain.CurrentDomain.BaseDirectory}\osu\Audio\audio.mp3"));

            playfieldBackground.ImageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Background\\bg.jpg"));
            musicPlayerVolume.Text = $"{musicPlayer.Volume * 100}%";
        }
        
        void MusicPlayer_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            if (musicPlayer.NaturalDuration.HasValue)
            {
                songMaxTimer.Text = musicPlayer.NaturalDuration.ToString()!.Substring(0, 8);
            }
        }
        
        // dont want DragStarted and stuff with it coz i want position changed only when drag is completed... for now...
        void SongSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            musicPlayer.Position = TimeSpan.FromSeconds(songSlider.Value);
        }
        
        void SongSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            songTimer.Text = TimeSpan.FromSeconds(songSlider.Value).ToString(@"hh\:mm\:ss");
        }
        
        void TimerTick(object sender, EventArgs e)
        {
            if (musicPlayer.Source != null && musicPlayer.NaturalDuration.HasValue)
            {
                songSlider.Minimum = 0;
                songSlider.Maximum = musicPlayer.NaturalDuration.Value.TotalSeconds;
                songSlider.Value = musicPlayer.Position.TotalSeconds;
            }
        }
        
        void PlayPauseButton(object sender, RoutedEventArgs e)
        {
            if (playerButton.Style == FindResource("PlayButton"))
            {
                playerButton.Style = Resources["PauseButton"] as Style;
                musicPlayer.Play();
            }
            else
            {
                playerButton.Style = Resources["PlayButton"] as Style;
                musicPlayer.Pause();
            }
        }
        
        void VolumeSliderValue(object sender, RoutedEventArgs e)
        {
            if (musicPlayerVolume != null)
            {
                musicPlayerVolume.Text = $"{volumeSlider.Value}%";
            }
        
            musicPlayer.Volume = volumeSlider.Value / 100;
        
            // thank you twitch
            if (musicPlayer.Volume == 0)
            {
                volumeIcon.Data = Geometry.Parse("m5 7 4.146-4.146a.5.5 0 0 1 .854.353v13.586a.5.5 0 0 1-.854.353L5 13H4a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h1zm7 1.414L13.414 7l1.623 1.623L16.66 7l1.414 1.414-1.623 1.623 1.623 1.623-1.414 1.414-1.623-1.623-1.623 1.623L12 11.66l1.623-1.623L12 8.414z");
            }
            else if (musicPlayer.Volume > 0 && musicPlayer.Volume < 0.5)
            {
                volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z");
            }
            else if (musicPlayer.Volume >= 0.5)
            {
                volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z M12 6a4 4 0 0 1 0 8v2a6 6 0 0 0 0-12v2z");
            }
        }
    }
}