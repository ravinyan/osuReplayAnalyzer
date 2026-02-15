using Microsoft.VisualBasic.Devices;
using Octokit;
using System.IO;
using System.IO.Compression;
using System.Net.Http;

namespace ReplayAnalyzer.Updater
{
    public class Updater
    {
        private static GitHubClient Client = new GitHubClient(new ProductHeaderValue("ReplayAnalyzer"));
        // doing code here to quicly test file moving stuff but after move it to separate solution which will be separate .exe
        async void Update()
        {
            if (IsAppOutdated() == false)
            {
                return;
            }

            // make this a window with minimal UI

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
                            // second executable that is just for the setup... when update is available then launch analyzer + setup
                            // and in setup if user click option to update have some UI with update progress (idk how tho) and close analyzer
                            // if update is completed have in window 2 options for:
                            //  closing updater without re opening analyzer
                            //  closing updater and re open analyzer
                            // or just update and restart app coz why not

                            Directory.CreateDirectory($"{AppContext.BaseDirectory}/temp");
                            zip.ExtractToDirectory($"{AppContext.BaseDirectory}/temp");

                            // just GetFiles is enough coz i dont want to replace skin and osu folder
                            DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}");
                            FileInfo[] files = dir.GetFiles();
                            foreach (FileInfo file in files)
                            {
                                file.Delete();
                            }

                            DirectoryInfo tempDir = new DirectoryInfo($"{AppContext.BaseDirectory}/temp");
                            FileInfo[] tempFiles = tempDir.GetFiles();
                            foreach(FileInfo file in tempFiles)
                            {
                                file.MoveTo($"{AppContext.BaseDirectory}");
                            }
                            tempDir.Delete();
                        }
                    }
                }
            }
        }

        private static bool IsAppOutdated()
        {

            return false;
        }
    }
}
