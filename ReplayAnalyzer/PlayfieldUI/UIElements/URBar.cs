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
        private static Canvas URBarBox = new Canvas();

        // later i could add customizability like in osu lazer coz that is pretty easy
        public static Canvas Create()
        {// need to refresh UR bar coz of OD changing in beatmaps changing how bar looks/behaves and how judgements are shown
            if (URBarBox.Name != "")
            {
                RemoveOldURBar();
            }

            OsuMath math = new OsuMath();
            double h300 = math.GetOverallDifficultyHitWindow300();
            double h100 = math.GetOverallDifficultyHitWindow100();
            double h50 = math.GetOverallDifficultyHitWindow50();

            ApplyPropertiesToURBarBox((h300 * 2) + (h100 * 2) + (h50 * 2));
            
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
                URBarBox.Children.Add(p);
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
            Canvas.SetLeft(line, timing + URBarBox.Width / 2);
            Canvas.SetZIndex(line, 2);

            line.Loaded += async delegate (object sender, RoutedEventArgs e)
            {
                await Task.Delay(2000);
                URBarBox.Children.Remove(line);
            };

            URBarBox.Children.Add(line);
        }

        private static void RemoveOldURBar()
        {
            MainWindow Window = (MainWindow)Application.Current.MainWindow;
            Window.osuReplayWindow.Children.Remove(URBarBox);
            URBarBox = new Canvas();
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

        private static void ApplyPropertiesToURBarBox(double width)
        {
            URBarBox.Name = "URBarBox";
            URBarBox.Height = 20;
            URBarBox.Width = width;
            URBarBox.Margin = new Thickness(0, 0, 0, 5);
            URBarBox.HorizontalAlignment = HorizontalAlignment.Center;
            URBarBox.VerticalAlignment = VerticalAlignment.Bottom;

            // this scale makes it so UR bar width and height will be always the same size
            // and allows use of Paths width as values of where hits should be (hopefully that will work < it did lol)
            //   tweak this ↓ number: higher == bigger bar (will add this as option later)
            double scale = 230 / URBarBox.Width;
            URBarBox.RenderTransform = new ScaleTransform(scale, scale, URBarBox.Width / 2, URBarBox.Height / 2);
        }

        private static void CreateURBars((double, SolidColorBrush)[] judgements, Path[] paths)
        {
            int i = 0;
            int j = paths.Length - 1;
            int k = 0;
            int l = judgements.Length - 1;
            double startPos = 0;
            double startPos2 = URBarBox.Width / 2;
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
            path.StrokeThickness = 10;
            path.Stroke = color;

            LineGeometry pathGeometryLine = new LineGeometry();
            pathGeometryLine.StartPoint = new Point(start, 0);
            pathGeometryLine.EndPoint = new Point(end, 0);
            pathGeometryLine.Freeze();

            path.Data = pathGeometryLine;

            return path;
        }
    }
}
