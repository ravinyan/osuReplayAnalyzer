using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.SettingsMenu;
using System.DirectoryServices;
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
            if (MainWindow.IsReplayPreloading == true)
            {
                return null;
            }

            return CreateMarker(index);
        }

        private static FrameMarker CreateMarker(int index)
        {
            FrameMarkerData data = FrameMarkerData.FrameMarkersData[index];

            int diameter = 3;
            FrameMarker marker = new FrameMarker(data.SpawnTime, data.EndTime, data.Position);
            marker.Width = diameter;
            marker.Height = diameter;
            marker.Name = $"FrameMarker{index}";

            Ellipse dot = new Ellipse();
            dot.Width = diameter;
            dot.Height = diameter;
            dot.Fill = new SolidColorBrush(Colors.Pink);

            Canvas.SetLeft(marker, marker.Position.X - diameter / 2.0);
            Canvas.SetTop(marker, marker.Position.Y - diameter / 2.0);
            Canvas.SetZIndex(dot, 9999);

            marker.Children.Add(dot);

            string showMarkers = SettingsOptions.GetConfigValue("ShowFrameMarkers");
            if (showMarkers == "false")
            {
                marker.Visibility = Visibility.Collapsed;
            }

            return marker;
        }
    }
}
