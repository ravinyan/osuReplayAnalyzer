using ReplayParsers.Classes.Replay;
using System.Windows.Controls;
using System.Windows.Shapes;
using WpfApp1.Animations;

namespace WpfApp1.Analyser.UIElements
{
    public class HitMarker
    {
        private static readonly MainWindow Window = (MainWindow)System.Windows.Application.Current.MainWindow;
        private static readonly Ellipse Cursor = Window.playfieldCursor;

        public static TextBlock Create(ReplayFrame frame, double osuScale, string direction, int index)
        {
            TextBlock hitMarker = new TextBlock();
            hitMarker.FontSize = 16;
            hitMarker.Width = 20;
            hitMarker.Height = 20;
            hitMarker.Text = "X";
            hitMarker.Name = $"HitMarker{index}";
            hitMarker.DataContext = frame;
            hitMarker.TextAlignment = System.Windows.TextAlignment.Center;

            if (direction == "left")
            {
                hitMarker.Foreground = System.Windows.Media.Brushes.Cyan;
            }
            else if (direction == "right")
            {
                hitMarker.Foreground = System.Windows.Media.Brushes.Red;
            }

            Canvas.SetLeft(hitMarker, (frame.X * osuScale) - (Cursor.Width / 2));
            Canvas.SetTop(hitMarker, (frame.Y * osuScale) - (Cursor.Width / 2));
            Canvas.SetZIndex(hitMarker, 999999999);

            HitMarkerAnimation.Create(hitMarker, frame);

            return hitMarker;
        }
    }
}
