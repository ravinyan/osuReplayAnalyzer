using System.Windows;
using System.Windows.Controls;
using WpfApp1.GameClock;

namespace WpfApp1.MusicPlayer.Controls
{
    public static class PlayPauseControls
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void InitializeEvents()
        {
            Window.playerButton.Click += PlayPauseButton;
        }

        private static void PlayPauseButton(object sender, RoutedEventArgs e)
        {
            List<Canvas> aliveObjects = Playfield.Playfield.GetAliveHitObjects();
            if (Window.musicPlayer.MediaPlayer != null)
            {
                if (Window.playerButton.Style == Window.FindResource("PlayButton"))
                {
                    GamePlayClock.Start();
                    MusicPlayer.Play();

                    Window.playerButton.Style = Window.Resources["PauseButton"] as Style;

                    //foreach (Canvas o in aliveObjects)
                    //{
                    //    //HitObjectAnimations.Resume(o);
                    //}
                }
                else
                {
                    GamePlayClock.Pause();
                    MusicPlayer.Pause();

                    // this one line just correct very small offset when pausing...
                    // from testing it doesnt cause any audio problems or any delay anymore so yaaay
                    Window.musicPlayer.MediaPlayer.Time = GamePlayClock.TimeElapsed;
                    Window.playerButton.Style = Window.Resources["PlayButton"] as Style;

                    //foreach (Canvas o in aliveObjects)
                    //{
                    //    //HitObjectAnimations.Pause(o);
                    //}
                }
            }
        }
    }
}
