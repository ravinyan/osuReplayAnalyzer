using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Replay;
using OsuFileParsers.Decoders;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.AnalyzerTools.KeyOverlay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.FileWatcher;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.KeyboardShortcuts;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using ReplayAnalyzer.PlayfieldUI;
using ReplayAnalyzer.SettingsMenu;
using ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Input;
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

    (not needed but maybe?) 
        > maybe do slider tick and end judgements ? < NO. maybe but... N O.
        > think about using osu API v2 for custom osu lazer mods (only Difficulty Reduction/Increase and Difficulty Adjust, no Fun mods)
           ^ only do that if there will be actually people using this app... otherwise NO THANK YOU I DONT WANT TO USE API GIVE ME MODS IN REPLAY FILE PEPPYYYYY
        > make spinners work... not needed coz its effort and if someone misses spinner... skill issue + there is nothing to analyze in spinner miss
       
    (low prority)
        > audio offset (after music player is changed)
        > learning how to make most of UI movable like in osu lazer would be cool
        > make Frame Markers like in osu lazer (WPF perfomance will also not like this)
        > make Cursor Path like in osu lazer (WPF performance will NOT like this) 
        > figure out math for object animation when changing speed rate (animations kinda scuffed but works without problems)
           ^ lol did it 5min after writing this out... but anyway i guess do that for spinners one day
        > small visual bug when seeking backwards onto last beatmap object where sliders for 1 frame MIGHT show ticks and stuff
           ^ like anyone would even find or care about that i cant care enough to put it higher in priority

    (to do N O W) this main update will be not about features but about improvements so DO NOT ADD NEW STUFF IDIOT
        > improve preload speed
        > audio can go negative with song slider and it gives delay a
        > find problems and improve app by adding MessageBox.Show() in some places (if needed) for better clarity why stuff is not working
          properly, test random stuff to find and fix crashes (if there are any), just look for possible improvements

    (for later after N O W)
        > profit in skill increase

    (audio... whenever i feel like)      
       > there is some audio problem? on everlasting eternity the further replay is in the further the audio is from where it should be
           ^ check maps that start with DT/HT coz maybe thats issue (the map started with HT)
       > kotoha song has some weird audio delay thingy and its just that map as of now
           ^ im starting to doubt its audio problems and think its beatmap problem like it has some offset set up
             but even then i think i tested maps with offsets and blank audio at the start and there werent any problems...
             even if its offest sometimes audio plays correctly on beat and sometimes it delayed lol how to understand

    (I HAVE NO CLUE DID I FIX IT OR NOT???)
       > fix hit judgements being off randomly by idk even what at this point i hate it here 
          ^ it is and always was implemented correctly but somehow results are a bit different... but math is from osu source code so i will assume it just works
       > application sometimes lags for 1 frame coz of i think slider end... and if not then just something with sliders
          ^ i have no clue if its coz of performance issue somewhere or what so i will not do that anytime soon since its not that bad

    (tried and not going to do)
        > make slider border lines instead of full on thiccer slider body
        > try making opaque path in the middle of the slider to give effect kinda like osu sliders have in the middle
           ^ genuinely furious how i tried to find solution for this but it didnt work and i do not know even 1% math to do this myself so im giving up on both of this before i punch my monitor...

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
            //Visibility = Visibility.Hidden;
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

        // delete all data stuff after everything is done then put commit before deletion here so i have it for fun
        public void PreloadWholeReplay()
        {
            Stopwatch watch = new Stopwatch();
           
            // wait a second i think my brain cooked when i was gaming (or it is cooked)
            List<KeyValuePair<int, ReplayFrame>> replayFrames = replay.FramesDict.Where(rf => rf.Value.Click != 0).ToList();
            // just make separate functions optimized for preloading amount of frames doesnt change anything
            //replay.FramesDict.Count
            //replayFrames.Count()
            //  tested without debugger
            //  on exp 33 map
            //  new: 7485, 7217, 7980
            //  old: 8370, 8275, 8314
            //  on mega marthon aqours
            //  new: 38235, 35872, 36640 
            //  old: 42178, 46026, 45500
            // its faster but on expedition 33 i saw 62 miss count on new which is 1 too many... old was correct tho
            //  ^ WITH DEBUGGER why my times on aquors are now <20000ms (18k more or less) when i changed nothing i think? huh
            //                  same on exp33 wtf what did i do LOL gt 3500ms...
            //    WITHOUT DEBUGGER aquors: 11000ms AND IT GOT TO 9500ms ONCE???????
            //                     exp33 : 2800 average
            //                             a3700-4000
            //                             e900-1000

            long gameplayseek = 0;
            long spawner = 0;
            long cursor = 0;
            long hitmarkerseek = 0;
            long hitdetect = 0;
            long slidertick = 0;
            long sliderevers = 0;
            long sliderend = 0;
            long objectmanager = 0;
            long markermanager = 0;
            long judgementmanager = 0;
            //watch.Start();
            for (int i = 0; i < replay.FramesDict.Count; i++)
            //for (int i = 0; i < replayFrames.Count(); i++)
            {
                    watch.Start();
                long time = replay.FramesDict[i].Time;
                //long time = replayFrames[i].Value.Time;
                GamePlayClock.Seek(time);
                    watch.Stop();
                    gameplayseek += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              HitObjectSpawner.UpdateHitObjects();
                    watch.Stop();
                    spawner += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              CursorManager.UpdateCursor();
                    watch.Stop();
                    cursor += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              // sometimes hit markers are not properly updated and always in the same spot... why idk this is scuffed fix and works
              HitMarkerManager.UpdateHitMarkerAfterSeek(1);
                    watch.Stop();
                    hitmarkerseek += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              HitDetection.CheckIfObjectWasHit();
                    watch.Stop();
                    hitdetect += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              
              //SliderTick.UpdateSliderTicks();
                    watch.Stop();
                    slidertick += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              //SliderReverseArrow.UpdateSliderRepeats();
                    watch.Stop();
                    sliderevers += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              //SliderEndJudgement.HandleSliderEndJudgement();
                    watch.Stop();
                    sliderend += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              
              HitObjectManager.HandleVisibleHitObjects();
                    watch.Stop();
                    objectmanager += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              HitMarkerManager.HandleAliveHitMarkers();
                    watch.Stop();
                    markermanager += watch.ElapsedTicks;
                    watch.Reset();
                    watch.Start();
              HitJudgementManager.HandleAliveHitJudgements();
                    watch.Stop();
                    judgementmanager += watch.ElapsedTicks;
                    watch.Reset();
            }
            watch.Stop();

#if DEBUG
            gameplayclock.Text = $"t: {watch.ElapsedTicks}";
            musicclock.Text = $"m: {watch.ElapsedMilliseconds}";
#endif

            // i shall improve this before Ato or i will die trying
            Debug.WriteLine($"gameplayseek      = {gameplayseek};\r\n" +
                            $"spawner           = {spawner};\r\n" +
                            $"cursor            = {cursor};\r\n" +
                            $"hitmarkerseek     = {hitmarkerseek};\r\n" +
                            $"hitdetect         = {hitdetect};\r\n" +
                            $"slidertick        = {slidertick};\r\n" +
                            $"sliderevers       = {sliderevers};\r\n" +
                            $"sliderend         = {sliderend};\r\n" +
                            $"objectmanager     = {objectmanager};\r\n" +
                            $"markermanager     = {markermanager};\r\n" +
                            $"judgementmanager  = {judgementmanager};");

            var s = "";
            // performance in ticks divide by 10,000 and its 1ms
            // everything with debugger on
            // this is from eternity
            // spawner          = 16013028; 17990887;  18959915;
            // hitdetect        = 4150773;  4496927;   5533798;
            // hitmarkerseek    = 1461275;  1449140;   1537570;
            // objectmanager    = 884044;   1366707;   1477245;
            // markermanager    = 309332;   438762;    453716;
            // judgementmanager = 241567;   294546;    314590;
            // cursor           = 209704;   189650;    176084;
            // sliderend        = 81120;    93932;     119175;
            // slidertick       = 50951;    56572;     59637;
            // sliderevers      = 35291;    41386;     44997;
            // gameplayseek     = 27184;    35891;     36932;

            // in ms to easier visualize without decimal points
            // spawner          = 1601; 1799;  1895;
            // hitdetect        = 415;  449;   553;
            // hitmarkerseek    = 146;  144;   153;
            // objectmanager    = 88;   136;   147;
            // markermanager    = 30;   43;    45;
            // judgementmanager = 24;   29;    31;
            // cursor           = 20;   18;    17;
            // sliderend        = 8;    9;     11;
            // slidertick       = 5;    5;     5;
            // sliderevers      = 3;    4;     4;
            // gameplayseek     = 2;    3;     3;

            // this is from aquors
            //spawner           = 112860095;  129027417;  120994202;
            //hitmarkerseek     = 37714238;   39964301;   36268596;
            //hitdetect         = 28895633;   31439383;   28692094;
            //objectmanager     = 2990965;    3537356;    3559812;
            //markermanager     = 1369422;    1797036;    1599825;
            //cursor            = 1100938;    1006992;    1259087;
            //judgementmanager  = 859498;     1199734;    926339;
            //slidertick        = 375538;     420755;     371755;
            //sliderend         = 333354;     337043;     331666;
            //sliderevers       = 193824;     224865;     198823;
            //gameplayseek      = 161106;     200050;     164944;

            // in ms
            //spawner           = 11286;  12902;  12099;
            //hitmarkerseek     = 3771;   3996;   3626;
            //hitdetect         = 2889;   3143;   2869;
            //objectmanager     = 299;    353;    355;
            //markermanager     = 136;    179;    159;
            //cursor            = 110;    100;    125;
            //judgementmanager  = 85;     119;    92;
            //slidertick        = 37;     42;     37;
            //sliderend         = 33;     33;     33;
            //sliderevers       = 19;     22;     19;
            //gameplayseek      = 16;     20;     16;

            // aquo after optimalization
            // why using binary search speeds seek by a lot but makes hitdetect slower when it relies on this....
            //hitdetect         = 1593;  1657;  1931; //hitdetect         = 2091; 2307; 2144;
            //hitmarkerseek     = 1333;  1510;  1472; //hitmarkerseek     = 24; 25;  21; 
            //spawner           = 206;   207;   253; < lol
            //objectmanager     = 181;   176;   234;
            //markermanager     = 87;    88;    112;
            //cursor            = 70;    111;   82;
            //judgementmanager  = 71;    66;    76;
            //gameplayseek      = 10;    12;    14;
            //slidertick        = 2;     2;     2;
            //sliderevers       = 1;     1;     2;
            //sliderend         = 2;     1;     2;

            // updated once more after more optimalizations
            //hitdetect         = 1965; 1900; 1862; < I SEE YOU
            //markermanager     = 237;  248;  209;
            //spawner           = 224;  199;  228;
            //objectmanager     = 202;  195;  206;
            //cursor            = 74;   81;   76;
            //judgementmanager  = 64;   67;   84;
            //hitmarkerseek     = 20;   20;   21;
            //gameplayseek      = 12;   11;   11;
            //slidertick        = 2;    2;    2;
            //sliderevers       = 2;    1;    2;
            //sliderend         = 1;    1;    2;

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

        Stopwatch stopwatch = new Stopwatch();
        void TimerTick(object sender, ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                stopwatch.Start();

                HitObjectSpawner.UpdateHitObjects();
                CursorManager.UpdateCursor();
                HitDetection.CheckIfObjectWasHit();

                SliderTick.UpdateSliderTicks();
                SliderReverseArrow.UpdateSliderRepeats();
                SliderEndJudgement.HandleSliderEndJudgement();

                HitObjectManager.HandleVisibleHitObjects();
                HitMarkerManager.HandleAliveHitMarkers();
                HitJudgementManager.HandleAliveHitJudgements();

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

                stopwatch.Stop();
#if DEBUG
                //gameplayclock.Text = $"{stopwatch.ElapsedTicks}";
                //musicclock.Text = $"{HitObjectAnimations.sbDict.Count}";
#endif
            });

            stopwatch.Reset();
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
            Playfield.ResetPlayfieldFields();
            MusicPlayer.JudgementTimeline.ResetFields();

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

            OsuBeatmap.ModifyDifficultyValues(replay.ModsUsed.ToString());

            HitMarkerData.CreateHitMarkerDataObjects();

            MusicPlayer.MusicPlayer.Initialize();

            playfieldBorder.Visibility = Visibility.Visible;

            ResizePlayfield.ResizePlayfieldCanva();

            GamePlayClock.Initialize();

            playfieldGrid.Children.Remove(startupInfo);
            
            PlayfieldUI.PlayfieldUI.CreateUIElementsAfterReplayLoaded();

            RateChangerControls.ChangeBaseRate();

            MusicPlayer.JudgementTimeline.Initialize();

            ApplyComboColoursFromSkin();

            PreloadWholeReplay();

            //PlayfieldUI.UIElements.JudgementCounter.Reset();

            timer.Start();
        }

        // move this function somewhere else but i have NO CLUE where yet... maybe when more skinning stuff is implemented?
        public static void ApplyComboColoursFromSkin()
        {
            List<Color> colours = SkinIniProperties.GetComboColours();
            int index = -1;
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
            /*mega marathon*/                 string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Aqours - Songs Compilation (Sakurauchi Riko) [Sweet Sparkling Sunshine!!] (2024-07-21_03-49).osr";
            /*olibomby sliders/tech*/         //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\MALISZEWSKI playing Raphlesia & BilliumMoto - My Love (Mao) [Our Love] (2023-12-09_23-55).osr";
            /*marathon*/                      //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Lorien Testard - Une vie a t'aimer (Iced Out) [Stop loving me      I will always love you] (2025-08-06_19-33).osr";
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
            /*fix miss count thx*/            //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Yooh - Eternity (Kojio) [Endless Suffering] (2025-10-23_13-15) (12).osr";
            /*i love song (audio problem)*/   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Kotoha - Aisuru Youni (Faruzan1577) [We live in loneliness] (2026-01-01_21-20) (10).osr";
            /*null timing point*/             //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\RyuuBei playing LukHash - 8BIT FAIRY TALE (Delis) [Extra] (2018-10-31_18-24).osr";
            /*slider stream walker*/          //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing AXIOMA - Rift Walker (osu!team) [Expert] (2025-08-05_19-34).osr";

            Dispatcher.Invoke(() =>
            {
                if (MusicPlayer.MusicPlayer.AudioFile != null)
                {
                    ResetReplay();
                }

                replay = ReplayDecoder.GetReplayData(file, StartDelay);

                map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash, StartDelay, $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu");

                /*  stress testing for artificially increased object count for preloading   
                //map.HitObjects.AddRange(map.HitObjects);
                //map.HitObjects.AddRange(map.HitObjects);
                //map.HitObjects.AddRange(map.HitObjects);
                //map.HitObjects.AddRange(map.HitObjects);
                //int i = 0;
                //foreach (var item in map.HitObjects)
                //{
                //    if (item is CircleData)
                //    {
                //        var aa = HitCircle.CreateCircle((CircleData)item, 50, 1, i, Color.FromArgb(15, 203, 245));
                //        i++;
                //
                //        aa.Dispose();
                //    }
                //    else if (item is SliderData)
                //    {
                //        var aa = Slider.CreateSlider((SliderData)item, 50, 1, i, Color.FromArgb(15, 203, 245));
                //        i++;
                //
                //        aa.Dispose();
                //    }
                //    else
                //    {
                //        var aa = Spinner.CreateSpinner((SpinnerData)item, 50, i);
                //        i++;
                //
                //        aa.Dispose();
                //    }
                //}
                */

                InitializeReplay();
            });
        }
    }
}

