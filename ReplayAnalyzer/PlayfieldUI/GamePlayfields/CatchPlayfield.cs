using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static ReplayAnalyzer.HitObjects.Catch.CatchJuiceStream;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class CatchPlayfield
    {
        // there is actually high chance that this and osu playfield might be the same...
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static OsuMaths.OsuMath math = new OsuMaths.OsuMath();

        public static Canvas Playfield { get; private set; } = new Canvas();

        // number in ms will be based of AR, or maybe this will never need to be used? idk how it will work yet
        // or i will just make it adjustable like in taiko coz im lazy
        public static double ScrollSpeed { get; set; } = 400;

        public static Image Catcher = new Image();
        public static bool CatcherDirectionLeft = true;

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

            // i should add some sort of bar that shows exact hit boxes + real hit boxes of fruits (what colour for that tho...)
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

        public static void UpdateGameplayLoop()
        {
            HitJudgementManager.HandleAliveHitJudgements();
            HitObjectManager.HandleVisibleHitObjects();
            CatchCatcherManager.UpdateCatcherMovement();
            HandleMissedHitObjects();
        }

        // this is for seeking backwards and correctly showing objects
        private static void HandleMissedHitObjects()
        {
            List<HitObject> hitObjects = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i] is CatchJuiceStream)
                {
                    CatchJuiceStream juiceStream = (CatchJuiceStream)hitObjects[i];
                    for (int j = 0; j < juiceStream.Children.Count; j++)
                    {
                        JuiceStreamFruit fruit = (JuiceStreamFruit)juiceStream.Children[j];
                        if (fruit.IsMissed == true && fruit.Visibility == Visibility.Visible && fruit.SpawnTime > GamePlayClock.TimeElapsed)
                        {
                            fruit.IsMissed = false;
                        }
                    }
                }
                else if (hitObjects[i] is CatchFruit && hitObjects[i].SpawnTime > GamePlayClock.TimeElapsed)
                {
                    CatchFruit fruit = (CatchFruit)hitObjects[i];
                    fruit.IsMissed = false;
                }
            }
        }

        // loading same replay gives different judgements on timeline lol
        public static void PreloadReplay()
        {
            for (int i = 0; i < MainWindow.replay.FramesDict.Count; i++)
            {
                MainWindow.CurrentFrame = MainWindow.replay.FramesDict[i];
                GamePlayClock.Seek(MainWindow.CurrentFrame.Time);

                HitObjectSpawner.UpdateHitObjects();
                CatchCatcherManager.UpdateCatcherMovement();
                HitObjectManager.HandleVisibleHitObjects();
            }

            PlayfieldGameplay.Playfield.ResetPlayfieldFields();

            for (int i = Playfield.Children.Count - 1; i >= 0; i--)
            {
                if (Playfield.Children[i] is CatchFruit || Playfield.Children[i] is CatchJuiceStream
                ||  Playfield.Children[i] is CatchBananaShower)
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

            float scale = math.CalculateScaleFromCircleSize(MainWindow.map.Difficulty.CircleSize) * 2;
            // 106.75f is base deez nuts catcher size taken from osu lazer code, 0.8(needs to be float) is hitboxes
            Catcher.Width = 106.75f * Math.Abs(scale) * 0.8f;

            Canvas.SetTop(Catcher, Playfield.Height - Catcher.Width / 5);
        }

        // simple visualization of clicks probably copy/paste of key overlay but with 3 buttons
        public static void UpdateClickUI(bool isSeekingForward = false)
        {

        }
    }
}
