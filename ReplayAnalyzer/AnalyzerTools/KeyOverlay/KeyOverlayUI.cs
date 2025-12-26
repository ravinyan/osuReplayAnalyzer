using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.AnalyzerTools.KeyOverlay
{
    public class KeyOverlayUI
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Grid KeyOverlay = new Grid();

        public static Grid Create()
        {
            KeyOverlay.Width = 100;
            //KeyOverlay.Height = 150;
            // 2 columns for each key
            // 2 rows for separate button click look and the hold duration UI element
            // ^ uh maybe?
            // create storyboard that will always move stuff up? honestly i have no clue how to do click stuff
            // also take code from hit markers for knowing which key is clicked

            CreateHoldDurationUI(new Thickness(0, 0, 5, 0), 100, 0, 0, new SolidColorBrush(Colors.MediumPurple));
            CreateHoldDurationUI(new Thickness(5, 0, 0, 0), 100, 0, 1, new SolidColorBrush(Colors.MediumPurple));

            CreateKeyButton("K1", new Thickness(0, 0, 5, 0), new Thickness(0, 11.5, 0, 0), 40, 1, 0);
            CreateKeyButton("K2", new Thickness(5, 0, 0, 0), new Thickness(0, 11.5, 0, 0), 40, 1, 1);

            Canvas.SetLeft(KeyOverlay, (Window.Width - KeyOverlay.Width) - 20);
            Canvas.SetTop(KeyOverlay, (Window.Height - Window.musicControlUI.ActualHeight) - 190);

            return KeyOverlay;
        }

        public static void Resize()
        {
            // scale size with osu scale maybe?


            Canvas.SetLeft(KeyOverlay, (Window.Width - KeyOverlay.Width) - 20);
            Canvas.SetTop(KeyOverlay, (Window.Height - Window.musicControlUI.ActualHeight) - 190);
        }

        private static void CreateHoldDurationUI(Thickness margin, int height, int row, int col, SolidColorBrush colour)
        {
            RowDefinition holdRow = new RowDefinition();
            holdRow.MaxHeight = height;

            Canvas keyHoldUI = new Canvas();
            keyHoldUI.Opacity = 0.7;
            keyHoldUI.Margin = margin;
            keyHoldUI.Height = height;
            keyHoldUI.Background = new SolidColorBrush(Colors.MediumPurple);

            KeyOverlay.RowDefinitions.Add(holdRow);
            KeyOverlay.Children.Add(keyHoldUI);

            Grid.SetRow(keyHoldUI, row);
            Grid.SetColumn(keyHoldUI, col);
        }

        private static void CreateKeyButton(string keyName, Thickness margin, Thickness padding, int keySize, int row, int col)
        {
            ColumnDefinition keyCol = new ColumnDefinition();
            keyCol.Width = GridLength.Auto;

            TextBlock key = new TextBlock();
            key.Width = keySize;
            key.Height = keySize;
            key.Text = keyName;
            key.Foreground = new SolidColorBrush(Colors.White);
            key.TextAlignment = TextAlignment.Center;
            key.Padding = padding;

            Border keyBorder = new Border();
            keyBorder.BorderThickness = new Thickness(1);
            keyBorder.BorderBrush = new SolidColorBrush(Colors.White);
            keyBorder.Margin = margin;
            keyBorder.VerticalAlignment = VerticalAlignment.Bottom;
            keyBorder.Child = key;

            Grid.SetRow(keyBorder, row);
            Grid.SetColumn(keyBorder, col);
            
            KeyOverlay.ColumnDefinitions.Add(keyCol);
            KeyOverlay.Children.Add(keyBorder);
        }
    }
}
