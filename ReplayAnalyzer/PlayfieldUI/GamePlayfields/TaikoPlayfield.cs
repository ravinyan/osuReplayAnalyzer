using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
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
            Playfield.Children.Add(taikoKeyOverlay);


            Image donHitLeft = new Image();
            donHitLeft.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoInnerButton);
            donHitLeft.Height = PlayfieldHeight;

            Canvas.SetTop(donHitLeft, 0);
            Canvas.SetLeft(donHitLeft, 0);
            Playfield.Children.Add(donHitLeft);

            Image donHitRight = new Image();
            donHitRight.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoInnerButton);
            donHitRight.Height = PlayfieldHeight;
            Image.SetFlowDirection(donHitRight, FlowDirection.RightToLeft);

            Canvas.SetTop(donHitRight, 0);
            Canvas.SetLeft(donHitRight, (PlayfieldHeight - 10) / 2);
            Playfield.Children.Add(donHitRight);

            Image katHitLeft = new Image();
            katHitLeft.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoOuterButton);
            katHitLeft.Height = PlayfieldHeight;
            Image.SetFlowDirection(katHitLeft, FlowDirection.RightToLeft);

            Canvas.SetTop(katHitLeft, 0);
            Canvas.SetLeft(katHitLeft, 0);
            Playfield.Children.Add(katHitLeft);

            Image katHitRight = new Image();
            katHitRight.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoOuterButton);
            katHitRight.Height = PlayfieldHeight;
            
            Canvas.SetTop(katHitRight, 0);
            Canvas.SetLeft(katHitRight, (PlayfieldHeight - 10) / 2);
            Playfield.Children.Add(katHitRight);


            Window.ApplicationWindowUI.Children.Add(Playfield);

            ActiveClicks = new (Clicks, bool)[4];

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
            return;
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
            int startIndex = 3;
            int k1Value = (int)Clicks.ManiaK1;
            int columnCount = (int)MainWindow.map.Difficulty.CircleSize;

            List<HitObject> notes = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < columnCount; i++)
            {
                int column = i;
                if (frame.Clicks.Contains((Clicks)column + k1Value))
                {
                    for (int j = 0; j < notes.Count; j++)
                    {
                        // check for Dan/Kat
                        //if (notes[j] is ManiaNote)
                        //{
                        //    ManiaNote n = (ManiaNote)notes[j];
                        //
                        //    if (n.ColumnIndex == column && ActiveClicks[column].active == false)
                        //    {
                        //        //TaikoHitDetection.GetHitJudgment(n, frame.Time, 0, 100);
                        //        ActiveClicks[column].active = true;
                        //        break;
                        //    }
                        //}
                    }
                }
                else
                {
                    ActiveClicks[column].active = false;
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

        private static (Clicks click, bool active)[] ActiveClicks;
        public static void UpdateClickUI(bool isSeekingForward = false)
        {
            ReplayFrame frame = MainWindow.CurrentFrame;
            int startIndex = 1;
            int columnCount = (int)MainWindow.map.Difficulty.CircleSize;
            HitObjectManager.GetAliveHitObjects().Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));

            // manipulating active skin elements and lighting skin elements

            List<HitObject> notes = HitObjectManager.GetAliveHitObjects();

            if (frame.Clicks.Count > 0)
            {
                Playfield.Children[startIndex + 1].Opacity = 1;
                Playfield.Children[startIndex + 1].Opacity = 1;
            }

            return;
            for (int i = 0; i < columnCount; i++)
            {
                //int column = i;
                //if (frame.Clicks.Contains((Clicks)column + k1Value))
                //{
                //    //Playfield.Children[startIndex + 2 * column].Opacity = 0.5;
                //    //Playfield.Children[(startIndex + (2 * columnCount)) + column - 1].Opacity = 1;
                //
                //    if (GamePlayClock.IsPaused() == false || isSeekingForward == true)
                //    {
                //        for (int j = 0; j < notes.Count; j++)
                //        {
                //            if (notes[j].Visibility == Visibility.Collapsed)
                //            {
                //                continue;
                //            }
                //
                //            // check for Dan/Kat
                //            //if (notes[j] is ManiaNote)
                //            //{
                //            //    ManiaNote n = (ManiaNote)notes[j];
                //            //
                //            //    if (n.ColumnIndex == column && ActiveClicks[column].active == false)
                //            //    {
                //            //        ManiaHitDetection.GetHitJudgment(n, (long)GamePlayClock.TimeElapsed, ColumnWidth * column, JudgementYPosition);
                //            //        ActiveClicks[column].active = true;
                //            //        break;
                //            //    }
                //            //}
                //        }
                //    }
                //}
                //else
                //{
                //    //Playfield.Children[startIndex + 2 * column].Opacity = 0;
                //    //Playfield.Children[(startIndex + (2 * columnCount)) + column - 1].Opacity = 0;
                //
                //    ActiveClicks[column].active = false;
                //}
            }
        }

        private static void CreateButton(SkinElement.SkinElements skinElementIdle, SkinElement.SkinElements skinElementActive, int width, double X, int i, Canvas maniaPlayfield)
        {
            Image idleButton = new Image();
            idleButton.Width = width;
            idleButton.Height = Playfield.Height;
            idleButton.Opacity = 0.5;
            idleButton.Source = SkinElement.GetElement(skinElementIdle);
            idleButton.Name = "Idle" + i;

            Image activeButton = new Image();
            activeButton.Width = width;
            activeButton.Height = Playfield.Height;
            activeButton.Source = SkinElement.GetElement(skinElementActive);
            activeButton.Opacity = 0;
            activeButton.Name = "Active" + i;

            Canvas.SetTop(idleButton, X);
            Canvas.SetLeft(idleButton, width * i);

            Canvas.SetTop(activeButton, X);
            Canvas.SetLeft(activeButton, width * i);

            maniaPlayfield.Children.Add(idleButton);
            maniaPlayfield.Children.Add(activeButton);
        }
    }
}
