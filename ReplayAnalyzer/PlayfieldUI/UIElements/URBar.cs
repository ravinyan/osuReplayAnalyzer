using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    public class URBar
    {
        // 6 Path lines where 3 are on one side and other 3 mirrored?
        // https://osu.ppy.sh/wiki/en/Beatmap/Overall_difficulty
        // lowest UR is -4.44 at 266.67ms and highest is 11.11 at 66.67ms
        // i dont know what im doing but im doing it anyway!
        private double Scale = 1;

        public static Canvas Create()
        {
            Canvas URBarBox = new Canvas();
            URBarBox.Name = "URBarBox";
            //URBarBox.Width = 470;
            URBarBox.Height = 20;
            URBarBox.Margin = new Thickness(0, 0, 0, 5);
            //URBarBox.Background = new SolidColorBrush(Colors.White);
            URBarBox.HorizontalAlignment = HorizontalAlignment.Center;
            URBarBox.VerticalAlignment = VerticalAlignment.Bottom;

            Line path1 = new Line();
            path1.Fill = new SolidColorBrush(Colors.Blue);
            path1.Width = 20;
            path1.Height = 20;
            path1.Stroke = new SolidColorBrush(Colors.Blue);
            path1.StrokeThickness = 5;
            path1.X1 = 0;
            path1.X2 = 20;
            path1.Y1 = 0;
            path1.Y2 = 0;

            Line path2 = new Line();
            path2.Fill = new SolidColorBrush(Colors.Green);
            path2.Width = 60;
            path2.Height = 20;
            path2.Stroke = new SolidColorBrush(Colors.Green);
            path2.StrokeThickness = 5;
            path2.X1 = 0;
            path2.X2 = 20;
            path2.Y1 = 5;
            path2.Y2 = 5;

            Line path3 = new Line();
            path3.Fill = new SolidColorBrush(Colors.Yellow);
            path3.Width = 100;
            path3.Height = 20;
            path3.Stroke = new SolidColorBrush(Colors.Yellow);
            path3.StrokeThickness = 5;
            path3.X1 = 0;
            path3.X2 = 20;
            path3.Y1 = 10;
            path3.Y2 = 10;

            Path path = new Path();
            path.Width = 100;

            OsuMath math = new OsuMath();
            double h300 = math.GetOverallDifficultyHitWindow300(MainWindow.map.Difficulty!.OverallDifficulty);
            double h100 = math.GetOverallDifficultyHitWindow100(MainWindow.map.Difficulty.OverallDifficulty);
            double h50 = math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);

            // if different UR bar sized will feel weird then apply scale here + scale to point where judgement was hit
            URBarBox.Width = (h300 * 2) + (h100 * 2) + (h50 * 2);

            (double, SolidColorBrush) bar300 = (h300, new SolidColorBrush(Colors.Blue));
            (double, SolidColorBrush) bar100 = (h100, new SolidColorBrush(Colors.Green));
            (double, SolidColorBrush) bar50 = (h50, new SolidColorBrush(Colors.Yellow));

            (double, SolidColorBrush)[] judgements =
            {
                 bar50, bar100, bar300
            };

            Path[] paths = new Path[6];

            double startPos = 0;
            int max = 2;
            for (int i = 0; i <= max; i++)
            {
                (double endPos, SolidColorBrush colour) judge = judgements[i];
                paths[i] = CreateBar(startPos, startPos + judge.endPos, judge.colour);

                startPos += judge.Item1;
            }

            int j = 2;
            for (int i = paths.Length - 1; j >= 0; i--)
            {
                (double endPos, SolidColorBrush colour) judge = judgements[j];
                paths[i] = CreateBar(startPos, startPos + judge.endPos, judge.colour);

                startPos += judge.Item1;
                j--;
            }

            //PathGeometry myPathGeometry = CreateLine(new Point(100, 0));

            //path.Data = myPathGeometry;
            path.StrokeThickness = 5;
            path.Stroke = new SolidColorBrush(Colors.Red);

            Point point = new Point(0, 0);
            Point tangent = new Point(0, 0);
            double progress = 0.5;

            //myPathGeometry.GetPointAtFractionLength(progress, out point, out tangent);

            //URBarBox.Children.Add(path1);
            //URBarBox.Children.Add(path2);
            //URBarBox.Children.Add(path3);
            foreach (var p in paths)
            {
                if (p == null)
                {
                    continue;
                }
                URBarBox.Children.Add(p);
            }

            URBarBox.RenderTransform = new ScaleTransform(2, 2);

            return URBarBox;
        }

        private static Path CreateBar(double start, double end, SolidColorBrush color)
        {
            Path path = new Path();
            path.StrokeThickness = 5;
            path.Stroke = color;
            path.Data = CreateLine(start, end);

            return path;
        }

        private static PathGeometry CreateLine(double start, double end)
        {
            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = new Point(start, 0);

            PointCollection myPointCollection = new PointCollection(2);
            myPointCollection.Add(new Point(end, 0));

            PolyLineSegment polyLineSegment = new PolyLineSegment();
            polyLineSegment.Points = myPointCollection;

            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(polyLineSegment);
            myPathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);

            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;

            return myPathGeometry;
        }
    }
}
