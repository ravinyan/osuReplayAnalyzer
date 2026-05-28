using ReplayAnalyzer.SettingsMenu;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.AnalyzerTools
{
    // idk how else to call this... 
    public class HitMap
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Canvas HitMapUI { get; private set; } = new Canvas();

        // for moving UI element
        private static double X = -1;
        private static double Y = -1;
        private static double W = -1;
        private static double H = -1;
        private static bool IsDragged = false;

        public static Canvas Create()
        {
            HitMapUI.Width = 80;
            HitMapUI.Height = 80;

            HitMapUI.Background  = Brushes.Transparent;
            HitMapUI.MouseMove  += HitMapUI_MouseMove;
            // need both of these so that in any possible case saving position will work
            // mouse up mostly if someone somehow changes position and closes the app
            // mouse leave so that when you open options menu and it will cover the object, position will still get saved
            HitMapUI.MouseUp    += HitMapUI_MouseUp;
            HitMapUI.MouseLeave += HitMapUI_MouseLeave;
            if (SettingsOptions.GetConfigValue("HitMapPosition") != "")
            {
                Resize();
            }
            else
            {
                Canvas.SetLeft(HitMapUI, Window.ActualWidth - HitMapUI.Width - 35);
                Canvas.SetTop(HitMapUI, 35);
            }

            Ellipse border = new Ellipse();
            border.StrokeThickness = 2;
            border.Stroke = Brushes.White;
            border.Width = 80;
            border.Height = 80;

            HitMapUI.Children.Add(border);
            HitMapUI.Children.Add(HitMapCross());

            return HitMapUI;
        }

        public static void ResetPositionToDefault()
        {
            Canvas.SetLeft(HitMapUI, Window.ActualWidth - HitMapUI.Width - 35);
            Canvas.SetTop(HitMapUI, 35);
            SettingsOptions.SaveConfigOption("HitMapPosition", "");
        }

        private static void HitMapUI_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsDragged == true && e.LeftButton == MouseButtonState.Released)
            {
                SettingsOptions.SaveConfigOption("HitMapPosition", $"{X}:{Y}:{W}:{H}");
                IsDragged = false;
            }
        }

        private static void HitMapUI_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDragged == true && e.LeftButton == MouseButtonState.Released)
            {
                SettingsOptions.SaveConfigOption("HitMapPosition", $"{X}:{Y}:{W}:{H}");
                IsDragged = false;
            }
        }

        private static void HitMapUI_MouseMove(object sender, MouseEventArgs e)
        {
            // Y goes from top  (Y=0) to bottom
            // X goes from left (X=0) to right 
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsDragged = true;
                //HitMapUI.Background = Brushes.Red;

                Point pos = e.GetPosition(Window.ApplicationWindowUI);
                Canvas.SetLeft(HitMapUI, pos.X - (HitMapUI.Width  / 2));
                Canvas.SetTop(HitMapUI,  pos.Y - (HitMapUI.Height / 2));

                W = Window.Width;
                H = Window.Height;
                X = pos.X < W / 2 ? pos.X : W - pos.X - (HitMapUI.Width  / 2);
                Y = pos.Y < H / 2 ? pos.Y : H - pos.Y - (HitMapUI.Height / 2);
                if (X == pos.X)
                {
                    W = -1;
                }
                if (Y == pos.Y)
                {
                    H = -1;
                }
            }
        }

        // im so horrible at math its not even funny my head hurts brain work not
        // only problem left(?) is if element is close to center then it will bork when resolutions becames too little
        public static void Resize()
        {
            string[] pos = SettingsOptions.GetConfigValue("HitMapPosition").Split(":");
            if (pos[0] == "")
            {
                ResetPositionToDefault();
                return;
            }

            double x = double.Parse(pos[0]);
            double y = double.Parse(pos[1]);
            double w = double.Parse(pos[2]);
            double h = double.Parse(pos[3]);

            if (w == -1)
            {
                Canvas.SetLeft(HitMapUI, x - HitMapUI.Width / 2);
            }
            else
            {
                Canvas.SetLeft(HitMapUI, Window.Width - HitMapUI.Width - (Window.Width - (Window.Width - x)));
            }

            if (h == -1)
            {
                Canvas.SetTop(HitMapUI, y - HitMapUI.Height / 2);
            }
            else
            {
                Canvas.SetTop(HitMapUI, Window.Height - HitMapUI.Height - (Window.Height - (Window.Height - y)));
            }
        }

        public static void AddHitMarker(double percentageXfromCenter, double percentageYfromCenter)
        {
            if (MainWindow.IsReplayPreloading == true || HitMapUI.Visibility == Visibility.Collapsed)
            {
                return;
            }

            Path line = CreateHitMarker();

            // im getting good at this elementary lvl math im proud of myself
            double Y = ((HitMapUI.Height / 2) - line.Height / 2) + HitMapUI.Height * percentageYfromCenter;
            double X = ((HitMapUI.Width  / 2) - line.Width  / 2) + HitMapUI.Height * percentageXfromCenter;

            // dont show hits that are very far from the circle idk if 15 is good number tho
            // the higher it is the further hitmap needs to be from application window border
            if ((Y > HitMapUI.Height + 15 || Y < -15)
            ||  (X > HitMapUI.Width  + 15 || X < -15))
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
