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

            //TimelineUI.Children.Add(TestNewTimeline());
        }

        public static void help()
        {
            // priority of showing stuff is miss > x50 > x100 so start with misses
            // also i should combine all of timelines into 1 array and use Path names to check for overlaps? maybe?

            for (int i = 0; i < TimelineJudgementsMiss.Count; i++)
            {
                // first index needs to be added
                if (i == 0)
                {
                    TimelineUI.Children.Add(TimelineJudgementsMiss[i]);
                    continue;
                }

                if (!IsOverlapping((Path)TimelineUI.Children[TimelineUI.Children.Count - 1], TimelineJudgementsMiss[i]))
                {
                    TimelineUI.Children.Add(TimelineJudgementsMiss[i]);
                }
            }

            for (int i = 0; i < TimelineJudgements50.Count; i++)
            {
                // first index needs to be added
                if (i == 0)
                {
                    TimelineUI.Children.Add(TimelineJudgements50[i]);
                    continue;
                }

                if (!IsOverlapping((Path)TimelineUI.Children[TimelineUI.Children.Count - 1], TimelineJudgements50[i]))
                {
                    TimelineUI.Children.Add(TimelineJudgements50[i]);
                }
            }

            for (int i = 0; i < TimelineJudgements100.Count; i++)
            {
                // first index needs to be added
                if (i == 0)
                {
                    TimelineUI.Children.Add(TimelineJudgements100[i]);
                    continue;
                }

                if (!IsOverlapping((Path)TimelineUI.Children[TimelineUI.Children.Count - 1], TimelineJudgements100[i]))
                {
                    TimelineUI.Children.Add(TimelineJudgements100[i]);
                }
            }
            
            // now that everything is spawned hide ex. x50 when there is miss EXACTLY on top
            // also need to skip first index to not have i == 0 check
            for (int i = 1; i < TimelineUI.Children.Count; i++)
            {
                if (IsOnTop((Path)TimelineUI.Children[i - 1], (Path)TimelineUI.Children[i - 1]))
                {

                }
            }
        }

        private static bool IsOnTop(Path previousLine, Path currentLine)
        {
            // these numbers are ints and not doubles, also names are judgement names
            if (Canvas.GetLeft(previousLine) == Canvas.GetLeft(currentLine)
            &&  previousLine.Name != currentLine.Name)
            {
                return true;
            }

            return false;
        }

        private static bool IsOverlapping(Path previousLine, Path currentLine)
        {
            // these numbers are ints and not doubles
            if (Canvas.GetLeft(currentLine) - Canvas.GetLeft(previousLine) < 3
            &&  previousLine.Name == currentLine.Name)
            {
                return true;
            }

            return false;
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

            //TimelineUI.Children.Add(line);

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

        private static Path TestNewTimeline()
        {
            // things i know
            // WPF is dogshit
            // the more dense is area with points the more laggy the app is, when its more evenly distributed it lags WAY less
            // like setting rng.Next(100) makes app have 1fps, setting even 500 makes app a bit laggy, 3000 is 0 problems
            // WPF is absolute dogshit
            // using big Path with 5k elements is the same as 5k elements alone
            // i hope WPF creator steps on a lego
            // freezing everything changes absolutely nothing and it pisses me off coz everyone says to do that like its some holy thing
            // WHY WPF IS SO BAD AT EVERYTHING WHY AM I EVEN USING IT AAAAAAAAA
            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = new Point(100, 0);
            
            Random rng = new Random();
            PointCollection myPointCollection = new PointCollection(5000);
            for (int i = 1; i < 1000; i++)
            {
                myPointCollection.Add(new Point(rng.Next(50), rng.Next(50)));
            }
            
            PolyLineSegment polyLineSegment = new PolyLineSegment();
            polyLineSegment.Points = myPointCollection;
            
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(polyLineSegment);
            myPathFigure.Segments = myPathSegmentCollection;

            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);
            
            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;
            myPathGeometry.Freeze();
            myPathFigureCollection.Freeze();
            myPathSegmentCollection.Freeze();
            polyLineSegment.Freeze();
            myPointCollection.Freeze();
            myPathFigure.Freeze();

            Path path = new Path();
            path.Data = myPathGeometry;
            path.Stroke = Brushes.Red;
            path.StrokeThickness = 1;
            path.StrokeLineJoin = PenLineJoin.Round;

            return path;
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

            line2.Data = myLineGeometry;

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
