using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.PlayfieldGameplay;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Slider = ReplayAnalyzer.Objects.Slider;

namespace ReplayAnalyzer.MusicPlayer.Controls
{
    // just in case coz you never know https://github.com/ravinyan/osuReplayAnalyzer/tree/91247ef84de8346682bd3c0df4e5b8e49730adb6
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
                // if music player "finished" playing this makes it so when slider bar is used it will
                // instantly make song play again without needing to unpause it manually
                if (Window.playerButton.Style == Window.Resources["PauseButton"] || Window.musicPlayer.MediaPlayer.Time == -1)
                {
                    MusicPlayer.Play();
                    GamePlayClock.Start();
                    Window.playerButton.Style = Window.Resources["PauseButton"] as Style;
                }

                bool continuePaused = GamePlayClock.IsPaused() == true;

                double direction = e.HorizontalChange;
                // i will need to rewrite this function to be more readable so let this if statement be scuffed for now
                if (direction == 0)
                {
                    IsDragged = false;
                    return;
                }

                // clear all alive hit objects before seeking from slider bar is applied
                // without that when seeking using slider bar when there are objects on screen it will show misses
                foreach (Canvas hitObject in HitObjectManager.GetAliveHitObjects())
                {
                    hitObject.Visibility = Visibility.Collapsed;
                    Window.playfieldCanva.Children.Remove(hitObject);
                }
                HitObjectManager.GetAliveHitObjects().Clear();

                // for counting misses and hit judgements to like track that then maybe loop and add/substract counters based on Judgement value
                if (direction > 0)
                {
                    #region random thing to remember coz one day it might be useful    
                    /*   
                    // https://stackoverflow.com/questions/37787388/how-to-force-a-ui-update-during-a-lengthy-task-on-the-ui-thread
                    // i kinda hate how dispatcher works and all that but at least stack overflow is nice
                    // but that works for updating UI extremely fast without any lag... also just in case stack overflow dies
                    // this forces UI to update pushing empty frame... not needed now but might be useful one day
                    
                    //Thread thread = new Thread(() => help());
                    //thread.SetApartmentState(ApartmentState.STA);
                    //thread.Start();
                    //thread.Join();
                    
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
                    */
                    #endregion

                    List<ReplayFrame> frames = MainWindow.replay.Frames;
                    ReplayFrame f = direction < 0
                           ? frames.LastOrDefault(f => f.Time < Window.songSlider.Value) ?? frames.First()
                           : frames.FirstOrDefault(f => f.Time > Window.songSlider.Value) ?? frames.Last();
                    CursorManager.UpdateCursorPositionAfterSeek(f);
                    HitMarkerManager.UpdateHitMarkerAfterSeek(direction, f.Time);

                    GamePlayClock.Seek(f.Time);
                    MusicPlayer.Seek(f.Time);

                    CatchUpToAliveHitObjects((long)GamePlayClock.TimeElapsed);

                    if (continuePaused == true)
                    {
                        foreach (HitObject o in HitObjectManager.GetAliveHitObjects())
                        {
                            HitObjectAnimations.Pause(o);
                        }
                    }

                    if (HitObjectManager.GetAliveHitObjects().First() is Slider slider)
                    {
                        if (slider is Slider s && s.EndTime >= GamePlayClock.TimeElapsed)
                        {
                            HitObjectManager.UpdateCurrentSliderValues(s);
                        }
                    }

                    HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());

                    IsDragged = false;
                }
                else if (direction < 0)// back
                {
                    List<ReplayFrame> frames = MainWindow.replay.Frames;
                    ReplayFrame f = direction < 0
                           ? frames.LastOrDefault(f => f.Time < Window.songSlider.Value) ?? frames.First()
                           : frames.FirstOrDefault(f => f.Time > Window.songSlider.Value) ?? frames.Last();
                    CursorManager.UpdateCursorPositionAfterSeek(f);
                    HitMarkerManager.UpdateHitMarkerAfterSeek(direction, f.Time);

                    GamePlayClock.Seek(f.Time);
                    MusicPlayer.Seek(f.Time);

                    CatchUpToAliveHitObjects((long)GamePlayClock.TimeElapsed);

                    if (continuePaused == true)
                    {
                        foreach (HitObject o in HitObjectManager.GetAliveHitObjects())
                        {
                            HitObjectAnimations.Pause(o);
                        }
                    }

                    bool isCurrentSliderUpdated = false;
                    foreach (HitObject slider in OsuBeatmap.HitObjectDictByIndex.Values)
                    {
                        // this is for single currently playing slider to update its ticks, reverse arrows and slider head
                        if (isCurrentSliderUpdated == false && slider is Slider s && s.EndTime >= GamePlayClock.TimeElapsed)
                        {
                            HitObjectManager.UpdateCurrentSliderValues(s);

                            isCurrentSliderUpdated = true;
                        }

                        if (slider is Slider && slider.SpawnTime > GamePlayClock.TimeElapsed)
                        {
                            Slider.ResetToDefault(slider);
                        }
                    }

                    HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());

                    IsDragged = false;
                }
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

        private static void Seek(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.OemPeriod || e.Key == Key.OemComma)
            {
                if (GamePlayClock.IsPaused() == false)
                {
                    GamePlayClock.Pause();
                    MusicPlayer.Pause();
                    Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
                }

                int direction = 0;
                // i have direction issues
                if (e.Key == Key.OemComma) // left is going back
                {
                    direction = -727;
                }
                else if (e.Key == Key.OemPeriod) // right is going forward
                {
                    direction = 727;
                }

                List<ReplayFrame> frames = MainWindow.replay.Frames;
                ReplayFrame f = direction < 0
                       ? frames.LastOrDefault(f => f.Time < GamePlayClock.TimeElapsed) ?? frames.First()
                       : frames.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? frames.Last();

                GamePlayClock.Seek(f.Time);
                Window.songSlider.Value = GamePlayClock.TimeElapsed;

                CursorManager.UpdateCursorPositionAfterSeek(f);
                HitMarkerManager.UpdateHitMarkerAfterSeek(direction);

                // please work or i will eat rock
                HitObjectSpawner.FindObjectIndexAfterSeek(f.Time, direction);

                HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects(), direction);
            }
        }

        private static void CatchUpToAliveHitObjects(long time)
        {
            // first object
            HitObjectSpawner.FindObjectIndexAfterSeek(time, -1);

            // last object
            HitObjectSpawner.FindObjectIndexAfterSeek(time, 1);

            // fill in middle objects (needs first and last object index up to date hence last in execution
            HitObjectSpawner.UpdateHitObjectsBetweenFirstAndLast();
        }
    }
}
