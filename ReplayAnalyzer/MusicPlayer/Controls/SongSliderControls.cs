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

        // test
        private static bool BlockSlider = false;
        // sometimes i wish i was smarter... i dont know if its hard to fix this stuff or im stupid
        private static double SliderDraggedAt = 0;
        private static void SongSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (BlockSlider == true)
            {
                return;
            }

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

                bool continuePaused = GamePlayClock.IsPaused() == true 
                                    ? true 
                                    : false;

                double direction = e.HorizontalChange;
                // i will need to rewrite this function to be more readable so let this if statement be scuffed for now
                if (direction == 0)
                {
                    IsDragged = false;
                    return;
                }

                BlockSlider = true;
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

                    MainWindow.timer.Stop();
                    GamePlayClock.Pause();

                    double currentTime = SliderDraggedAt;
                    //HitObjectSpawner.FindObjectIndexAfterSeek((long)currentTime, direction);

                    // please work
                    // sometimes hit markers just dont update correctly and need to catch up... this catches up...
                    // if catch up is needed and this catches up it will make it work... right? RIGHT? AAAAAAA
                    HitMarkerManager.UpdateHitMarkerAfterSeek(-direction);
                    for (int i = 0; i < 50; i++)
                    {
                        HitMarkerManager.UpdateHitMarker();
                    }
                    HitMarkerManager.UpdateHitMarkerAfterSeek(direction);

                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(1);
                    timer.Tick += FastForwardReplay;

                    // stop main gameplay timer for optimalization and less lag/bugs
                    
                    timer.Start();
                    void FastForwardReplay(object? sender, EventArgs e)
                    {
                        List<ReplayFrame> frames = MainWindow.replay.Frames;

                        ReplayFrame f1 = currentTime > Window.songSlider.Maximum / 2
                           ? frames.LastOrDefault(f => f.Time < GamePlayClock.TimeElapsed) ?? frames.First()
                           : frames.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? frames.Last();

                        ReplayFrame f2 = Window.songSlider.Value > Window.songSlider.Maximum / 2
                           ? frames.LastOrDefault(f => f.Time < Window.songSlider.Value) ?? frames.First()
                           : frames.FirstOrDefault(f => f.Time > Window.songSlider.Value) ?? frames.Last();

                        //CatchUpToAliveHitObjects(f2.Time);

                        int currFrameIndex = frames.IndexOf(f1);
                        int frameIndexToCatchUpTo = frames.IndexOf(f2);

                        for (int j = currFrameIndex; j <= frameIndexToCatchUpTo; j++)
                        {
                            long time = frames[j].Time;
                            GamePlayClock.Seek(time);

                            HitObjectSpawner.UpdateHitObjects();

                            HitMarkerManager.HandleAliveHitMarkers();

                            HitDetection.CheckIfObjectWasHit(time);

                            CursorManager.UpdateCursor();
                            HitJudgementManager.HandleAliveHitJudgements();
                            HitObjectManager.HandleVisibleHitObjects();

                            SliderTick.UpdateSliderTicks();
                            SliderReverseArrow.UpdateSliderRepeats();
                            SliderEndJudgement.HandleSliderEndJudgement();
                        }
                        MusicPlayer.Seek(f2.Time);

                        HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());

                        // dont know if this will be hit but why not whatever
                        if (HitObjectManager.GetAliveHitObjects().Count != 0 && HitObjectManager.GetAliveHitObjects().Last().SpawnTime < GamePlayClock.TimeElapsed)
                        {
                            HitObjectManager.AnnihilateHitObject(HitObjectManager.GetAliveHitObjects().Last());
                        }

                        // please work
                        // sometimes hit markers just dont update correctly and need to catch up... this catches up...
                        // if catch up is needed and this catches up it will make it work... right? RIGHT? AAAAAAA
                        HitMarkerManager.UpdateHitMarkerAfterSeek(-direction);
                        for (int i = 0; i < 50; i++)
                        {
                            HitMarkerManager.UpdateHitMarker();
                        }
                        HitMarkerManager.UpdateHitMarkerAfterSeek(direction);

                        timer.Stop();
                        MainWindow.timer.Start();

                        if (continuePaused == false)
                        {
                            GamePlayClock.Start();
                            MusicPlayer.Play();
                        }

                        IsDragged = false;
                    }
                }
                else if (direction < 0)// back
                {
                    // clear all alive hit objects before seeking from slider bar is applied
                    // without that when seeking using slider bar when there are objects on screen it will show misses
                    foreach (Canvas hitObject in HitObjectManager.GetAliveHitObjects())
                    {
                        hitObject.Visibility = Visibility.Collapsed;
                        Window.playfieldCanva.Children.Remove(hitObject);
                    }
                    HitObjectManager.GetAliveHitObjects().Clear();

                    List<ReplayFrame> frames = MainWindow.replay.Frames;
                    ReplayFrame f = direction < 0
                           ? frames.LastOrDefault(f => f.Time < Window.songSlider.Value) ?? frames.First()
                           : frames.FirstOrDefault(f => f.Time > Window.songSlider.Value) ?? frames.Last();
                    CursorManager.UpdateCursorPositionAfterSeek(f);
                    HitMarkerManager.UpdateHitMarkerAfterSeek(direction, f.Time);

                    GamePlayClock.Seek(f.Time);

                    HitObjectSpawner.FindObjectIndexAfterSeek(f.Time, direction);

                    //CatchUpToAliveHitObjects((long)GamePlayClock.TimeElapsed);

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

                    bool test = false;
                    foreach (var slider in OsuBeatmap.HitObjectDictByIndex)
                    {
                        // this is for single currently playing slider to update its tick amount
                        // needs to be special case coz it needs its head to be removed and im too lazy to write
                        // separate function for this lol
                        if (test == false && slider.Value is Objects.Slider s && s.EndTime >= GamePlayClock.TimeElapsed)
                        {
                            SliderTick.HidePastTicks(s);
                            
                            // to check
                            for (int i = 0; i < s.RepeatCount - 1; i++)
                            {
                                SliderReverseArrow.UpdateSliderRepeats();
                            }

                            HitObjectManager.RemoveSliderHead(slider.Value.Children[1] as Canvas);
                            test = true;
                        }

                        if (slider.Value is Objects.Slider && slider.Value.SpawnTime > GamePlayClock.TimeElapsed)
                        {
                            Objects.Slider.ResetToDefault(slider.Value);
                        }
                    }

                    HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());

                    IsDragged = false;
                }

                BlockSlider = false;
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
                HitMarkerManager.UpdateHitMarkerAfterSeek(direction);

                // please work or i will eat rock
                HitObjectSpawner.FindObjectIndexAfterSeek(f.Time, direction);

                HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects(), direction);
            }
        }

        // i guess that works but some other stuff dont work so need to figure out that first
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
