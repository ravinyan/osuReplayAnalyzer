using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
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
        public static int PlayfieldHeight = 100;

        // number in ms will be based of AR
        public static double ScrollSpeed { get; set; } = 700;

        public static Vector2 JudgementPosition = new Vector2(100, 20);
        private static bool[] ActiveClicks = new bool[4];

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

            Canvas.SetTop(Playfield, 150);
            Canvas.SetLeft(Playfield, 0);

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

            Canvas.SetTop(donHitLeft, 0);
            Canvas.SetLeft(donHitLeft, 0);
            Canvas.SetZIndex(donHitLeft, 10);
            Playfield.Children.Add(donHitLeft);

            Image donHitRight = new Image();
            donHitRight.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoInnerButton);
            donHitRight.Height = PlayfieldHeight;
            Image.SetFlowDirection(donHitRight, FlowDirection.RightToLeft);

            Canvas.SetTop(donHitRight, 0);
            Canvas.SetLeft(donHitRight, (PlayfieldHeight - 10) / 2);
            Canvas.SetZIndex(donHitRight, 10);
            Playfield.Children.Add(donHitRight);

            Image katHitLeft = new Image();
            katHitLeft.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoOuterButton);
            katHitLeft.Height = PlayfieldHeight;
            Image.SetFlowDirection(katHitLeft, FlowDirection.RightToLeft);

            Canvas.SetTop(katHitLeft, 0);
            Canvas.SetLeft(katHitLeft, 0);
            Canvas.SetZIndex(katHitLeft, 10);
            Playfield.Children.Add(katHitLeft);

            Image katHitRight = new Image();
            katHitRight.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoOuterButton);
            katHitRight.Height = PlayfieldHeight;
            
            Canvas.SetTop(katHitRight, 0);
            Canvas.SetLeft(katHitRight, (PlayfieldHeight - 10) / 2);
            Canvas.SetZIndex(katHitRight, 10);
            Playfield.Children.Add(katHitRight);

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
                    if (hitObjects[i].Judgement.SpawnTime > MainWindow.CurrentFrame.Time)
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
                UpdateClickPreload(MainWindow.replay.FramesDict[i]);
            }

            PlayfieldGameplay.Playfield.ResetPlayfieldFields();

            for (int i = Playfield.Children.Count - 1; i >= 0; i--)
            {
                if (Playfield.Children[i] is ManiaNote || Playfield.Children[i] is ManiaLongNote)
                {
                    Playfield.Children.Remove(Playfield.Children[i]);
                }
            }
        }

        private static void UpdateClickPreload(ReplayFrame frame)
        {
            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i].Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                // left don
                if (frame.Clicks.Contains(Clicks.M1) && ActiveClicks[0] == false)
                {
                    ActiveClicks[0] = true;
                    TaikoHitDetection.GetHitJudgment(aliveObjects[i], frame.Time, JudgementPosition, true);
                    continue;
                }
                else if (!frame.Clicks.Contains(Clicks.M1) && ActiveClicks[0] == true)
                {
                    ActiveClicks[0] = false;
                }

                // right don
                if (frame.Clicks.Contains(Clicks.K1A) && ActiveClicks[1] == false)
                {
                    ActiveClicks[1] = true;
                    TaikoHitDetection.GetHitJudgment(aliveObjects[i], frame.Time, JudgementPosition, true);
                    continue;
                }
                else if (!frame.Clicks.Contains(Clicks.K1A) && ActiveClicks[1] == true)
                {
                    ActiveClicks[1] = false;
                }

                // left kat
                if (frame.Clicks.Contains(Clicks.M2) && ActiveClicks[2] == false)
                {
                    ActiveClicks[2] = true;
                    TaikoHitDetection.GetHitJudgment(aliveObjects[i], frame.Time, JudgementPosition, false);
                    continue;
                }
                else if (!frame.Clicks.Contains(Clicks.M2) && ActiveClicks[2] == true)
                {
                    ActiveClicks[2] = false;
                }

                // right cat
                if (frame.Clicks.Contains(Clicks.K2A) && ActiveClicks[3] == false)
                {
                    ActiveClicks[3] = true;
                    TaikoHitDetection.GetHitJudgment(aliveObjects[i], frame.Time, JudgementPosition, false);
                    continue;
                }
                else if (!frame.Clicks.Contains(Clicks.K2A) && ActiveClicks[3] == true)
                {
                    ActiveClicks[3] = false;
                }
            }
        }

        public static void SeekGameplay(double direction, ReplayFrame f)
        {

        }

        public static void Resize()
        {
            // brain empty
            //double scale2 = Window.ApplicationWindowUI.ActualWidth / 512;
            //Playfield.RenderTransform = new ScaleTransform(scale2, scale2);
            //
            //Canvas.SetTop(Playfield, 150 * scale2);
            //Canvas.SetLeft(Playfield, 0);
        }

        public static void UpdateClickUI()
        {
            ScrollSpeed = 1000;
            ReplayFrame frame = MainWindow.CurrentFrame;
            int startIndex = 1;

            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            //aliveObjects.Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));

            // manipulating active skin elements and lighting skin elements
            HitObject firstObject = null!;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i].Visibility != Visibility.Collapsed)
                {
                    firstObject = aliveObjects[i];
                    break;
                }
            }

            // left don
            if (frame.Clicks.Contains(Clicks.M1) && ActiveClicks[0] == false)
            {
                ActiveClicks[0] = true;
                Playfield.Children[startIndex].Opacity = 1;
                Playfield.Children[startIndex].Opacity = 1;
                TaikoHitDetection.GetHitJudgment(firstObject, frame.Time, JudgementPosition, true);
            }
            else if (!frame.Clicks.Contains(Clicks.M1) && ActiveClicks[0] == true)
            {
                ActiveClicks[0] = false;
                Playfield.Children[startIndex].Opacity = 0;
                Playfield.Children[startIndex].Opacity = 0;
            }

            // right don
            if (frame.Clicks.Contains(Clicks.K1A) && ActiveClicks[1] == false)
            {
                ActiveClicks[1] = true;
                Playfield.Children[startIndex + 1].Opacity = 1;
                Playfield.Children[startIndex + 1].Opacity = 1;
                TaikoHitDetection.GetHitJudgment(firstObject, frame.Time, JudgementPosition, true);
            }
            else if (!frame.Clicks.Contains(Clicks.K1A) && ActiveClicks[1] == true)
            {
                ActiveClicks[1] = false;
                Playfield.Children[startIndex + 1].Opacity = 0;
                Playfield.Children[startIndex + 1].Opacity = 0;
            }

            // left kat
            if (frame.Clicks.Contains(Clicks.M2) && ActiveClicks[2] == false)
            {
                ActiveClicks[2] = true;
                Playfield.Children[startIndex + 2].Opacity = 1;
                Playfield.Children[startIndex + 2].Opacity = 1;
                TaikoHitDetection.GetHitJudgment(firstObject, frame.Time, JudgementPosition, false);
            }
            else if (!frame.Clicks.Contains(Clicks.M2) && ActiveClicks[2] == true)
            {
                ActiveClicks[2] = false;
                Playfield.Children[startIndex + 2].Opacity = 0;
                Playfield.Children[startIndex + 2].Opacity = 0;
            }

            // right cat
            if (frame.Clicks.Contains(Clicks.K2A) && ActiveClicks[3] == false)
            {
                ActiveClicks[3] = true;
                Playfield.Children[startIndex + 3].Opacity = 1;
                Playfield.Children[startIndex + 3].Opacity = 1;
                TaikoHitDetection.GetHitJudgment(firstObject, frame.Time, JudgementPosition, false);
            }
            else if (!frame.Clicks.Contains(Clicks.K2A) && ActiveClicks[3] == true)
            {
                ActiveClicks[3] = false;
                Playfield.Children[startIndex + 3].Opacity = 0;
                Playfield.Children[startIndex + 3].Opacity = 0;
            }
        }
    }
}
