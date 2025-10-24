using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.SettingsMenu;
using System.Configuration;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ReplayAnalyzer;

namespace ReplayAnalyzer.Analyser.UIElements
{
    // i never really used inheritance outside of entity framework so i dont know what im doing and this might be terrible
    // maybe try using it in other classes to improve code if this goes well?
    public class HitMarker : Canvas
    {
        private static readonly MainWindow Window = (MainWindow)System.Windows.Application.Current.MainWindow;
        private static readonly Ellipse Cursor = Window.playfieldCursor;

        public long SpawnTime { get; }
        public long EndTime { get; }
        public Vector2 Position { get; }
        public Clicks Click { get; }

        public HitMarker(long spawnTime, long endTime, Vector2 position, Clicks click)
        {
            SpawnTime = spawnTime;
            EndTime = endTime;
            Position = position;
            Click = click;
        }

        public static HitMarker Create(ReplayFrame frame, string direction, int index)
        {
            HitMarker hitMarker = new HitMarker(frame.Time, frame.Time + 800, new Vector2(frame.X, frame.Y), frame.Click);

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

            if (direction == "left")
            {
                leftHalf.Stroke = Brushes.HotPink;
                rightHalf.Stroke = Brushes.LightGray;
            }
            else if (direction == "right")
            {
                rightHalf.Stroke = Brushes.HotPink;
                leftHalf.Stroke = Brushes.LightGray;
            }

            SetLeft(hitMarker, frame.X - Cursor.Width / 2);
            SetTop(hitMarker, frame.Y - Cursor.Width / 2);
            SetZIndex(hitMarker, 999);

            string showMarkers = SettingsOptions.config.AppSettings.Settings["ShowHitMarkers"].Value;
            if (showMarkers == "false")
            {
                hitMarker.Visibility = System.Windows.Visibility.Collapsed;
            }

            return hitMarker;
        }
    }
}
