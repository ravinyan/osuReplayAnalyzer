using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using Slider = ReplayAnalyzer.HitObjects.Osu.Slider;

namespace ReplayAnalyzer.MusicPlayer.Controls
{
    // just in case coz you never know https://github.com/ravinyan/osuReplayAnalyzer/tree/91247ef84de8346682bd3c0df4e5b8e49730adb6
    public static class SongSliderControls
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static bool IsDragged { get; private set; } = false;

        public static void InitializeEvents()
        {
            Window.songSlider.ValueChanged += SongSliderValueChanged;
            Window.songSlider.AddHandler(Thumb.DragStartedEvent, (DragStartedEventHandler)SongSliderDragStarted);
            Window.songSlider.AddHandler(Thumb.DragCompletedEvent, (DragCompletedEventHandler)SongSliderDragCompleted);
        }

        public static void SeekByFrame(int direction)
        {
            if (GamePlayClock.IsPaused() == false)
            {
                GamePlayClock.Pause();
                MusicPlayer.Pause();
                PlayPauseControls.ChangeButtonStyle();
            }

            SeekGameplayToCurrentFrame(direction);
            if (MainWindow.replay.GameMode == GameMode.Osu)
            {
                KeyOverlay.UpdateHoldPositions(true);
            }

            ManiaPlayfield.UpdateClickUI(direction > 0);
        }

        public static void SeekGameplayToCurrentFrame(double direction)
        {
            if (MainWindow.replay.FramesDict.Count == 0)
            {
                return;
            }

            ReplayFrame f = GetCurrentFrame(direction);

            GamePlayClock.Seek(f.Time);
            Window.songSlider.Value = f.Time;
            MusicPlayer.Seek(f.Time);

            if (MainWindow.replay.GameMode == GameMode.Osu)
            {
                CursorManager.UpdateCursorPositionAfterSeek(f);
                SliderTick.UpdateSliderTicks();
                HitMarkerManager.UpdateIndexAfterSeek(f.Time);
                FrameMarkerManager.UpdateIndexAfterSeek(direction, f);
                CursorPathManager.UpdateIndexAfterSeek(direction, f);
            }

            // this thing might not be needed since other game modes are extremely simple to do seeking (just do nothing lol)
            // but will leave this in here in case im wrong, and if it is not needed then just delete this code
            //PlayfieldManager.SeekGameplay(MainWindow.replay.GameMode, direction, f, isSeekingByFrame);

            HitObjectSpawner.CatchUpToAliveHitObjects(f.Time);

            MainWindow.UpdateFrame(f);
        }

        private static void SongSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {
            double direction = e.HorizontalChange;
            if (direction == 0)
            {
                IsDragged = false;
                return;
            }

            HitObjectManager.ClearAliveObjects();

            SeekGameplayToCurrentFrame(direction);
            if (MainWindow.replay.GameMode == GameMode.Osu)
            {
                Slider.UpdateAliveSliderEvents();
            }

            IsDragged = false;
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
