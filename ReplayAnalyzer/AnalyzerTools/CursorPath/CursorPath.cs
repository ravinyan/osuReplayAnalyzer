using ReplayAnalyzer.SettingsMenu;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.AnalyzerTools.CursorPath
{
    public class CursorPath : Canvas
    {
        public long SpawnTime { get; }
        public long EndTime { get; }

        public Vector2 LineStart { get; }
        public Vector2 LineEnd { get; }

        public CursorPath(long spawnTime, long endTime, Vector2 lineStarts, Vector2 lineEnd)
        {
            SpawnTime = spawnTime;
            EndTime = endTime;

            LineStart = lineStarts;
            LineEnd = lineEnd;
        }

        public static CursorPath Create(int index)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                return null;
            }

            return CreatePath(index);
        }

        private static CursorPath CreatePath(int index)
        {
            if (index >= CursorPathData.CursorPathsData.Count)
            {
                index--;
            }    

            CursorPathData data = CursorPathData.CursorPathsData[index];
            CursorPath path = new CursorPath(data.SpawnTime, data.EndTime, data.LineStart, data.LineEnd);

            double width = path.LineEnd.X - path.LineStart.X;
            double height = path.LineEnd.Y - path.LineStart.Y;

            path.Width = Math.Abs(width) + 1;
            path.Height = Math.Abs(height) + 1;
            path.Name = $"CursorPath{index}";

            Path line = new Path();
            line.Width = Math.Abs(width) + 1;
            line.Height = Math.Abs(height) + 1;
            line.StrokeThickness = 1;
            line.Opacity = 1;
            line.Stroke = new SolidColorBrush(Colors.Pink);

            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = new Point(0, 0);
            myLineGeometry.EndPoint = new Point(width, height);
            myLineGeometry.Freeze();

            line.Data = myLineGeometry;

            Canvas.SetLeft(path, path.LineStart.X);
            Canvas.SetTop(path, path.LineStart.Y);
            Canvas.SetZIndex(line, 9999);

            path.Children.Add(line);

            string showPaths = SettingsOptions.GetConfigValue("ShowCursorPath");
            if (showPaths == "false")
            {
                path.Visibility = Visibility.Collapsed;
            }

            return path;
        }
    }
}
