using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.SettingsMenu;
using System.Windows;

namespace ReplayAnalyzer.MusicPlayer.Controls
{
    public static class PlayPauseControls
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void InitializeEvents()
        {
            Window.playerButton.Click += PlayPauseButton;
        }

        //                              wait didnt knew i could do that... thats very useful lol
        public static void PlayPauseButton(object sender = null, RoutedEventArgs e = null)
        {
            if (MusicPlayer.AudioFile == null)
            {
                return;
            }

            if (Window.playerButton.Style == Window.FindResource("PlayButton"))
            {
                double fps = SettingsOptions.GetConfigValue("FPSLimit") != "Unlimited"
                           ? 1000 / double.Parse(SettingsOptions.GetConfigValue("FPSLimit"))
                           : 1;
                Window.ChangeGameplayLoopFrameRate(fps);

                MusicPlayer.Play();
                GamePlayClock.Start();

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

                // i wanted to use timer.Stop() to stop gameplay loop instantly and have 0% cpu usage when idle but it broke animations
                // changing timer.Interval makes it always complete its loop and correcty pausing/unpausing animations with also almost 0% cpu usage
                // 28 is adjusted by hand to be high enough and not break visuals when seeking and stuff like that which is ~35fps, 30fps breaks stuff slightly
                Window.ChangeGameplayLoopFrameRate(28);
            }
        }
    }
}
