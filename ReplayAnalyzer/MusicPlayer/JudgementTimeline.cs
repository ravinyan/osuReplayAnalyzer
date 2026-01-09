using ReplayAnalyzer.SettingsMenu;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;

namespace ReplayAnalyzer.MusicPlayer
{
    public class JudgementTimeline
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static Canvas TimelineUI = new Canvas();
        public static List<Path> TimelineJudgements100 = new List<Path>();
        public static List<Path> TimelineJudgements50 = new List<Path>();
        public static List<Path> TimelineJudgementsMiss = new List<Path>();

        public static void ResetFields()
        {
            Grid? grid = Window.musicControlUI.Children[0] as Grid;
            grid.Children.Remove(TimelineUI);
            TimelineUI = new Canvas();

            TimelineJudgements100 = new List<Path>();
            TimelineJudgements50 = new List<Path>();
            TimelineJudgementsMiss = new List<Path>();
        }

        public static void Initialize()
        {
            CreateTimelineUI();

            // i fucking hate WPF it took 2min to write SetColumnChildren function that would do this with
            // grid.SetColumnChildren(TimelineUI, 4); instead of using this random forgettable ass Grid.SetColumn()...
            Grid? grid = Window.musicControlUI.Children[0] as Grid;
            Grid.SetColumn(TimelineUI, 4);
            grid.Children.Add(TimelineUI);
        }

        public static void ChangeTimelineSizeOnResize()
        {
            if (Window.songSlider.RenderSize.Width - 20 > 0)
            {
                TimelineUI.Width = Window.songSlider.RenderSize.Width - 20;
            }

            double percent;
            double hitPositionOnTimeline;
            foreach (var line in TimelineJudgements100)
            {
                percent = ((double)line.DataContext / Window.songSlider.Maximum);
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }

            foreach (var line in TimelineJudgements50)
            {
                percent = ((double)line.DataContext / Window.songSlider.Maximum);
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }

            foreach (var line in TimelineJudgementsMiss)
            {
                percent = ((double)line.DataContext / Window.songSlider.Maximum);
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }
        }

        private static void CreateTimelineUI()
        {
            TimelineUI.Width = Window.songSlider.RenderSize.Width - 20;
            TimelineUI.Height = Window.musicControlUI.RenderSize.Height;
            TimelineUI.Background = new SolidColorBrush(Colors.Transparent);
            Canvas.SetZIndex(TimelineUI, -1);
        }

        public static void AddJudgementToTimeline(Brush colour, double hitAt, string name)
        {
            var a = CreateJudgementLine(colour, hitAt, name);
            if (a == null)
            {
                return;
            }

            TimelineUI.Children.Add(a);
            CreateJudgementLine(colour, hitAt, name);
        }

        // not sure if Path is more efficient than Line as of now but Path has more possibilities for performance improvements
        // try changing to PathGeometry for the Freeze() property later
        private static Path CreateJudgementLine(Brush colour, double hitAt, string name)
        {
            double percent = (hitAt / Window.songSlider.Maximum);
            double hitPositionOnTimeline = TimelineUI.Width * percent;

            //Line line = new Line();
            //line.CacheMode = new BitmapCache();
            //line.DataContext = hitAt;
            //line.Width = 3;
            //line.Height = Window.musicControlUI.ActualHeight;
            //line.StrokeThickness = 2;
            //line.Opacity = 0.5;
            //line.Stroke = colour;
            //line.X1 = 2;
            //line.X2 = 2;
            //// height is 50 and this splits height evenly for line to be in the middle of song slider bar
            //line.Y1 = 6;
            //line.Y2 = 44;

            Path line2 = new Path();
            // my eyes are skill issued i dont know if i see difference or not but i feel like it helps performance
            line2.Name = $"n{name}";
            line2.CacheMode = new BitmapCache();
            line2.DataContext = hitAt;
            line2.Width = 2;
            line2.Height = Window.musicControlUI.ActualHeight;
            line2.StrokeThickness = 2;
            line2.Opacity = 1;
            line2.Stroke = colour;
            line2.Data = Geometry.Parse($"M0, 6 L0, 42");

            // scuffed just so it kinda works head too hurt
            if (TimelineUI.Children.Count > 1)
            {
                int prevIndex = TimelineUI.Children.Count - 1;
                Path? p = TimelineUI.Children[prevIndex] as Path;

                if (p.Name != "nmiss" && line2.Name != "nmiss" && Canvas.GetLeft(p) == Math.Round(hitPositionOnTimeline))
                {
                    // remove overlapping lines for better performance

                    // do specific check to intelligently remove lines in this priority
                    // if everything is visible then highest to lowest: miss > 50 > 100, if all 3 overlap add only miss
                    // else only remove overlaps of the same category (name) if only 1 is visible

                    return null!;  
                }
            }

            Canvas.SetLeft(line2, Math.Round(hitPositionOnTimeline));
            switch (name)
            {
                case "100":
                    if (SettingsOptions.GetConfigValue("Show100OnTimeline") == "false")
                    {
                        line2.Visibility = Visibility.Collapsed;
                    }

                    TimelineJudgements100.Add(line2);
                    break;
                case "50":
                    if (SettingsOptions.GetConfigValue("Show50OnTimeline") == "false")
                    {
                        line2.Visibility = Visibility.Collapsed;
                    }
            
                    TimelineJudgements50.Add(line2);
                    break;
                case "miss":
                    if (SettingsOptions.GetConfigValue("ShowMissOnTimeline") == "false")
                    {
                        line2.Visibility = Visibility.Collapsed;
                    }
            
                    TimelineJudgementsMiss.Add(line2);
                    break;
                default:
                    throw new Exception("Wrong judgement timeline value");
            }

            return line2;
        }
    }

    // https://stackoverflow.com/questions/36126505/display-a-drawingvisual-on-canvas
    public class VisualHost : UIElement
    {
        public Visual? Visual { get; set; }


        protected override int VisualChildrenCount
        {
            get
            {
                return Visual != null ? 1 : 0;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Visual;
        }
    }
}
