using OsuFileParsers.Decoders;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldUI;
using ReplayAnalyzer.SettingsMenu;
using System.IO;
using System.Windows;

namespace ReplayAnalyzer.FileWatcher
{
    internal class BeatmapFile
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static FileSystemWatcher watcher = new FileSystemWatcher();

        public static void Load()
        {
            // its for resetting file watcher when changing osu versions to avoid bugs and unnecessary pain
            watcher = new FileSystemWatcher();

            string path;
            if (SettingsOptions.config.AppSettings.Settings["OsuClient"].Value == "osu!")
            {
                path = $"{SettingsOptions.config.AppSettings.Settings["OsuStableFolderPath"].Value}\\Replays";
            }
            else if (SettingsOptions.config.AppSettings.Settings["OsuClient"].Value == "osu!lazer")
            {
                path = $"{SettingsOptions.config.AppSettings.Settings["OsuLazerFolderPath"].Value}\\exports";
            }
            else // some error idk what
            {
                path = "";
            }

            if (path == "" || Path.Exists(path) == false)
            {
                return;
            }

            watcher.Path = path;
            watcher.EnableRaisingEvents = true;
            watcher.Created += OnCreated;
            
            void OnCreated(object sender, FileSystemEventArgs e)
            {
                Window.Dispatcher.Invoke(() =>
                {
                    if (Window.musicPlayer.MediaPlayer != null)
                    {
                        MainWindow.timer.Close();
                        Window.musicPlayer.MediaPlayer.Stop();
                        Window.musicPlayer.MediaPlayer = null;
                        Window.playfieldBackground.ImageSource = null;
                        OsuBeatmap.HitObjectDictByIndex.Clear();
                        OsuBeatmap.HitObjectDictByTime.Clear();
                        HitObjectAnimations.sbDict.Clear();
                        Analyser.Analyser.HitMarkers.Clear();
                        Playfield.ResetVariables();

                        for (int i = Window.playfieldCanva.Children.Count - 1; i >= 1; i--)
                        {
                            Window.playfieldCanva.Children.Remove(Window.playfieldCanva.Children[i]);
                        }

                        GamePlayClock.Restart();
                        Window.songSlider.Value = 0;

                        Window.playerButton.Style = Window.FindResource("PlayButton") as Style;
                    }

                    string file;
                    if (SettingsOptions.config.AppSettings.Settings["OsuClient"].Value == "osu!")
                    {
                        file = $"{path}\\{e.Name}";

                        MainWindow.replay = ReplayDecoder.GetReplayData(file);
                        MainWindow.map = BeatmapDecoder.GetOsuBeatmap(MainWindow.replay.BeatmapMD5Hash!);
                    }
                    else if (SettingsOptions.config.AppSettings.Settings["OsuClient"].Value == "osu!lazer")
                    {
                        // osu lazer for some reason have random string of numbers/letters in replay file
                        // when getting file name from file watcher... and its always 38 characters long
                        file = $"{path}\\{e.Name!.Substring(1, e.Name.Length - 38)}";

                        MainWindow.replay = ReplayDecoder.GetReplayData(file);
                        MainWindow.map = BeatmapDecoder.GetOsuLazerBeatmap(MainWindow.replay.BeatmapMD5Hash!);
                    }

                    OsuBeatmap.ModifyDifficultyValues(MainWindow.replay.ModsUsed.ToString());

                    MusicPlayer.MusicPlayer.Initialize();

                    Analyser.Analyser.CreateHitMarkers();

                    OsuBeatmap.Create(MainWindow.map);

                    Window.playfieldBorder.Visibility = Visibility.Visible;
                    ResizePlayfield.ResizePlayfieldCanva();

                    GamePlayClock.Initialize();

                    Window.playfieldGrid.Children.Remove(Window.startupInfo);

                    MainWindow.timer.Start();
                });
            }
        }
    }
}
