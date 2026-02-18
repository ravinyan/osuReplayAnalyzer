using Octokit;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    public class Updates
    {
        public static void AddOptions(StackPanel panel)
        {
            panel.Children.Add(IsUpdateAvailableText());
            
        }

        private static TextBlock IsUpdateAvailableText()
        {
            TextBlock noticeText = new TextBlock();
            noticeText.Foreground = new SolidColorBrush(Colors.White);

            if (IsUpdateAvailable() == true)
            {
                noticeText.Text = "New Update Available";
            }
            else
            {
                noticeText.Text = "No Update Available.";
            }

            return noticeText;
        }

        private static bool IsUpdateAvailable()
        {
            // how the hell was i supposed to know it is possible... i hate it here
            // some apps have Directory.Build.props that has some info about app so need to look into that
            // since i need version number globally and this assembly thing is only for here
            Version? aa = typeof(Updates).Assembly.GetName().Version;
            GitHubClient Client = new GitHubClient(new ProductHeaderValue("ReplayAnalyzer"));
            Task<Release> latestRelease = Client.Repository.Release.GetLatest("ravinyan", "osuReplayAnalyzer");
            
            ReleaseAsset release = latestRelease.Result.Assets.First(a => a.Name.Contains("win-x64"));

            return false;
        }
    }
}
