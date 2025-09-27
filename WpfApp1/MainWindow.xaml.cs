using ReplayParsers.Classes.Replay;
using ReplayParsers.Decoders;
using System.Configuration;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.MusicPlayer.Controls;
using WpfApp1.PlayfieldGameplay;
using WpfApp1.PlayfieldUI;
using Beatmap = ReplayParsers.Classes.Beatmap.osu.Beatmap;

#nullable disable
// https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/

// todo some other time when rendering objects only make 1 circle object without combo numbers and
// then copy that object and add combo numbers... dont know if it will be better or not just curious

// try making opaque path in the middle of the slider to give effect kinda like osu sliders have in the middle

/*  things to do since basic stuff is done in no particular order   

    X 1. make DT be DT (with custom speed scaling like in osu lazer
    X 2. make HT be HT same as above
    X 3. make HR be HR
    X 4. make EZ be EZ
    X 5. make Frame Markers like in osu lazer
    X 6. make Cursor Path like in osu lazer
    
    if too many options then
    X 7. make options be scrollable and add section labels at least
*/


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Beatmap map;
        public static Replay replay;

        public static double OsuPlayfieldObjectScale = 0;
        public static double OsuPlayfieldObjectDiameter = 0;

        Stopwatch stopwatch = new Stopwatch();
        System.Timers.Timer timer = new System.Timers.Timer();

        public MainWindow()
        {
            ResizeMode = ResizeMode.NoResize;
            InitializeComponent();

            playfieldBackground.Opacity = 0.1;

            GamePlayClock.Initialize();

            timer.Interval = 1;
            timer.Elapsed += TimerTick;

            KeyDown += LoadTestBeatmap;

            PlayfieldUI.PlayfieldUI.CreateUIGrid();

            //GetReplayFile();
            //InitializeMusicPlayer();
        }

        void TimerTick(object sender, ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                Playfield.UpdateHitMarkers();
                Playfield.UpdateCursor();
                Playfield.UpdateHitObjects();
                Playfield.HandleVisibleCircles();
                Playfield.UpdateSliderTicks();
                Playfield.UpdateSliderRepeats();
                Playfield.HandleSliderEndJudgement();

                if (SongSliderControls.IsDragged == false && musicPlayer.MediaPlayer.IsPlaying == true)
                {
                    songSlider.Value = musicPlayer.MediaPlayer!.Time;
                    songTimer.Text = TimeSpan.FromMilliseconds(GamePlayClock.TimeElapsed).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
                }

                if (GamePlayClock.IsPaused())
                {
                    if (MusicPlayer.MusicPlayer.IsPlaying())
                    {
                        MusicPlayer.MusicPlayer.Pause();
                    }

                    foreach (Canvas o in Playfield.GetAliveHitObjects())
                    {
                        HitObjectAnimations.Pause(o);
                    }

                    foreach (Canvas t in Playfield.AliveHitMarkers)
                    {
                        HitMarkerAnimation.Pause(t);
                    }
                }
                else
                {
                    foreach (Canvas o in Playfield.GetAliveHitObjects())
                    {
                        HitObjectAnimations.Resume(o);
                    }

                    foreach (Canvas t in Playfield.AliveHitMarkers)
                    {
                        HitMarkerAnimation.Resume(t);
                    }
                }
            });
        }

        void LoadTestBeatmap(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.T)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Tetoris();
                    timer.Start();
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
            /*non hidden play*/       //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\criller playing Laur - Sound Chimera (Nattu) [Chimera] (2025-05-11_21-32).osr";
            /*the maze*/              //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\-GN playing Erehamonika remixed by kors k - Der Wald (Kors K Remix) (Rucker) [Maze] (2020-11-08_20-27).osr";
            /*double click*/          //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\worst hr player playing Erehamonika remixed by kors k - Der Wald (Kors K Remix) (Rucker) [fuckface] (2023-11-25_05-20).osr";
            /*slider tick miss*/      //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing twenty one pilots - Heathens (Magnetude Bootleg) (funny) [Marathon] (2025-09-15_07-28).osr";
            /*non slider tick miss*/  //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing twenty one pilots - Heathens (Magnetude Bootleg) (funny) [Marathon] (2023-01-06_01-39).osr";
            /*heavy tech*/            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing ReeK & Asatsumei - Deity Mode (feat. L4hee) (-Links) [PROJECT-02 Digital Mayhem Symphony] (2025-06-14_10-50).osr";
            /*slider repeats*/        //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing senya - Kasou no Kimi no Miyako (Satellite) [s] (2025-09-22_09-18).osr";
            /*arrow slider no miss*/  string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\hyeok2044 playing Kaneko Chiharu - - FALLEN - (Kroytz) [O' Lord, I entrust this body to you—] (2024-11-17_07-41).osr";
            /*arrow slider ye miss*/  //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Kaneko Chiharu - - FALLEN - (Kroytz) [O' Lord, I entrust this body to you—] (2022-10-21_16-50).osr";

            replay = ReplayDecoder.GetReplayData(file);
            map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

            MusicPlayer.MusicPlayer.Initialize();

            Analyser.Analyser.CreateHitMarkers();

            OsuBeatmap.Create(playfieldCanva, map);

            playfieldBorder.Visibility = Visibility.Visible;
            ResizePlayfield.ResizePlayfieldCanva();
        }
    }
}

