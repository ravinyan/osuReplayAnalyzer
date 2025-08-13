using System.Windows;
using System.Windows.Controls;
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

            double ms = (double)math.GetFadeInTiming(MainWindow.map.Difficulty.ApproachRate);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Math.Ceiling(ms)));

            return doubleAnimation;
        }

        public DoubleAnimation ApproachCircle(Canvas hitObject)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();

            // numbers adjusted by hand i dont know how to math this
            doubleAnimation.From = 2;
            doubleAnimation.To = 0.4;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
            //var aaa = Math.Ceiling(1200 - 750 * (double)(MainWindow.map.Difficulty.ApproachRate - 5) / 5);
            double ms = (double)math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ms));

            return doubleAnimation;
        }
    }
}
