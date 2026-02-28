using ReplayAnalyzer.SettingsMenu;
using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.AnalyzerTools.HitMarkers
{
    public class HitMarker : Canvas
    {
        public long SpawnTime { get; }
        public long EndTime { get; }
        public Vector2 Position { get; }
        public string ClickPos { get; }

        public HitMarker(long spawnTime, long endTime, Vector2 position, string click)
        {
            SpawnTime = spawnTime;
            EndTime = endTime;
            Position = position;
            ClickPos = click;
        }

        public static HitMarker Create(int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateMarker(index);
            }

            return CreatePreload(index);
        }

        private static HitMarker CreateMarker(int index)
        {
            HitMarkerData hitMarkerData = HitMarkerData.HitMarkersData[index];
            HitMarker hitMarker = new HitMarker(hitMarkerData.SpawnTime, hitMarkerData.EndTime, hitMarkerData.Position, hitMarkerData.ClickPos);
            hitMarker.Width = 26;
            hitMarker.Height = 26;
            hitMarker.Name = $"HitMarker{index}";

            Canvas.SetLeft(hitMarker, hitMarker.Position.X - hitMarker.Width / 2);
            Canvas.SetTop(hitMarker, hitMarker.Position.Y - hitMarker.Width / 2);
            Canvas.SetZIndex(hitMarker, 9999);

            hitMarker.Children.Add(CreateHitMarkerRightSide(hitMarker.Width, hitMarker.ClickPos));
            hitMarker.Children.Add(CreateHitMarkerLeftSide(hitMarker.Width, hitMarker.ClickPos));
            hitMarker.Children.Add(CreateMiddleHitDot(hitMarker.Width));

            if (SettingsOptions.GetConfigValue("ShowHitMarkers") == "false")
            {
                hitMarker.Visibility = Visibility.Collapsed;
            }

            return hitMarker;
        }

        private static HitMarker CreatePreload(int index)
        {
            HitMarkerData hitMarkerData = HitMarkerData.HitMarkersData[index];
            HitMarker hitMarker = new HitMarker(hitMarkerData.SpawnTime, hitMarkerData.EndTime, hitMarkerData.Position, hitMarkerData.ClickPos);

            return hitMarker;
        }

        private static Rectangle CreateMiddleHitDot(double width)
        {
            Rectangle middleHit = new Rectangle();
            middleHit.Fill = Brushes.HotPink;
            middleHit.Width = 1;
            middleHit.Height = 1;

            Canvas.SetLeft(middleHit, (width - 1) / 2);
            Canvas.SetTop(middleHit, width / 2 - 1);

            return middleHit;
        }

        private static Path CreateHitMarkerRightSide(double width, string clickPos)
        {
            Path rightHalf = new Path();
            rightHalf.Data = Geometry.Parse($"M {(width / 2).ToString(CultureInfo.InvariantCulture)},2 a 1 1 0 0 0 1 {width - 5}");
            rightHalf.StrokeThickness = 2;

            if (clickPos == "right")
            {
                rightHalf.Stroke = Brushes.HotPink;
            }
            else
            {
                rightHalf.Stroke = Brushes.LightGray;
            }

            return rightHalf;
        }

        private static Path CreateHitMarkerLeftSide(double width, string clickPos)
        {
            Path leftHalf = new Path();
            leftHalf.Data = Geometry.Parse($"M {(width / 2).ToString(CultureInfo.InvariantCulture)},2 a 1 1 0 0 1 0 {width - 5}");
            leftHalf.StrokeThickness = 2;

            if (clickPos == "left")
            {
                leftHalf.Stroke = Brushes.HotPink;
            }
            else
            {
                leftHalf.Stroke = Brushes.LightGray;
            }

            return leftHalf;
        }
    }
}
