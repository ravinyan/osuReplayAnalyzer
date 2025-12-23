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
        public static List<Line> TimelineJudgements100 = new List<Line>();
        public static List<Line> TimelineJudgements50 = new List<Line>();
        public static List<Line> TimelineJudgementsMiss = new List<Line>();

        public static void ResetFields()
        {
            TimelineUI = new Canvas();
            TimelineJudgements100 = new List<Line>();
            TimelineJudgements50 = new List<Line>();
            TimelineJudgementsMiss = new List<Line>();
        }

        public static void Initialize()
        {
            CreateTimelineUI();

            // i fucking hate WPF it took 2min to write SetColumnChildren function that would do this with
            // grid.SetColumnChildren(TimelineUI, 4); instead of using this random forgettable ass Grid.SetColumn()...
            Grid? grid = Window.musicControlUI.Children[1] as Grid;
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
            foreach (Line line in TimelineJudgements100)
            {
                percent = ((double)line.DataContext / Window.songSlider.Maximum);
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }

            foreach (Line line in TimelineJudgements50)
            {
                percent = ((double)line.DataContext / Window.songSlider.Maximum);
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }

            foreach (Line line in TimelineJudgementsMiss)
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
            TimelineUI.Children.Add(CreateJudgementLine(colour, hitAt, name));
        }

        private static Line CreateJudgementLine(Brush colour, double hitAt, string name)
        {
            double percent = (hitAt / Window.songSlider.Maximum);
            double hitPositionOnTimeline = TimelineUI.Width * percent;

            // might change it to be at the bottom of slider bar but idk will see when i finish this
            Line line = new Line();
            line.DataContext = hitAt;
            line.Width = 3;
            line.Height = Window.musicControlUI.ActualHeight;
            line.StrokeThickness = 2;
            line.Opacity = 0.5;
            line.StrokeStartLineCap = PenLineCap.Round;
            line.StrokeEndLineCap = PenLineCap.Round;
            line.Stroke = colour;
            line.X1 = 2;
            line.X2 = 2;
            // height is 50 and this splits height evenly for line to be in the middle of song slider bar
            line.Y1 = 6;
            line.Y2 = 44;
            

            Canvas.SetLeft(line, hitPositionOnTimeline);
            switch (name)
            {
                case "100":
                    if (SettingsOptions.GetConfigValue("Show100OnTimeline") == "false")
                    {
                        line.Visibility = Visibility.Collapsed;
                    }

                    TimelineJudgements100.Add(line);
                    break;
                case "50":
                    if (SettingsOptions.GetConfigValue("Show50OnTimeline") == "false")
                    {
                        line.Visibility = Visibility.Collapsed;
                    }

                    TimelineJudgements50.Add(line);
                    break;
                case "miss":
                    if (SettingsOptions.GetConfigValue("ShowMissOnTimeline") == "false")
                    {
                        line.Visibility = Visibility.Collapsed;
                    }

                    TimelineJudgementsMiss.Add(line);
                    break;
                default:
                    throw new Exception("Wrong judgement timeline value");
            }

            return line;
        }
    }
}
