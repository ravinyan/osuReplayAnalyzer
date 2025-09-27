using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.Objects;
using WpfApp1.PlayfieldGameplay;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;

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
                IsDragged = false;

                // if music player "finished" playing this makes it so when slider bar is used it will
                // instantly make song play again without needing to unpause it manually
                if (Window.playerButton.Style == Window.Resources["PauseButton"])
                {
                    MusicPlayer.Play();
                    GamePlayClock.Start();
                }

                // clear all alive hit objects before seeking from slider bar is applied
                // without that when seeking using slider bar when there are objects on screen it will show misses
                foreach (Canvas hitObject in Playfield.GetAliveHitObjects())
                {
                    hitObject.Visibility = Visibility.Collapsed;
                    Window.playfieldCanva.Children.Remove(hitObject);
                }
                Playfield.GetAliveHitObjects().Clear();

                GamePlayClock.Seek((long)Window.songSlider.Value);
                MusicPlayer.Seek((long)Window.songSlider.Value);
                Playfield.UpdateHitObjectIndexAfterSeek((long)Window.songSlider.Value);

                List<ReplayFrame> frames = MainWindow.replay.Frames;
                double direction = e.HorizontalChange;
                ReplayFrame f = direction < 0
                       ? (frames.LastOrDefault(f => f.Time < GamePlayClock.TimeElapsed) ?? frames.First())
                       : (frames.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? frames.Last());

                Playfield.UpdateCursorPositionAfterSeek(f);
                Playfield.UpdateHitMarkerIndexAfterSeek(f);

                foreach (var slider in OsuBeatmap.HitObjectDictByIndex)
                {
                    if (slider.Value.DataContext is Slider)
                    {
                        SliderObject.ResetToDefault(slider.Value);
                    }
                }

                HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());
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
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                
            }
            else
            {
                return;
            }

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
                direction = -727;

            }
            else if (e.Key == Key.Right) // right is going forward
            {
                direction = 727;
            }

            List<ReplayFrame> frames = MainWindow.replay.Frames;
            ReplayFrame f = direction < 0
                   ? (frames.LastOrDefault(f => f.Time < GamePlayClock.TimeElapsed) ?? frames.First())
                   : (frames.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? frames.Last());

            GamePlayClock.Seek(f.Time);
            Window.songSlider.Value = GamePlayClock.TimeElapsed;

            Playfield.UpdateHitObjectIndexAfterSeek(f.Time, direction);
            Playfield.UpdateCursorPositionAfterSeek(f);
            Playfield.UpdateHitMarkerIndexAfterSeek(f);

            HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());
            HitMarkerAnimation.Seek(Playfield.AliveHitMarkers);
        }
    }
}
