using ReplayParsers.Classes.Replay;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.Objects;
using WpfApp1.PlayfieldGameplay;

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

        private static double SliderDraggedAt = 0;
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

                double direction = e.HorizontalChange;
                if (direction > 0)
                {
                    // this is so that when seeking forward it plays or catches up the whole replay to that point
                    // frame by frame in 60fps (the i += 16ms) instead of jumping instantly to the seeking point
                    // this is so all hit objects register the HitAt value that is needed when seeking backwards
                    // to correctly display spawning of circles/slider heads
                    
                    // need to stop and start timer to avoid application dying from lag
                    // EVEN when function was COMPLETELY EMPTY... logic i guess
                    MainWindow.timer.Stop();
                    FastForwardTo();
                    MainWindow.timer.Start();

                    MusicPlayer.Seek(Window.songSlider.Value);

                    #region 
                    /*   
                    // https://stackoverflow.com/questions/37787388/how-to-force-a-ui-update-during-a-lengthy-task-on-the-ui-thread
                    // i kinda hate how dispatcher works and all that but at least stack overflow is nice
                    // but that works for updating UI extremely fast without any lag... also just in case stack overflow dies
                    // this forces UI to update pushing empty frame... not needed now but might be useful one day
                    
                    //Thread thread = new Thread(() => help());
                    //thread.SetApartmentState(ApartmentState.STA);
                    //thread.Start();
                    //thread.Join();
                    */

                    void AllowUIToUpdate()
                    {
                        DispatcherFrame frame = new DispatcherFrame();
                        Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Input, new DispatcherOperationCallback(delegate (object parameter)
                        {
                            frame.Continue = false;
                            return null;
                        }), null);
                    
                        Dispatcher.PushFrame(frame);
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Input, new Action(delegate { }));
                    }

                    #endregion 

                    void FastForwardTo()
                    {
                        double i = SliderDraggedAt;
                        while (i < Window.songSlider.Value)
                        {
                            Window.Dispatcher.Invoke(() =>
                            {
                                i += 16;
                                GamePlayClock.Seek((long)i);

                                Playfield.UpdateHitMarkers(true);
                                Playfield.HandleAliveHitMarkers();
                                Playfield.HandleAliveHitJudgements();

                                Playfield.UpdateCursor();
                                Playfield.UpdateHitObjects();
                                Playfield.HandleVisibleHitObjects();

                                Playfield.UpdateSliderTicks();
                                Playfield.UpdateSliderRepeats();
                                Playfield.HandleSliderEndJudgement();
                            }, DispatcherPriority.Input);
                        }

                        //AllowUIToUpdate();
                    }
                }
                else
                {
                    GamePlayClock.Seek((long)Window.songSlider.Value);
                    MusicPlayer.Seek((long)Window.songSlider.Value);
                    
                    List<ReplayFrame> frames = MainWindow.replay.Frames;
                    ReplayFrame f = direction < 0
                           ? (frames.LastOrDefault(f => f.Time < GamePlayClock.TimeElapsed) ?? frames.First())
                           : (frames.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? frames.Last());
                    
                    Playfield.UpdateHitObjectIndexAfterSeek((long)Window.songSlider.Value, direction);
                    Playfield.UpdateCursorPositionAfterSeek(f);
                    Playfield.UpdateHitMarkerIndexAfterSeek(f, direction);

                    // only reset sliders that are yet to appear (spawn time lower that game clock time)
                    foreach (var slider in OsuBeatmap.HitObjectDictByIndex)
                    {
                        if (slider.Value is Sliderr && slider.Value.SpawnTime > GamePlayClock.TimeElapsed)
                        {
                            Objects.Slider.ResetToDefault(slider.Value);
                        }
                    }
                    
                    HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());
                }
            }
        }
        
        private static void SongSliderDragStarted(object sender, DragStartedEventArgs e)
        {
            IsDragged = true;

            SliderDraggedAt = Window.songSlider.Value;
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
            Playfield.UpdateHitMarkerIndexAfterSeek(f, direction);

            HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());
        }
    }
}
