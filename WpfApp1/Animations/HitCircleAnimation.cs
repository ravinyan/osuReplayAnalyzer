using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WpfApp1.Animations
{
    public static class HitCircleAnimation
    {
        private static Storyboard storyboard = new Storyboard();
        private static bool isPlaying = false;

        public static void ApplyHitCircleAnimation(FrameworkElement hitObject)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0.1;
            doubleAnimation.To = 1.0;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(800 - 500 * ((double)MainWindow.map.Difficulty.ApproachRate - 5) / 5)));
            
            Storyboard.SetTarget(doubleAnimation, hitObject);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));

            storyboard.Children.Add(doubleAnimation);
            storyboard.Begin(hitObject, true);

            ApplyAnimationVisibilityEvent(hitObject);
        }

        public static void Pause(FrameworkElement hitObject)
        {
            storyboard.Pause(hitObject);
        }

        public static void Start(FrameworkElement hitObject)
        {
            storyboard.Begin(hitObject, true);
        }

        public static void Resume(FrameworkElement hitObject)
        {
            storyboard.Resume(hitObject);
        }

        public static Storyboard GetStoryboard()
        {
            return storyboard;
        }

        public static void ApplyAnimationVisibilityEvent(FrameworkElement hitObject)
        {
            //hitObject.IsVisibleChanged += delegate(object sender, DependencyPropertyChangedEventArgs e)
            //{
            //    //if (hitObject.Visibility != Visibility.Collapsed
            //    //&&  storyboard.GetCurrentState(hitObject) != ClockState.Active)
            //    //{
            //    //    storyboard.Begin(hitObject, true);
            //    //}
            //};
        }
    }
}
