using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WpfApp1.Objects;
using WpfApp1.OsuMaths;
using SliderData = ReplayParsers.Classes.Beatmap.osu.Objects.SliderData;
#nullable disable

namespace WpfApp1.Animations
{
    public class AnimationTemplates
    {
        OsuMath math = new OsuMath();

        public MatrixAnimationUsingPath SliderBall(Sliderr slider)
        {
            MatrixAnimationUsingPath animation = new MatrixAnimationUsingPath();

            Canvas sliderBody = slider.Children[0] as Canvas;
            Path sliderBodyPath = sliderBody.Children[1] as Path;
            
            /*
            //PathGeometry pathGeometry = 
            //// from microsoft docs "// Freeze the PathGeometry for performance benefits."
            //// BETTER PERFORMANCE MY ASS I REMOVED IT AND IT IN FACT FIXED MY PERFORMANCE ISSUES
            //// I LOVE PROGRAMMING (at least from my testing removing this fixed lags where lags were)
            ////pathGeometry.Freeze();
            */

            animation.PathGeometry = sliderBodyPath.Data as PathGeometry;
            //animation.PathGeometry.Freeze();

            if (slider.RepeatCount > 1)
            {
                animation.AutoReverse = true;
                animation.RepeatBehavior = new RepeatBehavior(slider.RepeatCount - 1);
                animation.SpeedRatio = slider.RepeatCount;
            }

            animation.Duration = new Duration(TimeSpan.FromMilliseconds((long)(slider.EndTime - slider.SpawnTime)));
            animation.BeginTime = TimeSpan.FromMilliseconds(math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate));

            return animation;
        }

        public void SliderBall(DoubleAnimationUsingPath X, DoubleAnimationUsingPath Y, Canvas hitObject)
        {
            DoubleAnimationUsingPath animation = new DoubleAnimationUsingPath();

            Canvas sliderBody = hitObject.Children[0] as Canvas;
            Path sliderBodyPath = sliderBody.Children[1] as Path;

            PathGeometry pathGeometry = new PathGeometry();
            pathGeometry = sliderBodyPath.Data as PathGeometry;
            pathGeometry.Freeze();

            animation.PathGeometry = pathGeometry;

            SliderData slider = hitObject.DataContext as SliderData;
            if (slider.RepeatCount > 1)
            {
                animation.AutoReverse = true;
                animation.RepeatBehavior = RepeatBehavior.Forever;
                animation.SpeedRatio = slider.RepeatCount;
            }

            animation.Duration = new Duration(TimeSpan.FromMilliseconds((long)(slider.EndTime - slider.SpawnTime)));
            animation.BeginTime = TimeSpan.FromMilliseconds(math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate));
            
            X = animation;
            Y = animation;

            X.Source = PathAnimationSource.X;
            Y.Source = PathAnimationSource.Y;
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

        public DoubleAnimation ApproachCircle()
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

        public DoubleAnimation SpinnerApproachCircle(Spinnerr spinner)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();

            // numbers adjusted by hand i dont know how to math this
            doubleAnimation.From = 1;
            doubleAnimation.To = 0;
            // spinner has slight delay before animation starts... 200 is not accurate but its cosmeting spinner anyway
            // it wont have any functionality other than existing and looking pretty
            doubleAnimation.BeginTime = TimeSpan.FromMilliseconds(200);

            doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(spinner.EndTime - spinner.SpawnTime));

            return doubleAnimation;
        }
    }
}
