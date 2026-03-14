using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    public class JudgementCounter
    {
        private static StackPanel JudgementCounterPanel = new StackPanel();
        
        private static int Hit300Count = 0;
        private static int Hit100Count = 0;
        private static int Hit50Count = 0;
        private static int MissCount = 0;

        public static void Reset()
        {
            Hit300Count = 0;
            Hit100Count = 0;
            Hit50Count = 0;
            MissCount = 0;

            for (int i = 0; i < JudgementCounterPanel.Children.Count; i++)
            {
                TextBlock counter = (TextBlock)JudgementCounterPanel.Children[i];
                counter.Text = "0";
            }
        }

        public static StackPanel Create()
        {
            ApplyPropertiesToJudgementCounter();

            Brush[] brushes = { Brushes.Blue, Brushes.Green, Brushes.Orange, Brushes.Red };
            for (int i = 0; i < brushes.Length; i++)
            {
                JudgementCounterPanel.Children.Add(CreateJudgementCounter(brushes[i]));
            }

            return JudgementCounterPanel;
        }

        public static void Increment300()
        {
            TextBlock counter = (TextBlock)JudgementCounterPanel.Children[0];

            Hit300Count++;
            counter.Text = $"{Hit300Count}";
        }

        public static void Increment100()
        {
            TextBlock counter = (TextBlock)JudgementCounterPanel.Children[1];

            Hit100Count++;
            counter.Text = $"{Hit100Count}";
        }

        public static void Increment50()
        {
            TextBlock counter = (TextBlock)JudgementCounterPanel.Children[2];

            Hit50Count++;
            counter.Text = $"{Hit50Count}";
        }

        public static void IncrementMiss()
        {
            TextBlock counter = (TextBlock)JudgementCounterPanel.Children[3];

            MissCount++;
            counter.Text = $"{MissCount}";
        }

        private static void ApplyPropertiesToJudgementCounter()
        {
            JudgementCounterPanel.Name = "JudgementPanel";
            JudgementCounterPanel.Height = 15;
            JudgementCounterPanel.Orientation = Orientation.Horizontal;
            JudgementCounterPanel.HorizontalAlignment = HorizontalAlignment.Right;
            JudgementCounterPanel.VerticalAlignment = VerticalAlignment.Top;
            JudgementCounterPanel.Margin = new Thickness(0, 0, 5, 0);
        }

        private static TextBlock CreateJudgementCounter(Brush colour)
        {
            TextBlock counter = new TextBlock();
            counter.Background = Brushes.Transparent;
            counter.Foreground = colour;
            counter.Text = "0";

            return counter;
        }
    }
}
