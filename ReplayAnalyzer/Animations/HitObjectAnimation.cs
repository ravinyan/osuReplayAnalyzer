using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

#nullable disable

namespace ReplayAnalyzer.Animations
{
    public class HitObjectAnimations
    {
        public static Dictionary<string, List<Storyboard>> sbDict = new Dictionary<string, List<Storyboard>>();

        private static AnimationTemplates template = new AnimationTemplates();

        public static void ApplySpinnerAnimations(Spinner spinner)
        {
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
            List<Storyboard> storyboards = new List<Storyboard>();

            storyboards.Add(FadeIn(circle));
            storyboards.Add(ApproachCircle(circle));

            sbDict.Add(circle.Name, storyboards);
        }

        public static void ApplySliderAnimations(Objects.Slider slider)
        {
            List<Storyboard> storyboards = new List<Storyboard>();

            storyboards.Add(FadeIn(slider));
            storyboards.Add(ApproachCircle(slider));

            // show slider ball and remove slider head
            storyboards[1].Completed += delegate (object sender, EventArgs e)
            {
                Canvas sliderBody = slider.Children[0] as Canvas;
                Canvas ball = sliderBody.Children[2] as Canvas;
                ball.Visibility = Visibility.Visible;
            };
            
            storyboards.Add(SliderBall(slider));
            
            sbDict.Add(slider.Name, storyboards);
        }

        private static Storyboard FadeIn(HitObject hitObject)
        {
            DoubleAnimation fadeIn = template.FadeIn();

            Storyboard.SetTarget(fadeIn, hitObject);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(fadeIn);

            return storyboard;
        }

        private static Storyboard ApproachCircle(HitObject hitObject)
        {
            DoubleAnimation approachCircleX = template.ApproachCircle();
            DoubleAnimation approachCircleY = template.ApproachCircle();
            
            Image approachCircle;
            if (hitObject is Objects.Slider)
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
            storyboard.Children.Add(approachCircleX);
            storyboard.Children.Add(approachCircleY);

            return storyboard;
        }

        private static Storyboard SliderBall(Objects.Slider slider)
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
                        if (hitObject is Objects.Slider && sb == storyboards[2])
                        {
                            TimeSpan beginTime = sb.Children[0].BeginTime.Value;

                            Storyboard arSb = storyboards[1];

                            cur = TimeSpan.FromMilliseconds(GamePlayClock.TimeElapsed - hitObject.SpawnTime) + beginTime;
                            TimeSpan storyboardElapsedTime = cur;
                            if (cur <= beginTime)
                            {
                                cur = beginTime;

                                // its kinda scuffed but what is programming without a bit of scuffed?
                                Canvas head = hitObject.Children[1] as Canvas;
                                Canvas body = hitObject.Children[0] as Canvas;
                                if ((head.Children[0].Visibility == Visibility.Collapsed || head.Visibility == Visibility.Collapsed ||  body.Children[2].Visibility == Visibility.Visible)
                                &&  (hitObject.HitAt == -1 || hitObject.HitAt != -1 && hitObject.HitAt > GamePlayClock.TimeElapsed))
                                {
                                    body.Children[2].Visibility = Visibility.Collapsed;
                                }
                            }

                            // if approach circle exists then
                            if (storyboardElapsedTime >= TimeSpan.Zero && storyboardElapsedTime < beginTime)
                            {
                                sb.Seek(hitObject, storyboardElapsedTime, TimeSeekOrigin.BeginTime);
                            }
                            else if (cur >= beginTime && cur <= beginTime + sb.Children[0].Duration)
                            {
                                sb.Seek(hitObject, cur, TimeSeekOrigin.BeginTime);
                            }
                        }
                        else // and this is for approach rate and fade in... wont make it work for cosmetic spinners
                        {
                            OsuMath math = new OsuMath();

                            double duration = sb.Children[0].Duration.TimeSpan.TotalMilliseconds;

                            double arTime = math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
                            double fadeTime = math.GetFadeInTiming(MainWindow.map.Difficulty.ApproachRate);

                            // time when object is shown on playfield
                            int objectSpawnTime = hitObject.SpawnTime - (int)arTime;

                            double timePassed = GamePlayClock.TimeElapsed - objectSpawnTime;

                            if (duration == fadeTime)
                            {
                                if (timePassed <= fadeTime)
                                {
                                    cur = TimeSpan.FromMilliseconds(timePassed);
                                }
                                else
                                {
                                    cur = TimeSpan.FromMilliseconds(duration);
                                }
                            }
                            else if (duration == arTime)
                            {
                                // for event that fires off when animation is completed
                                if (timePassed >= duration)
                                {
                                    if (TimeSpan.FromMilliseconds(duration) == sb.GetCurrentTime(hitObject))
                                    {
                                        continue;
                                    }

                                    cur = TimeSpan.FromMilliseconds(duration);
                                }
                                else
                                {
                                    cur = TimeSpan.FromMilliseconds(timePassed);
                                }
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
