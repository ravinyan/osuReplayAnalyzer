using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

#nullable disable

namespace WpfApp1.Animations
{
    public class HitObjectAnimations
    {
        
        private static Dictionary<string, Storyboard> sbDict = new Dictionary<string, Storyboard>();

        private static AnimationTemplates template = new AnimationTemplates();

        // i will forget about it so this is for slider ball
        // https://learn.microsoft.com/en-us/dotnet/api/system.windows.uielement.beginanimation?view=windowsdesktop-9.0

        public static void ApplyHitCircleAnimations(Canvas hitObject)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Name = hitObject.Name;

            FadeIn(storyboard, hitObject);
            ApproachCircle(storyboard, hitObject);

            sbDict.Add(storyboard.Name, storyboard);
        }

        public static void ApplySliderAnimations(Canvas hitObject)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Name = hitObject.Name;

            FadeIn(storyboard, hitObject);
            ApproachCircle(storyboard, hitObject);
            SliderBall(storyboard, hitObject);

            sbDict.Add(storyboard.Name, storyboard);
        }
                

        private static void FadeIn(Storyboard storyboard, Canvas hitObject)
        {
            DoubleAnimation fadeIn = template.FadeIn();

            Storyboard.SetTarget(fadeIn, hitObject);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Canvas.OpacityProperty));

            storyboard.Children.Add(fadeIn);
        }

        private static void ApproachCircle(Storyboard storyboard, Canvas hitObject)
        {
            DoubleAnimation approachCircleX = template.ApproachCircle(hitObject);
            DoubleAnimation approachCircleY = template.ApproachCircle(hitObject);
            storyboard.Children.Add(approachCircleX);
            storyboard.Children.Add(approachCircleY);

            Image approachCircle;
            if (hitObject.DataContext is ReplayParsers.Classes.Beatmap.osu.Objects.Slider)
            {
                Canvas head = VisualTreeHelper.GetChild(hitObject, 1) as Canvas;
                approachCircle = VisualTreeHelper.GetChild(head, 3) as Image;
            }
            else
            {
                approachCircle = VisualTreeHelper.GetChild(hitObject, 3) as Image;
            }
                
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            approachCircle.RenderTransformOrigin = new Point(0.5, 0.5);
            approachCircle.RenderTransform = scale;

            Storyboard.SetTargetProperty(approachCircleX, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(approachCircleX, approachCircle);
            Storyboard.SetTargetProperty(approachCircleY, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(approachCircleY, approachCircle);

            storyboard.Children.Add(approachCircleX);
            storyboard.Children.Add(approachCircleY);
        }

        private static void SliderBall(Storyboard storyboard, Canvas hitObject)
        {
            //Canvas sliderBody = VisualTreeHelper.GetChild(hitObject, 0) as Canvas;
            //Canvas ball = VisualTreeHelper.GetChild(sliderBody, 2) as Canvas;
            //
            //TranslateTransform translateTransform = new TranslateTransform();
            //ball.RenderTransform = translateTransform;
            //
            //DoubleAnimationUsingPath pathAnimationX = new DoubleAnimationUsingPath();
            //DoubleAnimationUsingPath pathAnimationY = new DoubleAnimationUsingPath();
            //template.SliderBall(pathAnimationX, pathAnimationY, hitObject);
            //
            //Storyboard.SetTargetProperty(pathAnimationX, new PropertyPath(TranslateTransform.XProperty));
            //Storyboard.SetTarget(pathAnimationX, ball);
            //Storyboard.SetTargetProperty(pathAnimationY, new PropertyPath(TranslateTransform.YProperty));
            //Storyboard.SetTarget(pathAnimationY, ball);
            //
            //
            //storyboard.Children.Add(pathAnimationX);
            //storyboard.Children.Add(pathAnimationY);



            Canvas sliderBody = VisualTreeHelper.GetChild(hitObject, 0) as Canvas;
            Canvas ball = VisualTreeHelper.GetChild(sliderBody, 2) as Canvas;

            MatrixTransform buttonMatrixTransform = new MatrixTransform();
            ball.RenderTransform = buttonMatrixTransform;

            NameScope.SetNameScope(hitObject, new NameScope());
            hitObject.RegisterName($"{hitObject.Name}", buttonMatrixTransform);

            MatrixAnimationUsingPath matrixAnimation = template.SliderBall(hitObject);

            Storyboard.SetTargetProperty(matrixAnimation, new PropertyPath(MatrixTransform.MatrixProperty));
            Storyboard.SetTargetName(matrixAnimation, $"{hitObject.Name}");
            //Storyboard.SetTarget(matrixAnimation, ball);

            storyboard.Children.Add(matrixAnimation);
        }

        public static void Pause(Canvas hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];
            sb.Pause(hitObject);
        }

        public static void Start(Canvas hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];
            sb.Begin(hitObject, true);
        }

        public static void Resume(Canvas hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];
            sb.Resume(hitObject);
        }

        public static void Seek(List<Canvas> hitObjects, long time, int direction)
        {
            foreach (Canvas hitObject in hitObjects)
            {
                Storyboard sb = sbDict[hitObject.Name];
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

        public static void UpdateForward(List<Canvas> hitObjects)
        {
            foreach (Canvas hitObject in hitObjects)
            {
                Storyboard sb = sbDict[hitObject.Name];

                TimeSpan currentTime = sb.GetCurrentTime(hitObject).GetValueOrDefault();
                TimeSpan updatedTime = currentTime += TimeSpan.FromMilliseconds(16);
                
                sb.Seek(hitObject, updatedTime, TimeSeekOrigin.BeginTime);
            }
        }

        public static void UpdateBack(List<Canvas> hitObjects)
        {
            foreach (Canvas hitObject in hitObjects)
            {
                Storyboard sb = sbDict[hitObject.Name];

                TimeSpan currentTime = sb.GetCurrentTime(hitObject).GetValueOrDefault();
                TimeSpan updatedTime = currentTime -= TimeSpan.FromMilliseconds(16);

                if (updatedTime >= TimeSpan.FromMilliseconds(0))
                {
                    sb.Seek(hitObject, updatedTime, TimeSeekOrigin.BeginTime);
                }
            }
        }

        public static bool IsPlaying(Canvas hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];


            return sb.GetIsPaused();
        }

        public static void RemoveStoryboard(Canvas hitObject)
        {
            if (sbDict.ContainsKey(hitObject.Name))
            {
                sbDict.Remove(hitObject.Name);
            }
        }
    }
}
