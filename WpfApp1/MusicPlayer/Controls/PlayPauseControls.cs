using System.Windows;
using System.Windows.Controls;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.Objects;
using WpfApp1.PlayfieldGameplay;

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
            if (Window.musicPlayer.MediaPlayer != null)
            {
                if (Window.playerButton.Style == Window.FindResource("PlayButton"))
                {
                    GamePlayClock.Start();
                    MusicPlayer.Play();

                    HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());

                    MusicPlayer.Seek(GamePlayClock.TimeElapsed);

                    Window.playerButton.Style = Window.Resources["PauseButton"] as Style;
                }
                else
                {
                    GamePlayClock.Pause();
                    MusicPlayer.Pause();

                    HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());

                    // this one line just correct very small offset when pausing...
                    // from testing it doesnt cause any audio problems or any delay anymore so yaaay
                    MusicPlayer.Seek(GamePlayClock.TimeElapsed);

                    Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
                }
            }
        }
    }
}
