using System.Windows;
using System.Windows.Media.Animation;

namespace WpfApp1.Animations
{
    public class HitCircleAnimation : Window
    {
        private static Storyboard? storyboard;

        public static void ApplyHitCircleAnimation(FrameworkElement hitObject)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0.1;
            doubleAnimation.To = 1.0;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(800 - 500 * ((double)MainWindow.map.Difficulty.ApproachRate - 5) / 5)));

            Storyboard storyboard = new Storyboard();
            storyboard.Name = hitObject.Name;
            storyboard.Children.Add(doubleAnimation);

            Storyboard.SetTarget(doubleAnimation, hitObject);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(FrameworkElement.OpacityProperty));

            ApplyAnimationVisibilityEvent(hitObject, storyboard);
        }

        public static void Pause(FrameworkElement hitObject, Storyboard sb)
        {
            storyboard.Pause(hitObject);
        }

        public static void Start(FrameworkElement hitObject)
        {
            storyboard.Begin(hitObject, true);
        }

        public static void Resume(FrameworkElement hitObject, Storyboard sb)
        {
            storyboard.Resume(hitObject);
        }

        public static void ApplyAnimationVisibilityEvent(FrameworkElement hitObject, Storyboard storyboard)
        {
            hitObject.IsVisibleChanged += delegate(object sender, DependencyPropertyChangedEventArgs e)
            {
                storyboard.Begin(hitObject, true);
            };
        }
    }
}
