using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    // idk how else to call this... 
    public class HitMap
    {
        public static Movable HitMapUI { get; private set; } = new Movable(Movable.Movables.HitMapPosition, true);

        public static Movable Create()
        {
            HitMapUI.Width = 80;
            HitMapUI.Height = 80;

            HitMapUI.ApplyStartingPosition();

            Ellipse border = new Ellipse();
            border.StrokeThickness = 2;
            border.Stroke = Brushes.White;
            border.Width = 80;
            border.Height = 80;

            HitMapUI.Children.Add(border);
            HitMapUI.Children.Add(HitMapCross());

            return HitMapUI;
        }

        public static void AddHitMarker(double percentageXfromCenter, double percentageYfromCenter)
        {
            if (MainWindow.IsReplayPreloading == true || HitMapUI.Visibility == Visibility.Collapsed)
            {
                return;
            }

            Path line = CreateHitMarker();

            // im getting good at this elementary lvl math im proud of myself
            double Y = (HitMapUI.Height / 2) - (line.Height / 2) + HitMapUI.Height * percentageYfromCenter;
            double X = (HitMapUI.Width  / 2) - (line.Width  / 2) + HitMapUI.Height * percentageXfromCenter;

            // dont show hits that are very far from the circle idk if 15 is good number tho
            // the higher it is the further hitmap needs to be from application window border
            if (Y > HitMapUI.Height + 15 || Y < -15
            ||  X > HitMapUI.Width  + 15 || X < -15)
            {
                return;
            }

            Canvas.SetTop(line, Y);
            Canvas.SetLeft(line, X);

            // delete it this was so its easier to see what click is recent
            line.Loaded += async delegate (object sender, RoutedEventArgs e)
            {
                await Task.Delay(2000);
                HitMapUI.Children.Remove(line);
            };

            HitMapUI.Children.Add(line);
        }

        private static Path CreateHitMarker()
        {
            Path line = new Path();
            line.Width = 7;
            line.Height = 7;
            line.StrokeThickness = 1;
            line.Stroke = Brushes.Cyan;
            line.Opacity = 1;

            LineGeometry topLeftLine = new LineGeometry();
            topLeftLine.StartPoint = new Point(0, 0);
            topLeftLine.EndPoint = new Point(line.Width, line.Height);
            topLeftLine.Freeze();

            LineGeometry bottomLeftLine = new LineGeometry();
            bottomLeftLine.StartPoint = new Point(0, line.Height);
            bottomLeftLine.EndPoint = new Point(line.Width, 0);
            bottomLeftLine.Freeze();

            GeometryGroup geometries = new GeometryGroup();
            geometries.Children.Add(topLeftLine);
            geometries.Children.Add(bottomLeftLine);

            line.Data = geometries;

            return line;
        }

        private static Path HitMapCross()
        {
            Path line = new Path();
            line.StrokeThickness = 1;
            line.Stroke = Brushes.White;
            line.Opacity = 0.8;

            LineGeometry upDownLine = new LineGeometry();
            upDownLine.StartPoint = new Point(0, 0);
            upDownLine.EndPoint   = new Point(0, HitMapUI.Height);
            upDownLine.Freeze();

            LineGeometry leftRightLine = new LineGeometry();
            leftRightLine.StartPoint = new Point(-HitMapUI.Width / 2, HitMapUI.Height / 2);
            leftRightLine.EndPoint   = new Point( HitMapUI.Width / 2, HitMapUI.Height / 2);
            leftRightLine.Freeze();

            GeometryGroup geometries = new GeometryGroup();
            geometries.Children.Add(upDownLine);
            geometries.Children.Add(leftRightLine);

            line.Data = geometries;

            Canvas.SetTop(line, 0);
            Canvas.SetLeft(line, HitMapUI.Width / 2);

            return line;
        }
    }
}
