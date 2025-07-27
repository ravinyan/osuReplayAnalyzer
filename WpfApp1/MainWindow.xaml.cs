using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using ReplayParsers.Decoders;
using System.Diagnostics;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.Objects;
using WpfApp1.OsuMaths;
using WpfApp1.Playfield;
using WpfApp1.Skinning;
using Beatmap = ReplayParsers.Classes.Beatmap.osu.Beatmap;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
using Spinner = ReplayParsers.Classes.Beatmap.osu.Objects.Spinner;

#nullable disable
// https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Beatmap? map;
        public static Replay? replay;
        FileSystemWatcher watcher = new FileSystemWatcher();
        string skinPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuFileParser\\WpfApp1\\Skins\\Komori - PeguLian II (PwV)";
        private bool isDragged = false;

        decimal AnimationTiming = 0;
        decimal FadeIn = 0;

        public static List<FrameworkElement> AliveCanvasObjects = new List<FrameworkElement>();

        Stopwatch stopwatch = new Stopwatch();
        int HitObjectIndex = 0;

        DispatcherTimer timer2 = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            playfieldBackground.Opacity = 0.1;
            
            timer2.Interval = TimeSpan.FromMilliseconds(1);
            timer2.Tick += TimerTick2!;

            KeyDown += LoadTestBeatmap;
            //GetReplayFile();
            //InitializeMusicPlayer();
            //playfieldCanva.Loaded += loaded;

        }

        long last = 0;
        int deltaTime = 1000 / 60;
        long timeElapsed = 0;
        void GameplayClockTest()
        {
            long now = stopwatch.ElapsedMilliseconds;
            long passed = now - last;
            last = now;
            timeElapsed += passed;

            HandleVisibleCircles();

            if (musicPlayer.Source != null && musicPlayer.NaturalDuration.HasValue && isDragged == false)
            {
                //songSlider.Value = musicPlayer.Position.TotalMilliseconds;
                songSlider.Value = timeElapsed;
            }
        }

        int comboNumba = 1;
        // timing overitme is breaking (mostly stacked circles like triples) and it MIGHT be
        // due to clock being a bit faster than music player? dont know
        // from what i can see the MIDDLE circle in triples is borked
        // ^ nvm this was me ignoring rendering spinner (or in fact ANY object) and it caused circle spawn timing to get weird
        // rendered circle instead of spinner and it worked perfectly lol i love programming
        void HandleVisibleCircles()
        {
            /*
            if (stopwatch.ElapsedMilliseconds == (long)songSlider.Maximum)
            {
                // as predicted it will not be perfect and it probably was never supposed to be anyway
                fpsCounter.Text = timeElapsed.ToString() + "HOLY PIZZA"; 
            }
            else
            {
                fpsCounter.Text = timeElapsed.ToString();
            }

            if (stopwatch.ElapsedMilliseconds >= (long)songSlider.Maximum)
            {
                stopwatch.Stop();

            }
            */

            //fpsCounter.Text = GC.GetTotalMemory(true).ToString("#,###");
            fpsCounter.Text = timeElapsed.ToString();

            //const double AspectRatio = 1.33;
            //double height = playfieldCanva.Height / AspectRatio;
            //double width = playfieldCanva.Width / AspectRatio;
            //double osuScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);
            //double radius = (double)((54.4 - 4.48 * (double)map.Difficulty.CircleSize) * osuScale) * 2;



            if (HitObjectIndex < map.HitObjects.Count)
            {
                HitObject circle = map.HitObjects[HitObjectIndex];

                if (timeElapsed > circle.SpawnTime - AnimationTiming)
                {
                    if (circle.Type.HasFlag(ObjectType.StartNewCombo))
                    {
                        comboNumba = 1;
                    }

                    const double AspectRatio = 1.33;

                    double osuScale = Math.Min(playfieldCanva.Height / (384), playfieldCanva.Width / 512);
                    double radius = ((54.4 - 4.48 * (double)map.Difficulty.CircleSize) * osuScale) * 2;

                    Grid c = HitCircle.CreateCircle(circle, radius, comboNumba, osuScale);
                    playfieldCanva.Children.Add(c);
                    AliveCanvasObjects.Add(c);

                    HitObjectIndex++;
                    comboNumba++;
                }

                for (int i = 0; i < AliveCanvasObjects.Count; i++) 
                {
                    FrameworkElement obj = AliveCanvasObjects[i];
                    HitObject ep = (HitObject)obj.DataContext;

                    if (timeElapsed > ep.SpawnTime)
                    {
                        HitCircleAnimation.RemoveStoryboard((Grid)obj);
                        playfieldCanva.Children.Remove(obj);
                        AliveCanvasObjects.Remove(obj);
                        obj = null;
                    }
                }
            } 
        }

        void PlayfieldSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizePlayfield.ResizePlayfieldCanva(e, playfieldCanva, playfieldBorder, AliveCanvasObjects);
        }

        void loaded(object sender, RoutedEventArgs e)
        {
            const double AspectRatio = 1.33;
            double height = playfieldCanva.Height / AspectRatio;
            double width = playfieldCanva.Width / AspectRatio;
            double osuScale = Math.Min(height / 384, width / 512);
            double radius = (54.4 - 4.48 * 1.0) * osuScale;

            Color comboColor = Color.FromArgb(220, 24, 214);

            for (int i = 0; i < 3; i ++)
            {
                Grid hitObject = new Grid();
                hitObject.Width = radius;
                hitObject.Height = radius;



                Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle@2x.png"), comboColor);

                Image hitCircleBorder2 = new Image()
                {
                    Width = radius,
                    Height = radius,
                    Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png")),
                };

                Image hitCircleNumber = new Image()
                {
                    Height = (radius / 2) * 0.8,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-7.png")),
                    Name = "ComboNumber"
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-2.png")),
                    Name = "ComboNumber"
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = (radius / 2) * 0.7,
                    Source = new BitmapImage(new Uri($"{skinPath}\\default-7.png")),
                    Name = "ComboNumber"
                };

                StackPanel numberPanel = new StackPanel();
                numberPanel.HorizontalAlignment = HorizontalAlignment.Center;
                numberPanel.Orientation = Orientation.Horizontal;

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
                numberPanel.Children.Add(hitCircleNumber3);

                hitObject.Children.Add(hitCircle);
                hitObject.Children.Add(hitCircleBorder2);
                hitObject.Children.Add(numberPanel);

                Canvas.SetLeft(hitObject, 512);
                Canvas.SetTop(hitObject, 384);
                playfieldCanva.Children.Add(hitObject);
            }
        }

        // ok i almost killed my laptop by trying to render 6k+ objects... dont use
        void BeatmapObjectRenderer()
        {
            const double AspectRatio = 1.25;
            double height = playfieldCanva.Height / AspectRatio;
            double width = playfieldCanva.Width / AspectRatio;
            double osuScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);
            double radius = (double)((54.4 - 4.48 * (double)map.Difficulty.CircleSize) * osuScale) * 2;

            int comboNumber = 1;

            Stacking stacking = new Stacking();
            stacking.ApplyStacking(map);

            //https://learn.microsoft.com/en-us/previous-versions/windows/silverlight/dotnet-windows-silverlight/cc190397(v=vs.95)
            for (int i = 0; i < map.HitObjects.Count; i++) 
            {
                if (map.HitObjects[i].Type.HasFlag(ObjectType.StartNewCombo))
                {
                    comboNumber = 1;
                }
            
                //if (map.HitObjects[i] is Circle)
                //{
                //    Grid circle = HitCircle.CreateCircle(map.HitObjects[i], radius, comboNumber);
                //    playfieldCanva.Children.Add(circle);
                //
                //}
                //else if (map.HitObjects[i] is Slider)
                //{
                //    Grid circle = HitCircle.CreateCircle(map.HitObjects[i], radius, comboNumber);
                //    playfieldCanva.Children.Add(circle);
                //}
                //else if (map.HitObjects[i] is Spinner)
                //{
                //    Grid circle = HitCircle.CreateCircle(map.HitObjects[i], radius, comboNumber);
                //    playfieldCanva.Children.Add(circle);
                //}
            
                comboNumber++;
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
                if (Dispatcher.Invoke(() => musicPlayer.Source) != null)
                {
                    Dispatcher.Invoke(() => musicPlayer.Close());
                    Dispatcher.Invoke(() => playfieldBackground.ImageSource = null);
                    Thread.Sleep(2000);
                }
        
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
                songSlider.Maximum = musicPlayer.NaturalDuration.Value.TotalMilliseconds;
                fpsCounter2.Text = $"{(long)songSlider.Maximum}";
            }
        }
        
        void SongSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            musicPlayer.Position = TimeSpan.FromMilliseconds(songSlider.Value);
            isDragged = false;
        }

        void SongSliderDragStarted(object sender, DragStartedEventArgs e)
        {
            isDragged = true;
        }
        
        void SongSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            songTimer.Text = TimeSpan.FromMilliseconds(songSlider.Value).ToString(@"hh\:mm\:ss");
        }
        
        // BIG TODO FIX THIS THIS TIMING GETS OFF TIME
        // SOMEHOW USE GAMEPLAY CLOCK OR IDK WHAT TO VALIDATE
        // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
        // pausing causing this
        void TimerTick(object sender, EventArgs e)
        {
            if (musicPlayer.Source != null && musicPlayer.NaturalDuration.HasValue && isDragged == false)
            {
                //songSlider.Value = musicPlayer.Position.TotalMilliseconds;
                songSlider.Value = timeElapsed;
            }

        }

        void TimerTick2(object sender, EventArgs e)
        {
            GameplayClockTest();
        }

        void PlayPauseButton(object sender, RoutedEventArgs e)
        {
            if (playerButton.Style == FindResource("PlayButton"))
            {
                playerButton.Style = Resources["PauseButton"] as Style;
                musicPlayer.Play();
                stopwatch.Start();

                foreach (Grid o in AliveCanvasObjects)
                {
                    HitCircleAnimation.Resume(o);
                }
            }
            else
            {
                playerButton.Style = Resources["PlayButton"] as Style;
                musicPlayer.Pause();
                stopwatch.Stop();

                foreach (Grid o in AliveCanvasObjects)
                {
                    HitCircleAnimation.Pause(o);
                }
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

        void LoadTestBeatmap(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T)
            {
                //TetorisSO();
                TetorisCO();
                timer2.Start();
            }
        }

        // changing how things work
        void TetorisCO()
        {
            string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-04-02_17-15) (65).osr";
            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Aqours - Songs Compilation (Sakurauchi Riko) [Sweet Sparkling Sunshine!!] (2024-07-21_03-49).osr";
            replay = ReplayDecoder.GetReplayData(file);
            map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

            Stacking stacking = new Stacking();
            stacking.ApplyStacking(map);

            Dispatcher.Invoke(() => InitializeMusicPlayer());
            //Dispatcher.Invoke(() => BeatmapObjectRenderer());

            SizeChanged += PlayfieldSizeChanged;

            if (map.Difficulty.ApproachRate < 5)
            {
                AnimationTiming = Math.Ceiling(1200 + 600 * (5 - map.Difficulty.ApproachRate) / 5);
                FadeIn = Math.Ceiling(800 + 400 * (5 - map.Difficulty.ApproachRate) / 5);
            }
            else if (map.Difficulty.ApproachRate == 5)
            {
                AnimationTiming = 1200;
                FadeIn = 800;
            }
            else if (map.Difficulty.ApproachRate > 5)
            {
                AnimationTiming = Math.Ceiling(1200 - 750 * (map.Difficulty.ApproachRate - 5) / 5);
                FadeIn = Math.Ceiling(800 - 500 * (map.Difficulty.ApproachRate - 5) / 5);
            }
        }

        void TetorisSO()
        {
            string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Kensuke x Ascended_s EX] (2025-03-22_12-46) (1).osr";
            replay = ReplayDecoder.GetReplayData(file);
            map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

            Dispatcher.Invoke(() => InitializeMusicPlayer());
            Dispatcher.Invoke(() => BeatmapObjectRenderer());
        }

        void Tetoris()
        {
            string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Extra] (2025-03-26_21-18).osr";
            map = BeatmapDecoder.GetOsuLazerBeatmap(file);

            Dispatcher.Invoke(() => InitializeMusicPlayer());
            Dispatcher.Invoke(() => BeatmapObjectRenderer());
        }
    }
}

