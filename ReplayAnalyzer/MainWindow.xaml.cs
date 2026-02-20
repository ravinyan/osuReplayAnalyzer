using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Replay;
using OsuFileParsers.Decoders;
using ReplayAnalyzer.AnalyzerTools;
using ReplayAnalyzer.AnalyzerTools.CursorPath;
using ReplayAnalyzer.AnalyzerTools.FrameMarkers;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.AnalyzerTools.KeyOverlay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.FileWatcher;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.KeyboardShortcuts;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using ReplayAnalyzer.PlayfieldUI;
using ReplayAnalyzer.SettingsMenu;
using ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions;
using System.Drawing;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Beatmap = OsuFileParsers.Classes.Beatmap.osu.Beatmap;
using Slider = ReplayAnalyzer.HitObjects.Slider;
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

    (not needed but maybe?) 
        > make spinners work in case someone is worse than me at the game and misses them... and needs to analyze them... ..... 
    
    (low prority)        
        > if i feel like hating my own life then fix Random mod even tho i most likely cant do that
        > learning how to make most of UI movable like in osu lazer would be cool
        > circle shake animation on notelock
           ^ i made circle change colour maybe will change it some other time
        > stop being dumb (impossible)

    (to do N O W)
        > make spinner animations correct coz why not
        > make sure to test updater for the 50th time coz there cant be any errors once its published
        > fix any bug found i guess

    (for later after N O W)
        > profit in skill increase

    (I HAVE NO CLUE DID I FIX IT OR NOT???)
       > fix hit judgements being off randomly by idk even what at this point i hate it here 
          ^ it is and always was implemented correctly but somehow results are a bit different... but math is from osu source code so i will assume it just works
       > application sometimes lags for 1 frame coz of i think slider end... and if not then just something with sliders
          ^ i have no clue if its coz of performance issue somewhere or what so i will not do that anytime soon since its not that bad
             ^ does this still exists? idk
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
        public static Beatmap map;
        public static Replay replay;

        public static double OsuPlayfieldObjectScale = 0;
        public static double OsuPlayfieldObjectDiameter = 0;
        
        public static System.Timers.Timer timer = new System.Timers.Timer();

        public static bool IsReplayPreloading = true;

        /// <summary>
        /// Offset in ms before map starts
        /// </summary>
        public static int StartDelay = 000;
        
        public MainWindow()
        {
            ResizeMode = ResizeMode.CanMinimize;
            InitializeComponent();

            PropertyInfo dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            PropertyInfo dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            osuReplayWindow.Width = int.Parse(SettingsOptions.GetConfigValue("ScreenResolution").Split('x')[0]) / ((int)dpiXProperty!.GetValue(null, null)! / 96.0);
            osuReplayWindow.Height = int.Parse(SettingsOptions.GetConfigValue("ScreenResolution").Split('x')[1]) / ((int)dpiYProperty!.GetValue(null, null)! / 96.0);

            timer.Interval = 1;
            timer.Elapsed += TimerTick;

            #if DEBUG

            KeyDown += LoadTestBeatmap;

            #endif

            startupInfo.Text = "Press F2 on replay screen in game to load replay. Loading time depends on amount of objects in a beatmap.\n" +
                               "Click Options Cog in top left > go to Files > choose osu! and/or osu!lazer folder. \n" +
                               "(its folder containing Beatmaps, Skins, etc. Location can be found in osu client options > Open osu! folder)";

            PlayfieldUI.PlayfieldUI.CreateUIElementsBeforeReplayLoaded();

            BeatmapFile.Load();

            ShortcutManager.Initialize();

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

            if (SettingsPanel.SettingsPanelBox.Visibility == Visibility.Visible && Shortcuts.IsConfiguring == false)
            {
                SettingsPanel.SettingsPanelBox.Visibility = Visibility.Collapsed;
            }
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
                CursorManager.UpdateCursor();

                // sometimes hit markers are not properly updated and always in the same spot... why idk this is scuffed fix and works
                HitMarkerManager.UpdateHitMarkerAfterSeek(1, time);
                HitDetection.CheckIfObjectWasHit();
           
                // maybe from here ticks might be needed but for now nothing is
                //stopwatch.Start();
                SliderTick.UpdateSliderBodyEvents();
                //stopwatch.Stop();
                //timeee += stopwatch.ElapsedTicks;
                //stopwatch.Reset();
                //SliderReverseArrow.UpdateSliderRepeatsPreload();
                //SliderEndJudgement.HandleSliderEndJudgement();

                HitObjectManager.HandleVisibleHitObjects();
                HitMarkerManager.HandleAliveHitMarkers();
                HitJudgementManager.HandleAliveHitJudgements();
            }

            // cleanup and reset of things
            GamePlayClock.Restart();

            songSlider.Value = 0;

            Playfield.ResetPlayfieldFields();

            // clear stuck objects except cursor which is at index 0
            for (int i = playfieldCanva.Children.Count - 1; i > 0; i--)
            {
               playfieldCanva.Children.Remove(playfieldCanva.Children[i]);
            }

            IsReplayPreloading = false;
            HitObjectAnimations.ClearStoryboardDict();
            HitMarkerManager.GetAliveDataHitMarkers().Clear();
        }

        //Stopwatch stopwatch = new Stopwatch();
        void TimerTick(object sender, ElapsedEventArgs e)
        {
            
            Dispatcher.InvokeAsync(() =>
            {
                //stopwatch.Start();

                HitObjectSpawner.UpdateHitObjects();
                CursorManager.UpdateCursor();
                HitDetection.CheckIfObjectWasHit();

                FrameMarkerManager.UpdateFrameMarker();
                CursorPathManager.UpdateCursorPath();

                //UpdateSliderBallPos(Slider.GetFirstSliderBySpawnTime(), GamePlayClock.TimeElapsed);

                SliderReverseArrow.UpdateSliderRepeats();
                SliderTick.UpdateSliderBodyEvents();
                SliderEndJudgement.HandleSliderEndJudgement();

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

                //stopwatch.Stop();
#if DEBUG
                //gameplayclock.Text = $"{GamePlayClock.TimeElapsed}";
                //musicclock.Text = $"{MusicPlayer.MusicPlayer.AudioFile.CurrentTime.TotalMilliseconds}";
#endif
            });

            //stopwatch.Reset();
        }

        // change so works for multiple sliders (just add loop) when ticks work
        // this is replacement for slider animations from WPF coz its for sure better for performance but idk if i will use it
        // maybe use when i have patience for fixing slider ticks again in case it will break something
        void UpdateSliderBallPos(Slider s, double time)
        {
            if (s == null)
            {
                return;
            }

            Canvas body = s.Children[0] as Canvas;
            Canvas ball = body.Children[2] as Canvas;

            double distance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            double position = (time - s.SpawnTime) / distance;
            if (position > 1) // slider reached the end but reverse arrow didnt allow it to end
            {
                if ((int)position % 2 == 1)
                {
                    int reverseCount = (int)position;
                    position = reverseCount - (position - reverseCount);
                }
                else
                {
                    int reverseCount = (int)position;
                    position = position - reverseCount;
                }  
            }

            bool aaa = Math.Abs(s.SliderTicks[0].PositionAt - position) <= 0.001;



            // no i didnt misspell var... ok maybe
            var car = s.Path.PositionAt(position);

            Canvas.SetLeft(ball, car.X - OsuPlayfieldObjectDiameter * 1.4 / 2);
            Canvas.SetTop(ball, car.Y - OsuPlayfieldObjectDiameter * 1.4 / 2);
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

        public void ResetReplay()
        {
            timer.Close();
            MusicPlayer.MusicPlayer.ResetMusicPlayer();
            HitObjectAnimations.sbDict.Clear();

            HitMarkerData.ResetFields();
            FrameMarkerData.ResetFields();
            CursorPathData.ResetFields();

            MissFinder.ResetFields();

            Playfield.ResetPlayfieldFields();
            MusicPlayer.JudgementTimeline.ResetFields();
            PlayfieldUI.UIElements.JudgementCounter.Reset();

            for (int i = playfieldCanva.Children.Count - 1; i > 0; i--)
            {
                playfieldCanva.Children.Remove(playfieldCanva.Children[i]);
            }

            GamePlayClock.Restart();
            songSlider.Value = 0;

            playerButton.Style = FindResource("PlayButton") as Style;
        }

        public void InitializeReplay()
        {
            IsReplayPreloading = true;

            CursorSkin.ApplySkin();

            HitMarkerData.CreateData();
            FrameMarkerData.CreateData();
            CursorPathData.CreateData();

            MusicPlayer.MusicPlayer.Initialize();

            BeatmapMods.Apply();

            playfieldBorder.Visibility = Visibility.Visible;

            ResizePlayfield.ResizePlayfieldCanva();

            GamePlayClock.Initialize();

            playfieldGrid.Children.Remove(startupInfo);

            PlayfieldUI.PlayfieldUI.CreateUIElementsAfterReplayLoaded();

            MusicPlayer.JudgementTimeline.Initialize();

            ApplyComboColoursFromSkin();

            PreloadWholeReplay();

            timer.Start();
        }

        // move this function somewhere else but i have NO CLUE where yet... maybe when more skinning stuff is implemented?
        public static void ApplyComboColoursFromSkin()
        {
            List<Color> colours = SkinIniProperties.GetComboColours();
            int index = 0;
            foreach (HitObjectData hitObjectData in map.HitObjects)
            {
                if (hitObjectData.ComboNumber == 1)
                {
                    index++;
                    if (index == colours.Count - 1)
                    {
                        index = 0;
                    }
                }

                hitObjectData.RGBValue = colours[index];
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
            /*marathon*/                      //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Lorien Testard - Une vie a t'aimer (Iced Out) [Stop loving me      I will always love you] (2025-08-06_19-33).osr";
            /*non hidden play*/               //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\criller playing Laur - Sound Chimera (Nattu) [Chimera] (2025-05-11_21-32).osr";
            /*the maze*/                      //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\-GN playing Erehamonika remixed by kors k - Der Wald (Kors K Remix) (Rucker) [Maze] (2020-11-08_20-27).osr";
            /*double click*/                  //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\worst hr player playing Erehamonika remixed by kors k - Der Wald (Kors K Remix) (Rucker) [fuckface] (2023-11-25_05-20).osr";
            /*slider tick miss*/              //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing twenty one pilots - Heathens (Magnetude Bootleg) (funny) [Marathon] (2025-09-15_07-28).osr";
            /*non slider tick miss*/          string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing twenty one pilots - Heathens (Magnetude Bootleg) (funny) [Marathon] (2023-01-06_01-39).osr";
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
            /*fix miss count thx*/            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Yooh - Eternity (Kojio) [Endless Suffering] (2025-10-23_13-15) (12).osr";
            /*i love song (audio problem)*/   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Kotoha - Aisuru Youni (Faruzan1577) [We live in loneliness] (2026-01-01_21-20) (10).osr";
            /*null timing point*/             //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\RyuuBei playing LukHash - 8BIT FAIRY TALE (Delis) [Extra] (2018-10-31_18-24).osr";
            /*slider stream walker*/          //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing AXIOMA - Rift Walker (osu!team) [Expert] (2025-08-05_19-34).osr";
            /*OSU LAZER MODS ARE REAL*/       //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing senya - Kasou no Kimi no Miyako (Satellite) [s] (2026-01-16_08-14) (1).osr";
            /*(not)wrong miss < im stupid*/   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing TK from Ling tosite sigure - first death (TV Size) (Kyuukai) [we'll be working together until death do us part] (2025-08-13_21-08).osr";
            /*another audio thing*/           //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Ludicin - Everlasting Eternity (R3m) [Till The Epilogue Of Time] (2024-11-15_21-40).osr";
            
            Dispatcher.Invoke(() =>
            {
                if (MusicPlayer.MusicPlayer.AudioFile != null)
                {
                    ResetReplay();
                }

                replay = ReplayDecoder.GetReplayData(file, StartDelay);
                if (replay.GameMode != GameMode.Osu)
                {
                    MessageBox.Show($"Only replays from osu!standard gamemode are accepted. This replay is from {replay.GameMode}");
                    return;
                }

                map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash, StartDelay, $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu");

                InitializeReplay();
            });
        }
    }
}

