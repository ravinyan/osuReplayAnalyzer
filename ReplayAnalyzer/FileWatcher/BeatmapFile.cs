using OsuFileParsers.Decoders;
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
            // if someone changes name of folders here for any reason then i will die i guess coz why would you do it
            // and i have no clue how i would detect that
            string replayFolderName;
            if (SettingsOptions.GetConfigValue("OsuClient") == "osu!")
            {
                path = $"{SettingsOptions.GetConfigValue("OsuStableFolderPath")}";
                replayFolderName = "Replays";
            }
            else if (SettingsOptions.GetConfigValue("OsuClient") == "osu!lazer")
            {
                path = $"{SettingsOptions.GetConfigValue("OsuLazerFolderPath")}";
                replayFolderName = "exports";
            }
            else // some error idk what
            {
                path = "";
                replayFolderName = "";
            }

            // its mostly to avoid crash with default folder path set by app when opening app for the first time
            // if app starts and osu is not in default osu path then return to not cause crash in watcher.Path
            // it only looks for Replay/exports folder for osu!/osu!lazer coz both osu clients are adding folders/files when they dont exist
            if (path == "" || Path.Exists($"{path}\\{replayFolderName}") == false)
            {
                return;
            }

            watcher.Path = $"{path}\\{replayFolderName}";
            watcher.EnableRaisingEvents = true;
            watcher.Created += OnCreated;
            
            void OnCreated(object sender, FileSystemEventArgs e)
            {
                Window.Dispatcher.Invoke(() =>
                {
                    if (MusicPlayer.MusicPlayer.AudioFile != null)
                    {
                        Window.ResetReplay();
                    }

                    string file;
                    if (SettingsOptions.GetConfigValue("OsuClient") == "osu!")
                    {
                        file = $"{path}\\Replays\\{e.Name}";

                        MainWindow.replay = ReplayDecoder.GetReplayData(file, MainWindow.StartDelay);
                        MainWindow.map = BeatmapDecoder.GetOsuBeatmap(MainWindow.replay.BeatmapMD5Hash!, MainWindow.StartDelay, path);
                    }
                    else if (SettingsOptions.GetConfigValue("OsuClient") == "osu!lazer")
                    {
                        // osu lazer for some reason have random string of numbers/letters in replay file
                        // when getting file name from file watcher... and its always 38 characters long
                        file = $"{path}\\exports\\{e.Name!.Substring(1, e.Name.Length - 38)}";

                        MainWindow.replay = ReplayDecoder.GetReplayData(file, MainWindow.StartDelay);
                        MainWindow.map = BeatmapDecoder.GetOsuLazerBeatmap(MainWindow.replay.BeatmapMD5Hash!, MainWindow.StartDelay, path);
                    }

                    Window.InitializeReplay();
                });
            }
        }
    }
}
