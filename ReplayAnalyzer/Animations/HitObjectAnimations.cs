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

        // on resize object size is changed by ScaleTransform, to correctly update sizes/positions here we need to
        // divide values by this ScaleTransform value (if i find i way to not use ScaleTransform i can remove this)
        public static bool ShouldUpdateScale { private get; set; } = true;
        private static double LayoutScale = 0;

        // probably done with improving this and im happy with this (i lied x1 now im done)
        public static void RunAnimationLoop(double time)
        {
            // this is borked so will go back to using aliveObjects[i].LayoutTransform.Value.M11 since it still is extremely fast
            // and works correctly maybe will figure out how to do this later since this doesnt matter anyway
            //if (ShouldUpdateScale == true && HitObjectManager.GetAliveHitObjects().Count > 0)
            //{
            //        LayoutScale = HitObjectManager.GetAliveHitObjects().First().LayoutTransform.Value.M11;
            //        ShouldUpdateScale = false;
            //}

            //PerformanceBlanket(() => UpdateFadeAnimation(time), perf1, "FADE");
            //PerformanceBlanket(() => UpdateSliderBallAnimation(time), perf2, "BALL");
            //PerformanceBlanket(() => UpdateApproachCircleAnimation(time), perf3, "APPR");
            UpdateFadeAnimation(time);
            UpdateSliderBallAnimation(time);
            UpdateApproachCircleAnimation(time);
        }

        // now you... and you work! ~110 ticks average with 7 objects (the math here is wrong for spinners but i DONT CARE)
        // ok here performance will tank a bit coz of hidden mod
        private static void UpdateFadeAnimation(double time)
        {
            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            if (SettingsMenu.SettingsOptions.GetConfigValue("IsHiddenModEnabled") == "false")
            {
                for (int i = 0; i < aliveObjects.Count; i++)
                {
                    // scuffed to reset approach circles
                    if (aliveObjects[i] is HitCircle c)
                    {
                        if (HitCircle.ApproachCircle(c).Opacity == 0)
                            HitCircle.ApproachCircle(c).Opacity = 1;
                    }
                    else if (aliveObjects[i] is Slider s)
                    {
                        if (Slider.HeadApproachCircle(s).Opacity == 0)
                            Slider.HeadApproachCircle(s).Opacity = 1;
                    }

                    double objectSpawnTime = aliveObjects[i].SpawnTime - OsuMath.GetApproachRateTiming();
                    // apparently circles always get full opacity after 800ms of circle being alive unless fade in timing < 800
                    // but i dont know if i like this implementation... hidden should be exact but for this i think mine looked better
                    aliveObjects[i].Opacity = (time - objectSpawnTime) / (800 <= OsuMath.GetApproachRateTiming() ? 800 : OsuMath.GetApproachRateTiming());
                }
            }
            else
            {
                // hidden math formula (im horrible with reading math formulas i dont understand anything i read aaaaaaaaaaaa)
                // also sliders are differnet
                for (int i = 0; i < aliveObjects.Count; i++)
                {
                    if (1 == 2 && aliveObjects[i] is Slider)
                    {
                        // slider bodies have different fade in time coz of course they do...
                        // slider body opacity reaches 0 when its ending
                        // slider ball is always visible
                        double objectSpawnTime = aliveObjects[i].SpawnTime - OsuMath.GetApproachRateTiming();
                        aliveObjects[i].Opacity = 0;
                    }
                    else
                    {// i hate math and i need to figure out formula myself coz i dont know where osu lazer has code for this i hate it here

                        // approach circle begone
                        // everything else fading away
                        //Image head;
                        if (aliveObjects[i] is HitCircle c)
                        {
                            HitCircle.ApproachCircle(c).Opacity = 0;
                        }
                        else if(aliveObjects[i] is Slider s)
                        {
                            Slider.HeadApproachCircle(s).Opacity = 0;
                        }
                        else
                        {
                            continue;
                        }
                        double objectSpawnTime = aliveObjects[i].SpawnTime - OsuMath.GetApproachRateTiming();

                        double fadeInDuration = OsuMath.GetApproachRateTiming() * 0.4;
                        double fadeInOpacity = (time - objectSpawnTime) / fadeInDuration;
                        if (fadeInOpacity < 1)
                        {
                            aliveObjects[i].Opacity = fadeInOpacity;
                        }
                        else
                        {
                            // something like this? at 70% of approach circle finished this should reach 0
                            aliveObjects[i].Opacity = 1 - (fadeInOpacity - (1 / 1.3));
                            //objectSpawnTime = (aliveObjects[i].SpawnTime - OsuMath.GetApproachRateTiming())
                            //                + (OsuMath.GetApproachRateTiming() * 0.4);
                            //var fadeOutDuration = OsuMath.GetApproachRateTiming() * 0.7;
                            // double fadeOutOpacity = ((objectSpawnTime) / (time)); 
                            //aliveObjects[i].Opacity = 0;
                        } 
                    }
                }
            }   
        }

        // done hopefully this code is also fast... each iteration with 7 objects is ~100 ticks average
        private static void UpdateApproachCircleAnimation(double time)
        {
            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            Image approachCircle;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
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
                    approachCircle.Width = (MainWindow.OsuPlayfieldObjectDiameter * 6 * progresss) / aliveObjects[i].LayoutTransform.Value.M11;
                    approachCircle.Height = (MainWindow.OsuPlayfieldObjectDiameter * 6 * progresss) / aliveObjects[i].LayoutTransform.Value.M11;
                    
                    Canvas.SetTop(approachCircle, -(approachCircle.Height / 2) + (aliveObjects[i].Height / 2));
                    Canvas.SetLeft(approachCircle, -(approachCircle.Width / 2) + (aliveObjects[i].Width / 2));

                    continue;
                }

                double approachRate = OsuMath.GetApproachRateTiming();
                double approachCircleSpawnTime = aliveObjects[i].SpawnTime - approachRate;
                double progress = 1 - (time - approachCircleSpawnTime) / (approachRate * 1.35); // 1.35 adjusted by hand
                if (progress <= 0.25)
                {// block approach circle from being smaller than circle itself
                    progress = 0.25;
                }

                // * 4 coz its size of approach circles and needs to be calculated here
                approachCircle.Width = (MainWindow.OsuPlayfieldObjectDiameter * 4 * progress) / aliveObjects[i].LayoutTransform.Value.M11;
                approachCircle.Height = (MainWindow.OsuPlayfieldObjectDiameter * 4 * progress) / aliveObjects[i].LayoutTransform.Value.M11;

                Canvas.SetTop(approachCircle, -(approachCircle.Height / 2) + (aliveObjects[i].Height / 2));
                Canvas.SetLeft(approachCircle, -(approachCircle.Width / 2) + (aliveObjects[i].Width / 2));
            }
        }

        // hopefully this pretty fast... 200-250 ticks with like 230 average
        private static void UpdateSliderBallAnimation(double time)
        {
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
                Canvas.SetLeft(ball, (car.X - MainWindow.OsuPlayfieldObjectDiameter * 1.4 / 2) / aliveObjects[i].LayoutTransform.Value.M11);
                Canvas.SetTop(ball, (car.Y - MainWindow.OsuPlayfieldObjectDiameter * 1.4 / 2) / aliveObjects[i].LayoutTransform.Value.M11);
            }
        }

        private static void PerformanceBlanket(Action method, List<long> performanceCounter, string name)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            method();

            stopwatch.Stop();
            performanceCounter.Add(stopwatch.ElapsedTicks);

            //if (performanceCounter.Count > 200)
            //{
            //    Console.WriteLine($"Median of 200 iterations {name}: {performanceCounter.Sum() / performanceCounter.Count}");
            //    if (name == "APPR")
            //    {
            //        Console.WriteLine();
            //    }
            //    performanceCounter.Clear();
            //}

            // average throught the whole map (bloody devotion is good for testing approach rate, maze would be best for ball)
            if (performanceCounter.Count > 200 && GamePlayClock.IsPaused() == true)
            {
                Console.WriteLine($"Median of 200 iterations {name}: " + (performanceCounter.Sum() / performanceCounter.Count));
                Console.WriteLine($"Median of 200 iterations {name}: " + (performanceCounter.Sum()));
                if(name == "APPR")
                {
                    Console.WriteLine();
                }
                performanceCounter.Clear();
            }
        }
    }
    // in short WPF animations vs mine... also mine are FOR SURE faster by A LOT (done without debugging)
    // CPU mostly visible 5-7% vs 1.5-3% with 0.2% and 5% spikes sometimes
    // GPU literally halfed (10>5, 4.5>2.25 etc)
    // RAM it jumps with new stuff but its 5MB less or 5MB more or the same with mine implementation
    // a whole loop of 7 objects (5 sliders (1 started) + 1 spinner + 1 circle) takes ~500 ticks... i wish someone could tell me if this is good
    // ^ update1 after changing OsuMath to have saved values it went from 400-500 ticks to more stable ~400 ticks
}
