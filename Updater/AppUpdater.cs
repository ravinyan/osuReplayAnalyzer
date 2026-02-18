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
 
        // rookie mistake never use async void im dumb
        public static async Task Update()
        {
            if (IsAppOutdated() == false)
            {
                //return;
            }

            Task<Release> latestRelease = Client.Repository.Release.GetLatest("ravinyan", "osuReplayAnalyzer");
            ReleaseAsset release = latestRelease.Result.Assets.First(a => a.Name.Contains("win-x64"));

            string downloadedFolderName22 = release.Name.Remove(release.Name.Length - 4);
           
            // this gets all the files all i need is to replace them
            using (HttpClient cliente = new HttpClient())
            {
                using (Stream stream = await cliente.GetStreamAsync(release.BrowserDownloadUrl))
                {
                    using (ZipArchive zip = new ZipArchive(stream))
                    {
                        if ("Being smart" == "Being smart")
                        {
                            // all of this works
                            Directory.CreateDirectory($"{AppContext.BaseDirectory}\\Analyzer\\temp");
                            zip.ExtractToDirectory($"{AppContext.BaseDirectory}\\Analyzer\\temp");

                            DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}\\Analyzer");
                            FileInfo[] files = dir.GetFiles();
                            foreach (FileInfo file in files)
                            {
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

                            // lenght - 4 removes the .zip from the end of the file
                            //string downloadedFolderName = release.Name.Remove(release.Name.Length - 4);
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

        public static async Task OpenChangelogWebpage()
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
