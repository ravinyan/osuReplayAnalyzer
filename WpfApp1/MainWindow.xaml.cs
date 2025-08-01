using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using ReplayParsers.Decoders;
using System.Diagnostics;
using System.IO;
using System.Numerics;
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

        public static List<Canvas> AliveCanvasObjects = new List<Canvas>();

        Stopwatch stopwatch = new Stopwatch();
        int HitObjectIndex = 0;

        DispatcherTimer timer2 = new DispatcherTimer();
        OsuMath math = new OsuMath();

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
                songSlider.Value = timeElapsed;
            }
        }

        int comboNumba = 1;
        void HandleVisibleCircles()
        {
            //fpsCounter.Text = GC.GetTotalMemory(true).ToString("#,###");
            fpsCounter.Text = songSlider.Value.ToString();
            fpsCounter2.Text = musicPlayer.Position.TotalMilliseconds.ToString();

            if (HitObjectIndex < map.HitObjects.Count)
            {
                HitObject hitObject = map.HitObjects[HitObjectIndex];

                if (timeElapsed > hitObject.SpawnTime - AnimationTiming)
                {
                    if (hitObject.Type.HasFlag(ObjectType.StartNewCombo))
                    {
                        comboNumba = 1;
                    }

                    double osuScale = Math.Min(playfieldCanva.Height / (384), playfieldCanva.Width / 512);
                    double radius = ((54.4 - 4.48 * (double)map.Difficulty.CircleSize) * osuScale) * 2;

                    Canvas c;
                    if (hitObject is Circle)
                    {
                        c = HitCircle.CreateCircle(hitObject, radius, comboNumba, osuScale, HitObjectIndex);
                        playfieldCanva.Children.Add(c);
                    }
                    else if (hitObject is Slider)
                    {
                        c = SliderObject.CreateSlider((Slider)hitObject, radius, comboNumba, osuScale, HitObjectIndex);
                        playfieldCanva.Children.Add(c);
                    }
                    else // spin
                    {
                        c = HitCircle.CreateCircle(hitObject, radius, comboNumba, osuScale, HitObjectIndex);
                        playfieldCanva.Children.Add(c);
                    }

                    HitCircleAnimation.Start(c);
                    AliveCanvasObjects.Add(c);

                    HitObjectIndex++;
                    comboNumba++;
                }
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
                    playfieldCanva.Children.Remove(obj);
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
            musicPlayer.Volume = 0.00;
            await musicPlayer.Open(new Uri($@"{AppDomain.CurrentDomain.BaseDirectory}\osu\Audio\audio.mp3"));

            playfieldBackground.ImageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Background\\bg.jpg"));
            musicPlayerVolume.Text = $"{musicPlayer.Volume * 100}%";
        }

        void MusicPlayer_MediaOpened(object sender, Unosquare.FFME.Common.MediaOpenedEventArgs e)
        {
            if (musicPlayer.NaturalDuration.HasValue)
            {
                songMaxTimer.Text = musicPlayer.NaturalDuration.ToString()!.Substring(0, 12);
                songSlider.Maximum = musicPlayer.NaturalDuration.Value.TotalMilliseconds;
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
            songTimer.Text = TimeSpan.FromMilliseconds(songSlider.Value).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
        }

        void TimerTick2(object sender, EventArgs e)
        {
            GameplayClockTest();
        }

        // BIG TODO FIX THIS THIS TIMING GETS OFF TIME
        // SOMEHOW USE GAMEPLAY CLOCK OR IDK WHAT TO VALIDATE
        // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
        // pausing causing this
        void PlayPauseButton(object sender, RoutedEventArgs e)
        {
            if (playerButton.Style == FindResource("PlayButton"))
            {
                musicPlayer.Play();
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
                musicPlayer.Pause();
                stopwatch.Stop();
                playerButton.Style = Resources["PlayButton"] as Style;
                
                foreach (Canvas o in AliveCanvasObjects)
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
                Tetoris();
                //TetorisSO();
                //TetorisCO();
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
            string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Raphlesia & BilliumMoto - My Love (Mao) [Our Love] (2023-12-09_23-55).osr";
            replay = ReplayDecoder.GetReplayData(file);
            map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

            Stacking stacking = new Stacking();
            //stacking.ApplyStacking(map);

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
    }
}

