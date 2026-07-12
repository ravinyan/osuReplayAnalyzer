using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class CatchPlayfield
    {
        // there is actually high chance that this and osu playfield might be the same...
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static Canvas Playfield { get; private set; } = new Canvas();

        // number in ms will be based of AR, or maybe this will never need to be used? idk how it will work yet
        // or i will just make it adjustable like in taiko coz im lazy
        public static double ScrollSpeed { get; set; } = 400;

        public static Image Catcher = new Image();

        public static bool Create()
        {
            if (Window.playfieldGrid.Children.Contains(Playfield))
            {
                Dispose();
                Playfield = new Canvas();
            }

            ScrollSpeed = math.GetApproachRateTiming();
            Playfield.Height = 384;
            Playfield.Width = 512;
            Grid.SetColumn(Playfield, 1);
            Grid.SetRow(Playfield, 1);

            SolidColorBrush b = new SolidColorBrush();
            b.Opacity = 0.5;
            b.Color = Color.FromRgb(3, 252, 232);

            Playfield.Background = b;
            Catcher.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitCatcherIdle);
            Canvas.SetTop(Catcher, Playfield.Height - Catcher.Width / 5);
            Playfield.Children.Add(Catcher);

            Window.playfieldGrid.Children.Add(Playfield);

            return true;
        }

        public static void Dispose()
        {
            Playfield.Children.Remove(Catcher);
            Window.playfieldGrid.Children.Remove(Playfield);
        }

        private static OsuMaths.OsuMath math = new OsuMaths.OsuMath();
        public static void UpdateGameplayLoop()
        {
            HitJudgementManager.HandleAliveHitJudgements();
            HitObjectManager.HandleVisibleHitObjects();
            HandleCollapsedHitObjects();
            UpdateCatcherMovement();
        }

        // idk how to do that here but i want to have something for sure... maybe 3 key overlay?
        private static int JuiceStreamChildIndex = 0;
        private static void UpdateCatcherMovement()
        {
            Canvas.SetLeft(Catcher, (MainWindow.CurrentFrame.X * MainWindow.OsuPlayfieldObjectScale) - Catcher.Width / 2);

            ReplayFrame frame = MainWindow.CurrentFrame;

            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            aliveObjects.Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));

            HitObject firstObject = null!;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i].Visibility != Visibility.Collapsed)
                {
                    firstObject = aliveObjects[i];
                    break;
                }
            }

            if (firstObject == null)
            {
                return;
            }    

            // bounds values are incorrect but not sure why... will need to test how hitboxes work
            if (firstObject is CatchJuiceStream)
            {
                for (int i = 0; i < firstObject.Children.Count; i++)
                {
                    FrameworkElement? child = firstObject.Children[i] as FrameworkElement;
                    if (child.Visibility == Visibility.Collapsed)
                    {
                        //JuiceStreamChildIndex++;
                        continue;
                    }
                    var b = Canvas.GetTop(firstObject);
                    var a = Canvas.GetTop(child);
                    if (Canvas.GetTop(firstObject) + Canvas.GetTop(child) + child.Width / 2 >= Playfield.Height - Catcher.Width / 5)
                    {
                        double fruitLeftBound = Canvas.GetLeft(firstObject) + Canvas.GetLeft(child);
                        double fruitRightBound = Canvas.GetLeft(firstObject) + Canvas.GetLeft(child) + child.Width;
                        double catcherLeftBound = Canvas.GetLeft(Catcher);
                        double catcherRightBound = Canvas.GetLeft(Catcher) + Catcher.Width;
                        if (fruitLeftBound <= catcherRightBound && fruitRightBound >= catcherLeftBound)
                        {
                            CatchHitDetection.GetHitJudgment(child, frame.Time, HitObjectJudgement.Great);
                            //JuiceStreamChildIndex++;
                        }
                        else if (child.Name == "dwop")
                        {
                            CatchHitDetection.GetHitJudgment(child, frame.Time, HitObjectJudgement.Ok);
                        }
                        else
                        {
                            CatchHitDetection.GetHitJudgment(child, frame.Time, HitObjectJudgement.Miss);
                        }       
                    }
                    else
                    {
                        break;
                    }
                }
                
            }
            else if (firstObject is CatchFruit)
            {
                if (Canvas.GetTop(firstObject) + firstObject.Width / 2 >= Playfield.Height - Catcher.Width / 5)
                {
                    double fruitLeftBound = Canvas.GetLeft(firstObject);
                    double fruitRightBound = Canvas.GetLeft(firstObject) + firstObject.Width;
                    double catcherLeftBound = Canvas.GetLeft(Catcher);
                    double catcherRightBound = Canvas.GetLeft(Catcher) + Catcher.Width;
                     if (fruitLeftBound <= catcherRightBound && fruitRightBound >= catcherLeftBound)
                    {
                        CatchHitDetection.GetHitJudgment(firstObject, frame.Time, HitObjectJudgement.Great);
                        //JuiceStreamChildIndex++;
                    }
                    else
                    {
                        CatchHitDetection.GetHitJudgment(firstObject, frame.Time, HitObjectJudgement.Miss);
                    }
                }
            }
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
                MainWindow.CurrentFrame = MainWindow.replay.FramesDict[i];
                GamePlayClock.Seek(MainWindow.CurrentFrame.Time);

                HitObjectSpawner.UpdateHitObjects();
                HitObjectManager.HandleVisibleHitObjects();
                UpdateCatcherMovement();
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

        public static void SeekGameplay(double direction, ReplayFrame f)
        {

        }

        public static void Resize()
        {
            const double AspectRatio = 1.33;
            double height = (Window.ActualHeight - Window.musicControlUI.ActualHeight) / AspectRatio;
            double width = Window.ActualWidth / AspectRatio;
            double playfieldScale = Math.Min(height / 384, width / 512);

            // this still needs to be applied before object scale i guess
            Playfield.Width = 512 * playfieldScale;
            Playfield.Height = 384 * playfieldScale;

            double objectScale = Math.Min(Playfield.Width / 512, Playfield.Height / 384);
            double objectDiameter = (54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * objectScale * 2;

            MainWindow.OsuPlayfieldObjectScale = objectScale;
            MainWindow.OsuPlayfieldObjectDiameter = objectDiameter;

            // 106.75f is base deez nuts catcher size taken from osu lazer code
            Catcher.Width = 108.75f * objectScale;
            Canvas.SetTop(Catcher, Playfield.Height - Catcher.Width / 5);
        }

        // simple visualization of clicks probably copy/paste of key overlay but with 3 buttons
        public static void UpdateClickUI(bool isSeekingForward = false)
        {

        }
    }
}
