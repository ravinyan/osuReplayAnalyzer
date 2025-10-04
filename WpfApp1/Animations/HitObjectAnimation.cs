using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfApp1.GameClock;
using WpfApp1.Objects;
using WpfApp1.PlayfieldUI.UIElements;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;

#nullable disable

namespace WpfApp1.Animations
{
    public class HitObjectAnimations
    {
        private static Dictionary<string, List<Storyboard>> sbDict = new Dictionary<string, List<Storyboard>>();

        private static AnimationTemplates template = new AnimationTemplates();

        public static void ApplySpinnerAnimations(Canvas spinner)
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

        public static void ApplyHitCircleAnimations(Canvas circle)
        {
            List<Storyboard> storyboards = new List<Storyboard>();

            storyboards.Add(FadeIn(circle));
            storyboards.Add(ApproachCircle(circle));

            sbDict.Add(circle.Name, storyboards);
        }

        public static void ApplySliderAnimations(Canvas slider)
        {
            List<Storyboard> storyboards = new List<Storyboard>();

            storyboards.Add(FadeIn(slider));
            storyboards.Add(ApproachCircle(slider));

            // show slider ball and remove slider head
            storyboards[1].Completed += async delegate (object sender, EventArgs e)
            {
                Canvas sliderBody = slider.Children[0] as Canvas;
                Canvas head = slider.Children[1] as Canvas;
                Canvas ball = sliderBody.Children[2] as Canvas;

                ball.Visibility = Visibility.Visible;
                OsuMaths.OsuMath math = new OsuMaths.OsuMath();

                await Task.Delay((int)math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty));
                
                if (head.Children[0].Visibility == Visibility.Visible)
                {
                    MainWindow window = (MainWindow)Application.Current.MainWindow;
                
                    Image miss = Analyser.UIElements.HitJudgment.ImageMiss();
                    miss.Width = MainWindow.OsuPlayfieldObjectDiameter;
                    miss.Height = MainWindow.OsuPlayfieldObjectDiameter;
                
                    miss.Loaded += async delegate (object sender, RoutedEventArgs e)
                    {
                        await Task.Delay(800);
                        window.playfieldCanva.Children.Remove(miss);
                    };
                
                    HitObject dc = slider.DataContext as HitObject;
                    double X = (dc.X * MainWindow.OsuPlayfieldObjectScale) - (MainWindow.OsuPlayfieldObjectDiameter / 2);
                    double Y = (dc.Y * MainWindow.OsuPlayfieldObjectScale) - MainWindow.OsuPlayfieldObjectDiameter;
                
                    Canvas.SetLeft(miss, X);
                    Canvas.SetTop(miss, Y);
                
                    JudgementCounter.IncrementMiss();
                    window.playfieldCanva.Children.Add(miss);
                
                    head.Visibility = Visibility.Collapsed;
                }
            };

            storyboards.Add(SliderBall(slider));

            sbDict.Add(slider.Name, storyboards);
        }

        private static Storyboard FadeIn(Canvas hitObject)
        {
            DoubleAnimation fadeIn = template.FadeIn();

            Storyboard.SetTarget(fadeIn, hitObject);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Canvas.OpacityProperty));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(fadeIn);

            return storyboard;
        }

        private static Storyboard ApproachCircle(Canvas hitObject)
        {
            DoubleAnimation approachCircleX = template.ApproachCircle(hitObject);
            DoubleAnimation approachCircleY = template.ApproachCircle(hitObject);
            
            Image approachCircle;
            if (hitObject.DataContext is ReplayParsers.Classes.Beatmap.osu.Objects.Slider)
            {
                Canvas head = hitObject.Children[1] as Canvas;
                approachCircle = head.Children[3] as Image;
            }
            else if (hitObject.DataContext is ReplayParsers.Classes.Beatmap.osu.Objects.Circle)
            {
                approachCircle = hitObject.Children[3] as Image;
            }
            else
            {
                approachCircleX = template.SpinnerApproachCircle(hitObject);
                approachCircleY = template.SpinnerApproachCircle(hitObject);

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

        private static Storyboard SliderBall(Canvas hitObject)
        {
            Canvas sliderBody = hitObject.Children[0] as Canvas;
            Canvas ball = sliderBody.Children[2] as Canvas;

            MatrixTransform buttonMatrixTransform = new MatrixTransform();
            ball.RenderTransform = buttonMatrixTransform;

            NameScope.SetNameScope(hitObject, new NameScope());
            hitObject.RegisterName($"{hitObject.Name}", buttonMatrixTransform);

            MatrixAnimationUsingPath matrixAnimation = template.SliderBall(hitObject);

            Storyboard.SetTargetProperty(matrixAnimation, new PropertyPath(MatrixTransform.MatrixProperty));
            Storyboard.SetTargetName(matrixAnimation, $"{hitObject.Name}");

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(matrixAnimation);

            return storyboard;
        }

        public static void Pause(Canvas hitObject)
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

        public static void ResetSliderBallAnimation(Canvas hitObject)
        {
            if (hitObject.Name != "")
            {
                List<Storyboard> storyboards = sbDict[hitObject.Name];

                storyboards[2].Stop(hitObject);
            }
        }

        public static void Start(Canvas hitObject)
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

        public static void Resume(Canvas hitObject)
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

        public static void Seek(List<Canvas> hitObjects)
        {
            foreach (Canvas hitObject in hitObjects)
            {
                if (hitObject.Name != "")
                {
                    List<Storyboard> storyboards = sbDict[hitObject.Name];

                    foreach (Storyboard sb in storyboards)
                    {
                        HitObject dc = hitObject.DataContext as HitObject;
                        
                        // special case for slider ball coz it needs a bit of offset (beginTime)
                        TimeSpan cur = TimeSpan.Zero;
                        if (hitObject.DataContext is Slider && sb == storyboards[2])
                        {
                            TimeSpan beginTime = sb.Children[0].BeginTime.Value;

                            var arSb = storyboards[1];

                            cur = TimeSpan.FromMilliseconds((GamePlayClock.TimeElapsed - dc.SpawnTime)) + beginTime;
                            if (cur <= beginTime)
                            {
                                cur = beginTime;
                                
                                // its kinda scuffed but what is programming without a bit of scuffed?
                                Canvas head = hitObject.Children[1] as Canvas;
                                Canvas body = hitObject.Children[0] as Canvas;
                                if ((head.Children[0].Visibility == Visibility.Collapsed || head.Visibility == Visibility.Collapsed ||  body.Children[2].Visibility == Visibility.Visible)
                                &&  (dc.HitAt == -1 || (dc.HitAt != -1 && dc.HitAt > GamePlayClock.TimeElapsed)))
                                {
                                    SliderObject.ResetToDefault(hitObject);
                                }
                            }

                            // if approach circle exists then
                            if (arSb.GetCurrentTime(hitObject) != arSb.Children[0].Duration.TimeSpan)
                            {
                                sb.Seek(hitObject, arSb.GetCurrentTime(hitObject).Value, TimeSeekOrigin.BeginTime);
                                
                            }
                            else if (cur > beginTime && cur < beginTime + sb.Children[0].Duration)
                            {
                                sb.Seek(hitObject, cur, TimeSeekOrigin.BeginTime);
                            }
                        }
                        else // and this is for approach rate and fade in... wont make it work for cosmetic spinners
                        {
                            OsuMaths.OsuMath math = new OsuMaths.OsuMath();

                            double duration = sb.Children[0].Duration.TimeSpan.TotalMilliseconds;

                            double arTime = math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
                            double fadeTime = math.GetFadeInTiming(MainWindow.map.Difficulty.ApproachRate);

                            // time when object is shown on playfield
                            int objectSpawnTime = dc.SpawnTime - (int)arTime;

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
