using OsuFileParsers.Classes.Replay;
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

            (string path, string replayFolderName) osuFolder = GetOsuClientFolderPath();
            if (osuFolder.path != "")
            {
                watcher.Path = $"{osuFolder.path}\\{osuFolder.replayFolderName}";
                watcher.EnableRaisingEvents = true;
                watcher.Created += OnCreated;
            }

            // this feels messy and good at the same time coz of handling errors here and i cant come up with a way to make it look nicer rip
            void OnCreated(object sender, FileSystemEventArgs e)
            {
                Window.Dispatcher.Invoke(() =>
                {
                    if (MusicPlayer.MusicPlayer.AudioFile != null)
                    {
                        Window.ResetReplay();
                    }

                    string replayFilePath;
                    if (SettingsOptions.GetConfigValue("OsuClient") == "osu!")
                    {
                        replayFilePath = $"{osuFolder.path}\\Replays\\{e.Name}";
                        try
                        {
                            MainWindow.replay = ReplayDecoder.GetReplayData(replayFilePath, e.Name!, MainWindow.StartDelay);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex}", "Error while getting osu!stable replay.");
                            return;
                        }

                        if (MainWindow.replay.FramesDict.Count == 0)
                        {
                            MessageBox.Show("This replay is not available anymore, there are no frames to construct replay from. If it's Personal Best replay, osu!stable only saves replay data for current top 1000 plays on global leaderboards.", "Invalid Replay");
                            return;
                        }

                        if (MainWindow.replay.GameMode != GameMode.Osu)
                        {
                            MessageBox.Show($"Only replays from osu!standard gamemode are accepted. This replay is from {MainWindow.replay.GameMode}");
                            return;
                        }

                        // additional check just in case for osu!stable Songs folder when its being somehow changed and osu path is not
                        // so that user doesnt need to update path every change... it doesnt affect performance too so better have this than not
                        UpdateOsuStableSongsFolderLocation(osuFolder.path);
                        string songsFolderOutOfOsuPath = SettingsOptions.GetConfigValue("OsuStableSongsFolderPath") != ""
                                                       ? SettingsOptions.GetConfigValue("OsuStableSongsFolderPath")
                                                       : "";

                        try
                        {
                            MainWindow.map = BeatmapDecoder.GetOsuStableBeatmap(MainWindow.replay.BeatmapMD5Hash!, MainWindow.StartDelay, osuFolder.path, songsFolderOutOfOsuPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex}", "Error while getting osu!stable beatmap.");
                            return;
                        }
                    }
                    else if (SettingsOptions.GetConfigValue("OsuClient") == "osu!lazer")
                    {
                        // osu lazer for some reason have random string of numbers/letters in replay file
                        // when getting file name from file watcher... and its always 38 characters long
                        replayFilePath = $"{osuFolder.path}\\exports\\{e.Name!.Substring(1, e.Name.Length - 38)}";
                        try
                        {
                            MainWindow.replay = ReplayDecoder.GetReplayData(replayFilePath, e.Name!.Substring(1, e.Name.Length - 38), MainWindow.StartDelay);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex}", "Error while getting osu!lazer replay.");
                            return;
                        }

                        // its just osu!stable problem coz lazer saves all replays but will throw it here just in case
                        if (MainWindow.replay.FramesDict.Count == 0)
                        {
                            MessageBox.Show("This replay is not available anymore, there are no frames to construct replay from. If it's Personal Best replay, osu!stable only saves replay data for current top 1000 plays on global leaderboards.", "Invalid Replay");
                            return;
                        }

                        if (MainWindow.replay.GameMode != GameMode.Osu)
                        {
                            MessageBox.Show($"Only replays from osu!standard gamemode are accepted. This replay is from {MainWindow.replay.GameMode}");
                            return;
                        }

                        try
                        {
                            MainWindow.map = BeatmapDecoder.GetOsuLazerBeatmap(MainWindow.replay.BeatmapMD5Hash!, MainWindow.StartDelay, osuFolder.path);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{ex}", "Error while getting osu!lazer beatmap.");
                            return;
                        }
                    }

                    Window.InitializeReplay();
                });
            }
        }

        public static void LoadPreviousReplay()
        {
            string beatmapFilePath = FilePath.GetBeatmapFilePath();
            string replayFilePath = FilePath.GetReplayPath();
            string replayFileName = FilePath.GetReplayName();

            Window.Dispatcher.Invoke(() =>
            {
                if (MusicPlayer.MusicPlayer.AudioFile != null)
                {
                    Window.ResetReplay();
                }

                // exceptions still needed for at least replay
                try
                {
                    MainWindow.replay = ReplayDecoder.GetReplayData(replayFilePath, replayFileName, MainWindow.StartDelay);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Error while getting previous replay data.");
                    return;
                }

                if (MainWindow.replay.FramesDict.Count == 0)
                {
                    MessageBox.Show("This replay is not available anymore, there are no frames to construct replay from. If it's Personal Best replay, osu!stable only saves replay data for current top 1000 plays on global leaderboards.", "Invalid Replay");
                    return;
                }

                if (MainWindow.replay.GameMode != GameMode.Osu)
                {
                    MessageBox.Show($"Only replays from osu!standard gamemode are accepted. This replay is from {MainWindow.replay.GameMode}");
                    return;
                }

                try
                {
                    MainWindow.map = BeatmapDecoder.GetPreviouslyLoadedReplay(beatmapFilePath, MainWindow.StartDelay);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex}", "Error while getting previous beatmap data.");
                    return;
                }
                
                Window.InitializeReplay();
            });
        }

        private static (string, string) GetOsuClientFolderPath()
        {
            string path;
            string replayFolderName;
            if (SettingsOptions.GetConfigValue("OsuClient") == "osu!")
            {
                path = $"{SettingsOptions.GetConfigValue("OsuStableFolderPath")}";
                replayFolderName = "Replays";
            }
            else // if not osu then osu!lazer
            {
                path = $"{SettingsOptions.GetConfigValue("OsuLazerFolderPath")}";
                replayFolderName = "exports";
            }

            // its mostly to avoid crash with default folder path set by app when opening app for the first time
            // if app starts and osu is not in default osu path then return to not cause crash in watcher.Path
            // it only looks for Replay/exports folder for osu!/osu!lazer coz both osu clients are adding folders/files when they dont exist
            if (Path.Exists($"{path}\\{replayFolderName}") == false)
            {
                return ("", "");
            }

            return (path, replayFolderName);
        }

        private static void UpdateOsuStableSongsFolderLocation(string path)
        {
            if (Path.Exists($"{path}\\Replays") == true
            &&  Path.Exists($"{path}\\osu!.db") == true
            &&  Path.Exists($"{path}\\osu!.{Environment.UserName}.cfg") == true
            &&  Path.Exists($"{path}\\Songs") == false)
            {
                // if everything but Songs folder exists that means the path is set up in config file so yoink it and done
                string[] configLines = File.ReadAllLines($"{path}\\osu!.{Environment.UserName}.cfg");
                string[] split = new string[2];
                for (int i = 0; i < configLines.Length; i++)
                {
                    if (configLines[i].Contains("BeatmapDirectory"))
                    {
                        split = configLines[i].Split(" = ");
                        break;
                    }
                }

                SettingsOptions.SaveConfigOption("OsuStableSongsFolderPath", split[1]);
            }
            else if (Path.Exists($"{path}\\Replays") == true
            &&       Path.Exists($"{path}\\osu!.db") == true
            &&       Path.Exists($"{path}\\Songs") == true)
            {
                SettingsOptions.SaveConfigOption("OsuStableSongsFolderPath", "");
            }
        }
    }
}
