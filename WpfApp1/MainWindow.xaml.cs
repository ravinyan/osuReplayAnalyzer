using Microsoft.Win32;
using ReplayParsers;
using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Decoders;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            //canva.Width = SystemParameters.PrimaryScreenWidth;
            canva.Height = 50;
            //Beatmap map = BeatmapDecoder.GetOsuLazerBeatmap();

            // https://wpf-tutorial.com/audio-video/how-to-creating-a-complete-audio-video-player/

            slider.Minimum = 0;
            slider.Maximum = 1000;

            

            musicPlayer.Source = new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Audio\\audio.mp3");
            musicPlayer.Volume = 0.05;

           //DispatcherTimer timer = new DispatcherTimer();
           //timer.Interval = TimeSpan.FromSeconds(1);
           ////timer.Tick += timer_Tick;
           //timer.Start();

            //songTimer.Text = "";
        }

        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;



        //private void timer_Tick(object sender, EventArgs e)
        //{
        //    if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
        //    {
        //        sliProgress.Minimum = 0;
        //        sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
        //        sliProgress.Value = mePlayer.Position.TotalSeconds;
        //    }
        //}
        //
        //private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = true;
        //}
        //
        //private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    mePlayer.Source = new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Audio\\audio.mp3");
        //}
        //
        //private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        //}
        //
        //private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    mePlayer.Play();
        //    mediaPlayerIsPlaying = true;
        //}
        //
        //private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = mediaPlayerIsPlaying;
        //}
        //
        //private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    mePlayer.Pause();
        //}
        //
        //private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = mediaPlayerIsPlaying;
        //}
        //
        //private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    mePlayer.Stop();
        //    mediaPlayerIsPlaying = false;
        //}
        //
        //private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        //{
        //    userIsDraggingSlider = true;
        //}
        //
        //private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        //{
        //    userIsDraggingSlider = false;
        //    mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        //}
        //
        //private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    lblProgressStatus.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        //}
        //
        //private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    mePlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        //}



        void timerTick(object sender, EventArgs e)
        {
            

        }

        void StartSong(object sender, RoutedEventArgs e)
        {
            background.ImageSource = new BitmapImage(new Uri($"{AppDomain.CurrentDomain.BaseDirectory}\\osu\\Background\\bg.jpg"));


            //MediaTimeline timeline = new MediaTimeline(musicPlayer.Source);
            //musicPlayer.Clock = timeline.CreateClock(true) as MediaClock;
            //musicPlayer.Clock!.Controller.Seek(TimeSpan.FromSeconds(0), TimeSeekOrigin.BeginTime);
            //
            //if (musicPlayer.Clock.CurrentProgress != null)
            //{
            //
            //    
            //}

            

            //button.Visibility = Visibility.Collapsed;
            //button2.Visibility = Visibility.Visible;
            //musicPlayer.Play();

            //songTimer.Text = $"{musicPlayer.Position.ToString(@"mm\:ss")} - {musicPlayer.NaturalDuration.TimeSpan.TotalSeconds}";
        }

        void PauseSong(object sender, RoutedEventArgs e)
        {
            //button.Visibility = Visibility.Visible;
            //button2.Visibility = Visibility.Collapsed;
            //musicPlayer.Pause();
        }
    }
}