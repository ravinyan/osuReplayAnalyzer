using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.SettingsMenu;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.AnalyzerTools.FrameMarkers
{
    public class FrameMarker : Canvas
    {
        public long SpawnTime { get; }
        public long EndTime { get; }
        public Vector2 Position { get; }

        public FrameMarker(long spawnTime, long endTime, Vector2 position)
        {
            SpawnTime = spawnTime;
            EndTime = endTime;
            Position = position;
        }

        public static FrameMarker Create(int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateMarker(index);
            }

            return null!;
        }

        private static FrameMarker CreateMarker(int index)
        {
            //FrameMarkerData data = FrameMarkerData.FrameMarkersData[index];

            var d = MainWindow.replay.FramesDict[index];
            var data = new FrameMarkerData(d.Time, d.Time + HitMarkerData.ALIVE_TIME, new Vector2(d.X, d.Y));
            
            FrameMarker marker = new FrameMarker(data.SpawnTime, data.EndTime, data.Position);

            int dotDiameter = 3;
            marker.Width = dotDiameter;
            marker.Height = dotDiameter;
            marker.Name = $"FrameMarker{index}";

            Canvas.SetLeft(marker, marker.Position.X - dotDiameter / 2.0);
            Canvas.SetTop(marker, marker.Position.Y - dotDiameter / 2.0);

            marker.Children.Add(CreateFrameMarkerDot(dotDiameter));

            if (SettingsOptions.GetConfigValue("ShowFrameMarkers") == "false")
            {
                marker.Visibility = Visibility.Collapsed;
            }

            return marker;
        }

        private static Ellipse CreateFrameMarkerDot(int diameter)
        {
            Ellipse dot = new Ellipse();
            dot.Width = diameter;
            dot.Height = diameter;
            dot.Fill = new SolidColorBrush(Colors.Pink);

            Canvas.SetZIndex(dot, 9999);

            return dot;
        }
    }
}
