using LibVLCSharp.Shared;
using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using ReplayParsers.Decoders;
using System.Diagnostics;
using System.IO;
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

        public static List<Canvas> AliveCanvasObjects = new List<Canvas>();

        Stopwatch stopwatch = new Stopwatch();
        int HitObjectIndex = 0;

        DispatcherTimer timer2 = new DispatcherTimer();
        DispatcherTimer timer1 = new DispatcherTimer();
        OsuMath math = new OsuMath();


        LibVLC LibVLC;
        public MainWindow()
        {
            InitializeComponent();
            LibVLC = new LibVLC();

            playfieldBackground.Opacity = 0.1;

            timer2.Interval = TimeSpan.FromMilliseconds(1);
            timer2.Tick += TimerTick2;

            timer1.Interval = TimeSpan.FromMilliseconds(16);
            timer1.Tick += TimerTick1;

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

            if (isDragged == false)
            {
                songSlider.Value = musicPlayer.MediaPlayer.Time;
                
            }
        }

        int comboNumba = 1;
        int objectIndex = 0;
        void HandleVisibleCircles()
        {
            //fpsCounter.Text = GC.GetTotalMemory(true).ToString("#,###");
            //fpsCounter.Text = songSlider.Value.ToString();
            //fpsCounter2.Text = musicPlayer.MediaPlayer.Time.ToString();

            

            var hitObject = playfieldCanva.Children[HitObjectIndex] as Canvas;
            var hitObjectProperties = (HitObject)hitObject.DataContext;
            if (timeElapsed > hitObjectProperties.SpawnTime - AnimationTiming)
            {
                hitObject.Visibility = Visibility.Visible;
                HitCircleAnimation.Start(hitObject);
                AliveCanvasObjects.Add(hitObject);
            
                HitObjectIndex++;
            }

            for (int i = 0; i < AliveCanvasObjects.Count; i++)
            {
                Canvas obj = AliveCanvasObjects[i];
                HitObject ep = (HitObject)obj.DataContext;

                double timeToDelete;
                if (ep is Circle)
                {
                    timeToDelete = ep.SpawnTime;
                }
                else if (ep is Slider)
                {
                    Slider s = (Slider)ep;
                    timeToDelete = math.GetSliderEndTime(s, map.Difficulty.SliderMultiplier);
                }
                else
                {
                    Spinner s = (Spinner)ep;
                    timeToDelete = s.EndTime;
                }

                if (timeElapsed > timeToDelete)
                {
                    HitCircleAnimation.RemoveStoryboard(obj);
                    //playfieldCanva.Children.Remove(obj);
                    obj.Visibility = Visibility.Collapsed;
                    AliveCanvasObjects.Remove(obj);
                    obj = null;
                }
            }
        }

        void PlayfieldSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizePlayfield.ResizePlayfieldCanva(e, playfieldCanva, playfieldBorder, AliveCanvasObjects);
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

                if (map.HitObjects[i] is Circle)
                {
                    Canvas circle = HitCircle.CreateCircle(map.HitObjects[i], radius, comboNumber, osuScale, i);
                    playfieldCanva.Children.Add(circle);
                }
                else if (map.HitObjects[i] is Slider)
                {
                    Canvas slider = SliderObject.CreateSlider((Slider)map.HitObjects[i], radius, comboNumber, osuScale, i);
                    playfieldCanva.Children.Add(slider);
                }
                else if (map.HitObjects[i] is Spinner)
                {
                    Canvas circle = HitCircle.CreateCircle(map.HitObjects[i], radius, comboNumber, osuScale, i);
                    playfieldCanva.Children.Add(circle);
                }

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
                if (Dispatcher.Invoke(() => musicPlayer.MediaPlayer.Media) != null)
                {
                    Dispatcher.Invoke(() => musicPlayer.MediaPlayer.Stop());
                    Dispatcher.Invoke(() => playfieldBackground.ImageSource = null);
                    Thread.Sleep(2000);
                }

                string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\{e.Name!.Substring(1, e.Name.Length - 38)}";
                map = BeatmapDecoder.GetOsuLazerBeatmap(file);

                Dispatcher.Invoke(() => InitializeMusicPlayer());
                Dispatcher.Invoke(() => BeatmapObjectRenderer());
            }
        }

        private void InitializeMusicPlayer()
        {
            musicPlayer.MediaPlayer = new LibVLCSharp.Shared.MediaPlayer(LibVLC);
            musicPlayer.MediaPlayer.Media = new Media(LibVLC, $@"{AppDomain.CurrentDomain.BaseDirectory}\osu\Audio\audio.mp3");
            musicPlayer.MediaPlayer.Media.Parse();
            musicPlayer.MediaPlayer.Volume = 0;

            playfieldBackground.ImageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Background\\bg.jpg"));
            musicPlayerVolume.Text = $"{musicPlayer.MediaPlayer.Volume}%";

            while (musicPlayer.MediaPlayer.Media.ParsedStatus != MediaParsedStatus.Done) 
            {
                Thread.Sleep(10);
            }

            songMaxTimer.Text = TimeSpan.FromMilliseconds(musicPlayer.MediaPlayer.Media.Duration).ToString();
            songSlider.Maximum = musicPlayer.MediaPlayer.Media.Duration;
        }

        void MusicPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            //if (musicPlayer.NaturalDuration.HasValue)
            //{
            
            //}
        }

        void SongSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (musicPlayer.MediaPlayer != null)
            {
                //musicPlayer.MediaPlayer.Position = (float)( / musicPlayer.MediaPlayer.Length);
                musicPlayer.MediaPlayer.Time = (long)songSlider.Value;
                songSlider.Value = (long)songSlider.Value;
                isDragged = false;
            }
        }

        void SongSliderDragStarted(object sender, DragStartedEventArgs e)
        {
            isDragged = true;
        }

        void SongSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            songTimer.Text = TimeSpan.FromMilliseconds(songSlider.Value).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
        }

        void TimerTick2(object sender, EventArgs e)
        {
            GameplayClockTest();

            
        }

        void TimerTick1(object sender, EventArgs e)
        {
            //GameplayClockTest();
            fpsCounter.Text = stopwatch.ElapsedMilliseconds.ToString();
            fpsCounter2.Text = musicPlayer.MediaPlayer.Time.ToString();
            songTimer.Text = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString();


        }

        // BIG TODO FIX THIS THIS TIMING GETS OFF TIME
        // SOMEHOW USE GAMEPLAY CLOCK OR IDK WHAT TO VALIDATE
        // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
        // pausing causing this
        void PlayPauseButton(object sender, RoutedEventArgs e)
        {
            if (musicPlayer.MediaPlayer != null)
            {
                if (playerButton.Style == FindResource("PlayButton"))
                {
                    musicPlayer.MediaPlayer.Play();
                    stopwatch.Start();
                    playerButton.Style = Resources["PauseButton"] as Style;

                    foreach (Canvas o in AliveCanvasObjects)
                    {
                        HitCircleAnimation.Resume(o);
                    }
                }
                else
                {
                    //musicPlayer.Position = stopwatch.Elapsed;
                    musicPlayer.MediaPlayer.Pause();
                    stopwatch.Stop();

                    // this one line just correct very small offset when pausing...
                    // from testing it doesnt cause any audio problems or any delay anymore so yaaay
                    musicPlayer.MediaPlayer.Time = stopwatch.ElapsedMilliseconds;
                    playerButton.Style = Resources["PlayButton"] as Style;

                    foreach (Canvas o in AliveCanvasObjects)
                    {
                        HitCircleAnimation.Pause(o);
                    }
                }
            }
        }

        void VolumeSliderValue(object sender, RoutedEventArgs e)
        {
            if (musicPlayerVolume != null && musicPlayer.MediaPlayer != null)
            {
                musicPlayerVolume.Text = $"{volumeSlider.Value}%";
                musicPlayer.MediaPlayer.Volume = (int)volumeSlider.Value;

                if (musicPlayer.MediaPlayer.Volume == 0)
                {
                    volumeIcon.Data = Geometry.Parse("m5 7 4.146-4.146a.5.5 0 0 1 .854.353v13.586a.5.5 0 0 1-.854.353L5 13H4a2 2 0 0 1-2-2V9a2 2 0 0 1 2-2h1zm7 1.414L13.414 7l1.623 1.623L16.66 7l1.414 1.414-1.623 1.623 1.623 1.623-1.414 1.414-1.623-1.623-1.623 1.623L12 11.66l1.623-1.623L12 8.414z");
                }
                else if (musicPlayer.MediaPlayer.Volume > 0 && musicPlayer.MediaPlayer.Volume < 50)
                {
                    volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z");
                }
                else if (musicPlayer.MediaPlayer.Volume >= 50)
                {
                    volumeIcon.Data = Geometry.Parse("M9.146 2.853 5 7H4a2 2 0 0 0-2 2v2a2 2 0 0 0 2 2h1l4.146 4.146a.5.5 0 0 0 .854-.353V3.207a.5.5 0 0 0-.854-.353zM12 8a2 2 0 1 1 0 4V8z M12 6a4 4 0 0 1 0 8v2a6 6 0 0 0 0-12v2z");
                }
            }
        }

        void LoadTestBeatmap(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T)
            {
                Tetoris();
                //TetorisSO();
                //TetorisCO();
                timer2.Start();
                timer1.Start();
            }
        }

        void TetorisCO()
        {
            string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-04-02_17-15) (65).osr";
            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Aqours - Songs Compilation (Sakurauchi Riko) [Sweet Sparkling Sunshine!!] (2024-07-21_03-49).osr";
            replay = ReplayDecoder.GetReplayData(file);
            map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

            Stacking stacking = new Stacking();
            stacking.ApplyStacking(map);

            Dispatcher.Invoke(() => InitializeMusicPlayer());
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

            Stacking stacking = new Stacking();
            stacking.ApplyStacking(map);

            Dispatcher.Invoke(() => InitializeMusicPlayer());
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

        void Tetoris()
        {
            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Extra] (2025-03-26_21-18).osr";
            string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Aqours - Songs Compilation (Sakurauchi Riko) [Sweet Sparkling Sunshine!!] (2024-07-21_03-49).osr";
            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Raphlesia & BilliumMoto - My Love (Mao) [Our Love] (2023-12-09_23-55).osr";
            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Lorien Testard - Une vie a t'aimer (Iced Out) [Stop loving me      I will always love you] (2025-08-06_19-33).osr";

            replay = ReplayDecoder.GetReplayData(file);
            map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

            Stacking stacking = new Stacking();
            //stacking.ApplyStacking(map);

            Dispatcher.Invoke(() => InitializeMusicPlayer());
            BeatmapObjectRenderer();
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
    }
}

