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
                            // save for now, will be deleted later
                            if (file.Name == "ReplayAnalyzer.dll.config")
                            {
                                continue;
                            }

                            file.Delete();
                        }
                        // update files array to contain only config file that is left
                        files = dir.GetFiles();

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
                        DirectoryInfo tempDir = new DirectoryInfo($"{AppContext.BaseDirectory}\\Analyzer\\temp\\{downloadedFolderName}\\Analyzer");
                        FileInfo[] tempFiles = tempDir.GetFiles();
                        for (int i = 0; i < tempFiles.Length; i++)
                        {
                            FileInfo file = tempFiles[i];
                            if (file.Name == "ReplayAnalyzer.dll.config")
                            {
                                string fileFullPath = file.FullName;
                                List<string> oldFileText = File.ReadAllLines(files[0].FullName).ToList();
                                List<string> newFileText = File.ReadAllLines(fileFullPath).ToList();

                                // delete all config files since all data that is needed has been extracted from them
                                file.Delete();
                                files[0].Delete();

                                List<(string key, string value)> oldConfigs = ExtractKeysAndValues(oldFileText);
                                List<(string key, string value)> newConfigs = ExtractKeysAndValues(newFileText);

                                for (int j = 0; j < newConfigs.Count; j++)
                                {
                                    // configs can change name, position or just get poofed so looping like that seems like safest option
                                    for (int k = 0; k < oldConfigs.Count; k++)
                                    {
                                        if (newConfigs[j].key == oldConfigs[k].key)
                                        {
                                            newConfigs[j] = (oldConfigs[k].key, oldConfigs[k].value);
                                            break;
                                        }
                                    }
                                }

                                int l = 0;
                                for (int j = 0; j < newFileText.Count; j++)
                                {
                                    if (newFileText[j].Contains("<add"))
                                    {// \t\t for indentation for bonus style points
                                        newFileText[j] = $"\t\t{SettingBlueprint(newConfigs[l].key, newConfigs[l].value)}";
                                        l++;
                                    }
                                }

                                // now that the original file got deleted, create new file in the same place with same name
                                // and fill it with new correct data
                                using (StreamWriter outputFile = new StreamWriter(fileFullPath))
                                {
                                    for (int j = 0; j < newFileText.Count; j++)
                                    {
                                        outputFile.WriteLine(newFileText[j]);
                                    }
                                }

                                File.Move(fileFullPath, $"{AppContext.BaseDirectory}\\Analyzer\\ReplayAnalyzer.dll.config");
                                continue;
                            }

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
            string version;
            try
            {
                // i dont like this 1.0.0.0 format so this strips it to 1.0.0
                // also this checks for the analyzer exe version coz updater code wont be updated
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo($"{AppContext.BaseDirectory}\\Analyzer\\ReplayAnalyzer.exe");
                version = fileVersionInfo.FileVersion!.Remove(fileVersionInfo.FileVersion.Length - 2);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "ReplayAnalyzer.exe not found in Analyzer folder."); return false; }

            try
            {// also try catch coz my internet died and this gave exception and crashed app
                GitHubClient client = new GitHubClient(new ProductHeaderValue("ReplayAnalyzer"));
                Task<Release> latestRelease = client.Repository.Release.GetLatest("ravinyan", "osuReplayAnalyzer");

                // remove "v" from tag name
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

        private static List<(string key, string value)> ExtractKeysAndValues(List<string> configLines)
        {
            List<(string key, string value)> fileConfigs = new List<(string key, string value)>();
            for (int i = 0; i < configLines.Count; i++)
            {
                if (!configLines[i].Contains("<add"))
                {
                    continue;
                }

                bool startSavingKey = false;
                bool startSavingValue = false;
                string key = "";
                string value = "";
                for (int j = 0; j < configLines[i].Length; j++)
                {
                    // the structure looks like this: key="ShowHitMarkers" value="true"
                    if (configLines[i][j] == 'y' && configLines[i][j + 1] == '=')
                    {
                        startSavingKey = true;
                        j += 3;
                    }

                    while (startSavingKey == true)
                    {
                        if (configLines[i][j] == '"')
                        {
                            startSavingKey = false;
                            break;
                        }

                        string temp = key + configLines[i][j];
                        key = temp;
                        j++;
                    }

                    if (configLines[i][j] == 'e' && configLines[i][j + 1] == '=')
                    {
                        startSavingValue = true;
                        j += 3;
                    }

                    while (startSavingValue == true)
                    {
                        if (configLines[i][j] == '"')
                        {
                            startSavingValue = false;
                            break;
                        }

                        string temp = value + configLines[i][j];
                        value = temp;
                        j++;
                    }
                }

                fileConfigs.Add((key, value));
            }

            return fileConfigs;
        }

        private static string SettingBlueprint(string key, string value)
        {
            return $"<add key=\"{key}\" value=\"{value}\" />";
        }
    }
}
