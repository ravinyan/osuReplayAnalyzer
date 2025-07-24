using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using WpfApp1.OsuMaths;
#nullable disable

namespace WpfApp1.Animations
{
    public class AnimationTemplates
    {
        OsuMath math = new OsuMath();

        public DoubleAnimation FadeIn()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0;
            doubleAnimation.To = 1.0;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);

            //double ms = (double)math.GetFadeInTiming(1200 - 750 * (MainWindow.map.Difficulty.ApproachRate - 5) / 5);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(800 - 500 * ((double)MainWindow.map.Difficulty.ApproachRate - 5) / 5)));

            return doubleAnimation;
        }

        public DoubleAnimation ApproachCircle(Grid hitObject)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();

            // numbers adjusted by hand i dont know how to math this
            doubleAnimation.From = 1.1;
            doubleAnimation.To = 0.25;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);

            //double ms = (double)math.GetFadeInTiming(Math.Ceiling(1200 - 750 * (double)(MainWindow.map.Difficulty.ApproachRate - 5) / 5));
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(1200 - 750 * (double)(MainWindow.map.Difficulty.ApproachRate - 5) / 5)));

            return doubleAnimation;
        }
    }
}
