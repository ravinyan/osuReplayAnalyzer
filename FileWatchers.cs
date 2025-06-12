using System.Text;

namespace ReplayParsers
{
    public class FileWatchers
    {
        // since osu! and osu!lazer have different file paths this is just for osu!lazer
        public static string OsuLazerReplayFileWatcher()
        {
            string fileName = "";
            string osuLazerReplayFilesPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\";

            FileSystemWatcher watcher = new FileSystemWatcher(osuLazerReplayFilesPath);
            watcher.Created += OnChanged;
            watcher.EnableRaisingEvents = true;

            bool isFileAdded = false;
            while (isFileAdded == false)
            {
                Thread.Sleep(17);
            }

            return fileName == "" ? "" : fileName;

            void OnChanged(object source, FileSystemEventArgs e)
            {
                Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
                isFileAdded = true;

                StringBuilder path = new StringBuilder(e.Name);

                int l = 0;
                int r = path.Length - 1;
                bool swap = false;

                // need to delete "_" from e.Name and files can have "_" in the name
                // so i need to delete first "_" from left and then change pointer to then end of string and delete
                // first "_" from the right
                for (int i = 0; i < path.Length; i++)
                {
                    if (path[l] == '_')
                    {
                        path.Remove(l, 1);

                        if (swap == true)
                        {
                            break;
                        }

                        l = r - 1;
                        swap = true;
                        continue;
                    }

                    path.Remove(l, 1);

                    if (swap == false)
                    {
                        l++;
                    }
                    else
                    {
                        l--;
                    }
                }

                fileName = @$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\osu\exports\{path}";
                Console.WriteLine(fileName);
            }
        }

        public static string OsuReplayFileWatcher()
        {
            string fileName = "";
            string osuReplayFilesPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!\\Replays\\";

            bool isFileAdded = false;
            while (isFileAdded == false)
            {
                Thread.Sleep(17);
            }

            return "";
        }
    }
}
