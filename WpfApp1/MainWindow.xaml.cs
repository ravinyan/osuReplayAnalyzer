using ReplayParsers.Classes.Replay;
using ReplayParsers.Decoders;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.MusicPlayer.Controls;
using WpfApp1.OsuMaths;
using WpfApp1.Playfield;
using Beatmap = ReplayParsers.Classes.Beatmap.osu.Beatmap;

#nullable disable
// https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/

// todo some other time when rendering objects only make 1 circle object without combo numbers and
// then copy that object and add combo numbers... dont know if it will be better or not just curious
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Beatmap? map;
        public static Replay? replay;

        DispatcherTimer timer1 = new DispatcherTimer();

        OsuMath math = new OsuMath();

        //System.Timers.Timer timer2 = new System.Timers.Timer();
        //timer2.Interval = .001;
        //timer2.Elapsed += TimerTick2;

        public MainWindow()
        {
            InitializeComponent();

            playfieldBackground.Opacity = 0.1;

            GamePlayClock.Initialize();

            timer1.Interval = TimeSpan.FromMilliseconds(1);
            timer1.Tick += TimerTick1;

            KeyDown += LoadTestBeatmap;
            //GetReplayFile();
            //InitializeMusicPlayer();
        }

        private void animationTest_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation aaa = new DoubleAnimation(1.0, 0, TimeSpan.FromMilliseconds(math.GetFadeInTiming((decimal)1)));
            aaa.FillBehavior = FillBehavior.Stop;
            animationTest.BeginAnimation(Button.OpacityProperty, aaa);
            aaa.Completed += Aaa_Completed;
        }

        private void Aaa_Completed(object sender, EventArgs e)
        {
            animationTest.Opacity = 1.0;

        }

        void PlayfieldSizeChanged(object sender, SizeChangedEventArgs e)
        {
           ResizePlayfield.ResizePlayfieldCanva(playfieldCanva, playfieldBorder);
        }

        void TimerTick2(object sender, EventArgs e)
        {
            
        }

        void TimerTick1(object sender, EventArgs e)
        {
            fpsCounter.Text = GamePlayClock.GetElapsedTime().ToString();

            // ok this work for now do not touch until problems
            Dispatcher.InvokeAsync(() =>
            {
                Playfield.Playfield.HandleVisibleCircles();
                songTimer.Text = TimeSpan.FromMilliseconds(GamePlayClock.GetElapsedTime()).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            }, DispatcherPriority.Render);

            Dispatcher.InvokeAsync(() =>
            {
                if (SongSliderControls.IsDragged == false)
                {
                    songSlider.Value = musicPlayer.MediaPlayer!.Time;
                }

                if (GamePlayClock.IsPaused())
                {
                    foreach (Canvas o in Playfield.Playfield.GetAliveHitObjects())
                    {
                        HitObjectAnimations.Pause(o);
                    }
                }
                else
                {
                    foreach (Canvas o in Playfield.Playfield.GetAliveHitObjects())
                    {
                        HitObjectAnimations.Resume(o);
                    }
                }
            }, DispatcherPriority.SystemIdle);
        }

        void LoadTestBeatmap(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Tetoris();
                    //timer2.Start();
                    timer1.Start();
                });
            }
        }

        void Tetoris()
        {
            // i hate how i memorized the memory consumption of every file here after being rendered as beatmap
            // not rendering slider tail circle (which is ugly anyway and like 10 people use it) saves 400mb ram!
            // on marathon map and almost 1gb on mega marathon
            /*circle only*/           string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-04-02_17-15) (65).osr";
            /*slider only*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Kensuke x Ascended_s EX] (2025-03-22_12-46) (1).osr";
            /*mixed*/                 //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Extra] (2025-03-26_21-18).osr";
            /*mega marathon*/         //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Aqours - Songs Compilation (Sakurauchi Riko) [Sweet Sparkling Sunshine!!] (2024-07-21_03-49).osr";
            /*olibomby sliders/tech*/ //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Raphlesia & BilliumMoto - My Love (Mao) [Our Love] (2023-12-09_23-55).osr";
            /*marathon*/              //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Lorien Testard - Une vie a t'aimer (Iced Out) [Stop loving me      I will always love you] (2025-08-06_19-33).osr";

            replay = ReplayDecoder.GetReplayData(file);
            map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

            //worker.RunWorkerAsync();

            MusicPlayer.MusicPlayer.Initialize();
            OsuBeatmap.Create(playfieldCanva, map);
            

            SizeChanged += PlayfieldSizeChanged;
            ResizePlayfield.ResizePlayfieldCanva(playfieldCanva, playfieldBorder);
        }
    }
}

