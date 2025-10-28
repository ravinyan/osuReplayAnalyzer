﻿using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.PlayfieldGameplay;
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
                foreach (Canvas hitObject in Playfield.GetAliveHitObjects())
                {
                    hitObject.Visibility = Visibility.Collapsed;
                    Window.playfieldCanva.Children.Remove(hitObject);
                }
                Playfield.GetAliveHitObjects().Clear();

                bool continuePaused;
                if (GamePlayClock.IsPaused())
                {
                    continuePaused = true;
                }
                else
                {
                    continuePaused = false;
                }

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

                            Playfield.UpdateHitMarkers();
                            Playfield.HandleAliveHitMarkers();
                            Playfield.HandleAliveHitJudgements();

                            Playfield.UpdateCursor();
                            Playfield.UpdateHitObjects(true);
                            Playfield.HandleVisibleHitObjects();

                            Playfield.UpdateSliderTicks();
                            Playfield.UpdateSliderRepeats();
                            Playfield.HandleSliderEndJudgement();
                        }

                        if (continuePaused == false)
                        {
                            GamePlayClock.Start();
                            MusicPlayer.Play();

                            HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());
                        }
                        else
                        {
                            HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());
                            foreach (HitObject o in Playfield.GetAliveHitObjects())
                            {
                                HitObjectAnimations.Pause(o);
                            }
                        }

                        MusicPlayer.Seek(GamePlayClock.TimeElapsed);

                        timer.Stop();
                        MainWindow.timer.Start();
                        IsDragged = false;
                    }
                }
                else
                {
                    GamePlayClock.Seek((long)Window.songSlider.Value);
                    MusicPlayer.Seek((long)Window.songSlider.Value);

                    List<ReplayFrame> frames = MainWindow.replay.Frames;
                    ReplayFrame f = direction < 0
                           ? frames.LastOrDefault(f => f.Time < GamePlayClock.TimeElapsed) ?? frames.First()
                           : frames.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? frames.Last();

                    Playfield.UpdateHitObjectIndexAfterSeek((long)Window.songSlider.Value, direction);
                    Playfield.UpdateCursorPositionAfterSeek(f);
                    Playfield.UpdateHitMarkerIndexAfterSeek(f, direction);

                    // only reset sliders that are yet to appear (spawn time lower that game clock time)
                    foreach (var slider in OsuBeatmap.HitObjectDictByIndex)
                    {
                        if (slider.Value is Objects.Slider && slider.Value.SpawnTime > GamePlayClock.TimeElapsed)
                        {
                            Objects.Slider.ResetToDefault(slider.Value);
                        }
                    }

                    if (continuePaused == true)
                    {
                        // this is so scuffed and stupid and temporary hopefully...
                        // but it works for any map in the game LOL
                        for (int i = 0; i < 100; i++)
                        {
                            Playfield.UpdateHitObjects(true);
                        }
                        
                        foreach (HitObject o in Playfield.GetAliveHitObjects())
                        {
                            HitObjectAnimations.Pause(o);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            Playfield.UpdateHitObjects(true);
                        }
                    }
                        
                    HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());

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
                   ? frames.LastOrDefault(f => f.Time < GamePlayClock.TimeElapsed) ?? frames.First()
                   : frames.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? frames.Last();

            GamePlayClock.Seek(f.Time);
            Window.songSlider.Value = GamePlayClock.TimeElapsed;

            Playfield.UpdateHitObjectIndexAfterSeek(f.Time, direction);
            Playfield.UpdateCursorPositionAfterSeek(f);
            Playfield.UpdateHitMarkerIndexAfterSeek(f, direction);

            HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());
        }
    }
}
