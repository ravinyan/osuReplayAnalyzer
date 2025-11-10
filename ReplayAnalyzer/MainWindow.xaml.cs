using OsuFileParsers.Classes.Replay;
using OsuFileParsers.Decoders;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.FileWatcher;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldUI;
using ReplayAnalyzer.SettingsMenu;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Beatmap = OsuFileParsers.Classes.Beatmap.osu.Beatmap;

#nullable disable
// https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/

// todo some other time when rendering objects only make 1 circle object without combo numbers and
// then copy that object and add combo numbers... dont know if it will be better or not just curious

// try making opaque path in the middle of the slider to give effect kinda like osu sliders have in the middle

/*  things to do since basic stuff is done in no particular order   

    X 5. make Frame Markers like in osu lazer
    X 6. make Cursor Path like in osu lazer
    X 7. do HT and DT rate changes
    
    fix seeking using slider coz its a bit scuffed but hopefully nothing too bad
    only problem is with sliders disappearing and slider head appearing using rate change slider

    note for custom DT and HT rate changes: its impossible to implement due to how lazer implements it so goodbye
    spent 5h checking everywhere in osu lazer source code and they take it from air i dont understand how lol (i do tho)
*/


namespace ReplayAnalyzer
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

        public static System.Timers.Timer timer = new System.Timers.Timer();

        public MainWindow()
        {
            //Visibility = Visibility.Hidden;
            ResizeMode = ResizeMode.NoResize;
            InitializeComponent();

            osuReplayWindow.Width = int.Parse(SettingsOptions.config.AppSettings.Settings["ScreenResolution"].Value.Split('x')[0]) / 1.5;
            osuReplayWindow.Height = int.Parse(SettingsOptions.config.AppSettings.Settings["ScreenResolution"].Value.Split('x')[1]) / 1.5;

            timer.Interval = 1;
            timer.Elapsed += TimerTick;

            #if DEBUG

            KeyDown += LoadTestBeatmap;

            #endif

            startupInfo.Text = "Press F2 on replay screen in game to load replay.\n" +
                               "Click Options Cog in top left to choose osu! and/or osu!lazer folder. \n" +
                               "(its folder containing Beatmaps, Skins, etc. Location can be found in osu client options > Open osu! folder)";

            PlayfieldUI.PlayfieldUI.CreateUIGrid();

            BeatmapFile.Load();
            
            //GetReplayFile();
            //InitializeMusicPlayer();

            osuReplayWindow.MouseDown += OsuReplayWindowResetOpenWindows;
        }

        private void OsuReplayWindowResetOpenWindows(object sender, MouseButtonEventArgs e)
        {
            if (VolumeControls.VolumeWindow.Visibility == Visibility.Visible)
            {
                VolumeControls.VolumeWindow.Visibility = Visibility.Collapsed;
            }

            if (RateChangerControls.RateChangeWindow.Visibility == Visibility.Visible)
            {
                RateChangerControls.RateChangeWindow.Visibility = Visibility.Collapsed;
            }

            if (SettingsPanel.SettingPanelBox.Visibility == Visibility.Visible)
            {
                SettingsPanel.SettingPanelBox.Visibility = Visibility.Hidden;
            }
        }

        void TimerTick(object sender, ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                //Playfield.UpdateHitMarkers();
                //Playfield.HandleAliveHitMarkers();
                //Playfield.HandleAliveHitJudgements();
                //Playfield.UpdateCursor();
                //Playfield.UpdateHitObjects
                //Playfield.HandleVisibleHitObjects();
                //Playfield.UpdateSliderTicks();
                //Playfield.UpdateSliderRepeats();
                //Playfield.HandleSliderEndJudgement();

                // new stuff
                HitObjectSpawner.UpdateHitObjects(); 
                HitDetection.CheckIfObjectWasHit();
                HitMarkerManager.HandleAliveHitMarkers();

                CursorManager.UpdateCursor();
                HitJudgementManager.HandleAliveHitJudgements();
                HitObjectManager.HandleVisibleHitObjects();

                SliderEventss.UpdateSliderTicks();
                SliderEventss.UpdateSliderRepeats();
                SliderEventss.HandleSliderEndJudgement();

                #if DEBUG
                    gameplayclock.Text = $"{GamePlayClock.TimeElapsed}";
                    musicclock.Text = $"{musicPlayer.MediaPlayer.Time}";
                #endif

                if (SongSliderControls.IsDragged == false && musicPlayer.MediaPlayer.IsPlaying == true)
                {
                   songSlider.Value = musicPlayer.MediaPlayer!.Time;
                   songTimer.Text = TimeSpan.FromMilliseconds(GamePlayClock.TimeElapsed).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
                }
     
                // i may be stupid but i dont know how else to do this
                if (GamePlayClock.IsPaused() == true)
                {
                    foreach (HitObject o in HitObjectManager.GetAliveHitObjects())
                    {
                        HitObjectAnimations.Pause(o);
                    }
                }
                else
                {
                    foreach (HitObject o in HitObjectManager.GetAliveHitObjects())
                    {
                        HitObjectAnimations.Resume(o);
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
                });
            }
        }

        void Tetoris()
        {
            // i hate how i memorized the memory consumption of every file here after being rendered as beatmap
            // not rendering slider tail circle (which is ugly anyway and like 10 people use it) saves 400mb ram!
            // on marathon map and almost 1gb on mega marathon
            /*circle only*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-04-02_17-15).osr";
            /*slider only*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Kensuke x Ascended_s EX] (2025-03-22_12-46).osr";
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
            /*slider repeats/ticks*/  //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing senya - Kasou no Kimi no Miyako (Satellite) [s] (2025-09-22_09-18).osr";
            /*arrow slider no miss*/  string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\hyeok2044 playing Kaneko Chiharu - - FALLEN - (Kroytz) [O' Lord, I entrust this body to you—] (2024-11-17_07-41).osr";
            /*arrow slider ye miss*/  //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Kaneko Chiharu - - FALLEN - (Kroytz) [O' Lord, I entrust this body to you—] (2022-10-21_16-50).osr";
            /*HR*/                    //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\hyeok2044 playing Will Stetson - phony (Astronic) [identity crisis] (2024-12-17_02-44).osr";
            /*EZ*/                    //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing AKUGETSU, BL8M - BLINK GONE (AirinCat) [FINAL] (2025-09-19_19-29).osr";
            /*DT*/                    //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Will Stetson - KOALA (Luscent) [Niva's Extra] (2024-01-28_07-37).osr";
            /*HT*/                    //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Will Stetson - Kyu-kurarin (DeviousPanda) [...] (2025-09-28_10-55).osr";
            /*modified DT*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Will Stetson - Rainy Boots (- Clubber -) [Plead] (2025-09-28_11-01).osr";
            /*modified HT*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing PinpinNeon - Scars of Calamity (Nyaqua) [Slowly Incinerating by The Flames of Calamity] (2025-08-26_21-01).osr";
            /*another DT*/            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Mary Clare - Radiant (-[Pino]-) [dahkjdas' Insane] (2024-03-04_22-03).osr";
            /*precision hit/streams*/ //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\replay-osu_803828_4518727921.osr";
            /*I HATE .OGG FILES WHY THEN NEVER WORK LIKE ANY NORMAL FILE FORMAT*/ //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Akatsuki Records - Bloody Devotion (K4L1) [Pocket Watch of Blood] (2025-04-17_12-19).osr.";
            /*circle only HR*/        //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Umbre playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-02-14_00-10).osr";
            /*dt*/                    //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Tebi playing Will Stetson - KOALA (Luscent) [Niva's Extra] (2024-02-04_15-14).osr";

            Dispatcher.Invoke(() =>
            {
                if (musicPlayer.MediaPlayer != null)
                { 
                    timer.Close();
                    musicPlayer.MediaPlayer.Stop();
                    musicPlayer.MediaPlayer.Media = null;
                    musicPlayer.MediaPlayer = null;
                    playfieldBackground.ImageSource = null;
                    OsuBeatmap.HitObjectDictByIndex.Clear();
                    HitObjectAnimations.sbDict.Clear();
                    Analyser.Analyser.HitMarkers.Clear();
                    //Playfield.ResetVariables();
                    HitObjectSpawner.ResetFields();

                    for (int i = playfieldCanva.Children.Count - 1; i >= 1; i--)
                    {
                        playfieldCanva.Children.Remove(playfieldCanva.Children[i]);
                    }
                        
                    GamePlayClock.Restart();
                    songSlider.Value = 0;

                    playerButton.Style = FindResource("PlayButton") as Style;
                }

                MainWindow.replay = ReplayDecoder.GetReplayData(file);
                MainWindow.map = BeatmapDecoder.GetOsuLazerBeatmap(MainWindow.replay.BeatmapMD5Hash);

                OsuBeatmap.ModifyDifficultyValues(replay.ModsUsed.ToString());

                MusicPlayer.MusicPlayer.Initialize();

                Analyser.Analyser.CreateHitMarkers();

                OsuBeatmap.Create(MainWindow.map);

                playfieldBorder.Visibility = Visibility.Visible;
                ResizePlayfield.ResizePlayfieldCanva();
               
                GamePlayClock.Initialize();

                playfieldGrid.Children.Remove(startupInfo);

                MainWindow.timer.Start();
            });
        }
    }
}

