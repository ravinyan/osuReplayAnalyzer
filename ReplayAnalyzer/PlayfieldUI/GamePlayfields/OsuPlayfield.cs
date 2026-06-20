using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Slider = ReplayAnalyzer.HitObjects.Osu.Slider;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class OsuPlayfield
    {
        public static Border PlayfieldBorder { get; private set; } = new Border();
        public static Canvas Playfield { get; private set; } = new Canvas();
        public static Canvas PlayfieldCursor { get; private set; } = new Canvas();

        public static void Create()
        {
            // osu playfield never changes so it can be created only once
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            if (window.playfieldGrid.Children.Contains(PlayfieldBorder))
            {
                return;
            }

            CreateBorder();
            CreatePlayfield();
            CreateCursor();

            window.playfieldGrid.Children.Add(PlayfieldBorder);
        }

        public static void Dispose()
        {
            //Playfield = null!;
            //PlayfieldBorder = null!;
            //PlayfieldCursor = null!;
        }

        public static void UpdateGameplayLoop()
        {
            CursorManager.UpdateCursorPosition();
            HitDetection.CheckIfObjectWasHit();

            FrameMarkerManager.UpdateFrameMarker();
            CursorPathManager.UpdateCursorPath();

            SliderEndJudgement.UpdateSliderBodyEvents();
            SliderReverseArrow.UpdateSliderRepeats();
            SliderTick.UpdateSliderTicks();

            HitObjectManager.HandleVisibleHitObjects();
            HitJudgementManager.HandleAliveHitJudgements();

            HitMarkerManager.HandleAliveHitMarkers();
            FrameMarkerManager.HandleAliveFrameMarkers();
            CursorPathManager.HandleAliveCursorPaths();
        }

        public static void PreloadReplay()
        {
            for (int i = 0; i < MainWindow.replay.FramesDict.Count; i++)
            {
                long time = MainWindow.replay.FramesDict[i].Time;
                GamePlayClock.Seek(time);

                HitObjectSpawner.UpdateHitObjects();
                CursorManager.UpdateCursorPosition();
                HitMarkerManager.UpdateIndexAfterSeek(time);

                SliderEndJudgement.UpdateSliderBodyEvents();
                SliderReverseArrow.UpdateSliderRepeats(true);
                SliderTick.UpdateSliderTicks(true);

                HitObjectManager.HandleVisibleHitObjects();
                HitMarkerManager.HandleAliveHitMarkers();
                HitJudgementManager.HandleAliveHitJudgements();
            }

            PlayfieldGameplay.Playfield.ResetPlayfieldFields();

            // clear stuck objects except cursor which is at index 0
            for (int i = Playfield.Children.Count - 1; i >= 0; i--)
            {
                Playfield.Children.Remove(Playfield.Children[i]);
            }

            HitMarkerManager.GetAliveDataHitMarkers().Clear();
        }

        // might be useless
        public static void SeekGameplay(double direction, ReplayFrame f)
        {
            CursorManager.UpdateCursorPositionAfterSeek(f);
            SliderTick.UpdateSliderTicks();
            HitMarkerManager.UpdateIndexAfterSeek(f.Time);
            FrameMarkerManager.UpdateIndexAfterSeek(direction, f);
            CursorPathManager.UpdateIndexAfterSeek(direction, f);
        }

        private static void CreateCursor()
        {
            if (PlayfieldCursor == null)
            {
                PlayfieldCursor = new Canvas();
            }

            PlayfieldCursor.Name = "PlayfieldCursor";
            PlayfieldCursor.Width = 35;
            PlayfieldCursor.Height = 35;
            Canvas.SetZIndex(PlayfieldCursor, 99);
        }

        private static void CreatePlayfield()
        {
            if (Playfield == null)
            {
                Playfield = new Canvas();
            }

            Playfield.Name = "OsuPlayfield";
            Playfield.Width = 512;
            Playfield.Height = 384;
            Playfield.ClipToBounds = false;

            PlayfieldBorder!.Child = Playfield;
        }

        private static void CreateBorder()
        {
            if (PlayfieldBorder == null)
            {
                PlayfieldBorder = new Border();
            }

            PlayfieldBorder.Name = "OsuPlayfieldBorder";
            PlayfieldBorder.Visibility = Visibility.Visible;
            PlayfieldBorder.BorderBrush = Brushes.White;
            // in MainWindow.xaml there is grid and this playfield is in this position
            Grid.SetColumn(PlayfieldBorder, 1);
            Grid.SetRow(PlayfieldBorder, 1);
        }
    }
}
