using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    // i dont know what im doing but im doing it anyway!
    public class URBar
    {
        private static Canvas URBarContainer = new Canvas();
        private static Canvas UrBar = new Canvas();

        private static int URBarBaseWidth 
        { 
            get 
            { 
                return 200; 
            } 
        }

        // reminder for future me: trying to change current stuff = 2h of not working, making everything anew = 20min of success
        // later i could add customizability like in osu lazer coz that is pretty easy
        public static Canvas Create()
        {// need to refresh UR bar coz of OD changing in beatmaps changing how bar looks/behaves and how judgements are shown
            if (URBarContainer.Name != "")
            {
                RemoveOldURBar();
            }

            OsuMath math = new OsuMath();
            double h3002 = math.GetJudgement300HitWindow();
            double h1002 = math.GetJudgement100HitWindow();
            double h502 = math.GetJudgement50HitWindow();
            
            double h300 = h3002;
            double h100 = h1002 - h3002;
            double h50 = h502 - h1002;

            double URBarWidth = (h300 * 2) + (h100 * 2) + (h50 * 2);
            double scale = URBarBaseWidth / URBarWidth;

            ApplyPropertiesToURBarContainer();
            ApplyPropertiesToURBar(URBarWidth, scale);

            (double, SolidColorBrush)[] judgements =
            {
                // i have no clue what colours to give here this might be good enough? tried to make osu lazer colours
                (h50, ApplyColour(HitObjectJudgement.Meh)),
                (h100, ApplyColour(HitObjectJudgement.Ok)),
                (h300, ApplyColour(HitObjectJudgement.Max)),
            };

            Path[] paths = new Path[6];
            CreateURBars(judgements, paths);
            foreach (Path p in paths)
            {
                UrBar.Children.Add(p);
            }

            URBarContainer.Children.Add(CreateLateIconPath());
            URBarContainer.Children.Add(UrBar);
            URBarContainer.Children.Add(CreateEarlyIconPath(URBarWidth, scale));
            
            return URBarContainer;
        }

        public static void ShowHit(HitObjectJudgement judgement, double timing)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                return;
            }

            Line line = CreateURHitLine(ApplyColour(judgement), 3);

            Canvas.SetTop(line, 10);
            Canvas.SetLeft(line, timing + UrBar.Width / 2);
            Canvas.SetZIndex(line, 2);

            line.Loaded += async delegate (object sender, RoutedEventArgs e)
            {
                await Task.Delay(2000);
                UrBar.Children.Remove(line);
            };

            UrBar.Children.Add(line);
        }

        private static void RemoveOldURBar()
        {
            MainWindow Window = (MainWindow)Application.Current.MainWindow;
            Window.osuReplayWindow.Children.Remove(URBarContainer);
            UrBar = new Canvas();
            URBarContainer = new Canvas();
        }

        private static Line CreateURHitLine(SolidColorBrush color, double lineWidth)
        {
            Line line = new Line();
            line.Width = lineWidth;
            line.Height = 15;
            line.StrokeThickness = lineWidth;
            line.Opacity = 0.5;
            line.Stroke = color;
            line.X1 = 0;
            line.X2 = 0; 
            line.Y1 = -20;
            line.Y2 = 0;

            return line;
        }

        private static void ApplyPropertiesToURBar(double width, double scale)
        {
            UrBar.Name = "URBar";
            UrBar.Height = 5;
            UrBar.Width = width;
            UrBar.Margin = new Thickness(0, 0, 0, -10);
            UrBar.LayoutTransform = new ScaleTransform(scale, 1, (width * scale) / 2, 0);
        }

        private static void ApplyPropertiesToURBarContainer()
        {
            URBarContainer.Name = "URBarContainer";
            URBarContainer.Height = 10;
            URBarContainer.Width = URBarBaseWidth;
            URBarContainer.Margin = new Thickness(0, 0, 0, 10);
            URBarContainer.VerticalAlignment = VerticalAlignment.Bottom;

            // for sharp edges so UR bar looks connected and one plus icon line doesnt look off center with some specific sizes
            RenderOptions.SetEdgeMode(URBarContainer, EdgeMode.Aliased);
        }

        private static void CreateURBars((double, SolidColorBrush)[] judgements, Path[] paths)
        {
            int i = 0;
            int j = paths.Length - 1;
            int k = 0;
            int l = judgements.Length - 1;
            double startPos = 0;
            double startPos2 = UrBar.Width / 2;
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
            path.StrokeThickness = 6;
            path.Stroke = color;

            LineGeometry pathGeometryLine = new LineGeometry();
            pathGeometryLine.StartPoint = new Point(start, 0);
            pathGeometryLine.EndPoint = new Point(end, 0);
            pathGeometryLine.Freeze();

            path.Data = pathGeometryLine;

            return path;
        }

        private static Path CreateEarlyIconPath(double URBarWidth, double scale)
        {
            Path early = new Path();
            early.StrokeThickness = 2;
            early.Stroke = Brushes.White;
            early.Width = 15;
            early.Height = 15;
            early.Data = Geometry.Parse($"M 9,0 L 9,10 M 4,5 L 14,5");

            // adjusted by hand to be centered to the center of UrBar
            Canvas.SetTop(early, -2.5);
            Canvas.SetLeft(early, URBarWidth * scale);

            return early;
        }

        private static Path CreateLateIconPath()
        {
            Path late = new Path();
            late.StrokeThickness = 2;
            late.Stroke = Brushes.White;
            late.Width = 15;
            late.Height = 15;
            late.Data = Geometry.Parse("M -4,5 L 6,5");

            // adjusted by hand to be centered to the center of UrBar
            Canvas.SetTop(late, -2.5);
            Canvas.SetLeft(late, -10);

            return late;
        }

        private static SolidColorBrush ApplyColour(HitObjectJudgement judgement)
        {
            switch (judgement)
            {
                case HitObjectJudgement.Max: // blue
                    return new SolidColorBrush(Color.FromRgb(138, 216, 255));
                case HitObjectJudgement.Ok:  // green
                    return new SolidColorBrush(Color.FromRgb(176, 192, 25));
                case HitObjectJudgement.Meh: // orange/yellow-ish(?)
                    return new SolidColorBrush(Color.FromRgb(255, 217, 61));
                default:
                    throw new Exception("Wrong colour property");
            }
        }
    }
}
