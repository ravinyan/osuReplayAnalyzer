using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Decoders;
using System.IO;
using System.Windows;

namespace WpfApp1.Beatmaps.Replay
{
    public static class OsuReplay
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static FileSystemWatcher watcher = new FileSystemWatcher();

        public static void GetReplayFile(Beatmap map)
        {
            // $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\"
            // $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!\\Replays\\"
            watcher.Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports";
            watcher.EnableRaisingEvents = true;
            watcher.Created += OnCreated;

            void OnCreated(object sender, FileSystemEventArgs e)
            {
                if (Window.musicPlayer.MediaPlayer.Media != null)
                {
                    Window.musicPlayer.MediaPlayer.Stop();
                    Window.playfieldBackground.ImageSource = null;

                    // delay for now put exception later
                    Thread.Sleep(2000);
                }

                string file = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\{e.Name!.Substring(1, e.Name.Length - 38)}";
                MainWindow.map = BeatmapDecoder.GetOsuLazerBeatmap(file);

                MusicPlayer.MusicPlayer.InitializeMusicPlayer();
                Beatmaps.OsuBeatmap.Create();
            }
        }
    }
}
