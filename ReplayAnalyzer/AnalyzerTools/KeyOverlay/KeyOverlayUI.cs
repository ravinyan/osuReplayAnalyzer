using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.AnalyzerTools.HitMarkers;
using ReplayAnalyzer.PlayfieldGameplay;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReplayAnalyzer.AnalyzerTools.KeyOverlay
{
    public class KeyOverlayUI
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Grid KeyOverlay = new Grid();
        private static List<Canvas> KeyPresses = new List<Canvas>();

        // i give up on doing key overal working with user input for fun coz it will take too much effort... here actual code

        private static bool isHeldL = false;
        private static bool isHeldR = false;

        // idk what im doing
        public static void UpdateHoldPositions()
        {
            ReplayFrame frame = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex];

            

            var c1 = KeyOverlay.Children[0] as Canvas;
            var c2 = KeyOverlay.Children[1] as Canvas;

                bool leftClick = false;
                bool rightClick = false;
                Canvas canvas = new Canvas();
                canvas.Width = 49;
                canvas.Height = 10;
                canvas.Background = new SolidColorBrush(Colors.Red);

            if (frame.Click == Clicks.M1 || frame.Click == Clicks.K1)
            {
                leftClick = true;
                c1.Children.Add(canvas);
                KeyPresses.Add(canvas);

                Canvas.SetLeft(canvas, 0);
                Canvas.SetTop(canvas, c1.ActualHeight - canvas.Height);
            }
            else if (frame.Click == Clicks.M2 || frame.Click == Clicks.K2)
            {
                rightClick = true;
                c2.Children.Add(canvas);
                KeyPresses.Add(canvas);

                Canvas.SetLeft(canvas, 0);
                Canvas.SetTop(canvas, c2.ActualHeight - canvas.Height);
            }
            else if (frame.Click == Clicks.M12 || frame.Click == Clicks.K12)
            {
                leftClick = true;
                rightClick = true;
                c1.Children.Add(canvas);
                c2.Children.Add(canvas);

                KeyPresses.Add(canvas);
                KeyPresses.Add(canvas);

                Canvas.SetLeft(canvas, 0);
                Canvas.SetTop(canvas, c1.ActualHeight - canvas.Height);
            }

            if (isHeldL == true && leftClick == false)
            {
                isHeldL = false;
            }
            else if (isHeldL == false && leftClick == true)
            {
                isHeldL = true;  
            }

            if (isHeldR == true && rightClick == false)
            {
                isHeldR = false;
            }
            else if (isHeldR == false && rightClick == true)
            {
                isHeldR = true;
            }

            
            
            //if (isHeldL == true)
            //{
                for (int i = 0; i < c1.Children.Count; i++)
                {
                    Canvas? c = KeyPresses[i];
                    Canvas.SetTop(c, Canvas.GetTop(c) - 0.25);
                    if (Canvas.GetTop(c) < 0 || Double.IsNaN(Canvas.GetTop(c)))
                    {
                        c1.Children.Remove(c);
                        KeyPresses.Remove(c);
                    }
                }
            //}
            //else if (isHeldR == true)
            //{
                for (int i = 0; i < c2.Children.Count; i++)
                {
                    Canvas? c = KeyPresses[i];
                    Canvas.SetTop(c, Canvas.GetTop(c) - 0.25);
                    if (Canvas.GetTop(c) < 0 || Double.IsNaN(Canvas.GetTop(c)))
                    {
                        c2.Children.Remove(c);
                        KeyPresses.Remove(c);
                    }
                }
            //}    
        }


        //private static bool IsHold = false;
        //
        
        //private static List<(bool, Key)> KeysHold = new List<(bool, Key)>();

        //public static void UpdateHoldPositions()
        //{
        //    if (Keyboard.IsKeyDown(Key.X))
        //    {
        //        Canvas canvas = new Canvas();
        //        canvas.Width = 49;
        //        canvas.Background = new SolidColorBrush(Colors.Red);
        //        var k1 = KeyOverlay.Children[0] as Canvas;
        //
        //        k1.Children.Add(canvas);
        //
        //        Debug.WriteLine("i hate");
        //    }
        //
        //
        //    //Canvas? k1 = KeyOverlay.Children[0] as Canvas;
        //    //Canvas? k2 = KeyOverlay.Children[1] as Canvas;
        //    //if (Keyboard.IsKeyDown(Key.X))
        //    //{
        //    //    CurrentHold = new Canvas();
        //    //
        //    //    CurrentHold.Width = k1.ActualWidth;
        //    //    CurrentHold.Background = new SolidColorBrush(Colors.Red);
        //    //    //CurrentHold.RenderTransform = new RotateTransform(0);
        //    //
        //    //
        //    //    Canvas.SetLeft(CurrentHold, 0);
        //    //    Canvas.SetTop(CurrentHold, k1.ActualHeight - CurrentHold.Height);
        //    //    k1.Children.Add(CurrentHold);
        //    //}
        //    //
        //    //if (IsHold == true && KeyHold != Key.None && KeyHold == Key.X)
        //    //{
        //    //    CurrentHold.Height = CurrentHold.ActualHeight + 0.1;
        //    //}
        //    //
        //    //for (int i = 0; i < KeyPresses.Count; i++)
        //    //{
        //    //    Canvas? c = KeyPresses[i];
        //    //    Canvas.SetTop(c, Canvas.GetTop(c) - 0.25);
        //    //    if (Canvas.GetTop(c) < 0 || Double.IsNaN(Canvas.GetTop(c)))
        //    //    {
        //    //        k1.Children.Remove(c);
        //    //        KeyPresses.Remove(c);
        //    //    }
        //    //}
        //}
        //
        //
        //public static void InitializeTest()
        //{
        //    Window.KeyDown += Window_KeyDown;
        //    Window.KeyUp += Window_KeyUp;
        //}
        //
        //private static void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    IsHold = false;
        //    //KeyHold = Key.None;
        //
        //    CurrentHold = null;
        //}
        //
        //private static Canvas CurrentHold = new Canvas();
        //
        //// i fucking love when you have idea but then there is KeyDown event that doesnt work properly and has some weird delays
        //// so it makes entire idea go into trash and then figuring out for hours why it doesnt work
        //// i dont even need this i just wanted to try this stuff for fun but nooo in the end it was just waste of time
        //private static void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    // on click create new canvas children
        //    // while click is held stretch the added canva
        //    // if click is left then stop stretching
        //    // this is test implementation to make core thingy work
        //
        //    //KeyHold = e.Key;
        //
        //    IsHold = true;
        //    Canvas? k1 = KeyOverlay.Children[0] as Canvas;
        //    Canvas? k2 = KeyOverlay.Children[1] as Canvas;
        //    
        //    if (CurrentHold == null)
        //    {
        //        CurrentHold = new Canvas();
        //    }
        //
        //    CurrentHold.Width = k1.ActualWidth;
        //    CurrentHold.Background = new SolidColorBrush(Colors.Red);
        //    CurrentHold.RenderTransform = new RotateTransform(0);
        //
        //    if (!k1.Children.Contains(CurrentHold))
        //    {
        //        Canvas.SetLeft(CurrentHold, 0);
        //        Canvas.SetTop(CurrentHold, k1.ActualHeight - CurrentHold.Height);
        //        k1.Children.Add(CurrentHold);
        //        KeyPresses.Add(CurrentHold);
        //    }
        //}

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
