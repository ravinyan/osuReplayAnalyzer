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

            IHATEWPF.Click += async delegate (object sender, RoutedEventArgs e)
            {
                await AppUpdater.Update();
                Close();
            };

            IHATEWPF2.Click += async delegate (object sender, RoutedEventArgs e)
            {
                await AppUpdater.OpenChangelogWebpage();
            };
        }
    }
}