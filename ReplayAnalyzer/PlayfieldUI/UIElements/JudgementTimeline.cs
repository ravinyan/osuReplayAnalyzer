using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.SettingsMenu;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.PlayfieldUI.UIElements
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
        }

        public static void CreateJudgementLine(HitObjectJudgement judgement, long hitAt)
        {
            double percent = hitAt / Window.songSlider.Maximum;
            double hitPositionOnTimeline = TimelineUI.Width * percent;

            Path judgementLine = new Path();
            // my eyes are skill issued i dont know if i see difference or not but i feel like it helps performance
            judgementLine.Name = judgement.ToString();
            judgementLine.CacheMode = new BitmapCache();
            judgementLine.DataContext = hitAt;
            judgementLine.Width = 2;
            judgementLine.Height = Window.musicControlUI.ActualHeight;
            judgementLine.StrokeThickness = 2;
            judgementLine.Opacity = 1;
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
        }

        public static void PopulateJudgementTimeline()
        {
            List<Path> allPaths = new List<Path>();
            allPaths.AddRange(TimelineJudgements100);
            allPaths.AddRange(TimelineJudgements50);
            allPaths.AddRange(TimelineJudgementsMiss);

            // WHY THIS IS ONLY 2x FASTER THAN BUBBLE SORT??? EXCUSE ME N * LOG N MY ASS??? (on 1k elements so not that much i guess)
            MergeTHIS(allPaths); //~230 000 vs ~500 000 bubble sort < in 30k elements THIS IS SLOW AS FUCK did i do something wrong or what

            int last100Index  = - 1;
            int last50Index   = - 1;
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

            HideOverlappingJudgements();
        }

        public static void HideJudgements(List<Path> timeline)
        {
            foreach (Path line in timeline)
            {
                line.Visibility = Visibility.Collapsed;
            }
            HideOverlappingJudgements();
        }

        public static void ShowJudgements(List<Path> timeline)
        {
            foreach (Path line in timeline)
            {
                line.Visibility = Visibility.Visible;
            }
            HideOverlappingJudgements();
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
                percent = (long)line.DataContext / Window.songSlider.Maximum;
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }

            foreach (Path line in TimelineJudgements50)
            {
                percent = (long)line.DataContext / Window.songSlider.Maximum;
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }

            foreach (Path line in TimelineJudgementsMiss)
            {
                percent = (long)line.DataContext / Window.songSlider.Maximum;
                hitPositionOnTimeline = TimelineUI.Width * percent;

                Canvas.SetLeft(line, hitPositionOnTimeline);
            }
        }
       
        // after a bit too many hours i finally solved this leetcode easy problem... this was so simple i hate myself... now wat
        // also its fast as fuck (1k ticks on 400 elements) and was FUN to solve without any help whatsoever yaaay
        private static void HideOverlappingJudgements()
        {
            bool x100Enabled = SettingsOptions.GetConfigValue("Show100OnTimeline")  == "true";
            bool x50Enabled  = SettingsOptions.GetConfigValue("Show50OnTimeline")   == "true";
            bool x0Enabled   = SettingsOptions.GetConfigValue("ShowMissOnTimeline") == "true";
            int maxPriorityInConfig = -1;
            if (x0Enabled)
            {
                maxPriorityInConfig = 2;
            }
            else if (x50Enabled)
            {
                maxPriorityInConfig = 1;
            }
            else if (x100Enabled)
            {
                maxPriorityInConfig = 0;
            }
            else // the cake is a lie
            {
                return;
            }

            Path path1;
            Path path2;

            double path1Pos = - 1;
            double path2Pos = - 1;

            int maxPriorityInList = -1;

            List<Path> paths = new List<Path>();
            for (int i = 0; i < TimelineUI.Children.Count; i++)
            {
                maxPriorityInList = 0;
                int j = i;

                path1 = (Path)TimelineUI.Children[i];
                path1Pos = Canvas.GetLeft(path1); // int

                if (j + 1 < TimelineUI.Children.Count)
                {
                    path2 = (Path)TimelineUI.Children[++j];
                    path2Pos = Canvas.GetLeft(path2); // int
                }
                else // when index is at the end to not cause exception
                {
                    path2 = null!;
                    path2Pos = -1;
                }

                paths = new List<Path>();
                paths.Add(path1);
                if (Panel.GetZIndex(path1) <= maxPriorityInConfig)
                {
                    maxPriorityInList = Panel.GetZIndex(path1);
                }
                
                while (path1Pos == path2Pos)
                {
                    if (j + 1 >= TimelineUI.Children.Count)
                    {
                        break;
                    }

                    int path2Priority = Panel.GetZIndex(path2);
                    if (path2Priority == 0 && x100Enabled
                    ||  path2Priority == 1 && x50Enabled
                    ||  path2Priority == 2 && x0Enabled)
                    {
                        paths.Add(path2);

                        if (path2Priority > maxPriorityInList)
                        {
                            maxPriorityInList = path2Priority;
                        }
                    }

                    path2 = (Path)TimelineUI.Children[++j];
                    path2Pos = Canvas.GetLeft(path2);
                }

                for (int k = 0; k < paths.Count; k++)
                {
                    int priority = Panel.GetZIndex(paths[k]);
                    if (priority == 0 && x100Enabled == false
                    ||  priority == 1 && x50Enabled  == false
                    ||  priority == 2 && x0Enabled   == false)
                    {
                        paths[k].Visibility = Visibility.Collapsed;
                        continue;
                    }

                    if (paths.Count == 1 || priority == maxPriorityInList)
                    {
                        paths[k].Visibility = Visibility.Visible;
                    }
                    else if (priority < maxPriorityInList || priority > maxPriorityInList)
                    {
                        paths[k].Visibility = Visibility.Collapsed;
                    }
                }
                paths.Clear();

                // prevents infinite looping
                if (j - 1 > i)
                {
                    i = j - 1;
                }
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

        private static void CreateTimelineUI()
        {
            TimelineUI.Width = Window.songSlider.RenderSize.Width - 20; // 20 is song slider thumb diameter
            TimelineUI.Height = Window.musicControlUI.RenderSize.Height;
            TimelineUI.Background = new SolidColorBrush(Colors.Transparent);
            Panel.SetZIndex(TimelineUI, -1);
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
                    Panel.SetZIndex(path, 0);
                    break;
                case HitObjectJudgement.Meh:
                    Panel.SetZIndex(path, 1);
                    break;
                case HitObjectJudgement.Miss:
                    Panel.SetZIndex(path, 2);
                    break;
            }
        }
    }
}
