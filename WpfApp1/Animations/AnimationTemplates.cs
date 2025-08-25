using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WpfApp1.OsuMaths;
#nullable disable

namespace WpfApp1.Animations
{
    public class AnimationTemplates
    {
        OsuMath math = new OsuMath();

        public MatrixAnimationUsingPath SliderBall(Canvas hitObject)
        {
            MatrixAnimationUsingPath animation = new MatrixAnimationUsingPath();

            Canvas sliderBody = VisualTreeHelper.GetChild(hitObject, 0) as Canvas;
            Path sliderBodyPath = VisualTreeHelper.GetChild(sliderBody, 1) as Path;
            animation.PathGeometry = sliderBodyPath.Data as PathGeometry;

            //animation.AutoReverse = true;
            animation.RepeatBehavior = RepeatBehavior.Forever;
            ReplayParsers.Classes.Beatmap.osu.Objects.Slider slider = hitObject.DataContext as ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            //(long)(slider.EndTime - slider.SpawnTime)
            //animation.BeginTime = TimeSpan.FromMilliseconds(0);
            //math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
            return animation;
        }

        public DoubleAnimation FadeIn()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0;
            doubleAnimation.To = 1.0;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);

            double ms = math.GetFadeInTiming(MainWindow.map.Difficulty.ApproachRate);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ms));

            return doubleAnimation;
        }

        public DoubleAnimation ApproachCircle(Canvas hitObject)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();

            // numbers adjusted by hand i dont know how to math this
            doubleAnimation.From = 4;
            doubleAnimation.To = 1;
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(0);

            double ms = math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(ms));

            return doubleAnimation;
        }
    }
}
