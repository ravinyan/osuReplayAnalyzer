using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.PlayfieldGameplay;
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

        public static double ScrollSpeed { get; set; } = 400;

        public static Canvas CatcherBox = new Canvas();
        public static Canvas CatcherHitbox = new Canvas();
        private static Image Catcher = new Image();

        public static bool CatcherDirectionLeft = true;

        public static double FruitDiameter   { get; private set; } = MainWindow.OsuPlayfieldObjectDiameter * 0.9;
        public static double DropDiameter    { get; private set; } = MainWindow.OsuPlayfieldObjectDiameter * 0.6;
        public static double DropletDiameter { get; private set; } = MainWindow.OsuPlayfieldObjectDiameter * 0.4;

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

            // sizes of Catcher and CatcherHitbox are in resize function
            Catcher.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitCatcherIdle);
            CatcherBox.Children.Add(Catcher);

            CatcherHitbox.Height = 5;
            CatcherHitbox.Background = Brushes.Red;
            CatcherBox.Children.Add(CatcherHitbox);

            Canvas.SetZIndex(CatcherBox, 1);
            Playfield.Children.Add(CatcherBox);

            Window.playfieldGrid.Children.Add(Playfield);

            return true;
        }

        public static void Dispose()
        {
            Catcher = new Image();
            CatcherHitbox = new Canvas();
            CatcherBox = new Canvas();

            Playfield.Children.Remove(CatcherBox);
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
                        if (fruit.SpawnTime > CatchCatcherManager.CatcherFrame.Time)
                        {// i thought setting visibility like that would be slow but it isnt so eh its fine i guess
                            fruit.Visibility = Visibility.Visible;
                            fruit.IsMissed = false;

                            if (fruit.Name == "tael")
                            {
                                CatchJuiceStream? p = fruit.Parent as CatchJuiceStream;
                                p.Visibility = Visibility.Visible;
                            }
                        }
                    }
                }
                else if (hitObjects[i] is CatchFruit && hitObjects[i].SpawnTime > CatchCatcherManager.CatcherFrame.Time)
                {
                    CatchFruit fruit = (CatchFruit)hitObjects[i];
                    fruit.Visibility = Visibility.Visible;
                    fruit.IsMissed = false;
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
            CatchCatcherManager.UpdateCatcherPositionAfterSeek(f);
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

            FruitDiameter   = objectDiameter * 0.9;
            DropDiameter    = objectDiameter * 0.6;
            DropletDiameter = objectDiameter * 0.4;

            float scale = math.CalculateScaleFromCircleSize(MainWindow.map.Difficulty.CircleSize) * 2;
            // 106.75f is base deez nuts catcher size taken from osu lazer code, 0.8(needs to be float) is hitboxes
            Catcher.Width = (106.75f * Math.Abs(scale) * 0.8f) * objectScale;
            CatcherBox.Width = 106.75f * Math.Abs(scale) * 0.8f;
            CatcherHitbox.Width = (106.75f * Math.Abs(scale) * 0.8f) * objectScale;
            
            // im too lazy to figure out how to properly resize these objects since osu formula wont work here
            HitObjectManager.ClearAliveObjects();
            HitObjectSpawner.CatchUpToAliveHitObjects((long)GamePlayClock.TimeElapsed);
            
            Canvas.SetTop(CatcherBox, Playfield.Height);
        }
    }
}
