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

            IHATEWPF.Click += delegate (object sender, RoutedEventArgs e)
            {
                AppUpdater.Update();
                Close();
            };

            IHATEWPF2.Click += delegate (object sender, RoutedEventArgs e)
            {
                AppUpdater.OpenChangelogWebpage();
            };
        }
    }
}