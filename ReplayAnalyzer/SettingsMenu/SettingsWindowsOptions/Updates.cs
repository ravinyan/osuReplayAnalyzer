using Octokit;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    public class Updates
    {
        private static readonly MainWindow Window = (MainWindow)System.Windows.Application.Current.MainWindow;

        public static void AddOptions(StackPanel panel)
        {
            panel.Children.Add(IsUpdateAvailableText());
            panel.Children.Add(OpenUpdaterButton());
        }

        private static Button OpenUpdaterButton()
        {
            Button button = new Button();
            button.Height = 25;
            button.Width = 100;
            button.Margin = new Thickness(7);
            button.HorizontalAlignment = HorizontalAlignment.Center;
            button.Content = "Open Updater";

            button.Click += delegate (object sender, RoutedEventArgs e)
            {
                OpenUpdater();
            };

            return button;
        }

        private static TextBlock IsUpdateAvailableText()
        {
            TextBlock noticeText = new TextBlock();
            noticeText.Foreground = new SolidColorBrush(Colors.White);
            noticeText.TextAlignment = TextAlignment.Center;

            if (IsUpdateAvailable() == true)
            {
                noticeText.Text = "New Update is Available";
            }
            else
            {
                noticeText.Text = "No Update Available.";
            }

            return noticeText;
        }

        private static bool IsUpdateAvailable()
        {
            // i dont like this 1.0.0.0 format so this strips it to 1.0.0
            string fullVersion = typeof(Updates).Assembly.GetName().Version!.ToString();
            string version = fullVersion.Remove(fullVersion.Length - 2);

            GitHubClient client = new GitHubClient(new ProductHeaderValue("ReplayAnalyzer"));
            Task<Release> latestRelease = client.Repository.Release.GetLatest("ravinyan", "osuReplayAnalyzer");

            // remove "v" from tag name
            if (latestRelease.Result.TagName.Substring(1) == version)
            {
                return false;
            }

            OpenUpdater();
            return true;
        }

        private static void OpenUpdater()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}");
                //Process.Start($"{dir.Parent!.ToString()}\\Updater.exe");
                Process.Start($@"C:\Users\patry\source\repos\OsuReplayAnalyzer\Updater\bin\Debug\net8.0-windows\\Updater.exe");
            }
            catch
            {
                MessageBox.Show("Updater was not found in parent folder of Analyzer.");
            }
        }
    }
}
