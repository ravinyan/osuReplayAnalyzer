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
using Unosquare.FFME;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Beatmap = ReplayParsers.Classes.Beatmap.osu.Beatmap;

// https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private Beatmap? map;
        FileSystemWatcher watcher = new FileSystemWatcher();


        public MainWindow()
        {

            InitializeComponent();



            playfieldBackground.Opacity = 0.1;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick!;
            timer.Start();

            
            GetReplayFile();
            //InitializeMusicPlayer();
            //playfieldCanva.Loaded += loaded;
            //Loaded += windowLoaded;
        }

        void windowLoaded(object sender, RoutedEventArgs e)
        {
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            ScaleTransform dpiTransform = new ScaleTransform(1 / m.M11, 1 / m.M22);
            if (dpiTransform.CanFreeze)
                dpiTransform.Freeze();
            //LayoutTransform = dpiTransform;
            //playfieldGrid.LayoutTransform = dpiTransform;
            //window.LayoutTransform = dpiTransform;


            //double dpiX;
            //double dpiY;
            //PresentationSource presentationsource = PresentationSource.FromVisual(this);

            //if (presentationsource != null) // make sure it's connected
            //{
            //    dpiX = 96.0 * presentationsource.CompositionTarget.TransformToDevice.M11;
            //    dpiY = 96.0 * presentationsource.CompositionTarget.TransformToDevice.M22;
            //}
        }

        void loaded(object sender, RoutedEventArgs e)
        {
            const double AspectRatio = 1.25;
            double height = playfieldCanva.Height / AspectRatio;
            double width = playfieldCanva.Width / AspectRatio;
            double osuScale = Math.Min(height / 384, width / 512);
            double radius = (double)(54.4 - 4.48 * (double)8.0) * osuScale;

            Ellipse ellipse = new Ellipse();
            ellipse.Width = radius;
            ellipse.Height = radius;
            ellipse.Fill = new SolidColorBrush(Colors.Pink);
            ellipse.StrokeThickness = 3;
            ellipse.Stroke = new SolidColorBrush(Colors.White);
            ellipse.Opacity = 0.9;

            Canvas.SetLeft(ellipse, 512);
            Canvas.SetTop(ellipse, 384);

            playfieldCanva.Children.Add(ellipse);

            ellipse = new Ellipse();
            ellipse.Width = radius;
            ellipse.Height = radius;
            ellipse.Fill = new SolidColorBrush(Colors.Pink);
            ellipse.StrokeThickness = 3;
            ellipse.Stroke = new SolidColorBrush(Colors.White);
            ellipse.Opacity = 0.9;
            Canvas.SetLeft(ellipse, 0);
            Canvas.SetTop(ellipse, 0);

            playfieldCanva.Children.Add(ellipse);
        }

        // no sliders test (tetoris map)
        void BeatmapCircleRenderer()
        {
            const double AspectRatio = 1.25;
            double height = playfieldCanva.Height / AspectRatio;
            double width = playfieldCanva.Width / AspectRatio;
            double osuScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);
            double radius = (double)(54.4 - 4.48 * (double)8.0) * osuScale;
            int i = 1;

            foreach (var hitObject in map.HitObjects)
            {
                if (hitObject.Type.HasFlag(ObjectType.HitCircle))
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.Width = radius;
                    ellipse.Height = radius;
                    ellipse.Fill = new SolidColorBrush(Colors.Pink);
                    ellipse.StrokeThickness = 3;
                    ellipse.Stroke = new SolidColorBrush(Colors.White);
                    ellipse.Opacity = 0.9;
                    
                    //TextBlock textBlock = new TextBlock();
                    //textBlock.FontSize = 15 * osuScale;
                    //textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    //textBlock.VerticalAlignment = VerticalAlignment.Center;
                    //
                    //if (hitObject.Type.HasFlag(ObjectType.StartNewCombo))
                    //{
                    //    i = 1;
                    //}
                    //
                    //textBlock.Text = $"{i}";
                    //i++;

                    Canvas.SetLeft(ellipse, hitObject.X * osuScale);
                    Canvas.SetTop(ellipse, hitObject.Y * osuScale);

                    playfieldCanva.Children.Add(ellipse);
                }
            }
        }

        // I DONT KNOW WHAT IM DOING
        // next challenge > actually figure out how to render circles in real time... good luck to future me
        // ok something is off and i dont know what aaaaaaaa
        void ResizePlayfieldCanva(object sender, SizeChangedEventArgs e)
        {
            
            //Debug.WriteLine("Width  : " + Width);
            //Debug.WriteLine("Height : " + Height);
            //Debug.WriteLine("AWidth : " + ActualWidth);
            //Debug.WriteLine("AHeight: " + ActualHeight);
            //Debug.WriteLine("newsize: " + e.NewSize);
            const double AspectRatio = 1.25;
            double height = (e.NewSize.Height / AspectRatio);
            double width = (e.NewSize.Width / AspectRatio);
            // 576 448
            double circleSize = 8;

            double osuScale = Math.Min(height / 384, width / 512);
            double radius = ((54.4) - (4.48) * circleSize) * osuScale;

            playfieldCanva.Width = (512) * osuScale;
            playfieldCanva.Height = (384) * osuScale;

            Debug.WriteLine("Width  : " + playfieldCanva.Width);
            Debug.WriteLine("Height : " + playfieldCanva.Height);

            AdjustCanvasHitObjectsPlacementAndSize(radius);
        }

        private void AdjustCanvasHitObjectsPlacementAndSize(double radius)
        {
            double playfieldScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);

            for (int i = 0; i < playfieldCanva.Children.Count; i++)
            {
                // need FrameworkElement for widht and height values cos UiElement doesnt have it
                FrameworkElement child = (FrameworkElement)playfieldCanva.Children[i];
                // https://osu.ppy.sh/wiki/en/Client/Playfield
               HitObject hitObject = map.HitObjects[i];
               int baseHitObjectX = hitObject.X;
               int baseHitObjectY = hitObject.Y;

                //int baseHitObjectX;
                //int baseHitObjectY;
                //if (i == 0)
                //{
                //    baseHitObjectX = 512;
                //    baseHitObjectY = 0;
                //}
                //else
                //{
                //    baseHitObjectX = 0;
                //    baseHitObjectY = 384;
                //}

                Debug.WriteLine("X : " + baseHitObjectX * playfieldScale);
                Debug.WriteLine("Y : " + baseHitObjectY * playfieldScale);
                

                child.Width = radius;
                child.Height = radius;
                Canvas.SetLeft(child, (baseHitObjectX * playfieldScale) - radius);
                Canvas.SetTop(child, (baseHitObjectY * playfieldScale) - radius);

                Debug.WriteLine("CanvaLeft : " + Canvas.GetLeft(child));
                Debug.WriteLine("CanvaTop : " + Canvas.GetTop(child));
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
                Dispatcher.Invoke(() => BeatmapCircleRenderer());
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