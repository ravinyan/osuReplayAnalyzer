using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using ReplayParsers;
using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Decoders;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // https://stackoverflow.com/questions/46415019/how-to-draw-a-pause-button-using-wpf-path
            canva.Width = SystemParameters.PrimaryScreenWidth;
            canva.Height = 50;
            //Beatmap map = BeatmapDecoder.GetOsuLazerBeatmap();
            
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timerTick!;
            timer.Start();
        }

        void timerTick(object sender, EventArgs e)
        {
            if (musicPlayer.Source != null)
            {
                songTimer.Text = String.Format("{0} / {1}", musicPlayer.Position.ToString(@"mm\:ss"), musicPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss"));
            }
            else
            {
                songTimer.Text = "nothing";
            }
        }

        void OnClick(object sender, RoutedEventArgs e)
        {
            background.ImageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Background\\bg.jpg"));

            musicPlayer.Source = new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Audio\\audio.mp3");
            musicPlayer.Volume = 0.05;
        }
    }
}