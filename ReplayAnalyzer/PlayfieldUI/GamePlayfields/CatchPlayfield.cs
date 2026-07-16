using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Catch;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.HitDetection;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
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
        private static bool CatcherDirectionLeft = true;

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
            HandleCollapsedHitObjects();
            UpdateCatcherMovement();
        }

        // i thought this would be giga slow but it is in fact giga fast (for the 1k fps needs of the application at least)
        private static void UpdateCatcherMovement()
        {
            ReplayFrame frame;

            //else
            {
                frame = MainWindow.CurrentFrame;
            }

            if (MainWindow.frameIndex > 0 && frame.X <= MainWindow.replay.FramesDict[MainWindow.frameIndex - 1].X)
            {
                CatcherDirectionLeft = true;
            }
            else
            {
                CatcherDirectionLeft = false;
            }

            // visual position
            Canvas.SetLeft(Catcher, (frame.X * MainWindow.OsuPlayfieldObjectScale) - Catcher.Width / 2);
            // position for the correct catches/misses
            float catcherPos = (float)(frame.X - (Catcher.Width / 2.0f));
             
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

            if (firstObject is CatchJuiceStream)
            {
                for (int i = 0; i < firstObject.Children.Count; i++)
                {
                    JuiceStreamFruit child = firstObject.Children[i] as JuiceStreamFruit;
                    if (child.Visibility == Visibility.Collapsed)
                    {
                        continue;
                    }
                    
                    // ok after checking a bit how this stuff is handled i will say only one thing
                    // why. just save frames when object is caught, please... im suffering and dont know where to put this code

                    // sometimes there are frames that take 15 or 17 ms instead of usual 16ms
                    if (MainWindow.frameIndex > 0 && MainWindow.CurrentFrame.Time - MainWindow.replay.FramesDict[MainWindow.frameIndex - 1].Time > 17)
                    {
                        // so... uh... frames are not recorded when catching droplets but lazer has something somewhere
                        // that makes catcher move even if it doesnt have X position from the frame...
                        // and X position of catcher is in osu framework and not in osu lazer solution and i cant access that in any way
                        // to debug it... in short what the fuck is going on

                        // there are 2 frames, first one      start T=15397 X=425.4592 | 
                        // the frame of the caught droplet is +16.6 T=15413.666 X=408.446564
                        // second one                         +7.3  T=15421 X=400.961

                        // WAIT A SECOND ALL FRAMES ARE THIS FAR APART WHAT THE FUCK LIKE ACTUALLY WHY WHO HURT YOU WHOEVER CREATED THIS
                        // or more specifically the time between frames RANGES randomly and is not static 16ms + inputs like other game modes
                        // but more like 20ms every frame

                        ReplayFrame prevFrame = MainWindow.replay.FramesDict[MainWindow.frameIndex - 1];
                        ReplayFrame currFrame = MainWindow.CurrentFrame;

                        double s = prevFrame.Time;
                        double e = currFrame.Time;

                        // should i assume that if difference between 2 frames is > 16, then middle frame will add 16.6ms to first?
                        double newTime = s + 16.66666666666; // yes 66666666 is needed for precise float number it seems
                        //GamePlayClock.TimeElapsed = newTime; // update game clock correctly

                        // should i assume that if difference between 2 frames is > 16, then middle frame will add 16.6ms to first?
                        double diff = e - s;
                        double diff2 = newTime - s;

                        double sc = diff / diff2;
                        float newX = 0;
                        if (CatcherDirectionLeft == true)
                        {
                            newX = (float)(prevFrame.X - ((prevFrame.X - currFrame.X) / sc));
                            // result is 408.446564
                            // goal was  408.446564 yay
                            // should i create these frames in replay decoder... nah there might be exceptions to this
                        }
                        else
                        { // idk if this is correct im just guessing
                            newX = (float)(prevFrame.X + ((prevFrame.X - currFrame.X) / sc));
                        }

                        frame = new ReplayFrame();
                        frame.Time = (long)newTime;
                        frame.X = newX;

                        catcherPos = (float)(frame.X - (Catcher.Width / 2.0f));
                    }

                    if (child.SpawnTime <= frame.Time)
                    {
                        // frame position = 401 but in lazer code catcher position is 408... wat.
                        // and that is on same frame...
                        if (child.XPos >= catcherPos && child.XPos <= catcherPos + (float)Catcher.Width)
                        {
                            CatchHitDetection.GetHitJudgment(child, frame.Time, HitObjectJudgement.Great);
                        }
                        else if (child.Name == "dwoplet") // to mark missed droplets
                        {
                            CatchHitDetection.GetHitJudgment(child, frame.Time, HitObjectJudgement.Ok);
                        }
                        else // and drops will also give misses since they break combo
                        {
                            CatchHitDetection.GetHitJudgment(child, frame.Time, HitObjectJudgement.Miss);
                        }
                    }
                }
            }
            else if (firstObject is CatchFruit)
            {
                if (firstObject.SpawnTime <= frame.Time)
                {
                    if (firstObject.X >= catcherPos && firstObject.X <= catcherPos + (float)Catcher.Width)
                    {
                        CatchHitDetection.GetHitJudgment(firstObject, frame.Time, HitObjectJudgement.Great);
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
