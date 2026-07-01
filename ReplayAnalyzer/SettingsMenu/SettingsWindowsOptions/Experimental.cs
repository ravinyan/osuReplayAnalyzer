using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    public class Experimental
    {
        public static void AddOptions(StackPanel panel)
        {
            TextBlock text = new TextBlock();
            text.Text = "Random things that work/kinda work";
            text.Foreground = Brushes.White;
            text.FontWeight = FontWeights.Bold;
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.Padding = new Thickness(10);

            panel.Children.Add(text);

            TextBlock movableText = new TextBlock();
            movableText.Foreground = Brushes.White;
            movableText.TextWrapping = TextWrapping.Wrap;
            movableText.Padding = new Thickness(5);
            movableText.Text = "Note: This makes UR Bar, Hit Map, Key Overlay, osu!mania and osu!taiko playfields movable objects.";
            panel.Children.Add(movableText);
            panel.Children.Add(SettingsOptions.MakeUIMovable());
            panel.Children.Add(SettingsOptions.ResetUIPositions());
        }
    }
}
