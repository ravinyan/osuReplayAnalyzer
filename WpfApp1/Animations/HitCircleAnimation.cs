using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
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

        public static void ApplyHitCircleAnimations(Canvas hitObject)
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
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Canvas.OpacityProperty));

            // approach circle
            Image img = VisualTreeHelper.GetChild(hitObject, 3) as Image;
            
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            img.RenderTransformOrigin = new Point(0.5, 0.5);
            img.RenderTransform = scale;

            Storyboard.SetTargetProperty(approachCircleX, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(approachCircleX, img);
            Storyboard.SetTargetProperty(approachCircleY, new PropertyPath("(RenderTransform).(ScaleTransform.ScaleY)"));
            Storyboard.SetTarget(approachCircleY, img);

            // i really have no clue how else to do it and dictionary is extremely fast for this so hopefully no performance issues
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

        public static void RemoveStoryboard(Canvas hitObject)
        {
            if (sbDict.ContainsKey(hitObject.Name))
            {
                sbDict.Remove(hitObject.Name);
            }
        }
    }
}
