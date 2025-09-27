using ReplayParsers.Classes.Replay;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApp1.Animations;

namespace WpfApp1.Analyser.UIElements
{
    public class HitMarker
    {
        private static readonly MainWindow Window = (MainWindow)System.Windows.Application.Current.MainWindow;
        private static readonly Ellipse Cursor = Window.playfieldCursor;

        public static Canvas Create(ReplayFrame frame, string direction, int index)
        {
            Canvas hitMarker = new Canvas();
            hitMarker.Width = 20;
            hitMarker.Height = 20;
            hitMarker.DataContext = frame;
            hitMarker.Name = $"HitMarker{index}";

            Rectangle middleHit = new Rectangle();
            middleHit.Fill = Brushes.HotPink;
            middleHit.Width = 1;
            middleHit.Height = 1;

            Canvas.SetLeft(middleHit, (Cursor.Width / 2) - 1);
            Canvas.SetTop(middleHit, (Cursor.Width / 2) - 1);

            Path rightHalf = new Path();
            rightHalf.Data = Geometry.Parse($"M {(int)Cursor.Width / 2},2 a 1 1 0 0 0 1 20");
            rightHalf.StrokeThickness = 2;

            Path leftHalf = new Path();
            leftHalf.Data = Geometry.Parse($"M {(int)Cursor.Width / 2},2 a 1 1 0 0 1 0 20");
            leftHalf.StrokeThickness = 2;

            hitMarker.Children.Add(rightHalf);
            hitMarker.Children.Add(leftHalf);
            hitMarker.Children.Add(middleHit);

            if (direction == "left")
            {
                leftHalf.Stroke = Brushes.HotPink;
                rightHalf.Stroke = Brushes.LightGray;
            }
            else if (direction == "right")
            {
                rightHalf.Stroke = Brushes.HotPink;
                leftHalf.Stroke = Brushes.LightGray;
            }

            Canvas.SetLeft(hitMarker, (frame.X) - (Cursor.Width / 2));
            Canvas.SetTop(hitMarker, (frame.Y) - (Cursor.Width / 2));
            Canvas.SetZIndex(hitMarker, 999);

            HitMarkerAnimation.Create(hitMarker, frame);

            return hitMarker;
        }
    }
}
