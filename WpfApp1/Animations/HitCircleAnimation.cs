using NAudio.Mixer;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

#nullable disable

namespace WpfApp1.Animations
{
    public class HitObjectAnimations
    {
        private static Dictionary<string, Storyboard> sbDict = new Dictionary<string, Storyboard>();

        private static AnimationTemplates template = new AnimationTemplates();
        private static OsuMaths.OsuMath math = new OsuMaths.OsuMath();

        // i will forget about it so this is for slider ball
        // https://learn.microsoft.com/en-us/dotnet/api/system.windows.uielement.beginanimation?view=windowsdesktop-9.0

        public static Dictionary<string, DoubleAnimation> Animations = new Dictionary<string, DoubleAnimation>();

        public static DoubleAnimation FadeIn()
        {
            DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1.0
                , TimeSpan.FromMilliseconds(math.GetFadeInTiming(MainWindow.map.Difficulty.ApproachRate))
                , FillBehavior.HoldEnd);

            if (!Animations.ContainsKey("TEST"))
            {
                Animations.Add("TEST", fadeInAnimation);
            }
            

            return fadeInAnimation;
        }

        public static DoubleAnimation ApproachCircle()
        {
            DoubleAnimation approachCircleAnimation = new DoubleAnimation(1.0, 0.4
                , TimeSpan.FromMilliseconds(math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate))
                , FillBehavior.HoldEnd);
            
            return approachCircleAnimation;
        }

        public static void ApplyHitCircleAnimations(Canvas hitObject)
        {
            //// its sometimes bugging when pausing and resuming dont know why
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

        public static void UpdateForward(List<Canvas> hitObjects)
        {
            foreach (Canvas hitObject in hitObjects)
            {
                Storyboard sb = sbDict[hitObject.Name];

                var a = FadeIn();
                var b = Animations["TEST"];
               
                //sb.Begin(hitObject, true);
            

               
                TimeSpan currentTime = sb.GetCurrentTime(hitObject).GetValueOrDefault();

                if (currentTime != TimeSpan.Zero)
                {
                    string s = "i dotn care if you work or not im sot mad i leave goodbye";
                }

                sb.Seek(currentTime += TimeSpan.FromMilliseconds(16));
            }
        }

        public static void UpdateBack(List<Canvas> hitObjects)
        {
            foreach (Canvas hitObject in hitObjects)
            {
                Storyboard sb = sbDict[hitObject.Name];
                TimeSpan currentTime = sb.GetCurrentTime();

                if (currentTime - TimeSpan.FromMilliseconds(16) >= TimeSpan.FromMilliseconds(0))
                {
                    sb.Seek(currentTime -= TimeSpan.FromMilliseconds(16));
                }
            }
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
