namespace what
{
    public class FileWatchers
    {
        // since osu! and osu!lazer have different file paths this is just for osu!lazer
        public static string LazerReplayFileWatcher()
        {
            string fileName = "";
            string lazerReplayFilesPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\exports\\";

            CancellationTokenSource cts = new CancellationTokenSource();

            bool isFileAdded = false;
            FileSystemWatcher watcher = new FileSystemWatcher(lazerReplayFilesPath);
            watcher.Created += OnChanged;
            watcher.EnableRaisingEvents = true;
            //watcher.Filter = ".osr";

            while (isFileAdded == false)
            {
                Thread.Sleep(17);
            }

            return fileName == "" ? "" : fileName;

            void OnChanged(object source, FileSystemEventArgs e)
            {
                Console.WriteLine($"File: {e.FullPath} {e.ChangeType}");
                isFileAdded = true;

                // [0] is folder path
                // [1] is file name.osr
                // [2] some hash numbers or whatever not needed
                var path = e.Name;
                
                int separatorCount = 2
                foreach (char s in path)
                {
                    
                }

                //fileName = @$"{pathData[0]}{pathData[1]}";                
            }
        }
    }
}
