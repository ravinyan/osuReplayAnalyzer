using ReplayAnalyzer.SettingsMenu;
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
        private static Ellipse Cursor = Window.playfieldCursor;

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
            HitMarkerData hitMarkerData = HitMarkerData.HitMarkersData[index];
            HitMarker hitMarker = new HitMarker(hitMarkerData.SpawnTime, hitMarkerData.EndTime, hitMarkerData.Position, hitMarkerData.ClickPos);
            hitMarker.Width = 20;
            hitMarker.Height = 20;
            hitMarker.Name = $"HitMarker{index}";
            
            Rectangle middleHit = new Rectangle();
            middleHit.Fill = Brushes.HotPink;
            middleHit.Width = 1;
            middleHit.Height = 1;
            
            SetLeft(middleHit, Cursor.Width / 2 - 1);
            SetTop(middleHit, Cursor.Width / 2 - 1);
            
            Path rightHalf = new Path();
            rightHalf.Data = Geometry.Parse($"M {(int)Cursor.Width / 2},2 a 1 1 0 0 0 1 20");
            rightHalf.StrokeThickness = 2;
            
            Path leftHalf = new Path();
            leftHalf.Data = Geometry.Parse($"M {(int)Cursor.Width / 2},2 a 1 1 0 0 1 0 20");
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
            
            SetLeft(hitMarker, hitMarker.Position.X - Cursor.Width / 2);
            SetTop(hitMarker, hitMarker.Position.Y - Cursor.Width / 2);
            SetZIndex(hitMarker, 999);
            
            string showMarkers = SettingsOptions.config.AppSettings.Settings["ShowHitMarkers"].Value;
            if (showMarkers == "false")
            {
                hitMarker.Visibility = Visibility.Collapsed;
            }

            return hitMarker;
        }
    }
}
