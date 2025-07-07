using Microsoft.Win32;
using Realms.Exceptions;
using ReplayParsers;
using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Beatmap.osuLazer;
using ReplayParsers.Decoders;
using ReplayParsers.Decoders.SevenZip.Compress.LZ;
using ReplayParsers.FileWatchers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
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
        FileSystemWatcher watcher = new FileSystemWatcher();
        string skinPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuFileParser\\WpfApp1\\Skins\\Komori - PeguLian II (PwV)";

        public MainWindow()
        {
            InitializeComponent();

    

            playfieldBackground.Opacity = 0.1;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick!;
            timer.Start();
            
            //GetReplayFile();
            InitializeMusicPlayer();
            playfieldCanva.Loaded += loaded;
        }

        void CreateCircle(HitObject circle, double radius)
        {
            Grid container = new Grid();
            container.Width = radius;
            container.Height = radius;
            
            Image hitCircle = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircle.png")),
            };

            var a = new BitmapImage(new Uri($"{skinPath}\\hitcircle.png"));
            var aaa = a.Palette;

            Image hitCircle2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircle@2x.png"))
            };
            
            Image hitCircleBorder = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay.png"))
            };
            
            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png"))
            };
            
            container.Children.Add(hitCircle);
            container.Children.Add(hitCircle2);
            container.Children.Add(hitCircleBorder);
            container.Children.Add(hitCircleBorder2);
            
            Canvas.SetLeft(container, circle.X);
            Canvas.SetTop(container, circle.Y);
            playfieldCanva.Children.Add(container);
        }

        void CreateSlider(double radius)
        {

        }

        void CreateSpinner(double radius) // uhh is radius needed? idk
        {

        }

        void loaded(object sender, RoutedEventArgs e)
        {
            const double AspectRatio = 1.25;
            double height = playfieldCanva.Height / AspectRatio;
            double width = playfieldCanva.Width / AspectRatio;
            double osuScale = Math.Min(height / 384, width / 512);
            double radius = (54.4 - 4.48 * 4.0) * osuScale;

            Grid hitObject = new Grid();
            hitObject.Width = radius;
            hitObject.Height = radius;

            Image hitCircle = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircle.png")),
                
            };

            Image hitCircle2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircle@2x.png"))
            };

            Image hitCircleBorder = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay.png"))
            };

            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png"))
            };
            
            hitObject.Children.Add(hitCircle);
            hitObject.Children.Add(hitCircle2);
            hitObject.Children.Add(hitCircleBorder);
            hitObject.Children.Add(hitCircleBorder2);

            Canvas.SetLeft(hitObject, 512);
            Canvas.SetTop(hitObject, 384);
            playfieldCanva.Children.Add(hitObject);
        }

        // no sliders test (tetoris map)
        void BeatmapObjectRenderer()
        {
            const double AspectRatio = 1.25;
            double height = playfieldCanva.Height / AspectRatio;
            double width = playfieldCanva.Width / AspectRatio;
            double osuScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);
            double radius = (double)(54.4 - 4.48 * (double)8.0) * osuScale;

            foreach (HitObject hitObject in map.HitObjects)
            {
                if (hitObject.Type.HasFlag(ObjectType.HitCircle))
                {
                    CreateCircle(hitObject, radius);
                }
            }
        }

        // I DID IT IM SO SMART (not)
        void ResizePlayfieldCanva(object sender, SizeChangedEventArgs e)
        {
            const double AspectRatio = 1.33;
            double height = (e.NewSize.Height / AspectRatio);
            double width = (e.NewSize.Width / AspectRatio);

            double circleSize = 4;
            double osuScale = Math.Min(height / 384, width / 512);
            double radius = ((54.4) - (4.48) * circleSize) * osuScale;

            playfieldCanva.Width = 512 * osuScale;
            playfieldCanva.Height = 384 * osuScale;

            playfieldBorder.Width = (512 * osuScale) + radius;
            playfieldBorder.Height = (384 * osuScale) + radius;

            AdjustCanvasHitObjectsPlacementAndSize(radius);
        }

        private void AdjustCanvasHitObjectsPlacementAndSize(double radius)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);

            for (int i = 0; i < playfieldCanva.Children.Count; i++)
            {
                // need FrameworkElement for widht and height values cos UiElement doesnt have it
                FrameworkElement hitObject = (FrameworkElement)playfieldCanva.Children[i];
                // https://osu.ppy.sh/wiki/en/Client/Playfield
                //HitObject hitObject = map.HitObjects[i];
                //int baseHitObjectX = hitObject.X;
                //int baseHitObjectY = hitObject.Y;

                int baseHitObjectX;
                int baseHitObjectY;
                if (i == 0)
                {
                    baseHitObjectX = 300;
                    baseHitObjectY = 200;
                }
                else if (i == 1)
                {
                    baseHitObjectX = 0;
                    baseHitObjectY = 0;
                }
                else if (i == 2)
                {
                    baseHitObjectX = 0;
                    baseHitObjectY = 384;
                }
                else
                {
                    baseHitObjectX = 512;
                    baseHitObjectY = 0;
                }

                int childrenCount = VisualTreeHelper.GetChildrenCount(hitObject);
                for (int j = 0; j < childrenCount; j++)
                {
                    Image? c = VisualTreeHelper.GetChild(hitObject, j) as Image;

                    c.Width = radius;
                    c.Height = radius;
                }

                hitObject.Width = radius;
                hitObject.Height = radius;

                Canvas.SetTop(hitObject, (baseHitObjectY * playfieldScale) - (radius / 2));  
                Canvas.SetLeft(hitObject, (baseHitObjectX * playfieldScale) - (radius / 2));
            }
        }

        private void GetReplayFile()
        {
            // $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\"
            // $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!\\Replays\\"
            watcher.Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports";
            watcher.EnableRaisingEvents = true;
            watcher.Created += OnCreated;
        
            void OnCreated(object sender, FileSystemEventArgs e)
            {
                //if (Dispatcher.Invoke(() => musicPlayer.Source) != null)
                //{
                //    Dispatcher.Invoke(() => musicPlayer.Close());
                //    Dispatcher.Invoke(() => playfieldBackground.ImageSource = null);
                //    Thread.Sleep(2000);
                //}
        
                string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\{e.Name!.Substring(1, e.Name.Length - 38)}";
                map = BeatmapDecoder.GetOsuLazerBeatmap(file);
        
                Dispatcher.Invoke(() => InitializeMusicPlayer());
                Dispatcher.Invoke(() => BeatmapObjectRenderer());
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