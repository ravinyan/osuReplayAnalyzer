using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfApp1.PlayfieldUI.UIElements
{
    public class JudgementCounter
    {
        public static StackPanel panel = new StackPanel();
        
        private static int Hit300Count = 0;
        private static int Hit100Count = 0;
        private static int Hit50Count = 0;
        private static int MissCount = 0;

        public static StackPanel Create()
        {
            panel.Name = "JudgementPanel";
            panel.Height = 100;
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Right;

            TextBlock counter300 = new TextBlock();
            counter300.Background = Brushes.Transparent;
            counter300.Foreground = Brushes.Blue;
            counter300.Text = "0";

            TextBlock counter100 = new TextBlock();
            counter100.Background = Brushes.Transparent;
            counter100.Foreground = Brushes.Green;
            counter100.Text = "0";

            TextBlock counter50 = new TextBlock();
            counter50.Background = Brushes.Transparent;
            counter50.Foreground = Brushes.Orange;
            counter50.Text = "0";

            TextBlock missCounter = new TextBlock();
            missCounter.Background = Brushes.Transparent;
            missCounter.Foreground = Brushes.Red;
            missCounter.Text = "0";

            panel.Children.Add(counter300);
            panel.Children.Add(counter100);
            panel.Children.Add(counter50);
            panel.Children.Add(missCounter);

            return panel;
        }

        public static void Increment300()
        {
            TextBlock? t = panel.Children[0] as TextBlock;

            Hit300Count++;
            t.Text = $"{Hit300Count}";
        }

        public static void Increment100()
        {
            TextBlock? t = panel.Children[1] as TextBlock;

            Hit100Count++;
            t.Text = $"{Hit100Count}";
        }

        public static void Increment50()
        {
            TextBlock? t = panel.Children[2] as TextBlock;

            Hit50Count++;
            t.Text = $"{Hit50Count}";
        }

        public static void IncrementMiss()
        {
            TextBlock? t = panel.Children[3] as TextBlock;

            MissCount++;
            t.Text = $"{MissCount}";
        }
    }
}
