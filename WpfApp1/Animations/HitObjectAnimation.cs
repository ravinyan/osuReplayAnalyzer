using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfApp1.PlayfieldUI.UIElements;

#nullable disable

namespace WpfApp1.Animations
{
    public class HitObjectAnimations
    {
        private static Dictionary<string, List<Storyboard>> sbDict = new Dictionary<string, List<Storyboard>>();

        private static AnimationTemplates template = new AnimationTemplates();

        // i will forget about it so this is for slider ball
        // https://learn.microsoft.com/en-us/dotnet/api/system.windows.uielement.beginanimation?view=windowsdesktop-9.0

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

            storyboards[1].Completed += delegate (object sender, EventArgs e)
            {
                foreach (Storyboard sb in storyboards)
                {
                    sb.Stop();
                }
            };

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
                Canvas o = VisualTreeHelper.GetChild(slider, 1) as Canvas;
                Canvas sliderBody = VisualTreeHelper.GetChild(slider, 0) as Canvas;
                Canvas ball = VisualTreeHelper.GetChild(sliderBody, 2) as Canvas;

                ball.Visibility = Visibility.Visible;

                OsuMaths.OsuMath math = new OsuMaths.OsuMath();
                await Task.Delay((int)math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty));

                if (o.Visibility == Visibility.Visible)
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

                    o.Visibility = Visibility.Collapsed;
                }
            };

            storyboards.Add(SliderBall(slider));

            // reset slider ball and slider head to their default visibility
            storyboards[2].Completed += async delegate (object sender, EventArgs e)
            {
                await Task.Delay(100);
            
                Canvas head = VisualTreeHelper.GetChild(slider, 1) as Canvas;
                Canvas sliderBody = VisualTreeHelper.GetChild(slider, 0) as Canvas;
                Canvas ball = VisualTreeHelper.GetChild(sliderBody, 2) as Canvas;
            
                ball.Visibility = Visibility.Collapsed;
                head.Visibility = Visibility.Visible;

                foreach (Storyboard sb in storyboards)
                {
                    sb.Stop();
                }
            };

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
                Canvas head = VisualTreeHelper.GetChild(hitObject, 1) as Canvas;
                approachCircle = VisualTreeHelper.GetChild(head, 3) as Image;
            }
            else if (hitObject.DataContext is ReplayParsers.Classes.Beatmap.osu.Objects.Circle)
            {
                approachCircle = VisualTreeHelper.GetChild(hitObject, 3) as Image;
            }
            else
            {
                approachCircleX = template.SpinnerApproachCircle(hitObject);
                approachCircleY = template.SpinnerApproachCircle(hitObject);

                approachCircle = VisualTreeHelper.GetChild(hitObject, 2) as Image;
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
            Canvas sliderBody = VisualTreeHelper.GetChild(hitObject, 0) as Canvas;
            Canvas ball = VisualTreeHelper.GetChild(sliderBody, 2) as Canvas;

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

        public static void Seek(List<Canvas> hitObjects, long time, int direction)
        {
            foreach (Canvas hitObject in hitObjects)
            {
                if (hitObject.Name != "")
                {
                    List<Storyboard> storyboards = sbDict[hitObject.Name];

                    foreach (Storyboard sb in storyboards)
                    {
                        TimeSpan currentTime = sb.GetCurrentTime(hitObject).GetValueOrDefault();

                        TimeSpan updatedTime;
                        if (direction > 0)
                        {
                            updatedTime = currentTime - TimeSpan.FromMilliseconds(time);

                            if (updatedTime < TimeSpan.Zero)
                            {
                                updatedTime = TimeSpan.Zero;
                            }

                            sb.Seek(hitObject, updatedTime, TimeSeekOrigin.BeginTime);
                        }
                        else
                        {
                            updatedTime = currentTime - TimeSpan.FromMilliseconds(time);

                            if (updatedTime < TimeSpan.Zero)
                            {
                                updatedTime = TimeSpan.Zero;
                            }

                            sb.Seek(hitObject, updatedTime, TimeSeekOrigin.BeginTime);
                        }
                    }
                }
            }
        }
    }
}
