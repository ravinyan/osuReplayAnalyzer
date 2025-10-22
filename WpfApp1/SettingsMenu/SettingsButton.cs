using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WpfApp1.SettingsMenu
{
    public class SettingsButton
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static Button Create()
        {
            Button button = new Button();
            button.Width = 35;
            button.Height = 35;
            button.Background = Brushes.Black;
            button.BorderThickness = new Thickness(0);
            button.VerticalAlignment = VerticalAlignment.Top;
            button.HorizontalAlignment = HorizontalAlignment.Left;

            Path buttonIcon = new Path();
            buttonIcon.Data = Geometry.Parse("m 23.94,18.78 c .03,-0.25 .05,-0.51 .05,-0.78 0,-0.27 -0.02,-0.52 -0.05,-0.78 l 1.68,-1.32 c .15,-0.12 .19,-0.33 .09,-0.51 l -1.6,-2.76 c -0.09,-0.17 -0.31,-0.24 -0.48,-0.17 l -1.99,.8 c -0.41,-0.32 -0.86,-0.58 -1.35,-0.78 l -0.30,-2.12 c -0.02,-0.19 -0.19,-0.33 -0.39,-0.33 l -3.2,0 c -0.2,0 -0.36,.14 -0.39,.33 l -0.30,2.12 c -0.48,.2 -0.93,.47 -1.35,.78 l -1.99,-0.8 c -0.18,-0.07 -0.39,0 -0.48,.17 l -1.6,2.76 c -0.10,.17 -0.05,.39 .09,.51 l 1.68,1.32 c -0.03,.25 -0.05,.52 -0.05,.78 0,.26 .02,.52 .05,.78 l -1.68,1.32 c -0.15,.12 -0.19,.33 -0.09,.51 l 1.6,2.76 c .09,.17 .31,.24 .48,.17 l 1.99,-0.8 c .41,.32 .86,.58 1.35,.78 l .30,2.12 c .02,.19 .19,.33 .39,.33 l 3.2,0 c .2,0 .36,-0.14 .39,-0.33 l .30,-2.12 c .48,-0.2 .93,-0.47 1.35,-0.78 l 1.99,.8 c .18,.07 .39,0 .48,-0.17 l 1.6,-2.76 c .09,-0.17 .05,-0.39 -0.09,-0.51 l -1.68,-1.32 0,0 z m -5.94,2.01 c -1.54,0 -2.8,-1.25 -2.8,-2.8 0,-1.54 1.25,-2.8 2.8,-2.8 1.54,0 2.8,1.25 2.8,2.8 0,1.54 -1.25,2.8 -2.8,2.8 l 0,0 z");
            buttonIcon.Fill = Brushes.White;
            buttonIcon.Margin = new Thickness(-10.5, -10, 0, 0);
            // why everything has .Children but this has .Content why why why why why WHY I HATE YOU
            button.Content = buttonIcon;

            button.Click += delegate (object sender, RoutedEventArgs e)
            {
                ScrollViewer? scroll = SettingsPanel.SettingPanelBox.Parent as ScrollViewer;
                if (SettingsPanel.SettingPanelBox.Visibility == Visibility.Hidden)
                {
                    SettingsPanel.SettingPanelBox.Visibility = Visibility.Visible;
                    scroll!.Visibility = Visibility.Visible;
                }
                else
                {
                    SettingsPanel.SettingPanelBox.Visibility = Visibility.Hidden;
                    scroll!.Visibility = Visibility.Collapsed;
                }
            };

            Window.KeyDown += delegate (object sender, System.Windows.Input.KeyEventArgs e)
            {
                if (e.Key == System.Windows.Input.Key.Escape)
                {
                    ScrollViewer? scroll = SettingsPanel.SettingPanelBox.Parent as ScrollViewer;
                    if (scroll!.Visibility == Visibility.Collapsed)
                    {
                        SettingsPanel.SettingPanelBox.Visibility = Visibility.Visible;
                        scroll!.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SettingsPanel.SettingPanelBox.Visibility = Visibility.Hidden;
                        scroll!.Visibility = Visibility.Collapsed;
                    }
                }
            };

            return button;
        }
    }
}
