using System.Windows;
using System.Windows.Controls;
using WpfApp1.Animations;
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
            var a = Playfield.Playfield.GetAliveHitObjects();
            if (Window.musicPlayer.MediaPlayer != null)
            {
                if (Window.playerButton.Style == Window.FindResource("PlayButton"))
                {
                    Window.musicPlayer.MediaPlayer.Play();
                    GameClock.GamePlayClock.StartGameplayClock();
                    Window.playerButton.Style = Window.Resources["PauseButton"] as Style;

                    foreach (Canvas o in a)
                    {
                        HitCircleAnimation.Pause(o);
                    }
                }
                else
                {
                    Window.musicPlayer.MediaPlayer.Pause();
                    GameClock.GamePlayClock.StopGameplayClock();

                    // this one line just correct very small offset when pausing...
                    // from testing it doesnt cause any audio problems or any delay anymore so yaaay
                    Window.musicPlayer.MediaPlayer.Time = GamePlayClock.GetElapsedTime();
                    Window.playerButton.Style = Window.Resources["PlayButton"] as Style;

                    foreach (Canvas o in a)
                    {
                        HitCircleAnimation.Resume(o);
                    }
                }
            }
        }
    }
}
