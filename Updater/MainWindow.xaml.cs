using System.Diagnostics;
using System.Windows;

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // only allow update button to work when there is update available to not download stuff for nothing
            // always allow changelog coz it just opens web page with latest changes so it wont hurt if someone clicks it
            if (AppUpdater.IsAppOutdated() == true)
            {
                notificationText.Text = "There is new update available, see Changelog for details.";

                updateButton.Click += async delegate (object sender, RoutedEventArgs e)
                {
                    updateButton.Content = "Update in Progress...";

                    // close this if open coz otherwise windows cries coz files are open
                    Process? anaylyzer = Process.GetProcessesByName("ReplayAnalyzer").FirstOrDefault() ?? null;
                    if (anaylyzer != null)
                    {
                        anaylyzer.Kill();
                    }
                    
                    await AppUpdater.Update();
                    Close();
                };
            }
            else
            {
                notificationText.Text = "There is currently no new update available.";
            }

            changelogButton.Click += async delegate (object sender, RoutedEventArgs e)
            {
                await AppUpdater.OpenChangelogWebpage();
            };
        }
    }
}