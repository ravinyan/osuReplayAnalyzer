using Octokit;
using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using OsuFileParsers.Decoders;
using ReplayAnalyzer.AnalyzerTools;
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
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Beatmap = OsuFileParsers.Classes.Beatmap.osu.Beatmap;
using Color = System.Drawing.Color;
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

random stuff
    .deity mode 3:46:500 good reverse arrow test case if 
    .checked if saving OsuMath calculations in fields and then using fields would be better and it is like 3x faster
     but its like <10ms difference total on 10min replay so i literally dont care... but good to know

    (not needed but maybe?)
        > if i feel like hating my own life then fix Random mod even tho i most likely cant do that
        > do SD and HD skin texture changing code just so i can see how much the difference is with RAM coz WHY NOT
           ^ tested on SD circles with circle only map and difference was not noticable so i dont see a point of adding additional
             option menu just for this BUT if someone somehow finds my app and will want that then i will add this option coz then why not
               ^ or if im bored and there is nothing to do (im kinda bored and there is nothing to do) < why i wrote this there is so much to do
        > make spinners work in case someone is worse than me at the game and misses them... and needs to analyze them... < NO
    
    (low prority)
        > learning how to make most of UI movable like in osu lazer would be cool
           ^ this would be JUST FOR LEARNING... and might never do that anyway
        > (already started) slowly make custom project for benchmarking speed and memory coz it needs classes/methods done in certain way
            ^ another JUST FOR LEARNING... i know my app is VERY fast and doesnt use much memory
              but i want to learn how to be even better in the future... and do absolute overkill optimalization here lmao
        > circle shake animation on notelock
           ^ i made circle change colour maybe will change it some other time
        > so here is funny thing... while getting all beatmap data takes like no time at all there is audio conversion to mp3
          and oh boy on 8min god only knows song it took 2s to convert the audio which is A LOT when everything else 
          loaded in like 100ms so... maybe find a way to have different audio players for different audio files?
          someone could think that 2s is very fast but if everything can loand in 100ms instead of 2500ms then it would be 
          nice to get that 25x speed boost... but im writing all that not knowing if that will even be possible lol
        > stop being dumb (impossible)

    (to do N O W)
        > when slider head is clicked even tho cursor is NOT in slider ball, slider ball should have expanded hitbox (it doesnt now)
        > try to make all animations by myself (fade in, approach circle and slider ball (thanks ppy for PositionAt also its done))
           ^ its 100% for learning purpose coz i DONT need better performance...
              ^ actually doing this now coz it seems fun and interesting and WPF is so HORRIBLE i rather do animations myself
                also it might fix bad "framerate" when there is a lot of objects on screen ("framerate coz wpf is SO BAD it
                shows me i have like 1000fps in performance profiler when app has like 10fps LIKE HOW)
              (this is massive MAYBE) > also do all these animations on separate thread and learn how to nicely use multithreading maybe?
           ^ here things to do later:
             spawn slider ball at the end of the slider when seeking backwards
             REPEAT SLIDERS MALFUNCTION
             there is random lag only when debugging and idk why
        > UI improvements (custom styled dropdowns (i fucking hate xaml styling), options menu maybe scalable with app size,
          and whatever else i feel like its worth doing)
        > when updating the app, make it so all config files are saved before updating and then update new config file with
          new values before the update... just in case its ReplayAnalyzer.dll.config
        > make Judgement Timeline optimized so a lot of judgements wont lag the map coz WPF is so horrible it cant handle
          500 objects that are never supposed to be updated and AAAAAA I HATE WPF
           ^ or make one massive path (well 3 for each judgement type + there are sliders with 500 path points and its fine) 
             if WPF doesnt want to cooperate then i WILL force it to
              ^ after checking out stuff i think this doesnt reduce performance and im just stupid?
                fps in profiler dont drop and i think my eyes are trolling me thinking there is some lag maybe 
                coz replay is on 2x speed... idk what to do now why wpf sucks and cant tell me if im stupid or not
                 ^ so i have waited >10min for performance profiler to finish analyzing fps drops and apparently there werent any... even tho app had 10fps... I HATE WPF
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
        public static Beatmap map { get; set; } = new Beatmap();
        public static Replay replay { get; set; } = new Replay();

        public static double OsuPlayfieldObjectScale { get; set; } = 0;
        public static double OsuPlayfieldObjectDiameter { get; set; } = 0;
        
        private static System.Timers.Timer timer = new System.Timers.Timer();

        public static bool IsReplayPreloading { get; set; } = true;

        /// <summary>
        /// Offset in ms before map starts
        /// </summary>
        public static int StartDelay = 0000; // was supposed to be for audio delay but now its useless but i dont want to delete this
        
        public MainWindow()
        {
            ResizeMode = ResizeMode.CanMinimize;
            InitializeComponent();

            PropertyInfo dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            PropertyInfo dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            osuReplayWindow.Width = int.Parse(SettingsOptions.GetConfigValue("ScreenResolution").Split('x')[0]) / ((int)dpiXProperty!.GetValue(null, null)! / 96.0);
            osuReplayWindow.Height = int.Parse(SettingsOptions.GetConfigValue("ScreenResolution").Split('x')[1]) / ((int)dpiYProperty!.GetValue(null, null)! / 96.0);

            timer.Interval = SettingsOptions.GetConfigValue("FPSLimit") != "Unlimited"
                           ? 1000 / double.Parse(SettingsOptions.GetConfigValue("FPSLimit"))
                           : 1;
            timer.Elapsed += TimerTick;

            #if DEBUG
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

            CursorSkin.ApplySkin();
        }

        // god i love this SO MUCH I WISH I KNEW IT EARLIER AAAAAAAAAAAAAAAA
        private void PlayfieldCanva_MouseMove(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine(e.GetPosition(playfieldCanva));

            //Debug.WriteLine(SliderEndJudgement.IsTracking);
            //Debug.WriteLine(SliderEndJudgement.IsJudged);
            //Debug.WriteLine();
            //Debug.WriteLine();
            //Debug.WriteLine();
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
                HitMarkerManager.UpdateHitMarkerAfterSeek(1, time);
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
            //    var a = math.GetOverallDifficultyHitWindow50();
            //    stopwatch.Stop();
            //    timeee += stopwatch.ElapsedTicks;
            //    stopwatch.Reset();
            //}

            //Debug.WriteLine(timeee);
            //Debug.WriteLine(OsuMaths.OsuMath.count);

            Playfield.ResetPlayfieldFields();

            // clear stuck objects except cursor which is at index 0
            for (int i = playfieldCanva.Children.Count - 1; i > 0; i--)
            {
               playfieldCanva.Children.Remove(playfieldCanva.Children[i]);
            }

            IsReplayPreloading = false;
            HitObjectAnimations.ClearStoryboardDict();
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

        void TimerTick(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
#if DEBUG
                //FpsTimer();
#endif
                HitObjectSpawner.UpdateHitObjects();
                CursorManager.UpdateCursorPosition();
                HitDetection.CheckIfObjectWasHit();

                FrameMarkerManager.UpdateFrameMarker();
                CursorPathManager.UpdateCursorPath();

                UpdateFadeAnimation(GamePlayClock.TimeElapsed);
                UpdateSliderBallAnimation(GamePlayClock.TimeElapsed);
                UpdateApproachCircleAnimation(GamePlayClock.TimeElapsed);

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
         
                // i may be stupid but i dont know how else to do this
                //if (GamePlayClock.IsPaused() == true)
                //{
                //    HitObjectAnimations.PauseAliveHitObjectAnimations();
                //}
                //else
                //{
                //    HitObjectAnimations.ResumeAliveHitObjectAnimations();
                //}

#if DEBUG
                //gameplayclock.Text = $"{GamePlayClock.TimeElapsed}";
                //musicclock.Text = $"{MusicPlayer.MusicPlayer.AudioFile.CurrentTime.TotalMilliseconds}";
#endif
                
            });
        }

        /* performance friend
           List<long> perf = new List<long>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int j = 0; j < 3; j++)
            {
                stopwatch.Restart();

                stopwatch.Stop();
                perf.Add(stopwatch.ElapsedTicks);
            }
            Console.WriteLine("F - " + perf[0] + " " + "S - " + perf[1] + " " + "T - " + perf[2]);
        */
        // after i do all animations here i will move functions to HitObjectAnimations class
        // this is all so much math i dont understand what im doing
        // with new animations there is very funny difference in CPU and GPU LMAO... test on same map same everything
        // old: stable 5-7% CPU and 5-8% GPU with some spikes and all that
        // new: very spiky CPU from 1% to max like 6% with average staying between 2% and 5% + GPU usage is halfed... literally lol its between 2% and 5%
        // RAM usage is kinda the same with new being slightly worse on average by 5MB or sometimes better by 10MB
        // there is also chance that setting opacity is very intensive and thats why there is this big difference... will see soon

        // now you...
        void UpdateFadeAnimation(double time)
        {
            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                double fadeInTime = OsuMath.GetFadeInTiming();
                double fullOpacityTime = aliveObjects[i].SpawnTime - OsuMath.GetApproachRateTiming() + fadeInTime;
                double progress = (fadeInTime) / (fullOpacityTime - time);
                if (progress < 0)
                {// otherwise circle will become invisible near the end of its lifetime
                    continue;
                }

                aliveObjects[i].Opacity = progress - 1;
            }
        }

        // done hopefully this code is fast... each iteration with 7 objects is jumping from 200 to 1000 ticks with 500 average?
        // actually why the fuc it jumps from 500 to 2000 sometimes even... its not even 1/4th of 1ms but still why lol
        OsuMaths.OsuMath OsuMath = new OsuMaths.OsuMath();
        void UpdateApproachCircleAnimation(double time)
        {
            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            System.Windows.Controls.Image approachCircle;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i] is HitCircle)
                {
                    approachCircle = HitCircle.ApproachCircle((HitCircle)aliveObjects[i]);
                }
                else if (aliveObjects[i] is Slider)
                {
                    approachCircle = Slider.HeadApproachCircle((Slider)aliveObjects[i]);
                }
                else
                {// i hate this here also this is special case coz spinners need different math
                    Spinner spinner = (Spinner)aliveObjects[i];

                    approachCircle = Spinner.ApproachCircle(spinner);

                    double spinnerSpawnTime = spinner.SpawnTime + Spinner.SpawnOffset;
                    if (time < spinnerSpawnTime)
                    {
                        Canvas.SetTop(approachCircle, -(approachCircle.Width * 1 / 2) + spinner.Height / 2);
                        Canvas.SetLeft(approachCircle, -(approachCircle.Width * 1 / 2) + spinner.Width / 2);

                        approachCircle.RenderTransform = new ScaleTransform(1, 1);

                        continue;
                    }

                    double duration = spinner.EndTime - (spinner.SpawnTime + Spinner.SpawnOffset);
                    double progresss = 1 - (time - spinnerSpawnTime) / (duration);

                    Canvas.SetTop(approachCircle, -(approachCircle.Width * progresss / 2) + spinner.Height / 2);
                    Canvas.SetLeft(approachCircle, -(approachCircle.Width * progresss / 2) + spinner.Width / 2);

                    approachCircle.RenderTransform = new ScaleTransform(progresss, progresss);
                    continue;
                }

                double approachRateTime = OsuMath.GetApproachRateTiming();
                double approachCircleSpawnTime = aliveObjects[i].SpawnTime - approachRateTime;
                double progress = 1 - (time - approachCircleSpawnTime) / (approachRateTime * 1.35); // 1.35 adjusted by hand
                if (progress < 0.25)// oh this works... lol
                {// here you stop the progress of approach circle so the size of it wont become smaller than circle itself
                    continue;
                }

                Canvas.SetTop(approachCircle, -(approachCircle.Width * progress / 2) + (aliveObjects[i].Width / 2));
                Canvas.SetLeft(approachCircle, -(approachCircle.Width * progress / 2) + (aliveObjects[i].Width / 2));

                // using RenderTransform to not override Width and Height... set up MaxWidth and MaxHeight
                // for better performance <<< adjusting width and height is slightly better but pain in the ass im not doing it lol
                approachCircle.RenderTransform = new ScaleTransform(progress, progress);
            }
        }

        // this should be optimized to be extremely fast i hope? idk how to do it significantly better at least
        // it takes like ~100-300ticks per loop check even with multiple sliders if i remember correctly
        void UpdateSliderBallAnimation(double time)
        {
            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i] is not Slider)
                {
                    continue;
                }

                Slider s = (Slider)aliveObjects[i];

                // i guess this needs to be somewhere at the start just like in seeking function
                if (Slider.HeadApproachCircle(s).Visibility == Visibility.Collapsed
                && (s.Judgement.Judgement > HitObjectJudgement.Miss && s.Judgement.SpawnTime > GamePlayClock.TimeElapsed
                ||  s.Judgement.Judgement <= HitObjectJudgement.Miss && s.SpawnTime > GamePlayClock.TimeElapsed))
                {
                    Slider.ShowSliderHead(s);
                }
                if (Slider.BodyBall(s).Visibility == Visibility.Visible)
                {
                    Slider.BodyBall(s).Visibility = Visibility.Collapsed;
                }

                double distance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                double position = (time - s.SpawnTime) / distance;
                if (position < 0)
                {
                    continue;
                }
                
                if (position > 1 && time - s.SpawnTime > s.EndTime - s.SpawnTime)
                {// if current distance based of time is higher than slider distance including repeats, snap position to 1 so ball
                 // wont go into reverse in some edge cases with very short sliders
                    position = 1;
                }
                
                if (position >= 1) // slider reached the end but reverse arrow didnt allow it to end
                {
                    int reverseCount = (int)position;
                    if (reverseCount % 2 == 1)
                    {
                        position = reverseCount - (position - reverseCount);
                    }
                    else
                    {
                        position = position - reverseCount;
                    }
                }
                
                Canvas ball = Slider.BodyBall(s);
                if (ball.Visibility == Visibility.Collapsed)
                {
                    ball.Visibility = Visibility.Visible;
                }

                // no i didnt misspell var... ok maybe
                Vector2 car = s.Path.PositionAt(position);
                // diameter * 1.4 is the ball size, needs to be calculated here otherwise resize will bork ball (also im so happy i solved this im so bad at math some blue prince puzzles were easier)
                // layout value is scale transform value applied to objects when app is resized, just in case M11 = X, M22 = Y
                // M11 used in both coz its square and it will be probably faster for compiler coz of value caching and stuff (unless i remember something wrong)
                Canvas.SetLeft(ball, (car.X * OsuPlayfieldObjectScale - OsuPlayfieldObjectDiameter * 1.4 / 2) / s.LayoutTransform.Value.M11);
                Canvas.SetTop(ball, (car.Y * OsuPlayfieldObjectScale - OsuPlayfieldObjectDiameter * 1.4 / 2) / s.LayoutTransform.Value.M11);
            }
        }

        public void ResetReplay()
        {
            timer.Close();

            // this needs to be on top otherwise app have stroke and crashes coz of some memory reading issue with SoundTouch
            RateChangerControls.ResetFields();

            MusicPlayer.MusicPlayer.ResetMusicPlayer();

            MusicPlayer.JudgementTimeline.ResetFields();

            HitObjectAnimations.sbDict.Clear();

            HitMarkerData.ResetFields();

            MissFinder.ResetFields();

            Playfield.ResetPlayfieldFields();

            JudgementCounter.Reset();

            for (int i = playfieldCanva.Children.Count - 1; i > 0; i--)
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
            IsReplayPreloading = true;

            HitMarkerData.CreateData();

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

        // move this function somewhere else but i have NO CLUE where yet... maybe when more skinning stuff is implemented? < if it will be implemented
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
                if (MusicPlayer.MusicPlayer.AudioFile != null)
                {
                    ResetReplay();
                }

                replay = ReplayDecoder.GetReplayData(file, "replay", StartDelay);
                if (replay.GameMode != GameMode.Osu)
                {
                    MessageBox.Show($"Only replays from osu!standard gamemode are accepted. This replay is from {replay.GameMode}");
                    return;
                } 

                map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash, StartDelay, $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu");
            
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

