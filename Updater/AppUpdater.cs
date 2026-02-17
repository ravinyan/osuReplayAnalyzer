using Octokit;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace Updater
{
    public class AppUpdater
    {
        private static GitHubClient Client = new GitHubClient(new ProductHeaderValue("ReplayAnalyzer"));

        // doing code here to quicly test file moving stuff but after move it to separate solution which will be separate .exe
        public static async void Update()
        {
            if (IsAppOutdated() == false)
            {
                //return;
            }

            Task<Release> releases = Client.Repository.Release.GetLatest("ravinyan", "osuReplayAnalyzer");
            ReleaseAsset aa = releases.Result.Assets.First(a => a.Name.Contains("win-x64"));

            // this gets all the files all i need is to replace them
            using (HttpClient cliente = new HttpClient())
            {
                using (Stream stream = await cliente.GetStreamAsync(aa.BrowserDownloadUrl))
                {
                    using (ZipArchive zip = new ZipArchive(stream))
                    {
                        if ("Being smart" == "Being dumb")
                        {
                            // all of this works
                            Directory.CreateDirectory($"{AppContext.BaseDirectory}\\Analyzer\\temp");
                            zip.ExtractToDirectory($"{AppContext.BaseDirectory}\\Analyzer\\temp");

                            DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}\\Analyzer");
                            FileInfo[] files = dir.GetFiles();
                            foreach (FileInfo file in files)
                            {
                                if (file.Name == "Updater.exe")
                                {
                                    continue;
                                }

                                file.Delete();
                            }

                            DirectoryInfo[] directories = dir.GetDirectories();
                            foreach (DirectoryInfo directory in directories)
                            {
                                if (directory.Name == "temp")
                                {
                                    continue;
                                }

                                directory.Delete(true);
                            }

                            // windows sucks so might need this if deleting files too fast is a problem...
                            //Thread.Sleep(1000);

                            // lenght - 4 removes the .zip from the end of the file
                            //string downloadedFolderName = aa.Name.Remove(aa.Name.Length - 4);
                            string downloadedFolderName = "osu-replay-analyzer-win-x64 v0.5.0";
                            DirectoryInfo tempDir = new DirectoryInfo($"{AppContext.BaseDirectory}\\Analyzer\\temp\\{downloadedFolderName}");
                            FileInfo[] tempFiles = tempDir.GetFiles();
                            foreach (FileInfo file in tempFiles)
                            {
                                file.MoveTo($"{AppContext.BaseDirectory}\\Analyzer\\{file.Name}");
                            }

                            DirectoryInfo[] tempDirectories = tempDir.GetDirectories();
                            foreach (DirectoryInfo directory in tempDirectories)
                            {
                                directory.MoveTo($"{AppContext.BaseDirectory}\\Analyzer\\{directory.Name}");
                            }

                            dir.GetDirectories("temp").First().Delete(true);
                        }
                    }
                }
            }
        }

        public static async void OpenChangelogWebpage()
        {
            Release latestRelease = await Client.Repository.Release.GetLatest("ravinyan", "osuReplayAnalyzer");
            string releaseLink = latestRelease.HtmlUrl;

            Process.Start(new ProcessStartInfo(releaseLink) { UseShellExecute = true });
        }

        private static bool IsAppOutdated()
        {

            return false;
        }
    }
}
