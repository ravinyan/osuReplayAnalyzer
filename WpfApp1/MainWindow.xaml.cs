using ReplayParsers.Classes.Replay;
using ReplayParsers.Decoders;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp1.Analyser;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.MusicPlayer.Controls;
using WpfApp1.Playfield;
using Beatmap = ReplayParsers.Classes.Beatmap.osu.Beatmap;

#nullable disable
// https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/

// todo some other time when rendering objects only make 1 circle object without combo numbers and
// then copy that object and add combo numbers... dont know if it will be better or not just curious

// play pause button pausing/playing is kinda meh so maybe do that one day idk

// figure out how to make ALL animations on entirely separate thread that wont make any lags to cursor or anything else...
// if thats even possible coz WPF is so ANNOYING

// try making opaque path in the middle of the slider to give effect kinda like osu sliders have in the middle
namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Beatmap map;
        public static Replay replay;

        DispatcherTimer timer1 = new DispatcherTimer();
        DispatcherTimer timer2 = new DispatcherTimer();
        Stopwatch stopwatch = new Stopwatch();

        
        public MainWindow()
        {
            Thread thread = Thread.CurrentThread;
            var a = thread.GetApartmentState();
            InitializeComponent();

            playfieldBackground.Opacity = 0.1;

            GamePlayClock.Initialize();

            timer1.Interval = TimeSpan.FromMilliseconds(1);
            timer1.Tick += TimerTick1;

            //timer2.Interval = TimeSpan.FromMilliseconds(1);
            //timer2.Tick += TimerTick2;

            //Thread why = new Thread(ThreadStartingPoint);
            //why.SetApartmentState(ApartmentState.STA);
            //why.IsBackground = true;
            //why.Name = "BANANA";
            //why.Start();

            KeyDown += LoadTestBeatmap;

            //GetReplayFile();
            //InitializeMusicPlayer();
        }

        void PlayfieldSizeChanged(object sender, SizeChangedEventArgs e)
        {
           ResizePlayfield.ResizePlayfieldCanva(playfieldCanva, playfieldBorder);
        }

        private void ThreadStartingPoint()
        {
            //timer2.Interval = TimeSpan.FromMilliseconds(1);
            //timer2.Tick += TimerTick2;
            //
            //void TimerTick2(object sender, EventArgs e)
            //{
            //    //await Dispatcher.InvokeAsync(() =>
            //    //{
            //    //
            //    //}, DispatcherPriority.Send);
            //
            //
            //
            //   Playfield.Playfield.UpdateCursor();
            //}
            //

            Dispatcher.Run();
        }
    
        void TimerTick1(object sender, EventArgs e)
        {

            Playfield.Playfield.UpdateHitMarkers();
            Playfield.Playfield.UpdateCursor();
            Playfield.Playfield.UpdateHitObjects();
            Playfield.Playfield.HandleVisibleCircles();

            if (SongSliderControls.IsDragged == false)
            {
                songSlider.Value = musicPlayer.MediaPlayer!.Time;
                songTimer.Text = TimeSpan.FromMilliseconds(GamePlayClock.TimeElapsed).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            }

            //fpsCounter.Text = Playfield.Playfield.GetAliveHitObjects().Count.ToString();
            //fpsCounter.Text = playfieldCanva.Children.Count.ToString();
            if (GamePlayClock.IsPaused())
            {
                foreach (Canvas o in Playfield.Playfield.GetAliveHitObjects())
                {
                    HitObjectAnimations.Pause(o);
                }

                foreach (TextBlock t in Playfield.Playfield.AliveHitMarkers)
                {
                    HitMarkerAnimation.Pause(t);
                }
            }
            else
            {
                foreach (Canvas o in Playfield.Playfield.GetAliveHitObjects())
                {
                    HitObjectAnimations.Resume(o);
                }

                foreach (TextBlock t in Playfield.Playfield.AliveHitMarkers)
                {
                    HitMarkerAnimation.Resume(t);
                }
            }
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
                    //stopwatch.Start();
                });
            }
        }

        void Tetoris()
        {
            // i hate how i memorized the memory consumption of every file here after being rendered as beatmap
            // not rendering slider tail circle (which is ugly anyway and like 10 people use it) saves 400mb ram!
            // on marathon map and almost 1gb on mega marathon
            /*circle only*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-04-02_17-15) (65).osr";
            /*slider only*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Kensuke x Ascended_s EX] (2025-03-22_12-46) (1).osr";
            /*mixed*/                 //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Extra] (2025-03-26_21-18).osr";
            /*mega marathon*/         //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Aqours - Songs Compilation (Sakurauchi Riko) [Sweet Sparkling Sunshine!!] (2024-07-21_03-49).osr";
            /*olibomby sliders/tech*/ //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Raphlesia & BilliumMoto - My Love (Mao) [Our Love] (2023-12-09_23-55).osr";
            /*marathon*/              //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Lorien Testard - Une vie a t'aimer (Iced Out) [Stop loving me      I will always love you] (2025-08-06_19-33).osr";
            /*non hidden play*/       string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\criller playing Laur - Sound Chimera (Nattu) [Chimera] (2025-05-11_21-32).osr";
            /*the maze*/              //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\-GN playing Erehamonika remixed by kors k - Der Wald (Kors K Remix) (Rucker) [Maze] (2020-11-08_20-27).osr";
            /*double click*/          //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\worst hr player playing Erehamonika remixed by kors k - Der Wald (Kors K Remix) (Rucker) [fuckface] (2023-11-25_05-20).osr";
          
            replay = ReplayDecoder.GetReplayData(file);
            map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

            MusicPlayer.MusicPlayer.Initialize();

            Analyser.Analyser.CreateHitMarkers();

            OsuBeatmap.Create(playfieldCanva, map);

            SizeChanged += PlayfieldSizeChanged;
            ResizePlayfield.ResizePlayfieldCanva(playfieldCanva, playfieldBorder);
        }
    }
}

