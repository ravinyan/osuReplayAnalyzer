using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
                if (direction == 0)
                {
                    IsDragged = false;
                    return;
                }

                HitObjectManager.ClearAliveObjects();

                // for counting misses and hit judgements to like track that then maybe loop and add/substract counters based on Judgement value
                if (direction > 0)
                {
                    ReplayFrame f = GetCurrentFrame(direction);
                    SeekGameplayToFrame(f, direction);

                    if (continuePaused == true)
                    {
                        foreach (HitObject o in HitObjectManager.GetAliveHitObjects())
                        {
                            HitObjectAnimations.Pause(o);
                        }
                    }

                    if (HitObjectManager.GetAliveHitObjects().Count > 0)
                    {
                        if (HitObjectManager.GetAliveHitObjects().First() is Slider slider)
                        {
                            if (slider is Slider s && s.EndTime >= GamePlayClock.TimeElapsed)
                            {
                                UpdateCurrentSliderValues(s);
                            }
                        }

                        HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
                    }

                    IsDragged = false;
                }
                else if (direction < 0)// back
                {
                    ReplayFrame f = GetCurrentFrame(direction);
                    SeekGameplayToFrame(f, direction);

                    if (continuePaused == true)
                    {
                        foreach (HitObject o in HitObjectManager.GetAliveHitObjects())
                        {
                            HitObjectAnimations.Pause(o);
                        }
                    }

                    if (HitObjectManager.GetAliveHitObjects().Count > 0)
                    {
                        if (HitObjectManager.GetAliveHitObjects().First() is Slider slider)
                        {
                            if (slider is Slider s && s.EndTime >= GamePlayClock.TimeElapsed)
                            {
                                UpdateCurrentSliderValues(s);
                            }
                        }

                        HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
                    }

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

        public static void SeekByFrame(int direction)
        {
            if (GamePlayClock.IsPaused() == false)
            {
                GamePlayClock.Pause();
                MusicPlayer.Pause();
                Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
            }

            ReplayFrame f = GetCurrentFrame(direction);

            GamePlayClock.Seek(f.Time);
            Window.songSlider.Value = GamePlayClock.TimeElapsed;

            CursorManager.UpdateCursorPositionAfterSeek(f);
            HitMarkerManager.UpdateHitMarkerAfterSeek(direction, f.Time, direction == -727);

            HitObjectSpawner.UpdateHitObjectAfterSeek(f.Time, direction);

            HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
        }

        private static ReplayFrame GetCurrentFrame(double direction)
        {
            Dictionary<int, ReplayFrame>.ValueCollection? frames = MainWindow.replay.FramesDict.Values;
            ReplayFrame f = direction < 0
                   ? frames.LastOrDefault(f => f.Time < Window.songSlider.Value) ?? frames.First()
                   : frames.FirstOrDefault(f => f.Time > Window.songSlider.Value) ?? frames.Last();

            return f;
        }

        private static void SeekGameplayToFrame(ReplayFrame f, double direction)
        {
            CursorManager.UpdateCursorPositionAfterSeek(f);
            HitMarkerManager.UpdateHitMarkerAfterSeek(direction, f.Time);

            GamePlayClock.Seek(f.Time);
            MusicPlayer.Seek(f.Time);

            //                  (long)GamePlayClock.TimeElapsed
            HitObjectSpawner.CatchUpToAliveHitObjects(f.Time);
        }

        private static void UpdateCurrentSliderValues(Slider s)
        {
            // reset all slider properties to properly change all values since without resets
            // there will be many small visual bugs coz of previously saved properties
            Slider.ResetToDefault(s);
            SliderTick.ResetFields();
            SliderReverseArrow.ResetFields();

            HitObjectManager.RemoveSliderHead(s.Children[1] as Canvas);

            for (int i = 0; i < s.RepeatCount - 1; i++)
            {
                SliderReverseArrow.UpdateSliderRepeats();
            }

            SliderTick.HidePastTicks(s);
        }
    }
}

#region random thing to remember about multi threading stuff coz one day it might be useful    
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
