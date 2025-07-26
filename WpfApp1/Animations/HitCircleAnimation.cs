using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

#nullable disable

namespace WpfApp1.Animations
{
    public class HitCircleAnimation
    {
        private static Dictionary<string, Storyboard> sbDict = new Dictionary<string, Storyboard>();

        private static AnimationTemplates template = new AnimationTemplates();

        public static void ApplyHitCircleAnimations(Grid hitObject)
        {
            // its sometimes bugging when pausing and resuming dont know why
            DoubleAnimation fadeIn = template.FadeIn();
            DoubleAnimation approachCircleX = template.ApproachCircle(hitObject);
            DoubleAnimation approachCircleY = template.ApproachCircle(hitObject);
            Storyboard storyboard = new Storyboard();
            storyboard.Name = hitObject.Name;
            storyboard.Children.Add(fadeIn);

            // scaleX and scaleY for RenderTransform
            storyboard.Children.Add(approachCircleX);
            storyboard.Children.Add(approachCircleY);

            Storyboard.SetTarget(fadeIn, hitObject);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Grid.OpacityProperty));

            // approach circle
            Image img = VisualTreeHelper.GetChild(hitObject, 3) as Image;

            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            img.RenderTransformOrigin = new Point(0.505, 0.5);
            img.RenderTransform = scale;
            Storyboard.SetTargetProperty(approachCircleX, new PropertyPath("RenderTransform.ScaleX"));
            Storyboard.SetTarget(approachCircleX, img);
            Storyboard.SetTargetProperty(approachCircleY, new PropertyPath("RenderTransform.ScaleY"));
            Storyboard.SetTarget(approachCircleY, img);

            // i really have no clue how else to do it and dictionary is extremely fast for this so hopefully no performance issues
            sbDict.Add(storyboard.Name, storyboard);

            ApplyAnimationVisibilityEvent(hitObject, storyboard);

            storyboard.Begin(hitObject, true);
            storyboard.Pause(hitObject);
        }

        public static void Pause(Grid hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];
            sb.Pause(hitObject);
        }

        public static void Start(Grid hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];
            sb.Begin(hitObject, true);
        }

        public static void Resume(Grid hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];
            sb.Resume(hitObject);
        }

        public static TimeSpan GetTime(FrameworkElement hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];
            return sb.GetCurrentTime(hitObject).Value;
        }

        public static bool IsNotPlaying(FrameworkElement hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];
            return sb.GetIsPaused(hitObject);
        }

        public static void ApplyAnimationVisibilityEvent(Grid hitObject, Storyboard storyboard)
        {
            hitObject.IsVisibleChanged += delegate(object sender, DependencyPropertyChangedEventArgs e)
            {
                storyboard.Begin(hitObject, true);
            };
        }
    }
}
