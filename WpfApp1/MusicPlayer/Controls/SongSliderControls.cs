using ReplayParsers.Classes.Replay;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WpfApp1.Animations;
using WpfApp1.GameClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace WpfApp1.MusicPlayer.Controls
{
    public static class SongSliderControls
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static bool IsDragged = false;

        public static void InitializeEvents()
        {
            Window.songSlider.ValueChanged += SongSliderValueChanged;
            Window.songSlider.AddHandler(Thumb.DragStartedEvent, (DragStartedEventHandler)SongSliderDragStarted);
            Window.songSlider.AddHandler(Thumb.DragCompletedEvent, (DragCompletedEventHandler)SongSliderDragCompleted);

            Window.KeyDown += Seek;
        }

        private static void SongSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (Window.musicPlayer.MediaPlayer != null)
            {
                //Window.musicPlayer.MediaPlayer.Time = (long)Window.songSlider.Value;
                //Window.songSlider.Value = (long)Window.songSlider.Value;
                IsDragged = false;

                GamePlayClock.Seek((long)Window.songSlider.Value);
                MusicPlayer.Seek((long)Window.songSlider.Value);
            }
        }
        
        private static void SongSliderDragStarted(object sender, DragStartedEventArgs e)
        {
            IsDragged = true;
        }
        
        private static void SongSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue != e.OldValue)
            {
                Window.songTimer.Text = TimeSpan.FromMilliseconds(Window.songSlider.Value).ToString(@"hh\:mm\:ss\:fffffff").Substring(0, 12);
            }
        }

        // to work the song needs to be paused... or seek will automatically pause coz thats convinient
        private static void Seek(object sender, KeyEventArgs e)
        {
            if (GamePlayClock.IsPaused() == false)
            {
                GamePlayClock.Pause();
                MusicPlayer.Pause();
                Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
            }

            int direction = 0;
            // i have direction issues
            if (e.Key == Key.Left) // left is going back
            {
                HitObjectAnimations.UpdateBack(Playfield.Playfield.GetAliveHitObjects());

                direction = -727;

            }
            else if (e.Key == Key.Right) // right is going forward
            {
                HitObjectAnimations.UpdateForward(Playfield.Playfield.GetAliveHitObjects());

                direction = 727;
            }

            List<ReplayFrame> frames = MainWindow.replay.Frames;
            long t = direction < 0
                   ? (frames.LastOrDefault(f => f.Time < GamePlayClock.TimeElapsed) ?? frames.First()).Time
                   : (frames.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? frames.Last()).Time;

            GamePlayClock.Seek(t);
            MusicPlayer.Seek(t);
            Playfield.Playfield.Update(t);

            Window.fpsCounter.Text = GamePlayClock.TimeElapsed.ToString();
        }
    }
}
