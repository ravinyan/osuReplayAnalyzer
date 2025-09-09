using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp1.PlayfieldUI;

namespace WpfApp1.SettingsMenu
{
    public class SettingsOptions
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public List<StackPanel> Options = new List<StackPanel>();

        public void CreateOptions()
        {
            Grid panel = SettingsPanel.SettingPanel;

            BackgrounOpacity();
            Resolution();

            //panel.Children.Add(textBlock);
        }

        private void BackgrounOpacity()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;

            Slider slider = new Slider();
            slider.Value = Window.playfieldBackground.Opacity * 100;
            slider.Maximum = 100;
            slider.TickFrequency = 1;
            slider.Width = 100;

            name.Text = $"Background Opacity: {(int)slider.Value}%";

            slider.ValueChanged += delegate (object sender, RoutedPropertyChangedEventArgs<double> e)
            {
                Window.playfieldBackground.Opacity = slider.Value / 100;
                name.Text = $"Background Opacity: {(int)slider.Value}%";
            };

            panel.Children.Add(name);
            panel.Children.Add(slider);

            Options.Add(panel);
        }

        private void Resolution()
        {
            StackPanel panel = new StackPanel();
            panel.Orientation = Orientation.Horizontal;
            panel.HorizontalAlignment = HorizontalAlignment.Left;
            panel.VerticalAlignment = VerticalAlignment.Center;

            TextBlock name = new TextBlock();
            name.Text = "Resolution: ";
            name.Foreground = new SolidColorBrush(Colors.White);
            name.Width = 150;

            string[] resolutionOptions = new string[] 
            { 
               "800x600", "1280x800", "1360x786", "1440x1080", "1600x1050", "1980x1080", "2560x1440", "2560x1600" 
            };

            ComboBox comboBox = new ComboBox();
            comboBox.Width = 100;
            comboBox.Height = 25;
            comboBox.SelectedIndex = 0;
            comboBox.ItemsSource = resolutionOptions;

            // i hate math
            comboBox.SelectionChanged += delegate (object sender, SelectionChangedEventArgs e)
            {
                string[] res = comboBox.SelectedItem.ToString()!.Split('x');

                PresentationSource source = PresentationSource.FromVisual(Window);

                double dpiScaleWidht = source.CompositionTarget.TransformToDevice.M11;
                double dpiScaleHeight = source.CompositionTarget.TransformToDevice.M22;

                double borderWidth = (SystemParameters.BorderWidth * dpiScaleHeight) * 2;
               
                double width = (int.Parse(res[0]) + borderWidth) / dpiScaleWidht;
                double height = (int.Parse(res[1]) + borderWidth) / dpiScaleHeight;

                double maxScreenHeight = SystemParameters.PrimaryScreenHeight * dpiScaleHeight;

                double topWindowHeight = System.Windows.Forms.SystemInformation.ToolWindowCaptionHeight * dpiScaleHeight;
                double toolbarHeight = (SystemParameters.PrimaryScreenHeight - SystemParameters.FullPrimaryScreenHeight - SystemParameters.WindowCaptionHeight) + dpiScaleHeight;
                if (int.Parse(res[1]) == maxScreenHeight)
                {
                    Window.Height = height + borderWidth - toolbarHeight;
                }
                else
                {
                    Window.Height = height + borderWidth + topWindowHeight - 22;
                }

                // -22 and + 10 are idk what even adjusted it by hand and it works for pixel perfect screen size
                Window.Width = width + 10;

                if (MainWindow.map != null)
                {
                    ResizePlayfield.ResizePlayfieldCanva();
                }                
            };

            panel.Children.Add(name);
            panel.Children.Add(comboBox);

            Options.Add(panel);
        }
    }
}
