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
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using Beatmap = ReplayParsers.Classes.Beatmap.osu.Beatmap;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

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
            Color comboColor = Color.FromArgb(220, 24, 214);

            Image hitCircle = ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle@2x.png"), comboColor);

            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png")),
            };

            container.Children.Add(hitCircle);
            container.Children.Add(hitCircleBorder2);
            
            Canvas.SetLeft(container, circle.X);
            Canvas.SetTop(container, circle.Y);
            playfieldCanva.Children.Add(container);
        }

        // maybe replace set/get pixels since they are apparently bad... but for now use them coz i just need to test
        // https://stackoverflow.com/questions/39692914/convert-grayscale-partially-transparent-image-to-a-single-color-in-c-sharp
        // yep way better
        Image ApplyComboColourToHitObject(Bitmap hitObject, Color comboColor)
        {
            /*
            double opacity = 0;
            //for (int i = 0; i < hitObject.Height; i++)
            //{
            //    for (int j = 0; j < hitObject.Width; j++)
            //    {
            //        Color pixelColor = hitObject.GetPixel(i, j);
            //
            //        byte alpha = (byte)(pixelColor.A);
            //        byte newRed = (byte)(comboColor.R);
            //        byte newGreen = (byte)(comboColor.G);
            //        byte newBlue = (byte)(comboColor.B);
            //
            //        Color newPixelColor;
            //        if (alpha == 0)
            //        {
            //            newPixelColor = Color.FromArgb(0, newRed, newGreen, newBlue);
            //        }
            //        else
            //        {
            //            newPixelColor = Color.FromArgb(255, newRed, newGreen, newBlue);
            //        }
            //
            //            hitObject.SetPixel(i, j, newPixelColor);
            //    }
            //}
            */
            
            Graphics g = Graphics.FromImage(hitObject);
            
            ColorMatrix colorMatrix = new ColorMatrix(
            new float[][]
            {
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 0, 0},
                new float[] {0, 0, 0, 1, 0},
                new float[] {comboColor.R / 255f, comboColor.G / 255f, comboColor.B / 255f, 0, 1} 
            });

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(colorMatrix);
            //attributes.SetThreshold(0);
            
            g.DrawImage(hitObject,
                new System.Drawing.Rectangle(0, 0, hitObject.Width, hitObject.Height),
                0, 0, hitObject.Width, hitObject.Height,
                GraphicsUnit.Pixel, attributes);

            g.Dispose();

            IntPtr hBitmap = hitObject.GetHbitmap();
            BitmapSource recoloredImage = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            Image recoloredHitObject = new Image();
            recoloredHitObject.Source = recoloredImage;
            //recoloredHitObject.Opacity = 0.2;
            return recoloredHitObject;
        }

        void CreateSlider(double radius)
        {

        }

        void CreateSpinner(double radius)
        {
            // uhh is radius needed? idk
        }

        void loaded(object sender, RoutedEventArgs e)
        {
            const double AspectRatio = 1.25;
            double height = playfieldCanva.Height / AspectRatio;
            double width = playfieldCanva.Width / AspectRatio;
            double osuScale = Math.Min(height / 384, width / 512);
            double radius = (54.4 - 4.48 * 1.0) * osuScale;

            Grid hitObject = new Grid();
            hitObject.Width = radius;
            hitObject.Height = radius;

            Color comboColor = Color.FromArgb(220, 24, 214);



            var tet = new Bitmap("C:\\Users\\patry\\Documents\\ShareX\\Screenshots\\2025-07\\Discord_QsCWZp2QD8.png");

            var coloooooor = tet.GetPixel(1,1);
            

            Image hitCircle = ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle@2x.png"), comboColor);
       
            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png")),
            };

            hitObject.Children.Add(hitCircle);
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

            double circleSize = 1;
            double osuScale = Math.Min(height / 384, width / 512);
            double radius = ((54.4) - (4.48) * 0) * osuScale;

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
                FrameworkElement circle = (FrameworkElement)playfieldCanva.Children[i];
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

                int childrenCount = VisualTreeHelper.GetChildrenCount(circle);
                for (int j = 0; j < childrenCount; j++)
                {
                    Image? c = VisualTreeHelper.GetChild(circle, j) as Image;

                    c.Width = radius;
                    c.Height = radius;
                }

                circle.Width = radius;
                circle.Height = radius;

                Canvas.SetTop(circle, (baseHitObjectY * playfieldScale) - (radius / 2));  
                Canvas.SetLeft(circle, (baseHitObjectX * playfieldScale) - (radius / 2));
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

            playfieldBackground.Opacity = volumeSlider.Value / 100;

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