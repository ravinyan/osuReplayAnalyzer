using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.SettingsMenu.SettingsWindowsOptions
{
    public class Experimental
    {
        public static void AddOptions(StackPanel panel)
        {
            TextBlock text = new TextBlock();
            text.Text = "Random scuffed stuff that kinda works";
            text.Foreground = Brushes.White;
            text.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            text.Padding = new System.Windows.Thickness(10);

            panel.Children.Add(text);

            TextBlock movableText = new TextBlock();
            movableText.Foreground = Brushes.White;
            movableText.TextWrapping = System.Windows.TextWrapping.Wrap;
            movableText.Text = "Note: It might be scuffed when resizing the app. XY position is static position away from closest border of the app at the time of moving UI element.";
            panel.Children.Add(movableText);
            panel.Children.Add(SettingsOptions.MakeUIMovable());
            panel.Children.Add(SettingsOptions.ResetUIPositions());
        }
    }
}
