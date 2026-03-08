using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.SettingsMenu;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#nullable disable

namespace ReplayAnalyzer.AnalyzerTools.KeyOverlay
{
    // key overlay in style of what i saw one time on some osu streams which was https://github.com/Blondazz/KeyOverlay
    // also had idea to do this that way anyway coz its simple and easy to customize so oops
    public class KeyOverlay
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        
        public static Grid KeyOverlayWindow = new Grid();

        private static List<Canvas> KeyPressesL = new List<Canvas>();
        private static List<Canvas> KeyPressesR = new List<Canvas>();

        private static bool isHeldL = false;
        private static bool isHeldR = false;

        private static double VELOCITY = 3.5;

        private static Canvas ColLeft = null;
        private static Canvas ColRight = null;

        private static Stopwatch Cooldown = new Stopwatch();
        
        public static void UpdateHoldPositions(bool isSeeking = false)
        {
            if ((GamePlayClock.IsPaused() && isSeeking == false) || KeyOverlayWindow.Visibility == Visibility.Collapsed
            ||   MainWindow.replay.FramesDict.Count == 0)
            {
                return;
            }

            if (Cooldown.ElapsedMilliseconds <= 1000 / 60.0)
            {
                return;
            }
            Cooldown.Restart();

            // when map ended just move all alive objects to the end and clear them
            if ((ColLeft.Children.Count != 0 || ColRight.Children.Count != 0)
            &&   CursorManager.CursorPositionIndex >= MainWindow.replay.FramesDict.Count)
            {
                MoveClickBarsUp(KeyPressesL, ColLeft, isSeeking);
                MoveClickBarsUp(KeyPressesR, ColRight, isSeeking);

                return;
            }

            // looks scuffed but idk how else to do it
            if (CursorManager.CursorPositionIndex >= MainWindow.replay.FramesDict.Count)
            {
                return;
            }

            ReplayFrame frame = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex];

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
                ChangeKeyButtonBackground("left", new SolidColorBrush(Colors.Transparent));
            }
            else if (isHeldL == false && leftClick == true)
            {
                isHeldL = true;
                ChangeKeyButtonBackground("left", new SolidColorBrush(Color.FromRgb(63, 190, 221)));
                KeyPressesL.Add(CreateClickBar(ColLeft)); 
            }

            if (isHeldR == true && rightClick == false)
            {
                isHeldR = false;
                ChangeKeyButtonBackground("right", new SolidColorBrush(Colors.Transparent));
            }
            else if (isHeldR == false && rightClick == true)
            {
                isHeldR = true;
                ChangeKeyButtonBackground("right", new SolidColorBrush(Color.FromRgb(63, 190, 221)));
                KeyPressesR.Add(CreateClickBar(ColRight));
            }

            if (isHeldL == true)
            {
                StretchClickBar("left");
            }
            if (isHeldR == true)
            {
                StretchClickBar("right");
            }

            MoveClickBarsUp(KeyPressesL, ColLeft, isSeeking);
            MoveClickBarsUp(KeyPressesR, ColRight, isSeeking);
        }

        public static Grid Create()
        {
            KeyOverlayWindow.Width = 100;

            CreateHoldDurationUI(new Thickness(0, 0, 5, 0), 0);
            CreateHoldDurationUI(new Thickness(5, 0, 0, 0), 1);

            CreateKeyButtonUI("K1", new Thickness(0, 0, 5, 0), 0);
            CreateKeyButtonUI("K2", new Thickness(5, 0, 0, 0), 1);

            ColLeft = KeyOverlayWindow.Children[0] as Canvas;
            ColRight = KeyOverlayWindow.Children[1] as Canvas;

            Canvas.SetLeft(KeyOverlayWindow, (Window.Width - KeyOverlayWindow.Width) - 20);
            Canvas.SetTop(KeyOverlayWindow, (Window.Height - Window.musicControlUI.ActualHeight) - (242 + 50));

            if (SettingsOptions.GetConfigValue("ShowKeyOverlay") == "false")
            {
                KeyOverlayWindow.Visibility = Visibility.Collapsed;
            }

            Cooldown.Start();

            return KeyOverlayWindow;
        }

        public static void Resize()
        {
            // scale size with osu scale maybe?


            Canvas.SetLeft(KeyOverlayWindow, (Window.Width - KeyOverlayWindow.Width) - 20);
            Canvas.SetTop(KeyOverlayWindow, (Window.Height - Window.musicControlUI.ActualHeight) - (KeyOverlayWindow.ActualHeight + 50));
        }

        private static void StretchClickBar(string buttonPressed)
        {
            if (buttonPressed == "left" && KeyPressesL.Count > 0)
            {
                Canvas click = KeyPressesL.LastOrDefault();
                click.Height = ColLeft.Height - Canvas.GetTop(click);
            }
            else if (buttonPressed == "right" && KeyPressesR.Count > 0)
            {
                Canvas click = KeyPressesR.LastOrDefault();
                click.Height = ColRight.Height - Canvas.GetTop(click);
            }
        }

        private static void MoveClickBarsUp(List<Canvas> clicks, Canvas column, bool isSeeking)
        {
            int count = clicks.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                Canvas click = clicks[i];
                Canvas.SetTop(click, Canvas.GetTop(click) - VELOCITY);
                if (Canvas.GetTop(click) + click.Height <= 0 || Double.IsNaN(Canvas.GetTop(click)))
                {
                    if (column.Children.Contains(click))
                    {
                        column.Children.Remove(click);
                    }

                    clicks.Remove(click);
                }
            }
        }

        private static Canvas CreateClickBar(Canvas column)
        {
            Canvas canvas = new Canvas();
            canvas.Width = 49;
            canvas.Height = 3;
            canvas.Background = new SolidColorBrush(Color.FromRgb(63, 190, 221));

            Canvas.SetLeft(canvas, 0);
            Canvas.SetTop(canvas, column.ActualHeight - canvas.Height);

            column.Children.Add(canvas);

            return canvas;
        }

        private static void CreateHoldDurationUI(Thickness margin, int col)
        {
            RowDefinition holdRow = new RowDefinition();
            holdRow.MaxHeight = 200;

            Canvas keyHoldUI = new Canvas();
            keyHoldUI.Opacity = 0.7;
            keyHoldUI.Margin = margin;
            keyHoldUI.Height = 200;
            keyHoldUI.Background = new SolidColorBrush(Colors.Transparent);
            keyHoldUI.ClipToBounds = true;

            KeyOverlayWindow.RowDefinitions.Add(holdRow);
            KeyOverlayWindow.Children.Add(keyHoldUI);

            // row 0 is on the top part, col 0/1 is left/right side
            Grid.SetRow(keyHoldUI, 0);
            Grid.SetColumn(keyHoldUI, col);
        }

        private static void CreateKeyButtonUI(string keyName, Thickness margin, int col)
        {
            ColumnDefinition keyCol = new ColumnDefinition();
            keyCol.Width = GridLength.Auto;

            TextBlock key = new TextBlock();
            key.Width = 40;
            key.Height = 40;
            key.Text = keyName;
            key.Foreground = new SolidColorBrush(Colors.White);
            key.TextAlignment = TextAlignment.Center;
            key.Padding = new Thickness(0, 11.5, 0, 0);

            Border keyBorder = new Border();
            keyBorder.BorderThickness = new Thickness(1);
            keyBorder.BorderBrush = new SolidColorBrush(Colors.White);
            keyBorder.Margin = margin;
            keyBorder.VerticalAlignment = VerticalAlignment.Bottom;
            keyBorder.Child = key;

            // row 1 is the bottom part, col 0/1 is left/right side
            Grid.SetRow(keyBorder, 1);
            Grid.SetColumn(keyBorder, col);
            
            KeyOverlayWindow.ColumnDefinitions.Add(keyCol);
            KeyOverlayWindow.Children.Add(keyBorder);
        }

        private static void ChangeKeyButtonBackground(string buttonPressed, Brush color)
        {
            if (buttonPressed == "left")
            {
                Border leftButton = KeyOverlayWindow.Children[2] as Border;
                leftButton.Background = color;
            }
            else
            {
                Border rightButton = KeyOverlayWindow.Children[3] as Border;
                rightButton.Background = color;
            }
        }
    }
}
