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

        public static void ApplyHitCircleAnimation(Grid hitObject)
        {
            // its sometimes bugging when pausing and resuming dont know why
            DoubleAnimation fadeIn = FadeInAnimation();
            DoubleAnimation approachCircle = ApproachCircleAnimation(hitObject);
            
            Storyboard storyboard = new Storyboard();
            storyboard.Name = hitObject.Name;
            storyboard.Children.Add(fadeIn);
            storyboard.Children.Add(approachCircle);
            // i really have no clue how else to do it and dictionary is extremely fast for this so hopefully no performance issues

            Storyboard.SetTarget(fadeIn, hitObject);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(Grid.OpacityProperty));

            Image img = VisualTreeHelper.GetChild(hitObject, 3) as Image;
            Storyboard.SetTarget(approachCircle, img);
            Storyboard.SetTargetProperty(approachCircle, new PropertyPath(Image.WidthProperty));

            sbDict.Add(storyboard.Name, storyboard);

            ApplyAnimationVisibilityEvent(hitObject, storyboard);
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

        public static void AdjustApproachCircleAnimationSize(Grid hitObject)
        {
            Storyboard sb = sbDict[hitObject.Name];
            DoubleAnimation approachCircle = ApproachCircleAnimation(hitObject);
            AnimationClock animationClock = approachCircle.CreateClock();

            var a = approachCircle.GetCurrentValue(approachCircle.From, approachCircle.To, animationClock);
            var b = sb.GetCurrentProgress();
        }

        public static void ApplyAnimationVisibilityEvent(Grid hitObject, Storyboard storyboard)
        {
            hitObject.IsVisibleChanged += delegate(object sender, DependencyPropertyChangedEventArgs e)
            {
                storyboard.Begin(hitObject, true);
            };
        }

        private static DoubleAnimation FadeInAnimation()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0.1;
            doubleAnimation.To = 1.0;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(800 - 500 * ((double)MainWindow.map.Difficulty.ApproachRate - 5) / 5)));

            return doubleAnimation;
        }

        private static DoubleAnimation ApproachCircleAnimation(Grid hitObject)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            Image approachCircle = VisualTreeHelper.GetChild(hitObject, 3) as Image;
            Image hitCircle = VisualTreeHelper.GetChild(hitObject, 1) as Image;
            doubleAnimation.From = approachCircle.Width;  
            doubleAnimation.To = hitCircle.Width;   
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(1200 - 750 * (double)(MainWindow.map.Difficulty.ApproachRate - 5) / 5)));

            return doubleAnimation;
        }
    }
}
