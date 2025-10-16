using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Classes.Replay;
using ReplayParsers.Decoders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.MusicPlayer;
using WpfApp1.PlayfieldGameplay;
using WpfApp1.PlayfieldUI;

namespace WpfApp1.FileWatcher
{
    internal class BeatmapFile
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static FileSystemWatcher watcher = new FileSystemWatcher();

        public static void Load()
        {
            // $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\"
            // $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!\\Replays\\"
            watcher.Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports";
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
 
                    string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\{e.Name!.Substring(1, e.Name.Length - 38)}";
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
