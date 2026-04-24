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

        // i think making them private is better than public and just have everything inside here
        public static List<Path> TimelineJudgements100 { get; private set; } = new List<Path>();
        public static List<Path> TimelineJudgements50 { get; private set; } = new List<Path>();
        public static List<Path> TimelineJudgementsMiss { get; private set; } = new List<Path>();

        public static void ResetTimeline()
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

            Grid? grid = Window.musicControlUI.Children[0] as Grid;
            Grid.SetColumn(TimelineUI, 4);
            grid!.Children.Add(TimelineUI);

            //TimelineUI.Children.Add(TestNewTimeline());
        }

        public static Path CreateJudgementLine(HitObjectJudgement judgement, long hitAt)
        {
            double percent = (hitAt / Window.songSlider.Maximum);
            double hitPositionOnTimeline = TimelineUI.Width * percent;

            Path judgementLine = new Path();
            // my eyes are skill issued i dont know if i see difference or not but i feel like it helps performance
            judgementLine.Name = judgement.ToString();
            judgementLine.CacheMode = new BitmapCache();
            judgementLine.DataContext = hitAt;
            judgementLine.Width = 2;
            judgementLine.Height = Window.musicControlUI.ActualHeight;
            judgementLine.StrokeThickness = 2;
            judgementLine.Opacity = 0.5;
            judgementLine.Stroke = ApplyColour(judgement);

            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = new Point(0, 6);
            myLineGeometry.EndPoint = new Point(0, 42);
            myLineGeometry.Freeze();

            judgementLine.Data = myLineGeometry;

            Canvas.SetLeft(judgementLine, Math.Round(hitPositionOnTimeline));
            SetPriority(judgement, judgementLine);
            switch (judgement)
            {
                case HitObjectJudgement.Ok:
                    if (SettingsOptions.GetConfigValue("Show100OnTimeline") == "false")
                    {
                        judgementLine.Visibility = Visibility.Collapsed;
                    }
                    TimelineJudgements100.Add(judgementLine);

                    break;
                case HitObjectJudgement.Meh:
                    if (SettingsOptions.GetConfigValue("Show50OnTimeline") == "false")
                    {
                        judgementLine.Visibility = Visibility.Collapsed;
                    }
                    TimelineJudgements50.Add(judgementLine);

                    break;
                case HitObjectJudgement.Miss:
                    if (SettingsOptions.GetConfigValue("ShowMissOnTimeline") == "false")
                    {
                        judgementLine.Visibility = Visibility.Collapsed;
                    }
                    TimelineJudgementsMiss.Add(judgementLine);

                    break;
                default:
                    throw new Exception("Wrong judgement value");
            }

            return judgementLine;
        }

        public static void PopulateJudgementTimeline()
        {
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

            HideVisibleOverlappingJudgements(HitObjectJudgement.Ok);
            HideVisibleOverlappingJudgements(HitObjectJudgement.Meh);
            HideVisibleOverlappingJudgements(HitObjectJudgement.Miss);
            //HideOverlappingJudgements();
        }

        // apparently x50 judgements dont come back when misses are turned off... pain
        // nvm overall some judgements dont come back as visible... what a pain why must i be dumb
        public static void HideJudgements(List<Path> timeline, HitObjectJudgement judgement)
        {
            ShowBackVisibleOverlappingJudgements(judgement);
            foreach (Path line in timeline)
            {
                line.Visibility = Visibility.Collapsed;
            }
            HideVisibleOverlappingJudgements(judgement);
        }

        public static void ShowJudgements(List<Path> timeline, HitObjectJudgement judgement)
        {
            foreach (Path line in timeline)
            {
                line.Visibility = Visibility.Visible;
            }
            HideVisibleOverlappingJudgements(judgement);

            //int c = 1;
            //for (int i = 0; i < TimelineUI.Children.Count; i++)
            //{
            //    if (TimelineUI.Children[i].Visibility == Visibility.Visible)
            //    {
            //        c++;
            //    }
            //}
            //Window.gameplayclock.Text = $"{c}";
        }

        public static void ChangeTimelineSizeOnResize()
        {
            if (Window.songSlider.RenderSize.Width - 20 > 0)
            {
                TimelineUI.Width = Window.songSlider.RenderSize.Width - 20;
            }

            double percent;
            double hitPositionOnTimeline;
            foreach (Path line in TimelineJudgements100)
            {
                percent = ((long)line.DataContext / Window.songSlider.Maximum);
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }

            foreach (Path line in TimelineJudgements50)
            {
                percent = ((long)line.DataContext / Window.songSlider.Maximum);
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }

            foreach (Path line in TimelineJudgementsMiss)
            {
                percent = ((long)line.DataContext / Window.songSlider.Maximum);
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }
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

        private static void HideVisibleOverlappingJudgements(HitObjectJudgement currentJudgementOption)
        {// need to declare these here to keep references to the objects
            Path previousPath;
            Path currentPath;
            for (int i = 1; i < TimelineUI.Children.Count; i++)
            {
                previousPath = (Path)TimelineUI.Children[i - 1];
                currentPath = (Path)TimelineUI.Children[i];

                while (currentPath.Visibility == Visibility.Collapsed)
                {
                    i++;
                    if (i >= TimelineUI.Children.Count)
                    {
                        return;
                    }
                    currentPath = (Path)TimelineUI.Children[i];
                }

                if (IsOnTop(previousPath, currentPath))
                {
                    HideLowerPriority(previousPath, currentPath);
                }
            }
        }

        private static void ShowBackVisibleOverlappingJudgements(HitObjectJudgement currentJudgementOption)
        {// need to declare these here to keep references to the objects
            Path previousPath;
            Path currentPath;
            for (int i = 1; i < TimelineUI.Children.Count; i++)
            {
                previousPath = (Path)TimelineUI.Children[i - 1];
                currentPath = (Path)TimelineUI.Children[i];

                if ((SettingsOptions.GetConfigValue("Show100OnTimeline") == "false"
                &&  (previousPath.Name == "Ok" || currentPath.Name == "Ok")))
                {
                    continue;
                }
                else if ((SettingsOptions.GetConfigValue("Show50OnTimeline") == "false"
                &&       (previousPath.Name == "Meh" || currentPath.Name == "Meh")))
                {
                    continue;
                }
                else if ((SettingsOptions.GetConfigValue("ShowMissOnTimeline") == "false"
                &&       (previousPath.Name == "Miss" || currentPath.Name == "Miss")))
                {
                    continue;
                }

                if (IsOnTop(previousPath, currentPath))
                {
                    ShowLowerPriority(previousPath, currentPath);
                }
            }
        }

        private static void CreateTimelineUI()
        {
            TimelineUI.Width = Window.songSlider.RenderSize.Width - 20;
            TimelineUI.Height = Window.musicControlUI.RenderSize.Height;
            TimelineUI.Background = new SolidColorBrush(Colors.Transparent);
            Canvas.SetZIndex(TimelineUI, -1);
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

        private static void ShowLowerPriority(Path previousLine, Path currentLine)
        {
            if (Canvas.GetZIndex(previousLine) < Canvas.GetZIndex(currentLine))
            {
                previousLine.Visibility = Visibility.Visible;
            }
            else if (Canvas.GetZIndex(previousLine) > Canvas.GetZIndex(currentLine))
            {
                currentLine.Visibility = Visibility.Visible;
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
            && previousLine.Name != currentLine.Name)
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
    }
}
