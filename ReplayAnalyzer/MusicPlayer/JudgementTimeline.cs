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

            List<Path> allPaths = new List<Path>();
            allPaths.AddRange(TimelineJudgements100);
            allPaths.AddRange(TimelineJudgements50);
            allPaths.AddRange(TimelineJudgementsMiss);

            // WHY THIS IS ONLY 2x FASTER THAN BUBBLE SORT??? EXCUSE ME N * LOG N MY ASS??? (on 1k elements so not that much i guess)
            MergeTHIS(allPaths); //~230 000 vs ~500 000 bubble sort

            int last100Index = - 1;
            int last50Index = - 1;
            int lastMissIndex = - 1;
            for (int i = 0; i < allPaths.Count; i++)
            {
                if (allPaths[i].Name == "Ok")
                {
                    AddJudgementToTimeline(ref last100Index, i, allPaths);
                }
                else if (allPaths[i].Name == "Meh")
                {
                    AddJudgementToTimeline(ref last50Index, i, allPaths);
                }
                else if (allPaths[i].Name == "Miss")
                {
                    AddJudgementToTimeline(ref lastMissIndex, i, allPaths);
                }
            }

            // at this point everything is added to TimelineUI, check if anything is on top of each other
            HideOverlappingJudgements();
        }

        private static void AddJudgementToTimeline(ref int lastIndex, int currentIndex, List<Path> judgements)
        {
            if (lastIndex == -1)
            {
                TimelineUI.Children.Add(judgements[currentIndex]);
                lastIndex = TimelineUI.Children.Count - 1;
                return;
            }

            if (!IsOverlapping((Path)TimelineUI.Children[lastIndex], judgements[currentIndex]))
            {
                TimelineUI.Children.Add(judgements[currentIndex]);
                lastIndex = TimelineUI.Children.Count - 1;
            }
        }

        // im not gonna lie i just wrote this by hand copying on random website merge sort algorithm lol
        // one day need to understand how this logically work to write this from memory also i hate recursion
        private static void MergeTHIS(List<Path> list)
        {
            int n = list.Count;

            for (int i = 1; i <= n - 1; i = 2 * i)
            {
                for (int j = 0; j < n - 1; j += 2 * i)
                {
                    int mid = Math.Min(j + i - 1, n - 1);
                    int rightEnd = Math.Min(j + 2 * i - 1, n - 1);

                    Merge(list, i, mid, rightEnd);
                }
            }

            void Merge(List<Path> list, int left, int mid, int right)
            {
                int n1 = mid - left + 1;
                int n2 = right - mid;

                Path[] arr1 = new Path[n1];
                Path[] arr2 = new Path[n2];

                int i = 0;
                int j = 0;

                for (i = 0; i < n1; i++)
                {
                    arr1[i] = list[left + i];
                }

                for (j = 0; j < n2; j++)
                {
                    arr2[j] = list[mid + j + 1];
                }

                i = 0;
                j = 0;

                int k = left;

                while (i < n1 && j < n2)
                {
                    if ((long)arr1[i].DataContext <= (long)arr2[j].DataContext)
                    {
                        list[k] = arr1[i];
                        i++;
                    }
                    else
                    {
                        list[k] = arr2[j];
                        j++;
                    }
                    
                    k++;
                }

                while (i < n1)
                {
                    list[k] = arr1[i];
                    i++;
                    k++;
                }

                while (j < n2)
                {
                    list[k] = arr2[j];
                    j++;
                    k++;
                }
            }
        }

        public static void HideOverlappingJudgements()
        {// need to declare these here to keep references to the objects
            Path previousPath;
            Path currentPath; 
            for (int i = 1; i < TimelineUI.Children.Count; i++)
            {
                previousPath = (Path)TimelineUI.Children[i - 1];
                currentPath = (Path)TimelineUI.Children[i];
                if (IsOnTop(previousPath, currentPath))
                {
                    HideLowerPriority(previousPath, currentPath);
                }
            }
        }

        private static void HideLowerPriority(Path previousLine, Path currentLine)
        {
            if (Canvas.GetZIndex(previousLine) < Canvas.GetZIndex(currentLine))
            {
                previousLine.Visibility = Visibility.Collapsed;
            }
            else if (Canvas.GetZIndex(previousLine) > Canvas.GetZIndex(currentLine))
            {
                currentLine.Visibility = Visibility.Collapsed;
            }
        }

        private static bool IsOnTop(Path previousLine, Path currentLine)
        {
            // these numbers are ints and not doubles, also names are judgement names and cant be the same when comparing
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
            if (Canvas.GetLeft(currentLine) - Canvas.GetLeft(previousLine) < 3)
            {
                return true;
            }

            return false;
        }

        private static void SetPriority(HitObjectJudgement judgement, Path path)
        {
            switch (judgement)
            {
                case HitObjectJudgement.Ok:
                    Canvas.SetZIndex(path, 0);
                    break;
                case HitObjectJudgement.Meh:
                    Canvas.SetZIndex(path, 1);
                    break;
                case HitObjectJudgement.Miss:
                    Canvas.SetZIndex(path, 2);
                    break;
            }
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
        public static void AddJudgementToTimeline(HitObjectJudgement judgement, long hitAt)
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
        private static Path CreateJudgementLine(HitObjectJudgement judgement, long hitAt)
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
            SetPriority(judgement, line2);
            switch (judgement)
            {
                case HitObjectJudgement.Ok:
                    if (SettingsOptions.GetConfigValue("Show100OnTimeline") == "false")
                    {
                        line2.Visibility = Visibility.Collapsed;
                    }
                    TimelineJudgements100.Add(line2);
                    
                    break;
                case HitObjectJudgement.Meh:
                    if (SettingsOptions.GetConfigValue("Show50OnTimeline") == "false")
                    {
                        line2.Visibility = Visibility.Collapsed;
                    }
                    TimelineJudgements50.Add(line2);
                    
                    break;
                case HitObjectJudgement.Miss:
                    if (SettingsOptions.GetConfigValue("ShowMissOnTimeline") == "false")
                    {
                        line2.Visibility = Visibility.Collapsed;
                    }
                    TimelineJudgementsMiss.Add(line2);
                    
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
