using Octokit;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;

namespace Updater
{
    public class AppUpdater
    {
        private static readonly MainWindow Window = (MainWindow)System.Windows.Application.Current.MainWindow;
        private static GitHubClient Client = new GitHubClient(new ProductHeaderValue("ReplayAnalyzer"));
 
        // rookie mistake never use async void im dumb
        public static async Task Update()
        {
            Task<Release> latestRelease = Client.Repository.Release.GetLatest("ravinyan", "osuReplayAnalyzer");
            ReleaseAsset release = latestRelease.Result.Assets.First(a => a.Name.Contains("win-x64"));

            using (HttpClient cliente = new HttpClient())
            {
                using (Stream stream = await cliente.GetStreamAsync(release.BrowserDownloadUrl))
                {
                    Window.updateButton.Content = "Update in Progress...";
                    using (ZipArchive zip = new ZipArchive(stream))
                    {
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
                        string downloadedFolderName = release.Name.Remove(release.Name.Length - 4);
                        //string downloadedFolderName = "osu-replay-analyzer-win-x64 v0.5.0";
                        DirectoryInfo tempDir = new DirectoryInfo($"{AppContext.BaseDirectory}\\Analyzer\\temp\\{downloadedFolderName}\\Analyzer");
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

        public static async Task OpenChangelogWebpage()
        {
            Release latestRelease = await Client.Repository.Release.GetLatest("ravinyan", "osuReplayAnalyzer");

            Process.Start(new ProcessStartInfo(latestRelease.HtmlUrl) { UseShellExecute = true });
        }

        public static bool IsAppOutdated()
        {
            // i dont like this 1.0.0.0 format so this strips it to 1.0.0
            string fullVersion = typeof(AppUpdater).Assembly.GetName().Version!.ToString();
            string version = fullVersion.Remove(fullVersion.Length - 2);

            // remove "v" from tag name
            // also try catch coz my internet died and this gave exception and crashed app
            try
            {
                GitHubClient client = new GitHubClient(new ProductHeaderValue("ReplayAnalyzer"));
                Task<Release> latestRelease = client.Repository.Release.GetLatest("ravinyan", "osuReplayAnalyzer");
                if (latestRelease.Result.TagName.Substring(1) == version)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {// no internet in 2026 smh
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;
        }
    }
}
