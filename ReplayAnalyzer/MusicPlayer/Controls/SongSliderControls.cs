using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.KeyOverlay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using System.Windows;
using System.Windows.Controls.Primitives;
using Slider = ReplayAnalyzer.HitObjects.Slider;

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
                            Slider.UpdateCurrentSliderValues(s);
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
                            Slider.UpdateCurrentSliderValues(s);
                        }
                    }

                    HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
                }

                IsDragged = false;
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
            SeekGameplayToFrame(f, direction);
            KeyOverlay.UpdateHoldPositions(true);
        }

        private static ReplayFrame GetCurrentFrame(double direction)
        {
            Dictionary<int, ReplayFrame>.ValueCollection? frames = MainWindow.replay.FramesDict.Values;
            ReplayFrame f = direction < 0
                   ? frames.LastOrDefault(f => f.Time < Window.songSlider.Value) ?? frames.First()
                   : frames.FirstOrDefault(f => f.Time > Window.songSlider.Value) ?? frames.Last();

            // sometimes it happens in very specific scenario and it also should never be 0 coz it will break music player timing
            if (f.Time < 0)
            {
                f = frames.First(f => f.Time >= 0);
            }

            return f;
        }

        private static void SeekGameplayToFrame(ReplayFrame f, double direction)
        {
            GamePlayClock.Seek(f.Time);
            Window.songSlider.Value = f.Time;
            MusicPlayer.Seek(f.Time);

            CursorManager.UpdateCursorPositionAfterSeek(f);
            SliderTick.UpdateSliderBodyEvents(true);
            HitMarkerManager.UpdateHitMarkerAfterSeek(direction, f.Time);
            FrameMarkerManager.GetFrameMarkerAfterSeek(f);
            CursorPathManager.GetCursorPathAfterSeek(f);

            HitObjectSpawner.CatchUpToAliveHitObjects(f.Time);
            HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
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
