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
        string file = "a";

        public MainWindow()
        {

            InitializeComponent();

            NOTHINGWORKS();

            //var a = Task.Run(() => BeatmapDecoder.GetOsuLazerBeatmap());


            //DispatcherTimer timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(1);
            //timer.Tick += TimerTick!;
            //timer.Start();
            //InitializeMusicPlayer();
            //GetReplayFile();
        }

        // before i punch my monitor
        // awifblafoaifabial;asufhabipf;lalf;afas
        // ffmpeg thingy didnt work even after following all steps
        // after downloading sample project for this stuff worked
        // after a lot of smashing my head against the wall i saw in files ffme.win.dll
        // replaced this steiojwgp;asf file from sample project to mine and it worked
        // i dont want to know WHY THE FILE THAT WAS SUPPOSED TO WORK didnt work
        // i want to ohjasofhansp;fasofaasfhasfoasfoafhoas[hofiasuo[has [of[whqw[0r12 mu48r19pilsdkfjnsdklfjdqo[rfb;pf 
        // oh the best part is that file auto updates after building app... just f off
        // https://github.com/unosquare/ffmediaelement/issues/679 solved everything

        async void NOTHINGWORKS()
        {
           // Media.Open(new FileInputStream($"{AppDomain.CurrentDomain.BaseDirectory}osu\\Audio\\audio.mp3"));

            var m = Media;
            m.Volume = 0.05;
            var target = new Uri($@"C:\Users\patry\Desktop\Nowy folder\OsuLazerAudio\audio.mp3");
            await m.Open(new Uri(target.LocalPath));
            await m.Play();
        }

        //FileSystemWatcher watcher = new FileSystemWatcher();
        //
        //private void GetReplayFile()
        //{
        //    watcher.Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\";
        //    watcher.EnableRaisingEvents = true;
        //    watcher.Created += OnCreated;
        //
        //    void OnCreated(object sender, FileSystemEventArgs e)
        //    {
        //        //if (Dispatcher.Invoke(() => musicPlayer.Source) != null)
        //        //{
        //        //    Dispatcher.Invoke(() => musicPlayer.Source = null);
        //        //    Dispatcher.Invoke(() => background.ImageSource = null);
        //        //}
        //
        //        file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\{e.Name!.Substring(1, e.Name.Length - 38)}";
        //        map = BeatmapDecoder.GetOsuLazerBeatmap(file);
        //
        //        Dispatcher.Invoke(() => InitializeMusicPlayer());
        //    }
        //}
        //
        //private async void InitializeMusicPlayer()
        //{
        //    
        //    await Media.Open(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}osu\\Audio\\audio.mp3"));
        //    //background.ImageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Background\\bg.jpg"));
        //
        //    Media.Volume = 0.05;
        //    // you dont want to work the nice way so you will work the bruteforce way
        //    //await musicPlayer.Play();
        //    //await musicPlayer.Pause();
        //    //.MediaOpened += MusicPlayer_MediaOpened!;
        //
        //    //musicPlayerVolume.Text = $"{musicPlayer.Volume * 100}%";
        //}
        //
        //void MusicPlayer_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        //{
        //    if (musicPlayer.NaturalDuration.HasValue)
        //    {
        //        songMaxTimer.Text = musicPlayer.NaturalDuration.ToString()!.Substring(0, 8);
        //    }
        //}
        //
        //// dont want DragStarted and stuff with it coz i want position changed only when drag is completed... for now...
        //void SongSliderDragCompleted(object sender, DragCompletedEventArgs e)
        //{
        //    musicPlayer.Position = TimeSpan.FromSeconds(songSlider.Value);
        //}
        //
        //void SongSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    songTimer.Text = TimeSpan.FromSeconds(songSlider.Value).ToString(@"hh\:mm\:ss");
        //}
        //
        //void TimerTick(object sender, EventArgs e)
        //{
        //    if (musicPlayer.Source != null && musicPlayer.NaturalDuration.HasValue)
        //    {
        //        songSlider.Minimum = 0;
        //        songSlider.Maximum = musicPlayer.NaturalDuration.Value.TotalSeconds;
        //        songSlider.Value = musicPlayer.Position.TotalSeconds;
        //    }
        //}
        //
        //void PlayPauseButton(object sender, RoutedEventArgs e)
        //{
        //    if (playerButton.Style == FindResource("PlayButton"))
        //    {
        //        playerButton.Style = Resources["PauseButton"] as Style;
        //        musicPlayer.Play();
        //    }
        //    else
        //    {
        //        playerButton.Style = Resources["PlayButton"] as Style;
        //        musicPlayer.Pause();
        //    }
        //}
        //
        //void VolumeSliderValue(object sender, RoutedEventArgs e)
        //{
        //    if (musicPlayerVolume != null)
        //    {
        //        musicPlayerVolume.Text = $"{volumeSlider.Value}%";
        //    }
        //
        //    musicPlayer.Volume = volumeSlider.Value / 100;
        //
        //    // thank you twitch
        //    if (musicPlayer.Volume == 0)
        //    {
        //        volumeIcon.Data = Geometry.Parse("m5 7 4.146-4.146a.5.5 0 0 1 .854.353v13.586a.5.5 0 0 1-.854.353L5 13H4a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h1zm7 1.414L13.414 7l1.623 1.623L16.66 7l1.414 1.414-1.623 1.623 1.623 1.623-1.414 1.414-1.623-1.623-1.623 1.623L12 11.66l1.623-1.623L12 8.414z");
        //    }
        //    else if (musicPlayer.Volume > 0 && musicPlayer.Volume < 0.5)
        //    {
        //        volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z");
        //    }
        //    else if (musicPlayer.Volume >= 0.5)
        //    {
        //        volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z M12 6a4 4 0 0 1 0 8v2a6 6 0 0 0 0-12v2z");
        //    }
        //}
    }
}