using OsuFileParsers.Classes.Replay;
using OsuFileParsers.Decoders;
using ReplayAnalyzer.AnalyzerTools;
using ReplayAnalyzer.AnalyzerTools.Cursor;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.FileWatcher;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.KeyboardShortcuts;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using ReplayAnalyzer.PlayfieldUI;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions;
using System.Diagnostics;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Beatmap = OsuFileParsers.Classes.Beatmap.osu.Beatmap;
using SliderTick = ReplayAnalyzer.PlayfieldGameplay.SliderEvents.SliderTick;

#nullable disable
// https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/

#region losing my mind at the speed of light    
/*
     new stuff for performance fixing:
       i honestly dont know what im doing at all but slowly learning and understanding it... 
       if these comments will be stupid enough i will keep them coz funny
       MELTDOWN PANIK PANIK THIS IS NOT WHAT I EXPECTED EVERYTHING IS PROBLEM MEMORY LEAKING LIKE BROKEN WATER PIPE
                        fixed                fixed              on hunt to find other problems
       real problems: a lot of xaml + a lot of storyboards + PROBABLY SOMETHING ELSE AAAAAAAAAA <- check 2 snapshots saved after hitmarker fix
                                                             ^ wait hitmarkers?
       make hit markers spawn in real time too since they chonk the ram and with that the performance COZ XAML HATES A LOT OF XAML WHO WOULD HAVE THOUGHT AAAAAAAAAAAAAA
        ^ visibility of hitmarkers fix

                                                            d              ?                   d             X
       to remember: fix stacking, find way to remember slider events, implement resize back, circle colorr, CLEANUP
       I FRICKING HATE WPF WHY DID I DO THIS TO MYSELF THIS IS SO HORRIBLE AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

       reminder to self test shit without debugger sometimes you stupid idiot
        ^ IM FREE EVERYTHING WORKS AND HOLY SHIT IT WORKS SO GOOD AAAAAA FREEEEEEEEEDOOOOOOOOOOOOOOOOOOOOOM
       also coz of all this i learned a lot about various important things like WPF is asshole and i cant read...
       and also less important things like how c# or just WPF idk manages events and how to do them properly and a lot more

       old stuff:
       this might take some time to do without bugs but i also have a lot of time so no need to rush
     > extremely big maps (hit object count in hit object dict) have some performance problems so fix that
       ^ issue indentified: amount of created XAML objects in hit object array cause lag 
         (what hit object didnt matter since cirlce only 6k object map lagged same as aqours marathon)
         possible solution: render objects at runtime instead of having them all stored in array
         use map.HitObjects as properties and with that create XAML hit objects only when needed, then OBLITERATE IT FROM EXISTENCE        
*/
#endregion

/*  mostly things to do when i will do everything else working on and have nothing else to do

random stuff
    .after looking through and analyzing app RAM usage i can say that there are no further noticable improvements 
     unless i trade performance for memory (mainly replay frames) which would be like 0.8MB per 4.5-5min of the map (~20k frames)
     and i dont want to do that but im writing this here so i know i dont need to check for RAM improvements anymore

    (not needed but maybe?)
        > 2B maps work BUT spawning objects from backwards seeking is scuffed... i also dont thing i want to fix this problem but it exists
          BUT IT DOES WORK when backwards seeking objects wont be shown sometimes BUT unpausing/seeking 1 frame forwards will
          show all objects that were supposed to spawn 
           ^ only if someone tells me to do that (so basically no)
        > if i feel like hating my own life then fix Random mod even tho i most likely cant do that
           ^ no thank you i want to be happy
        > make spinners work in case someone is worse than me at the game and misses them... and needs to analyze them... 
           ^ NO
    
    (low prority)
        > stop being dumb (achieved)

    next update might be jump to 1.0 version since i did almost everything i wanted to... after that i will just update
    the database version of osu!lazer if devs change something there and app crashes from that 
    coz it already happened when they added pause count to replay stats and tags i think
    (to do N O W) 
        > attempt to make some UI elements movable (key overlay, hit map, UR bar, and i guess judgement counter)
           ^ final boss... might not do it but will try
        > fix any bug found i guess and mark project as finished woweee

    (for later after N O W)
        > profit in skill increase

    (WPF is dogshit???)
        > its maybe coz of debugger idk but when i left replay playing to test if stuff works and then clicked windows task bar
          i got: System.InvalidOperationException: 'The visual tree has been changed during a 'VisualTreeChanged' event.'
          and i got it 2 times doing same thing and cant get it anymore...
          read that it could be debugger issue but its better to know that this exists... and i hope this doesnt exists without debugger
*/

// reminder to self for publishing app before i lose my fucking mind again and forget AGAIN
// publish osu file parser in whatever trash folder > publish replay analyzer in the folder for github

namespace ReplayAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Beatmap map { get; set; } = new Beatmap();
        public static Replay replay { get; set; } = new Replay();

        public static double OsuPlayfieldObjectScale { get; set; } = 0;
        public static double OsuPlayfieldObjectDiameter { get; set; } = 0;
        
        private static System.Timers.Timer timer = new System.Timers.Timer();
        
        public static bool IsReplayPreloading { get; set; } = true;

        private static List<ReplayFrame> test = new List<ReplayFrame>();
        public MainWindow()
        {
            ResizeMode = ResizeMode.CanMinimize;
            InitializeComponent();

            PropertyInfo dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            PropertyInfo dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            osuReplayWindow.Width = int.Parse(SettingsOptions.GetConfigValue("ScreenResolution").Split('x')[0]) / ((int)dpiXProperty!.GetValue(null, null)! / 96.0);
            osuReplayWindow.Height = int.Parse(SettingsOptions.GetConfigValue("ScreenResolution").Split('x')[1]) / ((int)dpiYProperty!.GetValue(null, null)! / 96.0);

            // app starts paused so make framerate paused too while replay is not playing
            // 28ms is nice spot where app doesnt lag when seeking
            ChangeGameplayLoopFrameRate(16);
            timer.Elapsed += TimerTick;

#if DEBUG
            MouseDown += MainWindow_MouseDown;
            KeyDown += LoadTestBeatmap;
            playfieldCanva.MouseMove += PlayfieldCanva_MouseMove;
#endif

            startupInfo.Text = "Press F2 on replay screen in game to load replay.\n" +
                               "Click Options Cog in top left, go to General and set which \"osu! client replay is from\", then go to Files and choose osu! and/or osu!lazer folder. \n" +
                               "(its folder containing Beatmaps, Skins, etc. Location can be found in osu client options > Open osu! folder)";

            PlayfieldUI.PlayfieldUI.CreateUIElementsBeforeReplayLoaded();

            BeatmapFile.Load();

            ShortcutManager.Initialize();

            osuReplayWindow.MouseDown += OsuReplayWindowResetOpenWindows;

            // im tired boss... (took 20min to figure out but still tired i just woke up) so... 
            // if you launch app WITH debugger, the fps are fully uncapped (i need that)
            // if you launch app WITHOUT debugger, fps are capped to 60 UNLESS you use WPF animation THAT IS CURRENTLY RUNNING
            // if you use WPF animation like code below, WPF will have always uncapped fps
            // after deleting all WPF animations from my code i noticed this 60fps cap happening 
            // (also happened when there were no hit objects on beatmap with WPF animations)
            // i have so little respect to WPF this code doesnt deserve good syntax if i need it for uncapped fps (even if this kinda makes sense)
            // DO NOT CHANGE, DO NOT DELETE, DO NOT TOUCH, DO NOT SMELL, DO NOT LOOK, DO NOT EVEN ACKNOWLEDGE IT, LEAVE IT ALONE
            DoubleAnimation fuckWPF = new DoubleAnimation();fuckWPF.Duration = Duration.Forever;fuckWPF.From = 1;fuckWPF.To = 1;fuckWPF.SpeedRatio = 0.0000000000000000000001;Timeline.SetDesiredFrameRate(fuckWPF, 1);
            playerButton.BeginAnimation(OpacityProperty, fuckWPF);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        // god i love this SO MUCH I WISH I KNEW IT EARLIER AAAAAAAAAAAAAAAA
        private void PlayfieldCanva_MouseMove(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine(e.GetPosition(playfieldCanva));
            //Debug.WriteLine();
            //Debug.WriteLine();
        }

        public void ChangeGameplayLoopFrameRate(double frameDurationInMs)
        {
            timer.Interval = frameDurationInMs;
        }

        // data of newest optimalizations coz fun https://github.com/ravinyan/osuReplayAnalyzer/tree/70407abc041eb31b70b0a61a78207daa1ca82c07
        // if needed get code from there to measure performance again
        public void PreloadWholeReplay()
        {
            //Stopwatch stopwatch = new Stopwatch();
            //long timeee = 0;
            for (int i = 0; i < replay.FramesDict.Count; i++)
            {
                long time = replay.FramesDict[i].Time;
                GamePlayClock.Seek(time);

                HitObjectSpawner.UpdateHitObjects();
                CursorManager.UpdateCursorPosition();
                //stopwatch.Start();
                HitMarkerManager.UpdateHitMarkerAfterSeek(time);
                //stopwatch.Stop();
                //timeee += stopwatch.ElapsedTicks;
                //stopwatch.Reset();
                // sometimes hit markers are not properly updated and always in the same spot... why idk this is scuffed fix and works

                SliderEndJudgement.UpdateSliderBodyEvents();
                SliderReverseArrow.UpdateSliderRepeats(true);
                SliderTick.UpdateSliderTicks(true);

                HitObjectManager.HandleVisibleHitObjects();
                HitMarkerManager.HandleAliveHitMarkers();
                HitJudgementManager.HandleAliveHitJudgements();
            }

            //OsuMaths.OsuMath math = new OsuMaths.OsuMath();
            //for (int i = 0; i < 191381; i++)
            //{
            //    stopwatch.Start();
            //    var a = math.GetJudgement50HitWindow();
            //    stopwatch.Stop();
            //    timeee += stopwatch.ElapsedTicks;
            //    stopwatch.Reset();
            //}

            //Debug.WriteLine(timeee);
            //Debug.WriteLine(OsuMaths.OsuMath.count);

            Playfield.ResetPlayfieldFields();

            // clear stuck objects except cursor which is at index 0
            for (int i = playfieldCanva.Children.Count - 1; i >= 0; i--)
            {
               playfieldCanva.Children.Remove(playfieldCanva.Children[i]);
            }

            IsReplayPreloading = false;
            HitMarkerManager.GetAliveDataHitMarkers().Clear();

            // initialize default values with added offset
            int offsetValue = int.Parse(SettingsOptions.GetConfigValue("AudioOffset"));
            songSlider.Value = offsetValue;
            GamePlayClock.Seek(offsetValue);
        }

        // for me to not accidentaly publish this lol
        // THIS IS USELESS or is now at least coz it doesnt show any fps coz wpf is DOGSHIT
        Stopwatch fpsTimer = null;
        int iii = 0;
        private void FpsTimer()
        {
            if (fpsTimer == null)
            {
                fpsTimer = new Stopwatch();
            }
            if (fpsTimer.IsRunning == false)
            {
                fpsTimer.Start();
            }
            iii++;
            if (fpsTimer.ElapsedMilliseconds >= 1000)
            {
                gameplayclock.Text = iii.ToString();
                iii = 0;
                fpsTimer.Restart();
            }
            //if (fpsTimer == null)
            //{
            //    fpsTimer = new Stopwatch();
            //    fpsTimer.Start();
            //}
            //else
            //{
            //    // scuffed but shows fps
            //    if (fpsTimer.ElapsedMilliseconds > 1000)
            //    {
            //        // this is just random test
            //        JudgementCounter.Reset();
            //        fpsTimer.Restart();
            //    }
            //    else
            //    {
            //        JudgementCounter.Increment50();
            //    }
            //}
        }

        Stopwatch w = new Stopwatch();
        void TimerTick(object sender, ElapsedEventArgs e)
        {// to myself: use InvokeAsync otherwise you will spend 2h figuring out why the frick app freezes on first object spawn when refresh rate is too high 
            Dispatcher.InvokeAsync(() =>
            {
#if DEBUG
                //FpsTimer();
#endif

                HitObjectSpawner.UpdateHitObjects();
                CursorManager.UpdateCursorPosition();
                HitDetection.CheckIfObjectWasHit();
                
                FrameMarkerManager.UpdateFrameMarker();
                CursorPathManager.UpdateCursorPath();
                
                HitObjectAnimations.RunAnimationLoop(GamePlayClock.TimeElapsed);
                
                SliderEndJudgement.UpdateSliderBodyEvents();
                SliderReverseArrow.UpdateSliderRepeats();
                SliderTick.UpdateSliderTicks();
                
                HitObjectManager.HandleVisibleHitObjects();
                HitJudgementManager.HandleAliveHitJudgements();
                
                HitMarkerManager.HandleAliveHitMarkers();
                FrameMarkerManager.HandleAliveFrameMarkers();
                CursorPathManager.HandleAliveCursorPaths();
                
                KeyOverlay.UpdateHoldPositions();
  
                if (SongSliderControls.IsDragged == false)
                {
                    double aaa = GamePlayClock.TimeElapsed;
                    songSlider.Value = aaa;
                    songTimer.Text = TimeSpan.FromMilliseconds(GamePlayClock.TimeElapsed).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
                }

#if DEBUG
                //musicclock.Text = $"{timer.Interval}";
                //gameplayclock.Text = $"{FilePath.GetBeatmapAudioPath().Substring(FilePath.GetBeatmapAudioPath().Length -4)}";
                //musicclock.Text = $"{MusicPlayer.MusicPlayer.AudioFile.CurrentTime.TotalMilliseconds}";
#endif

            });
        }

        // i think i need to clean this up somewhat... somehow... one day in the future
        public void ResetReplay()
        {
            timer.Close();

            // this needs to be on top otherwise app have stroke and crashes coz of some memory reading issue with SoundTouch
            RateChangerControls.ResetFields();

            MusicPlayer.MusicPlayer.ResetMusicPlayer();

            MusicPlayer.JudgementTimeline.ResetTimeline();

            HitMarkerData.ResetFields();

            MissFinder.ResetFields();

            Playfield.ResetPlayfieldFields();

            JudgementCounter.Reset();

            OsuMaths.OsuMath.ResetFields();

            for (int i = playfieldCanva.Children.Count - 1; i >= 0; i--)
            {
                playfieldCanva.Children.Remove(playfieldCanva.Children[i]);
            }

            // initialize default values with added offset
            GamePlayClock.Pause();
            int offsetValue = int.Parse(SettingsOptions.GetConfigValue("AudioOffset"));
            songSlider.Value = offsetValue;
            GamePlayClock.Seek(offsetValue);

            playerButton.Style = FindResource("PlayButton") as Style;
        }
        
        public void InitializeReplay()
        {
            playfieldBorder.Visibility = Visibility.Visible;
            playfieldGrid.Children.Remove(startupInfo);

            IsReplayPreloading = true;

            // hit markers need to be before canva resize for accurate placement coz of different window sizes
            HitMarkerData.CreateData();
            ResizePlayfield.ResizePlayfieldCanva();

            // music player > mods coz DT/HT needs to change player rate, mods > UI coz HR etc. can change OD, which affects UR bar
            MusicPlayer.MusicPlayer.Initialize();
            BeatmapMods.Apply();
            PlayfieldUI.PlayfieldUI.CreateUIElementsAfterReplayLoaded();

            // initialize timeline (gives width to it needed for accurate judgement placement)
            // > preload saves judgements > populate timeline with preload judgements
            MusicPlayer.JudgementTimeline.Initialize();
            PreloadWholeReplay();
            MusicPlayer.JudgementTimeline.PopulateJudgementTimeline();
            
            // when user changes skins before loading replay this fixes wrong hit circle colour
            // and needs to be here coz it needs map.HitObjects initialized to get combo colours
            SkinIniProperties.ResetComboColours();
            CursorSkin.InitializeCursor();

            // forcibly clears OsuFileParsers memory since garbage collector even if it clears some of it it doesnt clear everything
            // reduction on 37min aquors marathon is: 100MB > 85MB after 10s on task manager after initializing replay
            // on 4.5min Bloody Devotion stream map: 61MB >  53MB, after waiting for 3min without clearing it it was still 61-62MB
            // playing through entire Bloody Devotion on 2x speed:
            //  without clearing memory: 75-83MB through play and 76MB at the end after waiting 10s
            //  with clearing memory   : 70-78MB through play and 73MB at the end after waiting 10s
            //  on average its about 4-5MB less memory usage, bigger beatmap = higher usage without clearing this RAM i guess?
            // (number variation depends on objects spawned and density of them, done on 1k FPS, 2x speed, everything enabled and RELEASE mode no debugger)
            // clearing takes 23ms on stream map, 50ms on 37min long map, so worth it for me
            // (and this might also remove rare slight lag from big GC auto clear, tho it only happened on debug with debugger on aquors map)
            // its basically just calling dispose but it affects whole OsuFileParsers from what i understand which is what i want
            ReplayDecoder.Clear();
            BeatmapDecoder.Clear();

            GamePlayClock.Initialize();
            timer.Start();
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
            // its so empty here without comment on top
            /*circle only*/                   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-04-02_17-15).osr";
            /*slider only*/                   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Kensuke x Ascended_s EX] (2025-03-22_12-46).osr";
            /*mixed*/                         //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Extra] (2025-03-26_21-18).osr";
            /*mega marathon*/                 //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Aqours - Songs Compilation (Sakurauchi Riko) [Sweet Sparkling Sunshine!!] (2024-07-21_03-49).osr";
            /*olibomby sliders/tech*/         //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Raphlesia & BilliumMoto - My Love (Mao) [Our Love] (2023-12-09_23-55).osr";
            /*marathon*/                      //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Lorien Testard - Une vie a t'aimer (Iced Out) [Stop loving me      I will always love you] (2026-03-16_21-05).osr";
            /*non hidden play*/               //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\criller playing Laur - Sound Chimera (Nattu) [Chimera] (2025-05-11_21-32).osr";
            /*the maze*/                      //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\-GN playing Erehamonika remixed by kors k - Der Wald (Kors K Remix) (Rucker) [Maze] (2020-11-08_20-27).osr";
            /*double click*/                  //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\worst hr player playing Erehamonika remixed by kors k - Der Wald (Kors K Remix) (Rucker) [fuckface] (2023-11-25_05-20).osr";
            /*slider tick miss*/              //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing twenty one pilots - Heathens (Magnetude Bootleg) (funny) [Marathon] (2025-09-15_07-28).osr";
            /*non slider tick miss*/          //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing twenty one pilots - Heathens (Magnetude Bootleg) (funny) [Marathon] (2023-01-06_01-39).osr";
            /*heavy tech*/                    //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing ReeK & Asatsumei - Deity Mode (feat. L4hee) (-Links) [PROJECT-02 Digital Mayhem Symphony] (2025-06-14_10-50).osr";
            /*slider repeats/ticks*/          //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing senya - Kasou no Kimi no Miyako (Satellite) [s] (2025-09-22_09-18).osr";
            /*arrow slider no miss*/          //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\hyeok2044 playing Kaneko Chiharu - - FALLEN - (Kroytz) [O' Lord, I entrust this body to you—] (2024-11-17_07-41).osr";
            /*arrow slider ye miss*/          //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Kaneko Chiharu - - FALLEN - (Kroytz) [O' Lord, I entrust this body to you—] (2022-10-21_16-50).osr";
            /*HR*/                            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\hyeok2044 playing Will Stetson - phony (Astronic) [identity crisis] (2024-12-17_02-44).osr";
            /*EZ*/                            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing AKUGETSU, BL8M - BLINK GONE (AirinCat) [FINAL] (2025-09-19_19-29).osr";
            /*DT*/                            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Will Stetson - KOALA (Luscent) [Niva's Extra] (2024-01-28_07-37).osr";
            /*HT*/                            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Will Stetson - Kyu-kurarin (DeviousPanda) [...] (2025-09-28_10-55).osr";
            /*modified DT*/                   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Will Stetson - Rainy Boots (- Clubber -) [Plead] (2025-09-28_11-01).osr";
            /*modified HT*/                   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing PinpinNeon - Scars of Calamity (Nyaqua) [Slowly Incinerating by The Flames of Calamity] (2025-08-26_21-01).osr";
            /*another DT*/                    //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Mary Clare - Radiant (-[Pino]-) [dahkjdas' Insane] (2024-03-04_22-03).osr";
            /*precision hit/streams*/         //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\replay-osu_803828_4518727921.osr";
            /*I HATE .OGG FILES WHY THEN NEVER WORK LIKE ANY NORMAL FILE FORMAT*/ //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Akatsuki Records - Bloody Devotion (K4L1) [Pocket Watch of Blood] (2025-04-17_12-19).osr.";
            /*circle only HR*/                //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Umbre playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-02-14_00-10).osr";
            /*dt*/                            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Tebi playing Will Stetson - KOALA (Luscent) [Niva's Extra] (2024-02-04_15-14).osr";
            /*i love arknights (tick test)*/  //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing AIYUE blessed Rina - Heavenly Me (Aoinabi) [tick] (2025-11-13_07-14).osr";
            /*delete this from osu lazer after testing*/ //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Various Artists - Long Stream Practice Maps 3 (DigitalHypno) [250BPM The Battle of Lil' Slugger (copy)] (2025-11-24_07-11).osr";
            /*for fixing wrong miss count*/   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing DJ Myosuke - Source of Creation (Icekalt) [Evolution] (2025-06-06_20-40).osr";
            /*fix miss count thx*/            string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Yooh - Eternity (Kojio) [Endless Suffering] (2025-10-23_13-15) (12).osr";
            /*i love song (audio problem)*/   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Kotoha - Aisuru Youni (Faruzan1577) [We live in loneliness] (2026-01-01_21-20) (10).osr";
            /*null timing point*/             //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\RyuuBei playing LukHash - 8BIT FAIRY TALE (Delis) [Extra] (2018-10-31_18-24).osr";
            /*slider stream walker*/          //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing AXIOMA - Rift Walker (osu!team) [Expert] (2025-08-05_19-34).osr";
            /*OSU LAZER MODS ARE REAL*/       //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing senya - Kasou no Kimi no Miyako (Satellite) [s] (2026-01-16_08-14) (1).osr";
            /*(not)wrong miss < im stupid*/   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing TK from Ling tosite sigure - first death (TV Size) (Kyuukai) [we'll be working together until death do us part] (2025-08-13_21-08).osr";
            /*another audio thing*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Ludicin - Everlasting Eternity (R3m) [Till The Epilogue Of Time] (2024-11-15_21-40).osr";
            /*ultimate slider test replay*/   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing RichaadEB feat. Cristina Vee - BAD APPLE!! (Wither) [New Difficulty] (2026-04-04_10-22).osr";
            /*ultimate slider test replay2*/
            Dispatcher.Invoke(() =>
            {
                if (MusicPlayer.MusicPlayer.AudioFileExists() == true)
                {
                    ResetReplay();
                }

                replay = ReplayDecoder.GetReplayData(file, "replay");
                if (replay.GameMode != GameMode.Osu)
                {
                    MessageBox.Show($"Only replays from osu!standard gamemode are accepted. This replay is from {replay.GameMode}");
                    return;
                } 

                map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash, $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu");

                InitializeReplay();
            });
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

            if (SettingsPanel.SettingsPanelBox.Visibility == Visibility.Visible && Shortcuts.IsConfiguring == false)
            {
                SettingsPanel.SettingsPanelBox.Visibility = Visibility.Collapsed;
            }
        }
    }
}

