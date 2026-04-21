using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.HitObjects.Slider;

namespace ReplayAnalyzer.Animations
{
    // this is the last commit that had WPF animations code... WPF animations are utter dogshit but i want to save that code
    // in case i one day need it https://github.com/ravinyan/osuReplayAnalyzer/tree/91551db8512d505cbc7a671825c72b465ed92651/ReplayAnalyzer/Animations
    public class HitObjectAnimations
    {
        private static List<long> perf1 = new List<long>();
        private static List<long> perf2 = new List<long>();
        private static List<long> perf3 = new List<long>();

        private static OsuMaths.OsuMath OsuMath = new OsuMaths.OsuMath();

        // probably done with improving this and im happy with this
        public static void RunAnimationLoop(double time)
        {
            UpdateFadeAnimation(time);
            UpdateSliderBallAnimation(time);
            UpdateApproachCircleAnimation(time);
        }

        // now you... and you work! ~110 ticks average with 7 objects (the math here is wrong for spinners but i DONT CARE)
        private static void UpdateFadeAnimation(double time)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                double objectSpawnTime = aliveObjects[i].SpawnTime - OsuMath.GetApproachRateTiming();
                aliveObjects[i].Opacity = (time - objectSpawnTime) / OsuMath.GetFadeInTiming();
            }

            //stopwatch.Stop();
            //perf1.Add(stopwatch.ElapsedTicks);

            //if (perf1.Count > 200)
            //{
            //    Console.WriteLine("Median of 200 iterations FADE: " + (perf1.Sum() / perf1.Count));
            //    perf1.Clear();
            //}

            //if (perf1.Count > 200 && GamePlayClock.IsPaused() == true)
            //{
            //    Console.WriteLine("Median of 200 iterations FADE: " + (perf1.Sum() / perf1.Count));
            //    Console.WriteLine("Median of 200 iterations FADE: " + (perf1.Sum()));
            //    perf1.Clear();
            //}
        }

        // done hopefully this code is also fast... each iteration with 7 objects is ~100 ticks average
        private static void UpdateApproachCircleAnimation(double time)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            Image approachCircle;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                // need to divide width and height by this value to properly calculate size of approach circle
                double objectLayoutScale = aliveObjects[i].LayoutTransform.Value.M11;
                if (aliveObjects[i] is HitCircle)
                {
                    approachCircle = HitCircle.ApproachCircle((HitCircle)aliveObjects[i]);
                }
                else if (aliveObjects[i] is Slider)
                {
                    if (Slider.HeadApproachCircle((Slider)aliveObjects[i]).Visibility == Visibility.Collapsed)
                    {// this means slider head doesnt exist no need for math
                        continue;
                    }
                    approachCircle = Slider.HeadApproachCircle((Slider)aliveObjects[i]);
                }
                else
                {// i hate this here also this is special case coz spinners need different math
                    Spinner spinner = (Spinner)aliveObjects[i];
                    approachCircle = Spinner.ApproachCircle(spinner);

                    double spinnerSpawnTime = spinner.SpawnTime + Spinner.SpawnOffset;
                    double duration = spinner.EndTime - spinnerSpawnTime;
                    double progresss = Math.Clamp(1 - (time - spinnerSpawnTime) / duration, 0, 1);

                    // * 6 coz its its base width and height and needs to be calculated here like this
                    approachCircle.Width = (MainWindow.OsuPlayfieldObjectDiameter * 6 * progresss) / objectLayoutScale;
                    approachCircle.Height = (MainWindow.OsuPlayfieldObjectDiameter * 6 * progresss) / objectLayoutScale;

                    Canvas.SetTop(approachCircle, -(approachCircle.Height / 2) + (aliveObjects[i].Height / 2));
                    Canvas.SetLeft(approachCircle, -(approachCircle.Width / 2) + (aliveObjects[i].Width / 2));

                    continue;
                }

                // apparently this is faster than assigning variable and using that lol
                double approachRate = OsuMath.GetApproachRateTiming();
                double approachCircleSpawnTime = aliveObjects[i].SpawnTime - approachRate;
                double progress = 1 - (time - approachCircleSpawnTime) / (approachRate * 1.35); // 1.35 adjusted by hand
                if (progress <= 0.25)
                {// block approach circle from being smaller than circle itself
                    progress = 0.25;
                }

                // * 4 coz its size of approach circles and needs to be calculated here
                approachCircle.Width = (MainWindow.OsuPlayfieldObjectDiameter * 4 * progress) / objectLayoutScale;
                approachCircle.Height = (MainWindow.OsuPlayfieldObjectDiameter * 4 * progress) / objectLayoutScale;

                Canvas.SetTop(approachCircle, -(approachCircle.Height / 2) + (aliveObjects[i].Height / 2));
                Canvas.SetLeft(approachCircle, -(approachCircle.Width / 2) + (aliveObjects[i].Width / 2));
            }

            //stopwatch.Stop();
            //perf2.Add(stopwatch.ElapsedTicks);

            //if (perf2.Count > 200)
            //{
            //    Console.WriteLine("Median of 200 iterations APPR: " + (perf2.Sum() / perf2.Count) + "\n");
            //    perf2.Clear();
            //}

            //if (perf2.Count > 200 && GamePlayClock.IsPaused() == true)
            //{
            //    Console.WriteLine("Median of 200 iterations APPR: " + (perf2.Sum() / perf2.Count) + "\n");
            //    Console.WriteLine("Median of 200 iterations APPR: " + (perf2.Sum()) + "\n");
            //    perf2.Clear();
            //}
        }

        // hopefully this pretty fast... 200-250 ticks with like 230 average
        private static void UpdateSliderBallAnimation(double time)
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i] is not Slider)
                {
                    continue;
                }

                Slider s = (Slider)aliveObjects[i];

                if ((s.Judgement.Judgement > HitObjectJudgement.Miss && s.Judgement.SpawnTime > time
                || s.Judgement.Judgement <= HitObjectJudgement.Miss && s.SpawnTime > time)
                && Slider.HeadApproachCircle(s).Visibility == Visibility.Collapsed)
                {
                    Slider.ShowSliderHead(s);
                }

                // if slider head is not clicked and duration of it is <36ms, loop here will make ball collapsed and visible
                // all the time... tho its not visible but writing this in case this will become a problem
                if (Slider.HeadApproachCircle(s).Visibility == Visibility.Visible
                && Slider.BodyBall(s).Visibility == Visibility.Visible)
                {
                    Slider.BodyBall(s).Visibility = Visibility.Collapsed;
                }

                double position;
                if (time - s.SpawnTime > s.EndTime - s.SpawnTime)
                {// if current distance based of time is higher than slider distance including repeats, snap position to 1 so ball
                 // wont go into reverse in some edge cases with very short sliders
                    position = 1;
                }
                else
                {
                    double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                    double sliderBallPosition = (time - s.SpawnTime) / sliderPathDistance;
                    if (sliderBallPosition < 0)
                    {
                        continue;
                    }

                    double overflowPosition = sliderBallPosition - (int)sliderBallPosition;
                    if ((int)sliderBallPosition % 2 == 1)
                    {
                        position = sliderBallPosition = 1 - overflowPosition;
                    }
                    else
                    {
                        position = overflowPosition;
                    }
                }

                Canvas ball = Slider.BodyBall(s);
                if (ball.Visibility == Visibility.Collapsed)
                {
                    ball.Visibility = Visibility.Visible;
                }

                // no i didnt misspell var... ok maybe
                Vector2 car = s.Path.PositionAt(position) * (float)MainWindow.OsuPlayfieldObjectScale;
                // diameter * 1.4 is the ball size, needs to be calculated here otherwise resize will bork ball (also im so happy i solved this im so bad at math some blue prince puzzles were easier)
                // layout value is scale transform value applied to objects when app is resized, just in case M11 = X, M22 = Y
                // M11 used in both coz its square and it will be probably faster for compiler coz of value caching and stuff (unless i remember something wrong)
                Canvas.SetLeft(ball, (car.X - MainWindow.OsuPlayfieldObjectDiameter * 1.4 / 2) / s.LayoutTransform.Value.M11);
                Canvas.SetTop(ball, (car.Y - MainWindow.OsuPlayfieldObjectDiameter * 1.4 / 2) / s.LayoutTransform.Value.M11);
            }

            //stopwatch.Stop();
            //perf3.Add(stopwatch.ElapsedTicks);

            //if (perf3.Count > 200)
            //{
            //    Console.WriteLine("Median of 200 iterations BALL: " + (perf3.Sum() / perf3.Count));
            //    perf3.Clear();
            //}

            //if (perf3.Count > 200 && GamePlayClock.IsPaused() == true)
            //{
            //    Console.WriteLine("Median of 200 iterations BALL: " + (perf3.Sum() / perf3.Count));
            //    Console.WriteLine("Median of 200 iterations BALL: " + (perf3.Sum()));
            //    perf3.Clear();
            //}
        }
    }
    /* performance friend
    Stopwatch stopwatch = new Stopwatch();
    stopwatch.Start();
    
    stopwatch.Stop();
    perf1.Add(stopwatch.ElapsedTicks);
    
    if (perf1.Count > 200)
    {
        Console.WriteLine("Median of 200 iterations FADE: " + (perf1.Sum() / perf1.Count));
        perf1.Clear();
    }
    */

    // in short WPF animations vs mine... also mine are FOR SURE faster by A LOT (done without debugging)
    // CPU mostly visible 5-7% vs 1.5-3% with 0.2% and 5% spikes sometimes
    // GPU literally halfed (10>5, 4.5>2.25 etc)
    // RAM it jumps with new stuff but its 5MB less or 5MB more or the same with mine implementation
    // a whole loop of 7 objects (5 sliders (1 started) + 1 spinner + 1 circle) takes ~500 ticks... i wish someone could tell me if this is good
    // ^ update1 after changing OsuMath to have saved values it went from 400-500 ticks to more stable ~400 ticks
}
