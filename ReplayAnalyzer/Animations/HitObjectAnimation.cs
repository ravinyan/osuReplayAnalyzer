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
                //Resume(circle);
                //return;
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

                Canvas sliderBody = slider.Children[0] as Canvas;
                Canvas ball = sliderBody.Children[2] as Canvas;
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
                Canvas head = hitObject.Children[1] as Canvas;
                approachCircle = head.Children[3] as Image;
            }
            else if (hitObject is HitCircle)
            {
                approachCircle = hitObject.Children[3] as Image;
            }
            else
            {
                approachCircleX = template.SpinnerApproachCircle(hitObject as Spinner);
                approachCircleY = template.SpinnerApproachCircle(hitObject as Spinner);

                approachCircle = hitObject.Children[2] as Image;
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
            Canvas sliderBody = slider.Children[0] as Canvas;
            Canvas ball = sliderBody.Children[2] as Canvas;

            MatrixTransform buttonMatrixTransform = new MatrixTransform();
            ball.RenderTransform = buttonMatrixTransform;

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
                if (hitObject.Name != "")
                {
                    List<Storyboard> storyboards = sbDict[hitObject.Name];

                    foreach (Storyboard sb in storyboards)
                    { 
                        // special case for slider ball coz it needs a bit of offset (beginTime)
                        TimeSpan cur = TimeSpan.Zero;
                        if (hitObject is Slider && sb == storyboards[2])
                        {
                            TimeSpan beginTime = sb.Children[0].BeginTime.Value;

                            double currentElapsedBallTime = (GamePlayClock.TimeElapsed - hitObject.SpawnTime) / RateChangerControls.RateChange;
                            cur = TimeSpan.FromMilliseconds(currentElapsedBallTime) + beginTime;
                            TimeSpan storyboardElapsedTime = cur;
                            if (cur <= beginTime)
                            {
                                cur = beginTime;

                                // its kinda scuffed but what is programming without a bit of scuffed?
                                Canvas head = hitObject.Children[1] as Canvas;
                                Canvas body = hitObject.Children[0] as Canvas;

                                if ((head.Children[0].Visibility == Visibility.Collapsed || head.Visibility == Visibility.Collapsed || body.Children[2].Visibility == Visibility.Visible)
                                &&  (hitObject.Judgement.ObjectJudgement <= HitObjectJudgement.Miss 
                                ||   hitObject.Judgement.ObjectJudgement > HitObjectJudgement.Miss && hitObject.Judgement.SpawnTime > GamePlayClock.TimeElapsed))
                                {
                                    body.Children[2].Visibility = Visibility.Collapsed;
                                }
                            }

                            // if approach circle exists then
                            if (storyboardElapsedTime >= TimeSpan.Zero && storyboardElapsedTime < beginTime)
                            {
                                sb.Seek(hitObject, storyboardElapsedTime, TimeSeekOrigin.BeginTime);
                            }
                            else if (cur >= beginTime)
                            {
                                sb.Seek(hitObject, cur, TimeSeekOrigin.BeginTime);
                            }
                        }
                        else // and this is for approach rate and fade in... wont make it work for cosmetic spinners < perhaps one day...
                        {
                            TimeSpan duration = sb.Children[0].Duration.TimeSpan;

                            OsuMath math = new OsuMath();
                            double arTime = math.GetApproachRateTiming();
                            double fadeTime = math.GetFadeInTiming();
                            
                            // time when object is shown on playfield
                            double objectSpawnTime = hitObject.SpawnTime - arTime;
                            double timePassed = (GamePlayClock.TimeElapsed - objectSpawnTime) / RateChangerControls.RateChange;

                            // since all storyboards change/are created live when object spawns/rate is changed
                            // this makes sure the values translate to duration values so that objects will be visible when Seek() is called
                            if (duration.TotalMilliseconds != fadeTime || duration.TotalMilliseconds != arTime)
                            {
                                fadeTime = fadeTime / RateChangerControls.RateChange;
                                arTime = arTime / RateChangerControls.RateChange;
                            }

                            // thank you my lord and saviour peppy for this function
                            if (OsuMath.AlmostEquals((float)fadeTime, (float)duration.TotalMilliseconds))
                            {
                                cur = TimeSpan.FromMilliseconds(timePassed);
                            }
                            else if (OsuMath.AlmostEquals((float)arTime, (float)duration.TotalMilliseconds))
                            {
                                cur = TimeSpan.FromMilliseconds(timePassed);

                                if (hitObject is Slider
                                && (hitObject.Judgement.ObjectJudgement > HitObjectJudgement.Miss && hitObject.Judgement.SpawnTime > GamePlayClock.TimeElapsed
                                ||  hitObject.Judgement.ObjectJudgement <= HitObjectJudgement.Miss && hitObject.SpawnTime > GamePlayClock.TimeElapsed))
                                {
                                    Canvas head = hitObject.Children[1] as Canvas;
                                    if (head.Children[0].Visibility == Visibility.Collapsed)
                                    {
                                        Slider.ShowSliderHead(head);
                                    }
                                    //if (hitObject.IsHit == false)
                                    //{
                                    //    Canvas body = hitObject.Children[0] as Canvas;
                                    //    Canvas ball = body.Children[2] as Canvas;
                                    //    if (ball.Visibility == Visibility.Collapsed)
                                    //    {
                                    //        HitObjectManager.ShowSliderHead(head);
                                    //    }
                                    //}
                                    //else if (head.Children[0].Visibility == Visibility.Collapsed)
                                    //{
                                    //    HitObjectManager.ShowSliderHead(head);
                                    //}
                                }
                            }

                            if (cur > duration)
                            {
                                cur = duration;
                            }

                            if (cur >= TimeSpan.Zero)
                            {
                                sb.Seek(hitObject, cur, TimeSeekOrigin.BeginTime);
                            }
                        }
                    }
                }
            }
        }
    }
}
