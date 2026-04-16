using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.SettingsMenu;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.MusicPlayer
{
    public class JudgementTimeline
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static Canvas TimelineUI = new Canvas();

        public static List<Path> TimelineJudgements100 { get; private set; } = new List<Path>();
        public static List<Path> TimelineJudgements50 { get; private set; } = new List<Path>();
        public static List<Path> TimelineJudgementsMiss { get; private set; } = new List<Path>();

        public static void ResetFields()
        {
            if (ff == true)
            {
                fff = true;
            }
            ff = true;
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

            Grid? grid = Window.musicControlUI.Children[0] as Grid;
            Grid.SetColumn(TimelineUI, 4);
            grid!.Children.Add(TimelineUI);
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

        private static bool ff = false;
        private static bool fff = false;
        public static void AddJudgementToTimeline(HitObjectJudgement judgement, double hitAt)
        {
            //if (fff == true)
            //{
            //    return;
            //}
            Path line = CreateJudgementLine(judgement, hitAt);
            if (line == null)
            {
                return;
            }

            TimelineUI.Children.Add(line);

            //if (ff == true)
            //{
            //    return;
            //}
            //for (int i = 0; i < 2; i++)
            //{
            //    line = CreateJudgementLine(judgement, hitAt);
            //    TimelineUI.Children.Add(line);
            //}
        }

        // also i could write something to remove overlapping stuff with priority miss > x50 > x100 but im too lazy
        //  ^ actually dont do that unless needed i think its good enough as is
        private static Path CreateJudgementLine(HitObjectJudgement judgement, double hitAt)
        {
            double percent = (hitAt / Window.songSlider.Maximum);
            double hitPositionOnTimeline = TimelineUI.Width * percent;

            Path line2 = new Path();
            // my eyes are skill issued i dont know if i see difference or not but i feel like it helps performance
            line2.Name = judgement.ToString();
            line2.CacheMode = new BitmapCache();
            line2.DataContext = hitAt;
            line2.Width = 2;
            line2.Height = Window.musicControlUI.ActualHeight;
            line2.StrokeThickness = 2;
            line2.Opacity = 1;
            line2.Stroke = ApplyColour(judgement);

            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = new Point(0, 6);
            myLineGeometry.EndPoint = new Point(0, 42);
            myLineGeometry.Freeze();

            line2.Data = myLineGeometry;//Geometry.Parse($"M0, 6 L0, 42");

            Canvas.SetLeft(line2, Math.Round(hitPositionOnTimeline));
            switch (judgement)
            {
                case HitObjectJudgement.Ok:
                    if (SettingsOptions.GetConfigValue("Show100OnTimeline") == "false")
                    {
                        line2.Visibility = Visibility.Collapsed;
                    }

                    if (TimelineJudgements100.Count > 0 
                    &&  IsLineOverlapping(TimelineJudgements100[TimelineJudgements100.Count - 1], hitPositionOnTimeline) == true)
                    {
                        line2 = null!;
                    }
                    else
                    {
                        Canvas.SetZIndex(line2, -3);
                        TimelineJudgements100.Add(line2);
                    }

                    break;
                case HitObjectJudgement.Meh:
                    if (SettingsOptions.GetConfigValue("Show50OnTimeline") == "false")
                    {
                        line2.Visibility = Visibility.Collapsed;
                    }

                    if (TimelineJudgements50.Count > 0
                    &&  IsLineOverlapping(TimelineJudgements50[TimelineJudgements50.Count - 1], hitPositionOnTimeline) == true)
                    {
                        line2 = null!;
                    }
                    else
                    {
                        Canvas.SetZIndex(line2, -2);
                        TimelineJudgements50.Add(line2);
                    }

                    break;
                case HitObjectJudgement.Miss:
                    if (SettingsOptions.GetConfigValue("ShowMissOnTimeline") == "false")
                    {
                        line2.Visibility = Visibility.Collapsed;
                    }

                    if (TimelineJudgementsMiss.Count > 0
                    &&  IsLineOverlapping(TimelineJudgementsMiss[TimelineJudgementsMiss.Count - 1], hitPositionOnTimeline) == true)
                    {
                        line2 = null!;
                    }
                    else
                    {
                        Canvas.SetZIndex(line2, -1);
                        TimelineJudgementsMiss.Add(line2);
                    }

                    break;
                default:
                    throw new Exception("Wrong judgement value");
            }

            return line2;
        }

        private static bool IsLineOverlapping(Path previousPath, double currentPathPosition)
        {
            if (Canvas.GetLeft(previousPath) >= Math.Round(currentPathPosition - 1)
            &&  Canvas.GetLeft(previousPath) <= Math.Round(currentPathPosition + 1))
            {
                return true;
            }

            return false;
        }

        private static SolidColorBrush ApplyColour(HitObjectJudgement judgement)
        {
            switch (judgement)
            {
                case HitObjectJudgement.Ok:   // green
                    return new SolidColorBrush(Color.FromRgb(11, 145, 9));
                case HitObjectJudgement.Meh:  // orange-ish?
                    return new SolidColorBrush(Color.FromRgb(242, 146, 2));
                case HitObjectJudgement.Miss: // red
                    return new SolidColorBrush(Color.FromRgb(245, 42, 42));
                default:
                    throw new Exception("Wrong judgement value");
            }
        }
    }
}
