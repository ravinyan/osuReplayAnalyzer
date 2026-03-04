using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
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
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreatePath(index);
            }

            return null!;
        }

        private static CursorPath CreatePath(int index)
        {
            if (index >= CursorPathData.CursorPathsData.Count)
            {
                index--;
            }
            if (index - 1 < 0)
            {
                index++;
            }

            //CursorPathData data = CursorPathData.CursorPathsData[index];

            ReplayFrame lineStart = MainWindow.replay.FramesDict[index - 1];
            ReplayFrame lineEnd = MainWindow.replay.FramesDict[index];
            
            var data = new CursorPathData(lineStart.Time, lineStart.Time + HitMarkerData.ALIVE_TIME, new Vector2(lineStart.X, lineStart.Y), new Vector2(lineEnd.X, lineEnd.Y));

            CursorPath path = new CursorPath(data.SpawnTime, data.EndTime, data.LineStart, data.LineEnd);

            // relative start/end from 0,0 coords of CursorPath path
            double replativeLineStart = path.LineEnd.X - path.LineStart.X;
            double relativeLineEnd = path.LineEnd.Y - path.LineStart.Y;

            // line start/end are basically offsets from 0,0 coords which give the width and height from that
            // and + 1 is coz otherwise lines are cut off a bit or just dont show at all
            double width = Math.Abs(replativeLineStart) + 1;
            double height = Math.Abs(relativeLineEnd) + 1;

            path.Width = width;
            path.Height = height;
            path.Name = $"CursorPath{index}";

            Canvas.SetLeft(path, path.LineStart.X);
            Canvas.SetTop(path, path.LineStart.Y);

            path.Children.Add(CreatePathLine(width, height, replativeLineStart, relativeLineEnd));

            if (SettingsOptions.GetConfigValue("ShowCursorPath") == "false")
            {
                path.Visibility = Visibility.Collapsed;
            }

            return path;
        }

        private static Path CreatePathLine(double width, double height, double lineStart, double lineEnd)
        {
            Path line = new Path();
            line.Width = width;
            line.Height = height;
            line.StrokeThickness = 1;
            line.Stroke = new SolidColorBrush(Colors.Pink);

            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = new Point(0, 0);
            myLineGeometry.EndPoint = new Point(lineStart, lineEnd);
            myLineGeometry.Freeze();

            line.Data = myLineGeometry;

            Canvas.SetZIndex(line, 9999);

            return line;
        }
    }
}
