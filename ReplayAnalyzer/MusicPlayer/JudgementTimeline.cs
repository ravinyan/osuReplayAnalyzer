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
            TimelineUI.Width = Window.songSlider.ActualWidth;
            // change position of all hit judgement lines too
        }

        private static void CreateTimelineUI()
        {
            TimelineUI.Width = Window.songSlider.ActualWidth;
            TimelineUI.Height = Window.musicControlUI.ActualHeight;
            TimelineUI.Background = new SolidColorBrush(Colors.Transparent);
            //TimelineUI.Margin = Window.songSlider.Margin;
            Canvas.SetZIndex(TimelineUI, -1);
        }

        public static void AddJudgementToTimeline(Brush colour, double hitAt)
        {
            double percent = (hitAt / Window.songSlider.Maximum);
            double hitPositionOnTimeline = TimelineUI.Width * percent;

            TimelineUI.Children.Add(CreateJudgementLine(colour, hitPositionOnTimeline));
        }

        private static Line CreateJudgementLine(Brush colour, double hitPos)
        {
            // might change it to be at the bottom of slider bar but idk will see when i finish this
            Line line = new Line();
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

            Canvas.SetLeft(line, hitPos);

            return line;
        }
    }
}
