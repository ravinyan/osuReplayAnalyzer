using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Osu;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.HitObjects.Osu.Slider;

namespace ReplayAnalyzer.Animations
{
    // this is the last commit that had WPF animations code... WPF animations are utter dogshit but i want to save that code
    // in case i one day need it https://github.com/ravinyan/osuReplayAnalyzer/tree/91551db8512d505cbc7a671825c72b465ed92651/ReplayAnalyzer/Animations
    public class HitObjectAnimations
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
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

            List<HitObject> aliveObjects = HitObjectManager.GetAliveHitObjects();
            GameMode mode = MainWindow.replay.GameMode;
            if (mode == GameMode.Osu)
            {
                //PerformanceBlanket(() => UpdateFadeAnimation(time, aliveObjects), perf1, "FADE");
                //PerformanceBlanket(() => UpdateSliderBallAnimation(time, aliveObjects), perf2, "BALL");
                //PerformanceBlanket(() => UpdateApproachCircleAnimation(time, aliveObjects), perf3, "APPR");
                UpdateFadeAnimation(time, aliveObjects);
                UpdateSliderBallAnimation(time, aliveObjects);
                UpdateApproachCircleAnimation(time, aliveObjects);

                EARTHQUAKE();
            }
            else if (mode == GameMode.OsuMania)
            {
                //PerformanceBlanket(() => MoveManiaNotes(time, aliveObjects), perf3, "MANIA");
                MoveManiaNotes(time, aliveObjects);
            }
            else if (mode == GameMode.OsuTaiko)
            {
                MoveTaikoObjects(time, aliveObjects);
            }
            else if (mode == GameMode.OsuCatch)
            {
                MoveCatchObjects(time, aliveObjects);
            }
        }

        private static void MoveCatchObjects(double time, List<HitObject> aliveObjects)
        {
            double h = CatchPlayfield.Playfield.Height;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                HitObject note = aliveObjects[i];

                double newPosition = h * ((time - note.SpawnTime + TaikoPlayfield.ScrollSpeed) / TaikoPlayfield.ScrollSpeed) - 50;
                Canvas.SetTop(note, newPosition);
            }
        }

        private static void MoveTaikoObjects(double time, List<HitObject> aliveObjects)
        {
            // it would be smart to somehow not move notes, but instead move entire playfield... just idea but this is good enough
            // h is height between top of the application and judgement line that is on top of mania keys
            double w = TaikoPlayfield.Playfield.ActualWidth;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                HitObject hitObject = aliveObjects[i];

                double newPosition = -(w * ((time - hitObject.SpawnTime + TaikoPlayfield.ScrollSpeed) / TaikoPlayfield.ScrollSpeed)) + w + 125;
                Canvas.SetLeft(hitObject, newPosition);
            }
        }

        private static void MoveManiaNotes(double time, List<HitObject> aliveObjects)
        {
            // it would be smart to somehow not move notes, but instead move entire playfield... just idea but this is good enough
            // h is height between top of the application and judgement line that is on top of mania keys
            double h = ManiaPlayfield.Playfield.Height - 80;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                HitObject note = aliveObjects[i];

                double newPosition = h * ((time - note.SpawnTime + ManiaPlayfield.ScrollSpeed) / ManiaPlayfield.ScrollSpeed);
                Canvas.SetTop(note, newPosition);
            }
        }

        private static List<(HitObject hitObject, double notelockTime, double basePos)> EARTHQUAKECIRCLE = new List<(HitObject, double, double)>();
        public static void ApplyShake(HitObject hitObject, double notelockTime)
        {
            // it happened so null check
            if (hitObject != null)
            {
                EARTHQUAKECIRCLE.Add((hitObject, notelockTime, Canvas.GetLeft(hitObject)));
            }
        }

        private static void EARTHQUAKE()
        {
            for (int i = 0; i < EARTHQUAKECIRCLE.Count; i++)
            {
                double timeUntilCompletion = 200;
                double moveAmount = 10; // strength of shake
                double completionPercent = 0;

                (HitObject hitObject, double notelockTime, double basePos) o = EARTHQUAKECIRCLE[i];

                int cycle = (int)((GamePlayClock.TimeElapsed - o.notelockTime) / (timeUntilCompletion / 4));

                // i dont know how to call it but this (a) makes equation (GamePlayClock.TimeElapsed - o.notelockTime - a)
                // be always <= 1/4th of timeUntilCompletion for accurate division for completionPercentage (im bad at math idk how else to do this)
                double a = cycle == 0 ? 0 : cycle * (timeUntilCompletion / 4);
                completionPercent = (GamePlayClock.TimeElapsed - o.notelockTime - a) / (timeUntilCompletion / 4);

                if (cycle % 2 == 0)
                {
                    if (o.hitObject is HitCircle)
                    {
                        Canvas.SetLeft(o.hitObject, o.basePos + (moveAmount * completionPercent));
                    }
                    else if (o.hitObject is Slider)
                    {// explanation: slider main object is ALWAYS at 0,0 coords, so no base position or anything here
                        Canvas.SetLeft(o.hitObject, moveAmount * completionPercent);
                    }
                }
                else
                {
                    if (o.hitObject is HitCircle)
                    {
                        Canvas.SetLeft(o.hitObject, o.basePos + moveAmount - (moveAmount * completionPercent));
                    }
                    else if (o.hitObject is Slider)
                    {// explanation: slider main object is ALWAYS at 0,0 coords, so no base position or anything here
                        Canvas.SetLeft(o.hitObject, moveAmount - (moveAmount * completionPercent));
                    }
                }

                if (GamePlayClock.TimeElapsed - o.notelockTime > timeUntilCompletion
                ||  GamePlayClock.TimeElapsed - o.notelockTime < 0)
                {
                    if (o.hitObject is HitCircle)
                    {
                        Canvas.SetLeft(o.hitObject, o.basePos);
                    }
                    else if (o.hitObject is Slider)
                    {// explanation: slider main object is ALWAYS at 0,0 coords, so no base position or anything here
                        Canvas.SetLeft(o.hitObject, 0);
                    }
                    
                    EARTHQUAKECIRCLE.Remove(o);
                }
            }
        }

        // now you... and you work! ~110 ticks average with 7 objects (the math here is wrong for spinners but i DONT CARE)
        // ok here performance will tank a bit coz of hidden mod ~200 ticks without and ~350 with hidden enabled
        private static void UpdateFadeAnimation(double time, List<HitObject> aliveObjects)
        {
            if (HiddenMod.IsEnabled == false)
            {
                for (int i = 0; i < aliveObjects.Count; i++)
                {
                    // this tanks performance which kinda sucks by 40% more or less
                    CorrectObjectVisibility(aliveObjects[i]);

                    double objectSpawnTime = aliveObjects[i].SpawnTime - OsuMath.GetApproachRateTiming();
                    aliveObjects[i].Opacity = (time - objectSpawnTime) / OsuMath.GetFadeInTiming();
                }
            }
            else
            {
                // notes:
                //  slider body has SLIGHLY higher opacity by like 0.04 opacity, which i will leave it at that coz it doesnt break anything
                //  circles become invisible one frame faster than in osu lazer, which also SHOULDNT be even noticable but it exists
                for (int i = 0; i < aliveObjects.Count; i++)
                {
                    double approachRate = OsuMath.GetApproachRateTiming();

                    double objectSpawnTime = aliveObjects[i].SpawnTime - approachRate;
                    double fadeInDuration = approachRate * 0.4;
                    double fadeInOpacity = (time - objectSpawnTime) / fadeInDuration;

                    double opacity;
                    if (fadeInOpacity < 1)
                    {
                        opacity = fadeInOpacity;
                    }
                    else
                    {   // something like this? at 70% of approach circle finished this should reach 0
                        // am i a genius or am i a genius i dont know anymore this works at least (i dont understand why this works but it does)
                        objectSpawnTime = objectSpawnTime + fadeInDuration;
                        opacity = 1 - (time - objectSpawnTime) / (approachRate * 0.3);
                    }

                    if (aliveObjects[i] is HitCircle c)
                    {
                        aliveObjects[i].Opacity = opacity;
                        // approach circle needs to be invisible
                        HitCircle.ApproachCircle(c).Opacity = 0;
                    }
                    else if (aliveObjects[i] is Slider s)
                    {
                        aliveObjects[i].Opacity = 1; // set main slider object opacity to 1 and below change children opacity as needed
                        for (int j = 0; j < aliveObjects[i].Children.Count; j++)
                        {
                            if (j == 0) // slider head approach circle stays invisible always
                            {
                                Slider.HeadHitCircleContainer(s).Opacity = opacity;
                                Slider.HeadApproachCircle(s).Opacity = 0;
                            }
                            else if (j == 1) // slider body ball doesnt change opacity
                            {
                                // additional thing... slider body here should also change opacity of ticks BUT i think
                                // it is nicer to have them clearly visible at all times to make it easier to analyze misses and stuff
                                // (this is 100% valid reason please ignore that im lazy and just dont want to put for loop here lol)
                                if (fadeInOpacity > 1)
                                {
                                    // body path has different math
                                    double sliderTotalLength = (s.EndTime - s.SpawnTime) + fadeInDuration;
                                    double sliderBodyOpacity = 1 - ((time - objectSpawnTime) / sliderTotalLength);

                                    Slider.BodyPath(s).Opacity = sliderBodyOpacity;
                                }
                                else
                                {
                                    Slider.BodyPath(s).Opacity = opacity;
                                }

                                Slider.BodyBall(s).Opacity = 1;
                            }
                            else if (j == 2) // fluffy tails > everything in existence so protect slider tail even tho
                            {                // its not fluffy but opacity needs to not change for reverse arrows
                                continue;
                            }
                        }
                    }
                    else if (aliveObjects[i] is Spinner sp) // here just disable approach circle
                    {
                        aliveObjects[i].Opacity = 1; // need to set to 1 otherwise it will be invisible
                        Spinner.ApproachCircle(sp).Opacity = 0;
                    }
                }
            }   
        }

        private static void CorrectObjectVisibility(HitObject hitObject)
        {
            if (hitObject is HitCircle)
            {
                HitCircle? c = hitObject as HitCircle;
                if (HitCircle.ApproachCircle(c).Opacity == 0)
                {
                    HitCircle.ApproachCircle(c).Opacity = 1;
                }
            }
            else if (hitObject is Slider)
            {
                Slider? s = hitObject as Slider;
                if (Slider.HeadHitCircleContainer(s).Opacity != 1)
                {
                    Slider.HeadApproachCircle(s).Opacity = 1;
                    Slider.BodyPath(s).Opacity = 0.8;
                    Slider.HeadHitCircleContainer(s).Opacity = 1;
                }
            }
            else if (hitObject is Spinner)
            {
                Spinner? sp = hitObject as Spinner;
                if (Spinner.ApproachCircle(sp).Opacity == 0)
                {
                    Spinner.ApproachCircle(sp).Opacity = 1;
                }
            }
        }

        // done hopefully this code is also fast... each iteration with 7 objects is ~100 ticks average
        private static void UpdateApproachCircleAnimation(double time, List<HitObject> aliveObjects)
        {
            if (HiddenMod.IsEnabled == true)
            {// there is no point updating something that is invisible
                return;
            }

            Image approachCircle;
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i] is HitCircle)
                {
                    approachCircle = HitCircle.ApproachCircle((HitCircle)aliveObjects[i]);
                }
                else if (aliveObjects[i] is Slider)
                {
                    if (Slider.HeadHitCircleContainer((Slider)aliveObjects[i]).Visibility == Visibility.Collapsed)
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
                double baseSize = (MainWindow.OsuPlayfieldObjectDiameter * (double)approachCircle.DataContext) * 4;
                approachCircle.Width  = (baseSize * progress) / aliveObjects[i].LayoutTransform.Value.M11;
                approachCircle.Height = (baseSize * progress) / aliveObjects[i].LayoutTransform.Value.M11;

                Canvas.SetTop(approachCircle, -(approachCircle.Height / 2) + (aliveObjects[i].Height / 2));
                Canvas.SetLeft(approachCircle, -(approachCircle.Width / 2) + (aliveObjects[i].Width / 2));
            }
        }

        // hopefully this pretty fast... 200-250 ticks with like 230 average
        private static void UpdateSliderBallAnimation(double time, List<HitObject> aliveObjects)
        {
            for (int i = 0; i < aliveObjects.Count; i++)
            {
                if (aliveObjects[i] is not Slider)
                {
                    continue;
                }

                Slider s = (Slider)aliveObjects[i];

                if ((s.Judgement.Judgement > HitObjectJudgement.Miss && s.Judgement.SpawnTime > time
                ||   s.Judgement.Judgement <= HitObjectJudgement.Miss && s.SpawnTime > time)
                &&   Slider.HeadHitCircleContainer(s).Visibility == Visibility.Collapsed)
                {
                    Slider.ShowSliderHead(s);
                }

                // if slider head is not clicked and duration of it is <36ms, loop here will make ball collapsed and visible
                // all the time... tho its not visible but writing this in case this will become a problem
                if (Slider.HeadHitCircleContainer(s).Visibility == Visibility.Visible
                &&  Slider.BodyBall(s).Visibility == Visibility.Visible)
                {
                    Slider.BodyBall(s).Visibility = Visibility.Collapsed;
                }

                double position;
                if (time - s.SpawnTime > s.EndTime - s.SpawnTime)
                {// if current distance based of time is higher than slider distance including repeats, snap position to 1 so ball
                 // wont go into reverse in some edge cases with very short sliders
                    position = 1;
                }
                else if (time < s.SpawnTime)
                {// just for when seeking backwards and slider ball reaches position <0 snap it to 0 coz otherwise its stuck a bit above 0
                    position = 0;
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
                if (position > 0 && ball.Visibility == Visibility.Collapsed)
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

            if (true == true)
            {
                if (performanceCounter.Count > 200)
                {
                    Console.WriteLine($"Median of 200 iterations {name}: {performanceCounter.Sum() / performanceCounter.Count}");
                    if (name == "APPR")
                    {
                        Console.WriteLine();
                    }
                    performanceCounter.Clear();
                }
            }
            else
            {
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
    }
    // in short WPF animations vs mine... also mine are FOR SURE faster by A LOT (done without debugging)
    // CPU mostly visible 5-7% vs 1.5-3% with 0.2% and 5% spikes sometimes
    // GPU literally halfed (10>5, 4.5>2.25 etc)
    // RAM it jumps with new stuff but its 5MB less or 5MB more or the same with mine implementation
    // a whole loop of 7 objects (5 sliders (1 started) + 1 spinner + 1 circle) takes ~500 ticks... i wish someone could tell me if this is good
    // ^ update1 after changing OsuMath to have saved values it went from 400-500 ticks to more stable ~400 ticks
    //   update2 i was living in a lie and the CPU and GPU performance reduction was a lie caused by WPF capping fps to 60 without debugger
    //           here are real numbers (old UNLIMITED vs new):
    //           60fps        CPU and GPU 50-60% better,
    //           144fps       CPU 20-40% better and GPU 10-20%,
    //           240fps       CPU kinda the same, GPU 10% better,
    //           unlimited 1k CPU sometimes better sometimes worse, GPU basically the same
    //   if comparing same fps across both implementations mine is basically the same but smoother
    //   and causes less stutters and strain on the app from using WPF coz WPF hates itself + seek works better
    //   + its easier to implement complex stuff like Hidden (which idk if it would be possible with WPF animations anyway)
}
