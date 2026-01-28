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
        public List<Vector2> LineCoordinates { get; }
        public Vector2 Position { get; }

        public CursorPath(long spawnTime, long endTime, Vector2 position, List<Vector2> lineCoordinates)
        {
            SpawnTime = spawnTime;
            EndTime = endTime;
            Position = position;
            LineCoordinates = lineCoordinates;
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
            CursorPath path = new CursorPath(data.SpawnTime, data.EndTime, data.Position, data.LineCoordinates);

            double width = data.LineCoordinates[1].X - data.LineCoordinates[0].X;
            double height = data.LineCoordinates[1].Y - data.LineCoordinates[0].Y;

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

            Canvas.SetLeft(path, path.LineCoordinates[0].X);
            Canvas.SetTop(path, path.LineCoordinates[0].Y);
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
