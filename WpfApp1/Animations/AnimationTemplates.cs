using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
#nullable disable

namespace WpfApp1.Animations
{
    public class AnimationTemplates
    {
        public DoubleAnimation FadeIn()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0;
            doubleAnimation.To = 1.0;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(800 - 500 * ((double)MainWindow.map.Difficulty.ApproachRate - 5) / 5)));

            return doubleAnimation;
        }

        public DoubleAnimation ApproachCircle(Grid hitObject)
        {
            // from and to is dynamically sized so it doesnt need to be here
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            Image approachCircle = VisualTreeHelper.GetChild(hitObject, 3) as Image;
            Image hitCircle = VisualTreeHelper.GetChild(hitObject, 1) as Image;

            // numbers adjusted by hand i dont know how to math this
            doubleAnimation.From = 1.1;
            doubleAnimation.To = 0.25;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(1200 - 750 * (double)(MainWindow.map.Difficulty.ApproachRate - 5) / 5)));

            return doubleAnimation;
        }

        //public DoubleAnimation ApproachCircleOnResize(FrameworkElement hitObject, TimeSpan newBeginTime)
        //{
        //    DoubleAnimation doubleAnimation = new DoubleAnimation();
        //    Image approachCircle = VisualTreeHelper.GetChild(hitObject, 3) as Image;
        //    Image hitCircle = VisualTreeHelper.GetChild(hitObject, 1) as Image;
        //    doubleAnimation.From = hitObject.Width;
        //    doubleAnimation.To = hitCircle.Width;
        //    doubleAnimation.BeginTime = -newBeginTime;
        //    doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(1200 - 750 * (double)(MainWindow.map.Difficulty.ApproachRate - 5) / 5)));

        //    Storyboard.SetTarget(approachCircle, approachCircle);
        //    Storyboard.SetTargetProperty(approachCircle, new PropertyPath(Image.WidthProperty));

        //    return doubleAnimation;
        //}
    }
}
