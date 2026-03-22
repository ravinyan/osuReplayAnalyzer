using ReplayAnalyzer.OsuMaths;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    // i dont know what im doing but im doing it anyway!
    public class URBar
    {
        private static StackPanel URBarBox = new StackPanel();
        private static Canvas URBarUI = new Canvas();

        // later i could add customizability like in osu lazer coz that is pretty easy
        public static StackPanel Create()
        {// need to refresh UR bar coz of OD changing in beatmaps changing how bar looks/behaves and how judgements are shown
            if (URBarUI.Name != "")
            {
                RemoveOldURBar();
            }

            OsuMath math = new OsuMath();
            double h300 = math.GetOverallDifficultyHitWindow300();
            double h100 = math.GetOverallDifficultyHitWindow100();
            double h50 = math.GetOverallDifficultyHitWindow50();

            double URBarWidth = (h300 * 2) + (h100 * 2) + (h50 * 2);
            ApplyPropertiesToURBarBox(URBarWidth + 30); // 25 for icons
            ApplyPropertiesToURBarUI(URBarWidth);

            URBarBox.Children.Add(CreateLateIconPath());
            URBarBox.Children.Add(URBarUI);
            URBarBox.Children.Add(CreateEarlyIconPath());

            (double, SolidColorBrush)[] judgements =
            {
                // i have no clue what colours to give here this might be good enough? tried to make osu lazer colours
                (h50, new SolidColorBrush(Color.FromRgb(255, 217, 61))),
                (h100, new SolidColorBrush(Color.FromRgb(176, 192, 25))),
                (h300, new SolidColorBrush(Color.FromRgb(138, 216, 255))),
            };

            Path[] paths = new Path[6];
            CreateURBars(judgements, paths);

            foreach (Path p in paths)
            {
                URBarUI.Children.Add(p);
            }

            return URBarBox;
        }

        public static void ShowHit(double timing, SolidColorBrush color)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                return;
            }

            Line line = CreateURHitLine(color, 5);

            Canvas.SetTop(line, 10);
            Canvas.SetLeft(line, timing + URBarUI.Width / 2);
            Canvas.SetZIndex(line, 2);

            line.Loaded += async delegate (object sender, RoutedEventArgs e)
            {
                await Task.Delay(2000);
                URBarUI.Children.Remove(line);
            };

            URBarUI.Children.Add(line);
        }

        private static void RemoveOldURBar()
        {
            MainWindow Window = (MainWindow)Application.Current.MainWindow;
            Window.osuReplayWindow.Children.Remove(URBarBox);
            URBarUI = new Canvas();
            URBarBox = new StackPanel();
        }

        private static Line CreateURHitLine(SolidColorBrush color, double lineWidth)
        {
            Line line = new Line();
            line.Width = lineWidth;
            line.Height = 25;
            line.StrokeThickness = lineWidth;
            line.Opacity = 0.5;
            line.StrokeStartLineCap = PenLineCap.Round;
            line.StrokeEndLineCap = PenLineCap.Round;
            line.Stroke = color;
            line.X1 = 0;
            line.X2 = 0;
            line.Y1 = -25;
            line.Y2 = 10;

            return line;
        }

        private static void ApplyPropertiesToURBarUI(double width)
        {
            URBarUI.Name = "URBarUI";
            URBarUI.Height = 10;
            URBarUI.Width = width;
            URBarUI.Margin = new Thickness(0, 0, 0, -10);
        }

        private static void ApplyPropertiesToURBarBox(double width)
        {
            URBarBox.Name = "URBarBox";
            URBarBox.Height = 10;
            URBarBox.Width = width;
            URBarBox.Margin = new Thickness(0, 0, 0, 10);
            URBarBox.HorizontalAlignment = HorizontalAlignment.Center;
            URBarBox.VerticalAlignment = VerticalAlignment.Bottom;
            URBarBox.Orientation = Orientation.Horizontal;

            // for sharp edges so UR bar looks connected and one plus icon line doesnt look off center with some specific sizes
            RenderOptions.SetEdgeMode(URBarBox, EdgeMode.Aliased);
            
            double scale = 300 / URBarBox.Width;
            URBarBox.RenderTransform = new ScaleTransform(scale, scale, URBarBox.Width / 2, URBarBox.Height / 2);
        }

        private static void CreateURBars((double, SolidColorBrush)[] judgements, Path[] paths)
        {
            int i = 0;
            int j = paths.Length - 1;
            int k = 0;
            int l = judgements.Length - 1;
            double startPos = 0;
            double startPos2 = URBarUI.Width / 2;
            while (i < j)
            {
                (double endPos, SolidColorBrush colour) judgement = judgements[k];
                paths[i] = CreateBar(startPos, startPos + judgement.endPos, judgement.colour);
                startPos += judgement.endPos;
                i++;
                k++;

                judgement = judgements[l];
                paths[j] = CreateBar(startPos2, startPos2 + judgement.endPos, judgement.colour);
                startPos2 += judgement.endPos;
                l--;
                j--;
            }
        }

        private static Path CreateBar(double start, double end, SolidColorBrush color)
        {
            Path path = new Path();
            path.StrokeThickness = 8;
            path.Stroke = color;

            LineGeometry pathGeometryLine = new LineGeometry();
            pathGeometryLine.StartPoint = new Point(start, 0);
            pathGeometryLine.EndPoint = new Point(end, 0);
            pathGeometryLine.Freeze();

            path.Data = pathGeometryLine;

            return path;
        }

        private static Path CreateEarlyIconPath()
        {
            Path early = new Path();
            early.Stroke = new SolidColorBrush(Colors.White);
            early.StrokeThickness = 3;
            early.Data = Geometry.Parse($"M 9,0 L 9,10 M 4,5 L 14,5");
            early.Height = 10;
            early.Width = 15;

            return early;
        }

        private static Path CreateLateIconPath()
        {
            Path late = new Path();
            late.Stroke = new SolidColorBrush(Colors.White);
            late.StrokeThickness = 3;
            late.Data = Geometry.Parse("M -4,5 L 6,5");
            late.Height = 10;
            late.Width = 15;

            return late;
        }
    }
}
