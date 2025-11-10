using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay;
using System.Windows;
using ReplayAnalyzer;

namespace ReplayAnalyzer.MusicPlayer.Controls
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
                    MusicPlayer.Seek(GamePlayClock.TimeElapsed);

                    GamePlayClock.Start();
                    MusicPlayer.Play();
                    
                    HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());

                    Window.playerButton.Style = Window.Resources["PauseButton"] as Style;
                }
                else
                {
                    GamePlayClock.Pause();
                    MusicPlayer.Pause();

                    HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());

                    // this one line just correct very small offset when pausing...
                    // from testing it doesnt cause any audio problems or any delay anymore so yaaay
                    MusicPlayer.Seek(GamePlayClock.TimeElapsed);

                    Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
                }
            }
        }
    }
}
