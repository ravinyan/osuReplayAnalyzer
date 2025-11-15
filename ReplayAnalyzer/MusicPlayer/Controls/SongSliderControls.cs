using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace ReplayAnalyzer.MusicPlayer.Controls
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
                // if music player "finished" playing this makes it so when slider bar is used it will
                // instantly make song play again without needing to unpause it manually
                if (Window.playerButton.Style == Window.Resources["PauseButton"] || Window.musicPlayer.MediaPlayer.Time == -1)
                {
                    MusicPlayer.Play();
                    GamePlayClock.Start();
                    Window.playerButton.Style = Window.Resources["PauseButton"] as Style;
                }
                
                // clear all alive hit objects before seeking from slider bar is applied
                // without that when seeking using slider bar when there are objects on screen it will show misses
                foreach (Canvas hitObject in HitObjectManager.GetAliveHitObjects())
                {
                    hitObject.Visibility = Visibility.Collapsed;
                    Window.playfieldCanva.Children.Remove(hitObject);
                }
                HitObjectManager.GetAliveHitObjects().Clear();

                bool continuePaused = GamePlayClock.IsPaused() == true 
                                    ? true 
                                    : false;

                double direction = e.HorizontalChange;
                if (direction > 0)
                {
                    // wanted to make it like in osu lazer but its not optimal and takes too long
                    // this code snaps gameplay into time the slider was dragged to and simulates gameplay to that point
                    // so all IsHit and HitAt are marked correctly for backwards seeking

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

                    double ii = SliderDraggedAt;
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(1);
                    timer.Tick += FastForwardReplay;

                    // stop main gameplay timer for optimalization and less lag/bugs
                    MainWindow.timer.Stop();
                    timer.Start();
                    GamePlayClock.Pause();

                    void FastForwardReplay(object? sender, EventArgs e)
                    {
                        while (ii < Window.songSlider.Value)
                        {
                            ii += 16;
                            GamePlayClock.Seek((long)ii);

                            HitObjectSpawner.UpdateHitObjects();
                            HitDetection.CheckIfObjectWasHit();
                            HitMarkerManager.HandleAliveHitMarkers();
                            
                            CursorManager.UpdateCursor();
                            HitJudgementManager.HandleAliveHitJudgements();
                            HitObjectManager.HandleVisibleHitObjects();

                            SliderTick.UpdateSliderTicks();
                            SliderReverseArrow.UpdateSliderRepeats();
                            SliderEndJudgement.HandleSliderEndJudgement();
                        }

                        HitObjectSpawner.FindObjectIndexAfterSeek((long)ii, direction);

                        if (continuePaused == false)
                        {
                            for (int i = 0; i < 100; i++)
                            {
                                HitObjectSpawner.UpdateHitObjects();
                            }

                            GamePlayClock.Start();
                            MusicPlayer.Play();
                        }
                        else
                        {
                            for (int i = 0; i < 100; i++)
                            {
                                HitObjectSpawner.UpdateHitObjects();
                            }

                            foreach (HitObject o in HitObjectManager.GetAliveHitObjects())
                            {
                                HitObjectAnimations.Pause(o);
                            }
                        }

                        MusicPlayer.Seek(GamePlayClock.TimeElapsed);

                        HitObjectSpawner.FindObjectIndexAfterSeek((long)ii, direction);

                        HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());

                        timer.Stop();
                        MainWindow.timer.Start();
                        IsDragged = false;
                    }
                }
                else // back
                {
                    GamePlayClock.Seek((long)Window.songSlider.Value);
                    MusicPlayer.Seek((long)Window.songSlider.Value);

                    List<ReplayFrame> frames = MainWindow.replay.Frames;
                    ReplayFrame f = direction < 0
                           ? frames.LastOrDefault(f => f.Time < GamePlayClock.TimeElapsed) ?? frames.First()
                           : frames.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? frames.Last();

                    CursorManager.UpdateCursorPositionAfterSeek(f);
                    HitMarkerManager.UpdateHitMarkerAfterSeek(f, direction);

                    bool test = false;
                    // only reset sliders that are yet to appear (spawn time lower that game clock time)
                    foreach (var slider in OsuBeatmap.HitObjectDictByIndex)
                    {
                        // this is for single currently playing slider to update its tick amount
                        // needs to be special case coz it needs its head to be removed and im too lazy to write
                        // separate function for this lol
                        if (test == false && slider.Value is Objects.Slider s && s.EndTime >= GamePlayClock.TimeElapsed)
                        {
                            Objects.Slider.ResetToDefault(slider.Value);
                            HitObjectManager.RemoveSliderHead(slider.Value.Children[1] as Canvas);
                            test = true;
                        }

                        if (slider.Value is Objects.Slider && slider.Value.SpawnTime > GamePlayClock.TimeElapsed)
                        {
                            Objects.Slider.ResetToDefault(slider.Value);
                        }
                    }

                    HitObjectSpawner.FindObjectIndexAfterSeek(f.Time, direction);

                    if (continuePaused == true)
                    {
                        // this is so scuffed and stupid and temporary hopefully...
                        // but it works for any map in the game LOL

                        // when seeking fully fixed
                        // maybe simple loop until current time > object spawn time - ar time would work
                        for (int i = 0; i < 100; i++)
                        {
                            HitObjectSpawner.UpdateHitObjects();
                        }
                        
                        foreach (HitObject o in HitObjectManager.GetAliveHitObjects())
                        {
                            HitObjectAnimations.Pause(o);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            HitObjectSpawner.UpdateHitObjects();
                        }
                    }

                    // this function is here for 2nd time to correct index of current object since it depends on
                    // alive hit object list
                    HitObjectSpawner.FindObjectIndexAfterSeek(f.Time, direction);

                    HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());

                    IsDragged = false;
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
                HitMarkerManager.UpdateHitMarkerAfterSeek(f, direction);

                // please work or i will eat rock
                HitObjectSpawner.FindObjectIndexAfterSeek(f.Time, direction);

                HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects(), direction);
            }
        }
    }
}
