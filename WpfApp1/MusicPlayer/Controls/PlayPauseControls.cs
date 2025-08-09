using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Animations;

namespace WpfApp1.MusicPlayer.Controls
{
    public static class PlayPauseControls
    {
        private static MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void InitializeEvents()
        {
            Window.playerButton.Click += PlayPauseButton;
        }

        private static void PlayPauseButton(object sender, RoutedEventArgs e)
        {
            if (Window.musicPlayer.MediaPlayer != null)
            {
                if (Window.playerButton.Style == Window.FindResource("PlayButton"))
                {
                    Window.musicPlayer.MediaPlayer.Play();
                    Window.stopwatch.Start();
                    Window.playerButton.Style = Window.Resources["PauseButton"] as Style;

                    PauseAnimations(Window.AliveCanvasObjects);
                }
                else
                {
                    Window.musicPlayer.MediaPlayer.Pause();
                    Window.stopwatch.Stop();

                    // this one line just correct very small offset when pausing...
                    // from testing it doesnt cause any audio problems or any delay anymore so yaaay
                    Window.musicPlayer.MediaPlayer.Time = Window.stopwatch.ElapsedMilliseconds;
                    Window.playerButton.Style = Window.Resources["PlayButton"] as Style;

                    PauseAnimations(Window.AliveCanvasObjects);
                }
            }
        }

        private static void PauseAnimations(List<Canvas> aliveObjects)
        {
            foreach (Canvas o in aliveObjects)
            {
                HitCircleAnimation.Pause(o);
            }
        }
    }
}
