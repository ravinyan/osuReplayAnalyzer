using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Taiko;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Taiko;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class TaikoPlayfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static Movable Playfield { get; private set; } = new Movable(Movable.Movables.TaikoPlayfieldPosition, false);
        private static int PlayfieldHeight { get; set; } = 100;

        // number in ms will be based of AR
        public static double ScrollSpeed { get; set; } = 700;

        public static Vector2 JudgementPosition = new Vector2(110, -10);
        public static bool[] ActiveClicks { get; set; } = new bool[4];

        public static bool Create()
        {
            if (Window.ApplicationWindowUI.Children.Contains(Playfield))
            {
                Playfield.Dispose();
                Playfield = new Movable(Movable.Movables.TaikoPlayfieldPosition, false);
            }

            Playfield.Width = Window.ActualWidth;
            Playfield.Height = PlayfieldHeight;
            Playfield.Background = Brushes.Black;

            Playfield.SetPositionToDefault();

            Image taikoKeyOverlay = new Image();
            taikoKeyOverlay.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoButtonsUI);
            taikoKeyOverlay.Height = PlayfieldHeight;
            Canvas.SetTop(taikoKeyOverlay, 0);
            Canvas.SetLeft(taikoKeyOverlay, 0);
            Canvas.SetZIndex(taikoKeyOverlay, 10);
            Playfield.Children.Add(taikoKeyOverlay);

            Image donHitLeft = new Image();
            donHitLeft.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoInnerButton);
            donHitLeft.Height = PlayfieldHeight;
            donHitLeft.Opacity = 0;
            Canvas.SetTop(donHitLeft, 0);
            Canvas.SetLeft(donHitLeft, 0);
            Canvas.SetZIndex(donHitLeft, 10);
            Playfield.Children.Add(donHitLeft);

            Image donHitRight = new Image();
            donHitRight.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoInnerButton);
            donHitRight.Height = PlayfieldHeight;
            donHitRight.Opacity = 0;
            Image.SetFlowDirection(donHitRight, FlowDirection.RightToLeft);
            Canvas.SetTop(donHitRight, 0);
            Canvas.SetLeft(donHitRight, (PlayfieldHeight - 10) / 2);
            Canvas.SetZIndex(donHitRight, 10);
            Playfield.Children.Add(donHitRight);

            Image katHitLeft = new Image();
            katHitLeft.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoOuterButton);
            katHitLeft.Height = PlayfieldHeight;
            katHitLeft.Opacity = 0;
            Image.SetFlowDirection(katHitLeft, FlowDirection.RightToLeft);
            Canvas.SetTop(katHitLeft, 0);
            Canvas.SetLeft(katHitLeft, 0);
            Canvas.SetZIndex(katHitLeft, 10);
            Playfield.Children.Add(katHitLeft);

            Image katHitRight = new Image();
            katHitRight.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoOuterButton);
            katHitRight.Height = PlayfieldHeight;
            katHitRight.Opacity = 0;
            Canvas.SetTop(katHitRight, 0);
            Canvas.SetLeft(katHitRight, (PlayfieldHeight - 10) / 2);
            Canvas.SetZIndex(katHitRight, 10);
            Playfield.Children.Add(katHitRight);

            Image hitPosition = new Image();
            hitPosition.Source = SkinElement.GetElement(SkinElement.SkinElements.ApproachCircle);
            hitPosition.Height = 100;

            Canvas.SetTop(hitPosition, 0);
            Canvas.SetLeft(hitPosition, 120);
            Canvas.SetZIndex(hitPosition, 0);
            Playfield.Children.Add(hitPosition);

            Window.ApplicationWindowUI.Children.Add(Playfield);

            return true;
        }

        public static void Dispose()
        {
            Playfield.Dispose();
        }

        public static void UpdateGameplayLoop()
        {
            HitJudgementManager.HandleAliveHitJudgements();
            HitObjectManager.HandleVisibleHitObjects();
            TaikoClickManager.UpdatePlayfieldClicks();
            HandleCollapsedHitObjects();
        }

        // this is for seeking backwards and correctly showing objects
        private static void HandleCollapsedHitObjects() 
        {
            List<HitObject> hitObjects = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i].Visibility == Visibility.Collapsed)
                {
                    if (hitObjects[i].Judgement.SpawnTime > TaikoClickManager.TaikoFrame.Time)
                    {
                        hitObjects[i].Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public static void PreloadReplay()
        {
            for (int i = 0; i < MainWindow.replay.FramesDict.Count; i++)
            {
                long time = MainWindow.replay.FramesDict[i].Time;
                GamePlayClock.Seek(time);

                HitObjectSpawner.UpdateHitObjects();
                HitObjectManager.HandleVisibleHitObjects();
                TaikoClickManager.UpdatePlayfieldClicks();
            }

            PlayfieldGameplay.Playfield.ResetPlayfieldFields();

            for (int i = Playfield.Children.Count - 1; i >= 0; i--)
            {
                if (Playfield.Children[i] is TaikoHitCircle || Playfield.Children[i] is TaikoDrumRoll
                ||  Playfield.Children[i] is TaikoSpinner)
                {
                    Playfield.Children.Remove(Playfield.Children[i]);
                }
            }
        }

        public static void SeekGameplay(double direction, ReplayFrame f)
        {
            TaikoClickManager.UpdateIndexAfterSeek(f);
        }

        public static void Resize()
        {
            double scale = Window.ApplicationWindowUI.ActualWidth / Playfield.Width;
            Playfield.RenderTransform = new ScaleTransform(scale, scale);
            
            Canvas.SetTop(Playfield, 150 * scale);
            Canvas.SetLeft(Playfield, 0);
        }
    }
}
