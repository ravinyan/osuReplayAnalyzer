using OsuFileParsers.Classes.Replay;
using OsuFileParsers.Decoders;
using ReplayAnalyzer.AnalyzerTools;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.FileWatcher;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.KeyboardShortcuts;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using ReplayAnalyzer.PlayfieldUI;
using ReplayAnalyzer.SettingsMenu;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Beatmap = OsuFileParsers.Classes.Beatmap.osu.Beatmap;
using Slider = ReplayAnalyzer.Objects.Slider;

#nullable disable
// https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/

// note for custom DT and HT rate changes: its impossible to implement due to how lazer implements it so goodbye
// spent 5h checking everywhere in osu lazer source code and they take it from air i dont understand how lol (i do tho)
// there is possibility of using osu API v2 to get custom mods but the point of this app was not using API so... pain

/*  mostly things to do when i will do everything else working on and have nothing else to do

    (not needed but maybe?) 
        > maybe do slider tick and end judgements ?

    (low prority)
        > make Frame Markers like in osu lazer
        > make Cursor Path like in osu lazer
        > make slider border lines instead of full on thiccer slider body
        > try making opaque path in the middle of the slider to give effect kinda like osu sliders have in the middle
        > extremely big maps (probably in hitobject count but maybe also in time? need to test) have some performance problems so fix that
        > do configurable keybinds coz why not i guess

    (to do N O W)
        > fix hit judgements being off randomly by idk even what at this point i hate it here 
          i used 1:1 osu math and it just doesnt work AAAAAAAAAAAAAAAAA

    (for later after N O W)
        > why one time slider head didnt spawn when seeking backwards by frame...
        > application sometimes lags for 1 frame coz of i think slider end... and if not then just something with sliders
        > make hit markers not spawn (flash for like 1ms) when they shouldnt when seeking BACKWARDS by frame      
        > profit in skill increase

    (I HAVE NO CLUE DID I FIX IT OR NOT???)
       
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

        public static bool IsReplayPreloading = true;

        public MainWindow()
        {
            //Visibility = Visibility.Hidden;
            ResizeMode = ResizeMode.NoResize;
            InitializeComponent();

            PropertyInfo dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            PropertyInfo dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            osuReplayWindow.Width = int.Parse(SettingsOptions.config.AppSettings.Settings["ScreenResolution"].Value.Split('x')[0]) / ((int)dpiXProperty!.GetValue(null, null)! / 96.0);
            osuReplayWindow.Height = int.Parse(SettingsOptions.config.AppSettings.Settings["ScreenResolution"].Value.Split('x')[1]) / ((int)dpiYProperty!.GetValue(null, null)! / 96.0);

            timer.Interval = 1;
            timer.Elapsed += TimerTick;

            #if DEBUG

            KeyDown += LoadTestBeatmap;

            #endif

            startupInfo.Text = "Press F2 on replay screen in game to load replay.\n" +
                               "Click Options Cog in top left to choose osu! and/or osu!lazer folder. \n" +
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

            if (SettingsPanel.SettingPanelBox.Visibility == Visibility.Visible)
            {
                SettingsPanel.SettingPanelBox.Visibility = Visibility.Hidden;
            }
        }

        // the purpose of this is to mark all objects hit correctly, and in the future some cool stuff like jumping to misses maybe?
        // maybe one day i could make specific functions for this preloading to be faster... maybe
        public void PreloadWholeReplay()
        {
            for (int i = 0; i < replay.Frames.Count; i++)
            {
                long time = replay.Frames[i].Time;
                GamePlayClock.Seek(time);

                HitObjectSpawner.UpdateHitObjects();

                HitMarkerManager.HandleAliveHitMarkers();

                // sometimes hit markers are not properly updated and always in the same spot... why idk this is scuffed fix and works
                HitMarkerManager.UpdateHitMarkerAfterSeek(1);
 
                CursorManager.UpdateCursor();
                HitDetection.CheckIfObjectWasHit();

                HitJudgementManager.HandleAliveHitJudgements();
                HitObjectManager.HandleVisibleHitObjects();

                SliderTick.UpdateSliderTicks();
                SliderReverseArrow.UpdateSliderRepeats();
                SliderEndJudgement.HandleSliderEndJudgement();
            }

            // cleanup and reset of things
            GamePlayClock.Restart();

            songSlider.Value = 0;

            foreach (HitObject hitObject in OsuBeatmap.HitObjectDictByIndex.Values)
            {
                if (hitObject is Slider)
                {
                    Slider.ResetToDefault(hitObject);
                } 
            }

            Playfield.ResetPlayfieldFields();

            // clear stuck objects except cursor at index 0
            for (int i = playfieldCanva.Children.Count - 1; i > 0; i--)
            {
                playfieldCanva.Children.Remove(playfieldCanva.Children[i]);
            }

            IsReplayPreloading = false;
        }

        void TimerTick(object sender, ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                HitObjectSpawner.UpdateHitObjects();
                CursorManager.UpdateCursor();
                HitDetection.CheckIfObjectWasHit();

                HitObjectManager.HandleVisibleHitObjects();
                HitMarkerManager.HandleAliveHitMarkers();
                HitJudgementManager.HandleAliveHitJudgements();

                SliderTick.UpdateSliderTicks();
                SliderReverseArrow.UpdateSliderRepeats();
                SliderEndJudgement.HandleSliderEndJudgement();

#if DEBUG
                // gameplayclock.Text = $"{GamePlayClock.TimeElapsed}";
                // musicclock.Text = $"{musicPlayer.MediaPlayer.Time}";
#endif

                if (SongSliderControls.IsDragged == false && musicPlayer.MediaPlayer.IsPlaying == true)
                {
                    var aaa = GamePlayClock.TimeElapsed;
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

        public void ResetReplay()
        {
            timer.Close();
            musicPlayer.MediaPlayer.Stop();
            musicPlayer.MediaPlayer.Media = null;
            musicPlayer.MediaPlayer = null;
            playfieldBackground.ImageSource = null;
            OsuBeatmap.HitObjectDictByIndex.Clear();
            HitObjectAnimations.sbDict.Clear();
            Analyzer.HitMarkers.Clear();
            Playfield.ResetPlayfieldFields();

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

            Analyzer.CreateHitMarkers();

            OsuBeatmap.Create(map);

            MusicPlayer.MusicPlayer.Initialize();

            playfieldBorder.Visibility = Visibility.Visible;

            ResizePlayfield.ResizePlayfieldCanva();

            GamePlayClock.Initialize();

            playfieldGrid.Children.Remove(startupInfo);

            PlayfieldUI.PlayfieldUI.CreateUIElementsAfterReplayLoaded();

            RateChangerControls.ChangeBaseRate();

            PreloadWholeReplay();

            //JudgementCounter.Reset();

            timer.Start();
        }

        void Tetoris()
        {
            // i hate how i memorized the memory consumption of every file here after being rendered as beatmap
            // not rendering slider tail circle (which is ugly anyway and like 10 people use it) saves 400mb ram!
            // on marathon map and almost 1gb on mega marathon
            /*circle only*/                   string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Why] (2025-04-02_17-15).osr";
            /*slider only*/                   //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Kensuke x Ascended_s EX] (2025-03-22_12-46).osr";
            /*mixed*/                         //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\ravinyan playing Hiiragi Magnetite - Tetoris (AirinCat) [Extra] (2025-03-26_21-18).osr";
            /*mega marathon*/                 //string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\Trail Mix playing Aqours - Songs Compilation (Sakurauchi Riko) [Sweet Sparkling Sunshine!!] (2024-07-21_03-49).osr";
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

            Dispatcher.Invoke(() =>
            {
                if (musicPlayer.MediaPlayer != null)
                {
                    ResetReplay();
                }

                replay = ReplayDecoder.GetReplayData(file);
                map = BeatmapDecoder.GetOsuLazerBeatmap(replay.BeatmapMD5Hash);

                InitializeReplay();
            });
        }
    }
}

