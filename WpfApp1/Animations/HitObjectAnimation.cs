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

            DoubleAnimation fadeIn = template.FadeIn();
            storyboard.Children.Add(fadeIn);

            Storyboard.SetTarget(fadeIn, hitObject);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Canvas.OpacityProperty));

            DoubleAnimation approachCircleX = template.ApproachCircle(hitObject);
            DoubleAnimation approachCircleY = template.ApproachCircle(hitObject);
            storyboard.Children.Add(approachCircleX);
            storyboard.Children.Add(approachCircleY);

            Image approachCircle = VisualTreeHelper.GetChild(hitObject, 3) as Image;
            
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            approachCircle.RenderTransformOrigin = new Point(0.5, 0.5);
            approachCircle.RenderTransform = scale;
            
            Storyboard.SetTargetProperty(approachCircleX, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(approachCircleX, approachCircle);
            Storyboard.SetTargetProperty(approachCircleY, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(approachCircleY, approachCircle);

            // i really have no clue how else to do it and dictionary is extremely fast for this so hopefully no performance issues
            sbDict.Add(storyboard.Name, storyboard);
        }

        public static void ApplySliderAnimations(Canvas hitObject)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Name = hitObject.Name;

            DoubleAnimation fadeIn = template.FadeIn();
            storyboard.Children.Add(fadeIn);

            Storyboard.SetTarget(fadeIn, hitObject);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Canvas.OpacityProperty));

            DoubleAnimation approachCircleX = template.ApproachCircle(hitObject);
            DoubleAnimation approachCircleY = template.ApproachCircle(hitObject);
            storyboard.Children.Add(approachCircleX);
            storyboard.Children.Add(approachCircleY);

            Canvas head = VisualTreeHelper.GetChild(hitObject, 1) as Canvas;
            Image approachCircle = VisualTreeHelper.GetChild(head, 3) as Image;

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            approachCircle.RenderTransformOrigin = new Point(0.5, 0.5);
            approachCircle.RenderTransform = scale;

            Storyboard.SetTargetProperty(approachCircleX, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(approachCircleX, approachCircle);
            Storyboard.SetTargetProperty(approachCircleY, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(approachCircleY, approachCircle);

            MatrixAnimationUsingPath sliderBallAnimation = template.SliderBall(hitObject);
            

            Canvas sliderBody = VisualTreeHelper.GetChild(hitObject, 1) as Canvas;
            var ball = sliderBody.Children[1]; // VisualTreeHelper.GetChild(sliderBody, 3) as Canvas;
            //var ball2 = VisualTreeHelper.GetChild(ball, 1);
            //ball2.RenderTransform = new MatrixTransform();
            ball.RenderTransform = new MatrixTransform();
            Storyboard.SetTargetProperty(sliderBallAnimation, new PropertyPath(MatrixTransform.MatrixProperty));
            Storyboard.SetTarget(sliderBallAnimation, ball);

            storyboard.Children.Add(sliderBallAnimation);

            sbDict.Add(storyboard.Name, storyboard);
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
