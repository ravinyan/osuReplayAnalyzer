using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.SettingsMenu;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#nullable disable

namespace ReplayAnalyzer.AnalyzerTools.KeyOverlay
{
    // inspired by https://github.com/Blondazz/KeyOverlay and streamer who used it so i assume its nice to have?
    // UI is the same but i wanted to do it that way anyway since it looks the best + perfect for customizing anything
    // if it came out as 1:1 copy them im really sorry i didnt mean to do that... this Blondazz dude is just very good at making nice UI
    public class KeyOverlay
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        
        public static Grid KeyOverlayWindow = new Grid();

        private static List<Canvas> KeyPressesL = new List<Canvas>();
        private static List<Canvas> KeyPressesR = new List<Canvas>();

        private static bool isHeldL = false;
        private static bool isHeldR = false;

        private static double VELOCITY = 0.2;

        private static Canvas ColLeft = null;
        private static Canvas ColRight = null;

        // i have ABSOLUTELY NO CLUE what im even doing hopefully this code wont be horrible
        public static void UpdateHoldPositions()
        {
            if (GamePlayClock.IsPaused() || KeyOverlayWindow.Visibility == Visibility.Collapsed)
            {
                return;
            }

            // when map ended just move all alive objects to the end and clear them
            if ((ColLeft.Children.Count != 0 || ColRight.Children.Count != 0)
            &&  CursorManager.CursorPositionIndex >= MainWindow.replay.FramesDict.Count)
            {
                MoveClickBars(KeyPressesL, ColLeft);
                MoveClickBars(KeyPressesR, ColRight);

                return;
            }

            // looks scuffed but idk how else to do it
            if (CursorManager.CursorPositionIndex >= MainWindow.replay.FramesDict.Count)
            {
                return;
            }

            ReplayFrame  frame = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex];
            
            // click code from HitMarkerDataClass
            bool leftClick = false;
            bool rightClick = false;
            if (frame.Click == Clicks.M1 || frame.Click == Clicks.K1)
            {
                leftClick = true;
            }
            else if (frame.Click == Clicks.M2 || frame.Click == Clicks.K2)
            {
                rightClick = true;
            }
            else if (frame.Click == Clicks.M12 || frame.Click == Clicks.K12)
            {
                leftClick = true;
                rightClick = true;
            }

            if (isHeldL == true && leftClick == false)
            {
                isHeldL = false;
                var a = KeyOverlayWindow.Children[2] as Border;
                a.Background = new SolidColorBrush(Colors.Transparent);
            }
            else if (isHeldL == false && leftClick == true)
            {
                isHeldL = true;
                var a = KeyOverlayWindow.Children[2] as Border;
                a.Background = new SolidColorBrush(Color.FromRgb(63, 190, 221));
                Canvas canvas = CreateClickBar(KeyPressesL, ColLeft);
            }

            if (isHeldR == true && rightClick == false)
            {
                isHeldR = false;
                var a = KeyOverlayWindow.Children[3] as Border;
                a.Background = new SolidColorBrush(Colors.Transparent);
            }
            else if (isHeldR == false && rightClick == true)
            {
                isHeldR = true;
                var a = KeyOverlayWindow.Children[3] as Border;
                a.Background = new SolidColorBrush(Color.FromRgb(63, 190, 221));
                Canvas canvas = CreateClickBar(KeyPressesR, ColRight);
            }

            if (isHeldL == true)
            {
                Canvas c = KeyPressesL.LastOrDefault();
                if (c != null)
                {
                    c.Height = ColLeft.Height - Canvas.GetTop(c);
                }
            }
            if (isHeldR == true)
            {
                Canvas c = KeyPressesR.LastOrDefault();
                if (c != null)
                {
                    c.Height = ColRight.Height - Canvas.GetTop(c);
                }
            }

            MoveClickBars(KeyPressesL, ColLeft);
            MoveClickBars(KeyPressesR, ColRight);
        }

        public static Grid Create()
        {
            KeyOverlayWindow.Width = 100;

            CreateHoldDurationUI(new Thickness(0, 0, 5, 0), 200, 0, 0, new SolidColorBrush(Colors.Transparent));
            CreateHoldDurationUI(new Thickness(5, 0, 0, 0), 200, 0, 1, new SolidColorBrush(Colors.Transparent));

            CreateKeyButton("K1", new Thickness(0, 0, 5, 0), new Thickness(0, 11.5, 0, 0), 40, 1, 0);
            CreateKeyButton("K2", new Thickness(5, 0, 0, 0), new Thickness(0, 11.5, 0, 0), 40, 1, 1);

            ColLeft = KeyOverlayWindow.Children[0] as Canvas;
            ColRight = KeyOverlayWindow.Children[1] as Canvas;

            Canvas.SetLeft(KeyOverlayWindow, (Window.Width - KeyOverlayWindow.Width) - 20);
            Canvas.SetTop(KeyOverlayWindow, (Window.Height - Window.musicControlUI.ActualHeight) - (242 + 50));

            if (SettingsOptions.GetConfigValue("ShowKeyOverlay") == "false")
            {
                KeyOverlayWindow.Visibility = Visibility.Collapsed;
            }

            return KeyOverlayWindow;
        }

        public static void Resize()
        {
            // scale size with osu scale maybe?


            Canvas.SetLeft(KeyOverlayWindow, (Window.Width - KeyOverlayWindow.Width) - 20);
            Canvas.SetTop(KeyOverlayWindow, (Window.Height - Window.musicControlUI.ActualHeight) - (KeyOverlayWindow.ActualHeight + 50));
        }

        private static void MoveClickBars(List<Canvas> clicks, Canvas column)
        {
            for (int i = 0; i < clicks.Count; i++)
            {
                Canvas c = clicks[i];
                Canvas.SetTop(c, Canvas.GetTop(c) - VELOCITY);
                if (Canvas.GetTop(c) + c.Height <= 0 || Double.IsNaN(Canvas.GetTop(c)))
                {
                    if (column.Children.Contains(c))
                    {
                        column.Children.Remove(c);
                    }

                    clicks.Remove(c);
                }
            }
        }

        private static Canvas CreateClickBar(List<Canvas> clicks, Canvas column)
        {
            Canvas canvas = new Canvas();
            canvas.Width = 49;
            canvas.Height = 1;
            canvas.Background = new SolidColorBrush(Color.FromRgb(63, 190, 221));
            column.Children.Add(canvas);
            clicks.Add(canvas);

            Canvas.SetLeft(canvas, 0);
            Canvas.SetTop(canvas, column.ActualHeight - canvas.Height);

            return canvas;
        }

        private static void CreateHoldDurationUI(Thickness margin, int height, int row, int col, SolidColorBrush colour)
        {
            RowDefinition holdRow = new RowDefinition();
            holdRow.MaxHeight = height;

            Canvas keyHoldUI = new Canvas();
            keyHoldUI.Opacity = 0.7;
            keyHoldUI.Margin = margin;
            keyHoldUI.Height = height;
            keyHoldUI.Background = colour;
            keyHoldUI.ClipToBounds = true;

            KeyOverlayWindow.RowDefinitions.Add(holdRow);
            KeyOverlayWindow.Children.Add(keyHoldUI);

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
            
            KeyOverlayWindow.ColumnDefinitions.Add(keyCol);
            KeyOverlayWindow.Children.Add(keyBorder);
        }
    }
}
