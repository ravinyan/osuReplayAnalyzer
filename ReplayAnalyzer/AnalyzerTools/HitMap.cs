using NAudio.Gui;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using System.Diagnostics;
using System.Numerics;
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
        public static Vector2 DefaultPosition = new Vector2((float)(Window.ActualWidth - HitMapUI.Width - 35), 35);

        public static Canvas Create()
        {
            HitMapUI.Width = 80;
            HitMapUI.Height = 80;

            HitMapUI.Background = Brushes.Transparent;
            HitMapUI.MouseMove += HitMapUI_MouseMove;

            //if (SettingsOptions.GetConfigValue("HitMapPosition") != "")
            //{
            //    string[] pos = SettingsOptions.GetConfigValue("HitMapPosition").Split(":");
            //    Canvas.SetLeft(HitMapUI, double.Parse(pos[0]));
            //    Canvas.SetTop( HitMapUI, double.Parse(pos[1]));
            //}
            //else
            //{
                Canvas.SetLeft(HitMapUI, DefaultPosition.X);
                Canvas.SetTop(HitMapUI,  DefaultPosition.Y);
            //}

            Ellipse border = new Ellipse();
            border.StrokeThickness = 2;
            border.Stroke = Brushes.White;
            border.Width = 80;
            border.Height = 80;

            HitMapUI.Children.Add(border);
            HitMapUI.Children.Add(HitMapCross());

            return HitMapUI;
        }

        private static Stopwatch w = new Stopwatch();
        private static double X = -1;
        private static double Y = -1;
        private static void HitMapUI_MouseMove(object sender, MouseEventArgs e)
        {
            // Y goes from top  (Y=0) to bottom
            // X goes from left (X=0) to right 
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                w.Start();
                Point pos = e.GetPosition(Window.ApplicationWindowUI);

                X = Window.Width  - pos.X - (HitMapUI.Width  / 2);
                Y = Window.Height - pos.Y - (HitMapUI.Height / 2);

                Canvas.SetLeft(HitMapUI, Window.Width  - HitMapUI.Width  - X);
                Canvas.SetTop(HitMapUI,  Window.Height - HitMapUI.Height - Y);
                w.Stop();
                //Console.WriteLine(w.ElapsedTicks);
                w.Reset();
                SettingsOptions.SaveConfigOption("HitMapPosition", $"{X}:{Y}:{Window.Width}:{Window.Height}");
                //SettingsOptions.SaveConfigOption("HitMapPosition", $"{X}:{Y}");

                

                //w.Start();
                //Point pos = e.GetPosition(Window.ApplicationWindowUI);
                //
                double centerX = Window.Width / 2;
                double centerY = Window.Height / 2;

                //Console.WriteLine("X: " + X);
                //Console.WriteLine("Y: " + Y);

                //if (pos.X < centerX)
                //{
                //    X = centerX - (pos.X - HitMapUI.Width / 2);
                //}
                //else
                //{
                //    X = (pos.X - HitMapUI.Width / 2) - centerX;
                //}
                //
                //if (pos.Y < centerY)
                //{
                //    Y = centerY - (pos.Y - HitMapUI.Height / 2);
                //}
                //else
                //{
                //    Y = (pos.Y - HitMapUI.Height / 2) - centerY;
                //}
                //
                //double posX = pos.X < centerX ? centerX - X : centerX + X;
                //double posY = pos.Y < centerY ? centerY - Y : centerY + Y;
                //
                //Canvas.SetLeft(HitMapUI, posX);
                //Canvas.SetTop( HitMapUI, posY);

                //X = Window.Width - HitMapUI.Width / 2 - (Window.Width - pos.X);
                //Y = Window.Height - HitMapUI.Height / 2 - (Window.Height - pos.Y);
                //
                //Canvas.SetLeft(HitMapUI, X);
                //Canvas.SetTop(HitMapUI, Y);
                //
                
                //
                //w.Stop();
                ////Console.WriteLine(w.ElapsedTicks);
                //w.Reset();
                //
                //Console.WriteLine("X: " + X);
                //Console.WriteLine("Y: " + Y);
            }
            else if (e.LeftButton == MouseButtonState.Released && X != -1 && Y != -1)
            {
               // 
               // X = -1;
               // Y = -1;
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

        public static void Resize()
        {
            string[] pos = SettingsOptions.GetConfigValue("HitMapPosition").Split(":");

            double X = double.Parse(pos[0]);
            double Y = double.Parse(pos[1]);
            double oldWidth = double.Parse(pos[2]);
            double oldHeight = double.Parse(pos[3]);
            // * (Window.Width / oldWidth)
            // * (Window.Height / oldHeight)

            double wp   = Window.Width  / oldWidth;
            double hp   = Window.Height / oldHeight;
            double xp   = (Window.Width  - X - (HitMapUI.Width  / 2) ) / X;
            double yp   = (Window.Height - Y - (HitMapUI.Height / 2) )/ Y;
            double newX = X * wp;
            double newY = Y * hp;
            double newW = Window.Width * wp;
            double newH = Window.Height * hp;

            Console.WriteLine("HMX: " + Canvas.GetLeft(HitMapUI));
            Console.WriteLine("HMY: " + Canvas.GetTop(HitMapUI));
            Console.WriteLine("-");

            if ((Window.Width - HitMapUI.Width - newX) < 0)
            {
                Canvas.SetLeft(HitMapUI, 0);
            }
           //else if ((Window.Width - HitMapUI.Width - newX) > Window.Width)
           //{
           //    Canvas.SetLeft(HitMapUI, Window.Width);
           //}
            else
            {
                Canvas.SetLeft(HitMapUI, (Window.Width - HitMapUI.Width - newX));
            }

            if ((Window.Height - HitMapUI.Height - newY) < 0)
            {
                Canvas.SetLeft(HitMapUI, 0);
            }
            //else if ((Window.Height - HitMapUI.Height - newY) > Window.Height)
            //{
            //    Canvas.SetLeft(HitMapUI, Window.Height);
            //}
            else
            {
                Canvas.SetLeft(HitMapUI, (Window.Height - HitMapUI.Height - newY));
            }

            Console.WriteLine("HMX: " + Canvas.GetLeft(HitMapUI));
            Console.WriteLine("HMY: " + Canvas.GetTop(HitMapUI));

            //Console.WriteLine("URX: " + Canvas.GetLeft(URBar.URBarContainer));
            //Console.WriteLine("URY: " + Canvas.GetTop( URBar.URBarContainer));
            Console.WriteLine();

            SettingsOptions.SaveConfigOption("HitMapPosition", $"{newX}:{newY}:{Window.Width}:{Window.Height}");

            //string[] data = SettingsOptions.GetConfigValue("HitMapPosition").Split(":");
            //double oldX = double.Parse(data[0]);
            //double oldY = double.Parse(data[1]);
            //double oldWidth = double.Parse(data[2]);
            //double oldHeight = double.Parse(data[3]);
            //
            //oldX = Window.Width - (HitMapUI.Width / 2) - (Window.Width - oldX);
            //oldY = Window.Height - (HitMapUI.Height / 2) - (Window.Height - oldY);
            //
            //Canvas.SetLeft(HitMapUI, oldX);
            //Canvas.SetTop(HitMapUI, oldY);

            //double w = Window.Width  / oldWidth;
            //double h = Window.Height / oldHeight;

            //double posX = oldX * w;
            //double posY = oldY * h;

            //double posX = oldX * (Window.Width  / oldWidth);
            //double posY = oldY * (Window.Height / oldHeight);

            //double centerX = Window.Width / 2;
            //double centerY = Window.Height / 2;
            //double posX = oldX < centerX ? centerX - oldX : centerX + oldX;
            //double posY = oldY < centerY ? centerY - oldY : centerY + oldY;

            //Canvas.SetLeft(HitMapUI, posX);
            //Canvas.SetTop(HitMapUI, posY);
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
