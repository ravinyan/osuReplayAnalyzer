using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.OsuMaths;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Slider = ReplayAnalyzer.HitObjects.Slider;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;

#nullable disable

namespace ReplayAnalyzer.Animations
{
    public class HitObjectAnimations
    {
        public static Dictionary<string, List<Storyboard>> sbDict = new Dictionary<string, List<Storyboard>>();

        private static AnimationTemplates template = new AnimationTemplates();

        public static void ApplySpinnerAnimations(Spinner spinner)
        {
            if (sbDict.ContainsKey(spinner.Name))
            {
                sbDict.Remove(spinner.Name);
            }

            List<Storyboard> storyboards = new List<Storyboard>();
            storyboards.Add(FadeIn(spinner));
            storyboards.Add(ApproachCircle(spinner));

            storyboards[0].Completed += delegate (object sender, EventArgs e)
            {
                foreach (Storyboard sb in storyboards)
                {
                    sb.Stop();
                }
            };

            sbDict.Add(spinner.Name, storyboards);
        }

        public static void ApplyHitCircleAnimations(HitCircle circle)
        {
            if (sbDict.ContainsKey(circle.Name))
            {
                sbDict.Remove(circle.Name);
            }

            List<Storyboard> storyboards = new List<Storyboard>();
            storyboards.Add(FadeIn(circle));
            storyboards.Add(ApproachCircle(circle));

            sbDict.Add(circle.Name, storyboards);
        }

        public static void ApplySliderAnimations(Slider slider)
        {
            if (sbDict.ContainsKey(slider.Name))
            {
                sbDict.Remove(slider.Name);
            }

            List<Storyboard> storyboards = new List<Storyboard>();
            storyboards.Add(FadeIn(slider));
            storyboards.Add(ApproachCircle(slider));
            storyboards.Add(SliderBall(slider));
            
            sbDict.Add(slider.Name, storyboards);

            AddEventCompleted(slider);
        }

        // elp
        public static EventHandler handler = (sender, e) => ApproachCircleCompleted(sender, e);

        private static void AddEventCompleted(Slider slider)
        {
            List<Storyboard> storyboards = sbDict[slider.Name];
            storyboards[1].Completed += handler;
        }

        public static void RemoveEventCompleted(Slider slider)
        {
            List<Storyboard> storyboards = sbDict[slider.Name];
            storyboards[1].Completed -= handler;
        }

        private static void ApproachCircleCompleted(object sender, EventArgs e)
        {
            ClockGroup clock = sender as ClockGroup;
            if (clock.CurrentProgress == null)
            {
                // when speed rate is changed this event is for some reason called even tho all values of clock are null
                // if the values are null then return coz this even should not be rised when changing speed rate
                return;
            }

            List<Slider> sliders = new List<Slider>();
            foreach (HitObject obj in HitObjectManager.GetAliveHitObjects())
            {
                if (obj is Slider)
                {
                    sliders.Add(obj as Slider);
                }
            }

            OsuMath math = new OsuMath();

            // track multiple sliders at the same time (i guess this will make aspire stuff work too?)
            // hopefully this wont cause bugs (spoiler it did coz im bad)
            foreach (Slider slider in sliders)
            {
                // -20 coz it sometimes not spawn ball probably coz of frame timings idk but it works now
                if (GamePlayClock.TimeElapsed < slider.SpawnTime - 20)
                {
                    continue;
                }

                Canvas ball = Slider.BodyBall(slider);
                ball.Visibility = Visibility.Visible;
            }
        }

        private static Storyboard FadeIn(HitObject hitObject)
        {
            DoubleAnimation fadeIn = template.FadeIn();

            Storyboard.SetTarget(fadeIn, hitObject);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Name = "FadeIn";
            storyboard.Children.Add(fadeIn);

            return storyboard;
        }

        private static Storyboard ApproachCircle(HitObject hitObject)
        {
            DoubleAnimation approachCircleX = template.ApproachCircle();
            DoubleAnimation approachCircleY = template.ApproachCircle();
            
            Image approachCircle;
            if (hitObject is Slider)
            {
                approachCircle = Slider.HeadApproachCircle(hitObject as Slider);
            }
            else if (hitObject is HitCircle)
            {
                approachCircle = HitCircle.ApproachCircle(hitObject as HitCircle);
            }
            else
            {
                approachCircleX = template.SpinnerApproachCircle(hitObject as Spinner);
                approachCircleY = template.SpinnerApproachCircle(hitObject as Spinner);

                approachCircle = Spinner.ApproachCircle(hitObject as Spinner);
            }

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            approachCircle.RenderTransformOrigin = new Point(0.5, 0.5);
            approachCircle.RenderTransform = scale;

            Storyboard.SetTargetProperty(approachCircleX, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(approachCircleX, approachCircle);
            Storyboard.SetTargetProperty(approachCircleY, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(approachCircleY, approachCircle);

            Storyboard storyboard = new Storyboard();
            storyboard.Name = "ApproachCircle";
            storyboard.Children.Add(approachCircleX);
            storyboard.Children.Add(approachCircleY);

            approachCircleX = null;
            approachCircleY = null;

            return storyboard;
        }

        private static Storyboard SliderBall(Slider slider)
        {
            MatrixTransform buttonMatrixTransform = new MatrixTransform();
            Slider.BodyBall(slider).RenderTransform = buttonMatrixTransform;

            NameScope.SetNameScope(slider, new NameScope());
            slider.RegisterName($"{slider.Name}", buttonMatrixTransform);

            MatrixAnimationUsingPath matrixAnimation = template.SliderBall(slider);

            Storyboard.SetTargetProperty(matrixAnimation, new PropertyPath(MatrixTransform.MatrixProperty));
            Storyboard.SetTargetName(matrixAnimation, $"{slider.Name}");

            Storyboard storyboard = new Storyboard();
            storyboard.Name = "SliderBall";
            storyboard.Children.Add(matrixAnimation);

            return storyboard;
        }

        public static void Pause(HitObject hitObject)
        {
            if (hitObject.Name != "")
            {
                List<Storyboard> storyboards = sbDict[hitObject.Name];
                foreach (Storyboard sb in storyboards)
                {
                    sb.Pause(hitObject);
                }
            }
        }

        public static void Remove(HitObject hitObject)
        {
            if (hitObject.Name != "")
            {
                List<Storyboard> storyboards = sbDict[hitObject.Name];
                foreach (Storyboard sb in storyboards)
                {
                    sb.Remove(hitObject);  
                }
            }
        }

        public static void RemoveStoryboardFromDict(HitObject hitObject)
        {
            sbDict.Remove(hitObject.Name);
        }

        public static void ClearStoryboardDict()
        {
            sbDict.Clear();
        }

        public static void Start(HitObject hitObject)
        {
            if (hitObject.Name != "")
            {
                List<Storyboard> storyboards = sbDict[hitObject.Name];
                foreach (Storyboard sb in storyboards)
                {
                    sb.Begin(hitObject, true);
                }
            }
        }

        public static void Resume(HitObject hitObject)
        {
            if (hitObject.Name != "")
            {
                List<Storyboard> storyboards = sbDict[hitObject.Name];
                foreach (Storyboard sb in storyboards)
                {
                    sb.Resume(hitObject);
                }
            }
        }

        public static void Seek(List<HitObject> hitObjects)
        {
            foreach (HitObject hitObject in hitObjects)
            {
                if (hitObject.Name == "")
                {
                    continue;
                }

                List<Storyboard> storyboards = sbDict[hitObject.Name];
                foreach (Storyboard sb in storyboards)
                { 
                    TimeSpan cur = TimeSpan.Zero;

                    // slider ball
                    if (storyboards.Count > 2 && hitObject is Slider && sb == storyboards[2])
                    {
                        HideSliderBallIfSliderHeadExists(hitObject as Slider);

                        double currentElapsedBallTime = (GamePlayClock.TimeElapsed - hitObject.SpawnTime) / RateChangerControls.RateChange;
                        cur = TimeSpan.FromMilliseconds(currentElapsedBallTime) + sb.Children[0].BeginTime.Value;
                        if (cur >= TimeSpan.Zero)
                        {
                            sb.Seek(hitObject, cur, TimeSeekOrigin.BeginTime);
                        }
                    }
                    else if (hitObject is not Spinner) // circle/slider fade in + approach circle
                    {
                        OsuMath math = new OsuMath();
                        double objectSpawnTime = hitObject.SpawnTime - math.GetApproachRateTiming();
                        double timePassed = (GamePlayClock.TimeElapsed - objectSpawnTime) / RateChangerControls.RateChange;

                        // shows slider head if time is before head was clicked/despawned
                        ShowSliderHead(hitObject);

                        cur = TimeSpan.FromMilliseconds(timePassed);
                        if (cur >= TimeSpan.Zero)
                        {
                            sb.Seek(hitObject, cur, TimeSeekOrigin.BeginTime);
                        }
                    }
                    else // spinner fade in + approach circle
                    {
                        if (sb.Name == "FadeIn")
                        {
                            double objectSpawnTime = hitObject.SpawnTime;
                            double timePassed = (GamePlayClock.TimeElapsed - objectSpawnTime) / RateChangerControls.RateChange;
                            cur = TimeSpan.FromMilliseconds(timePassed);
                        }
                        else // spinner approach circle
                        {
                            double objectSpawnTime = hitObject.SpawnTime + Spinner.SpawnOffset;
                            double timePassed = (GamePlayClock.TimeElapsed - objectSpawnTime) / RateChangerControls.RateChange;
                            cur = TimeSpan.FromMilliseconds(timePassed) + sb.Children[0].BeginTime.Value;
                        }

                        if (cur >= TimeSpan.Zero)
                        {
                            sb.Seek(hitObject, cur, TimeSeekOrigin.BeginTime);
                        }
                    }
                }
            }
        }

        private static void ShowSliderHead(HitObject slider)
        {
            if (slider is Slider s
            && (s.Judgement.ObjectJudgement > HitObjectJudgement.Miss && s.Judgement.SpawnTime > GamePlayClock.TimeElapsed
            ||  s.Judgement.ObjectJudgement <= HitObjectJudgement.Miss && s.SpawnTime > GamePlayClock.TimeElapsed))
            {
                if (Slider.HeadApproachCircle(s).Visibility == Visibility.Collapsed)
                {
                    Slider.ShowSliderHead(s);
                }
            }
        }

        private static void HideSliderBallIfSliderHeadExists(Slider s)
        {
            if ((Slider.HeadHitCircle(s).Visibility == Visibility.Collapsed || Slider.Head(s).Visibility == Visibility.Collapsed || Slider.BodyBall(s).Visibility == Visibility.Visible)
            &&  (s.Judgement.ObjectJudgement <= HitObjectJudgement.Miss
            ||   s.Judgement.ObjectJudgement > HitObjectJudgement.Miss && s.Judgement.SpawnTime > GamePlayClock.TimeElapsed))
            {
                Slider.BodyBall(s).Visibility = Visibility.Collapsed;
            }
        }
    }
}
