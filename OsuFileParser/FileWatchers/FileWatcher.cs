namespace ReplayParsers.FileWatchers
{
    public class FileWatcher
    {
        private static readonly FileSystemWatcher watcher = new FileSystemWatcher();


        public static string OsuLazerReplayFileWatcher(string fileName, out string outFile)
        {
            string osuLazerFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\";
            watcher.Path = osuLazerFilePath;
            watcher.Created += OnCreated1;
            watcher.EnableRaisingEvents = true;
            //watcher.WaitForChanged(WatcherChangeTypes.Created);

            outFile = fileName;

            return fileName;

            void OnCreated1(object source, FileSystemEventArgs e)
            {
                // for some reason osu!lazer file generates "_" at the start and "_" at the end
                // + 36 randomly generated characters string at the end... so 36 + 2 "_"
                string fileName = $"{osuLazerFilePath}{e.Name!.Substring(1, e.Name.Length - 38)}";

                OsuLazerReplayFileWatcher(fileName, out fileName);
            }


        }

        public static string UpdateFilePath(string filePath, string file = "")
        {
            return filePath;
        }

        public static string OsuReplayFileWatcher()
        {
            string fileName = "";

            string osuFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!\\Replays\\";
            watcher.Path = osuFilePath;
            watcher.Created += OnCreated2;
            watcher.EnableRaisingEvents = true;
            watcher.WaitForChanged(WatcherChangeTypes.Created);

            return fileName;

            void OnCreated2(object source, FileSystemEventArgs e)
            {
                fileName = e.FullPath;
            }
        }
    }
}
