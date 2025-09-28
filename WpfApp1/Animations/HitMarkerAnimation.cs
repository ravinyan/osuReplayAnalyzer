using ReplayParsers.Classes.Replay;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using WpfApp1.GameClock;
using WpfApp1.PlayfieldGameplay;

namespace WpfApp1.Animations
{
    public class HitMarkerAnimation
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static Dictionary<string, Storyboard> sbDict = new Dictionary<string, Storyboard>();

        public static void Create(Canvas hitMarker, ReplayFrame frame)
        {
            Storyboard storyboard = new Storyboard();
            storyboard.Name = hitMarker.Name;

            DoubleAnimation animation = new DoubleAnimation(0.99, 1, new Duration(TimeSpan.FromMilliseconds(800)));

            Storyboard.SetTarget(animation, hitMarker);
            Storyboard.SetTargetProperty(animation, new PropertyPath(TextBlock.OpacityProperty));

            storyboard.Children.Add(animation);
            sbDict.Add(storyboard.Name, storyboard);

            storyboard.Completed += delegate (object? sender, EventArgs e)
            {
                Window.playfieldCanva.Children.Remove(hitMarker);
                Playfield.AliveHitMarkers.Remove(hitMarker);
            };
        }

        public static void Pause(Canvas hitMarker)
        {
           Storyboard storyboard = sbDict[hitMarker.Name];
           storyboard.Pause(hitMarker);            
        }

        public static void Start(Canvas hitMarker)
        {
            Storyboard storyboard = sbDict[hitMarker.Name];
            storyboard.Begin(hitMarker, true);
        }

        public static void Resume(Canvas hitMarker)
        {
            Storyboard storyboard = sbDict[hitMarker.Name];
            storyboard.Resume(hitMarker);
        }

        public static void Seek(List<Canvas> hitMarkers)
        {
            foreach (Canvas hitMarker in hitMarkers)
            {
                Storyboard sb = sbDict[hitMarker.Name];

                ReplayFrame? dc = hitMarker.DataContext as ReplayFrame;
                double timePassed = GamePlayClock.TimeElapsed - dc!.Time;

                TimeSpan cur = TimeSpan.FromMilliseconds(timePassed);
                double duration = sb.Children[0].Duration.TimeSpan.TotalMilliseconds;
                if (cur >= TimeSpan.FromMilliseconds(duration))
                {
                    cur = TimeSpan.FromMilliseconds(duration);
                }

                if (cur > TimeSpan.Zero)
                {
                    sb.Seek(hitMarker, cur, TimeSeekOrigin.BeginTime);
                }
            }
        }
    }
}
