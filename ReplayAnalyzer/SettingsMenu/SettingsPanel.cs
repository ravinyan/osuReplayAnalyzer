using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;

namespace ReplayAnalyzer.SettingsMenu
{
    public class SettingsPanel
    {
        public static Grid SettingPanelBox = new Grid();

        public static Grid Create()
        {
            // new one
            // [grid] 2x col > 2 [grid] on left and right > left and right are stack panels > if needed give right ability to scroll

            Grid mainWindow = new Grid();
            mainWindow.Background = new SolidColorBrush(Colors.Red);
            mainWindow.Opacity = 0.9;
            mainWindow.Width = 500;
            mainWindow.Height = 400;
            mainWindow.HorizontalAlignment = HorizontalAlignment.Center;
            mainWindow.VerticalAlignment = VerticalAlignment.Center;

            ColumnDefinition settingsButtonsCol = new ColumnDefinition();
            settingsButtonsCol.MaxWidth = 250;
            ColumnDefinition settingsOptionsCol = new ColumnDefinition();
            settingsOptionsCol.MaxWidth = 250;

            StackPanel buttonsPanel = new StackPanel();
            buttonsPanel.Background = new SolidColorBrush(Colors.AliceBlue);
            buttonsPanel.Margin = new Thickness(20);
            buttonsPanel.Width = 230;
            buttonsPanel.Orientation = Orientation.Vertical;

            for (int i = 0; i < 10; i++)
            {

                System.Windows.FontStyle fontStyle = FontStyles.Normal;
                FontWeight fontWeight = FontWeights.Medium;


                System.Windows.Media.FontFamily aa = new System.Windows.Media.FontFamily("A");
                string a = $"Hello{i}";


#pragma warning disable CS0618 // Type or member is obsolete
                FormattedText formattedText = 
                    new FormattedText(a, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight,
                    new Typeface(aa, fontStyle, fontWeight, FontStretches.Normal),
                    25, Brushes.Black);
#pragma warning restore CS0618 // Type or member is obsolete

                // Build the geometry object that represents the text.
                var _textGeometry = formattedText.BuildGeometry(new System.Windows.Point(buttonsPanel.Width - 10 - formattedText.Width * 2, 10));

                
                 var _textHighLightGeometry = formattedText.BuildHighlightGeometry(new System.Windows.Point(0, 0));

                Path p = new Path();
                p.Height = 40;
                p.Width = buttonsPanel.Width;
                p.Fill = Brushes.White;
                p.Stroke = Brushes.Black;
                p.StrokeThickness = 1;
                p.Data = _textGeometry;
                p.HorizontalAlignment = HorizontalAlignment.Center;
                p.VerticalAlignment = VerticalAlignment.Center;


                TextBlock imTiredOfStylizingButtons = new TextBlock();
                imTiredOfStylizingButtons.Name = $"RealButton{i}";
                imTiredOfStylizingButtons.Text = $"Hello{i}";
                imTiredOfStylizingButtons.Width = buttonsPanel.Width - 10;
                imTiredOfStylizingButtons.Height = 40;
                imTiredOfStylizingButtons.Foreground = new SolidColorBrush(Colors.Cyan);
                imTiredOfStylizingButtons.TextAlignment = TextAlignment.Center;
                imTiredOfStylizingButtons.HorizontalAlignment = HorizontalAlignment.Center;


                p.MouseEnter += delegate (object sender, MouseEventArgs e)
                {
                    p.Stroke = Brushes.Yellow;
                    imTiredOfStylizingButtons.Foreground = new SolidColorBrush(Colors.Yellow);
                };

                p.MouseLeave += delegate (object sender, MouseEventArgs e)
                {
                    p.Stroke = Brushes.Black;
                    imTiredOfStylizingButtons.Foreground = new SolidColorBrush(Colors.White);
                };


                buttonsPanel.Children.Add(p);
            }



            Grid.SetColumn(buttonsPanel, 0);
            mainWindow.ColumnDefinitions.Add(settingsButtonsCol);

            StackPanel optionsPanel = new StackPanel();
            optionsPanel.Background = new SolidColorBrush(Colors.AliceBlue);
            optionsPanel.Margin = new Thickness(20);
            optionsPanel.Width = 230;
            optionsPanel.Orientation = Orientation.Vertical;

            Grid.SetColumn(optionsPanel, 1);
            mainWindow.ColumnDefinitions.Add(settingsOptionsCol);

            mainWindow.Children.Add(buttonsPanel);
            mainWindow.Children.Add(optionsPanel);



            // [main] grid (widht and height of the visible control
            // [2nd] scroll
            // [3rd] grid with options (no height, width same as [main])
            SettingPanelBox.Name = "SettingsPanelBox";
            SettingPanelBox.Width = 400;
            SettingPanelBox.Height = 400;
            SettingPanelBox.VerticalAlignment = VerticalAlignment.Top;
            SettingPanelBox.HorizontalAlignment = HorizontalAlignment.Left;
            SettingPanelBox.Visibility = Visibility.Hidden;
            SettingPanelBox.Background = new SolidColorBrush(Colors.Black) { Opacity = 0.6 };

            Grid settingPanel = new Grid();
            settingPanel.Name = "SettingsPanel";
            settingPanel.Width = 300;
            settingPanel.VerticalAlignment = VerticalAlignment.Top;
            settingPanel.HorizontalAlignment = HorizontalAlignment.Left;

            SettingsOptions settingsOptions = new SettingsOptions();
            settingsOptions.CreateOptions();

            SettingPanelBox.ColumnDefinitions.Add(new ColumnDefinition());
            
            RowDefinition padddingRow = new RowDefinition();
            padddingRow.MaxHeight = 50;
            SettingPanelBox.RowDefinitions.Add(padddingRow);

            for (int i = 1; i <= settingsOptions.Options.Count; i++)
            {
                RowDefinition row = new RowDefinition();
                row.MaxHeight = 50;
                SettingPanelBox.RowDefinitions.Add(row);

                StackPanel option = settingsOptions.Options[i - 1];
                option.Margin = new Thickness(10);
                SettingPanelBox.Children.Add(option);
                Grid.SetRow(option, i);
            }

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Visibility = Visibility.Collapsed;
            scrollViewer.CanContentScroll = true;

            settingPanel.Children.Add(scrollViewer);
            scrollViewer.Content = SettingPanelBox;

            return mainWindow;
        }
    }
}
