using ReplayParsers.Classes.Replay;
using ReplayParsers.Decoders;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.Playfield;
using Beatmap = ReplayParsers.Classes.Beatmap.osu.Beatmap;

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

        DispatcherTimer timer22 = new DispatcherTimer();
        DispatcherTimer timer1 = new DispatcherTimer();

        System.Timers.Timer timer2 = new System.Timers.Timer();

        public MainWindow()
        {
            InitializeComponent();


            playfieldBackground.Opacity = 0.1;

            timer2.Interval = .001;
            timer2.Elapsed += TimerTick2;
            
            timer1.Interval = TimeSpan.FromMilliseconds(16);
            timer1.Tick += TimerTick1;
            
            KeyDown += LoadTestBeatmap;
            //GetReplayFile();
            //InitializeMusicPlayer();
        }
     
        void PlayfieldSizeChanged(object sender, SizeChangedEventArgs e)
        {
           Dispatcher.Invoke(() => ResizePlayfield.ResizePlayfieldCanva(e, playfieldCanva, playfieldBorder));
        }

        /* use dispatch invoke and figure out how to do it coz its answer to all the lagging
        
        Dispatcher.Invoke(() =>
        {
            stuff
        });  
        */
        void TimerTick2(object sender, EventArgs e)
        {
            songTimer.Text = TimeSpan.FromMilliseconds(GamePlayClock.GetElapsedTime()).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);

            fpsCounter.Text = GamePlayClock.GetElapsedTime().ToString();
            fpsCounter2.Text = musicPlayer.MediaPlayer.Time.ToString();
        }

        void TimerTick1(object sender, EventArgs e)
        {
            Playfield.Playfield.HandleVisibleCircles();
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
            /*mixed*/                 //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Extra] (2025-03-26_21-18).osr";
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
        }
    }
}

