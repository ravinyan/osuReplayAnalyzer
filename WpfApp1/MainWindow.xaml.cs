using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using ReplayParsers.Decoders;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.MusicPlayer.Controls;
using WpfApp1.OsuMaths;
using WpfApp1.Playfield;
using Beatmap = ReplayParsers.Classes.Beatmap.osu.Beatmap;
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

        decimal AnimationTiming = 0;
        decimal FadeIn = 0;

        public List<Canvas> AliveCanvasObjects = new List<Canvas>();

        public Stopwatch stopwatch = new Stopwatch();
        int HitObjectIndex = 0;

        DispatcherTimer timer2 = new DispatcherTimer();
        DispatcherTimer timer1 = new DispatcherTimer();
        OsuMath math = new OsuMath();

        public MainWindow()
        {
            InitializeComponent();

            playfieldBackground.Opacity = 0.1;

            timer2.Interval = TimeSpan.FromMilliseconds(1);
            timer2.Tick += TimerTick2;

            timer1.Interval = TimeSpan.FromMilliseconds(16);
            timer1.Tick += TimerTick1;

            KeyDown += LoadTestBeatmap;
            //GetReplayFile();
            //InitializeMusicPlayer();
        }

        long last = 0;
        long timeElapsed = 0;
        void GameplayClockTest()
        {
            long now = stopwatch.ElapsedMilliseconds;
            long passed = now - last;
            last = now;
            timeElapsed += passed;

            HandleVisibleCircles();

            if (SongSliderControls.IsDragged == false)
            {
                songSlider.Value = musicPlayer.MediaPlayer.Time;
            }
        }


        Canvas hitObject = null;
        HitObject hitObjectProperties = null;
        void HandleVisibleCircles()
        {
            if (hitObject != playfieldCanva.Children[HitObjectIndex] as Canvas)
            {
                hitObject = playfieldCanva.Children[HitObjectIndex] as Canvas;
                hitObjectProperties = (HitObject)hitObject.DataContext;
            }

            if (timeElapsed > hitObjectProperties.SpawnTime - AnimationTiming 
            &&  HitObjectIndex <= map.HitObjects.Count)
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

        void TimerTick2(object sender, EventArgs e)
        {
            GameplayClockTest();
        }

        void TimerTick1(object sender, EventArgs e)
        {
            fpsCounter.Text = stopwatch.ElapsedMilliseconds.ToString();
            fpsCounter2.Text = musicPlayer.MediaPlayer.Time.ToString();
            songTimer.Text = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
        }

        void LoadTestBeatmap(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T)
            {
                Tetoris();
                timer2.Start();
                timer1.Start();
            }
        }

        void Tetoris()
        {
            /*circle only*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-04-02_17-15) (65).osr";
            /*slider only*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Kensuke x Ascended_s EX] (2025-03-22_12-46) (1).osr";
            /*mixed only*/            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Extra] (2025-03-26_21-18).osr";
            /*mega marathon*/         //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Aqours - Songs Compilation (Sakurauchi Riko) [Sweet Sparkling Sunshine!!] (2024-07-21_03-49).osr";
            /*olibomby sliders/tech*/ //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Raphlesia & BilliumMoto - My Love (Mao) [Our Love] (2023-12-09_23-55).osr";
            /*marathon*/              string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Lorien Testard - Une vie a t'aimer (Iced Out) [Stop loving me      I will always love you] (2025-08-06_19-33).osr";

            replay = ReplayDecoder.GetReplayData(file);
            map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

            Stacking stacking = new Stacking();
            stacking.ApplyStacking(map);

            MusicPlayer.MusicPlayer.InitializeMusicPlayer();
            Beatmaps.OsuBeatmap.Create();

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

