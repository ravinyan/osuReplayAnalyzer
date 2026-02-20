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
        protected static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

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
            
            Rectangle middleHit = new Rectangle();
            middleHit.Fill = Brushes.HotPink;
            middleHit.Width = 1;
            middleHit.Height = 1;
            
            SetLeft(middleHit, (hitMarker.Width - 1) / 2);
            SetTop(middleHit, hitMarker.Width / 2 - 1);

            Path rightHalf = new Path();        // Invartant culture needed coz if size is odd number app will crash
            rightHalf.Data = Geometry.Parse($"M {(hitMarker.Width / 2).ToString(CultureInfo.InvariantCulture)},2 a 1 1 0 0 0 1 {hitMarker.Width - 5}");
            rightHalf.StrokeThickness = 2;
            
            Path leftHalf = new Path();
            leftHalf.Data = Geometry.Parse($"M {(hitMarker.Width / 2).ToString(CultureInfo.InvariantCulture)},2 a 1 1 0 0 1 0 {hitMarker.Width - 5}");
            leftHalf.StrokeThickness = 2;
            
            hitMarker.Children.Add(rightHalf);
            hitMarker.Children.Add(leftHalf);
            hitMarker.Children.Add(middleHit);
            
            if (hitMarker.ClickPos == "left")
            {
                leftHalf.Stroke = Brushes.HotPink;
                rightHalf.Stroke = Brushes.LightGray;
            }
            else if (hitMarker.ClickPos == "right")
            {
                rightHalf.Stroke = Brushes.HotPink;
                leftHalf.Stroke = Brushes.LightGray;
            }
            
            SetLeft(hitMarker, hitMarker.Position.X - hitMarker.Width / 2);
            SetTop(hitMarker, hitMarker.Position.Y - hitMarker.Width / 2);
            SetZIndex(hitMarker, 999);

            string showMarkers = SettingsOptions.GetConfigValue("ShowHitMarkers");
            if (showMarkers == "false")
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
    }
}
