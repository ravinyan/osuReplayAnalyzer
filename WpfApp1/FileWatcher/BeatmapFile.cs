using ReplayParsers.Decoders;
using System.IO;
using System.Windows;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.PlayfieldGameplay;
using WpfApp1.PlayfieldUI;
using WpfApp1.SettingsMenu;

namespace WpfApp1.FileWatcher
{
    internal class BeatmapFile
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static FileSystemWatcher watcher = new FileSystemWatcher();

        public static void Load()
        {
            

            string path;
            if (SettingsOptions.config.AppSettings.Settings["OsuClient"].Value == "stable")
            {
                path = $"{SettingsOptions.config.AppSettings.Settings["OsuStableFolderPath"].Value}\\Replays\\";
            }
            else if (SettingsOptions.config.AppSettings.Settings["OsuClient"].Value == "lazer")
            {
                path = $"{SettingsOptions.config.AppSettings.Settings["OsuLazerFolderPath"].Value}\\exports";
            }
            else // some error idk what
            {
                path = "";
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
                    if (SettingsOptions.config.AppSettings.Settings["OsuClient"].Value == "stable")
                    {
                        file = $"{path}\\{e.Name}";
                    }
                    else if (SettingsOptions.config.AppSettings.Settings["OsuClient"].Value == "lazer")
                    {
                        file = $"{path}\\{e.Name!.Substring(1, e.Name.Length - 38)}";
                    }
                    else
                    {
                        file = "";
                    }

                    MainWindow.replay = ReplayDecoder.GetReplayData(file);
                    MainWindow.map = BeatmapDecoder.GetOsuLazerBeatmap(MainWindow.replay.BeatmapMD5Hash);

                    MusicPlayer.MusicPlayer.Initialize();

                    Analyser.Analyser.CreateHitMarkers();

                    OsuBeatmap.Create(MainWindow.map);

                    Window.playfieldBorder.Visibility = Visibility.Visible;
                    ResizePlayfield.ResizePlayfieldCanva();

                    GamePlayClock.Initialize();

                    Window.playfieldGrid.Children.Remove(Window.fpsCounter);

                    MainWindow.timer.Start();
                });
            }
        }
    }
}
