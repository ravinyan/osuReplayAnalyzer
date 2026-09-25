using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Mania;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Osu;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Taiko;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#nullable disable

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    // key overlay in style of what i saw one time on some osu streams which was https://github.com/Blondazz/KeyOverlay
    // also had idea to do this that way anyway coz its simple and easy to customize so oops
    public class KeyOverlay
    {
        public static Movable KeyOverlayUI = new Movable(Movable.Movables.KeyOverlayPosition, true);
        public static Grid KeyOverlayWindow = new Grid();

        private static double VELOCITY = 3.5;

        private static Stopwatch Cooldown = new Stopwatch();

        // mission rework this code so all game modes can use it
        private static int KeyCount = 0;
        private static List<Canvas> KeyColumns = new List<Canvas>();
        private static List<List<Canvas>> KeyPresses = new List<List<Canvas>>();
        private static List<bool> KeyPressStates = new List<bool>();

        private static Clicks[] PossibleClicks;

        public static void UpdateHoldPositions(bool isSeeking = false)
        {
            if (GamePlayClock.IsPaused() && isSeeking == false || KeyOverlayWindow.Visibility == Visibility.Collapsed
            ||  MainWindow.replay.FramesDict.Count == 0)
            {
                return;
            }

            if (Cooldown.ElapsedMilliseconds <= 1000 / 60.0)
            {
                return;
            }
            Cooldown.Restart();

            // early return here might not be needed... wait it does in case index goes out of bounds lol
            ReplayFrame frame;
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    if (CursorManager.CursorPositionIndex >= MainWindow.replay.FramesDict.Count)
                    {
                        return;
                    }
                    frame = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex];
                    break;
                case GameMode.OsuMania:
                    if (ManiaClickManager.ManiaFrameIndex >= MainWindow.replay.FramesDict.Count)
                    {
                        return;
                    }
                    frame = MainWindow.replay.FramesDict[ManiaClickManager.ManiaFrameIndex];
                    break;
                case GameMode.OsuTaiko:
                    if (TaikoClickManager.TaikoFrameIndex >= MainWindow.replay.FramesDict.Count)
                    {
                        return;
                    }
                    frame = MainWindow.replay.FramesDict[TaikoClickManager.TaikoFrameIndex + 1];
                    break;
                case GameMode.OsuCatch:
                    if (CatchCatcherManager.CatcherFrameIndex >= MainWindow.replay.FramesDict.Count)
                    {
                        return; 
                    }
                    frame = MainWindow.replay.FramesDict[CatchCatcherManager.CatcherFrameIndex];
                    break;
                default:
                    throw new Exception("how the f did you get here");
            }

            var a= MainWindow.replay.FramesDict.Values;

            // hmm i think i did a good job with this code?
            // for catch i think i will need to put key data movement into replay frames myself
            // since osu doesnt store them... or use X and Y?
            if (MainWindow.replay.GameMode == GameMode.OsuCatch)
            {
                // for now scuffed way to see if this works?
                if (KeyPressStates[0] == false && CatchPlayfield.CatcherDirectionLeft == true)
                {
                    KeyPressStates[0] = true;
                    KeyPresses[0].Add(CreateClickBar(KeyColumns[0]));
                    ChangeKeyButtonBackground((0 * 2) + 1, ColourBank.KeyOverlayClick);
                }
                else if (KeyPressStates[0] == true && CatchPlayfield.CatcherDirectionLeft == false)
                {
                    KeyPressStates[0] = false;
                    ChangeKeyButtonBackground((0 * 2) + 1, ColourBank.KeyOverlayButtonInactive);
                }

                if (KeyPressStates[1] == false && CatchPlayfield.CatcherDirectionRight == true)
                {
                    KeyPressStates[1] = true;
                    KeyPresses[1].Add(CreateClickBar(KeyColumns[1]));
                    ChangeKeyButtonBackground((1 * 2) + 1, ColourBank.KeyOverlayClick);
                }
                else if (KeyPressStates[1] == true && CatchPlayfield.CatcherDirectionRight == false)
                {
                    KeyPressStates[1] = false;
                    ChangeKeyButtonBackground((1 * 2) + 1, ColourBank.KeyOverlayButtonInactive);
                }

                if (KeyPressStates[2] == false && frame.Clicks.Contains(Clicks.M1))
                {
                    KeyPressStates[2] = true;
                    KeyPresses[2].Add(CreateClickBar(KeyColumns[2]));
                    ChangeKeyButtonBackground((2 * 2) + 1, ColourBank.KeyOverlayClick);
                }
                else if (KeyPressStates[2] == true && !frame.Clicks.Contains(Clicks.M1))
                {
                    KeyPressStates[2] = false;
                    ChangeKeyButtonBackground((2 * 2) + 1, ColourBank.KeyOverlayButtonInactive);
                }
            }
            else if (MainWindow.replay.GameMode == GameMode.OsuMania)
            {
                int K1Value = (int)Clicks.ManiaK1;
                for (int key = 0; key < KeyCount; key++)
                {
                    if (KeyPressStates[key] == false && frame.Clicks.Contains((Clicks)key + K1Value))
                    {
                        KeyPressStates[key] = true;
                        KeyPresses[key].Add(CreateClickBar(KeyColumns[key]));
                        ChangeKeyButtonBackground((key * 2) + 1, ColourBank.KeyOverlayClick);
                    }
                    else if (KeyPressStates[key] == true && !frame.Clicks.Contains((Clicks)key + K1Value))
                    {
                        KeyPressStates[key] = false;
                        ChangeKeyButtonBackground((key * 2) + 1, ColourBank.KeyOverlayButtonInactive);
                    }
                }
            }
            else
            {
                for (int key = 0; key < KeyCount; key++)
                {
                    if (KeyPressStates[key] == false && frame.Clicks.Contains(PossibleClicks[key]))
                    {
                        KeyPressStates[key] = true;
                        KeyPresses[key].Add(CreateClickBar(KeyColumns[key]));
                        ChangeKeyButtonBackground((key * 2) + 1, ColourBank.KeyOverlayClick);
                    }
                    else if (KeyPressStates[key] == true && !frame.Clicks.Contains(PossibleClicks[key]))
                    {
                        KeyPressStates[key] = false;
                        ChangeKeyButtonBackground((key * 2) + 1, ColourBank.KeyOverlayButtonInactive);
                    }
                }
            }

            for (int i = 0; i < KeyPressStates.Count; i++)
            {
                if (KeyPressStates[i] == true)
                {
                    StretchClickBar(i);
                }

                MoveClickBarsUp(KeyPresses[i], KeyColumns[i], isSeeking);
            }
        }

        public static Movable Create()
        {
            if (KeyOverlayUI.Children.Count > 0)
            {
                KeyOverlayUI.Dispose();
                KeyOverlayUI.Children.Remove(KeyOverlayWindow);
                KeyOverlayWindow = new Grid();
                KeyPresses.Clear();
                KeyPressStates.Clear();
                KeyColumns.Clear();
            }

            KeyOverlayWindow.Height = 242;

            double baseWidth        = 40;
            double paddingSize      = 5;
            double totalPaddingSize = 0;
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    KeyCount = 2;
                    PossibleClicks = [Clicks.K1, Clicks.K2];
                    totalPaddingSize = paddingSize * KeyCount;
                    KeyOverlayWindow.Width = (baseWidth * KeyCount) + totalPaddingSize + (KeyCount * 2);
                    for (int i = 0; i < KeyCount; i++)
                    {
                        AddKeyOverlayColumn(i, paddingSize, baseWidth);
                    }
                    break;
                case GameMode.OsuMania:
                    KeyCount = (int)MainWindow.map.Difficulty.CircleSize;
                    totalPaddingSize = paddingSize * KeyCount;
                    // make max width 200 + border size (2 pixels per 1 box, which is count * 2)
                    if ((baseWidth * KeyCount) + totalPaddingSize > 200)
                    {
                        baseWidth = (200 - totalPaddingSize) / (double)KeyCount;
                    }
                    KeyOverlayWindow.Width = (baseWidth * KeyCount) + totalPaddingSize + (KeyCount * 2);
                    for (int i = 0; i < KeyCount; i++)
                    {
                        AddKeyOverlayColumn(i, paddingSize, baseWidth);
                    }
                    break;
                case GameMode.OsuTaiko:
                    KeyCount = 4;
                    PossibleClicks = [Clicks.M1, Clicks.K1A, Clicks.M2, Clicks.K2A];
                    totalPaddingSize = paddingSize * KeyCount;
                    KeyOverlayWindow.Width = (baseWidth * KeyCount) + totalPaddingSize + (KeyCount * 2);
                    for (int i = 0; i < KeyCount; i++)
                    {
                        AddKeyOverlayColumn(i, paddingSize, baseWidth);
                    }
                    break;
                case GameMode.OsuCatch:
                    KeyCount = 3;
                    PossibleClicks = [Clicks.M1]; // there is only dash in replay data and it is always represent by M1
                    totalPaddingSize = paddingSize * KeyCount;
                    KeyOverlayWindow.Width = (baseWidth * KeyCount) + totalPaddingSize + (KeyCount * 2);
                    for (int i = 0; i < KeyCount; i++)
                    {
                        AddKeyOverlayColumn(i, paddingSize, baseWidth);
                    }
                    break;
                default:
                    throw new Exception("how the f did you get here");
            }

            Cooldown.Start();

            KeyOverlayUI.Children.Add(KeyOverlayWindow);
            KeyOverlayUI.Background = Brushes.Transparent;
            KeyOverlayUI.Width = KeyOverlayWindow.Width;
            KeyOverlayUI.Height = KeyOverlayWindow.Height;

            KeyOverlayUI.ApplyStartingPosition();

            return KeyOverlayUI;
        }

        private static void StretchClickBar(int index)
        {
            if (KeyPresses[index].Count > 0)
            {
                Canvas click = KeyPresses[index].LastOrDefault();
                click.Height = KeyColumns[index].Height - Canvas.GetTop(click);
            }
        }

        private static void MoveClickBarsUp(List<Canvas> clicks, Canvas column, bool isSeeking)
        {
            int count = clicks.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                Canvas click = clicks[i];
                Canvas.SetTop(click, Canvas.GetTop(click) - VELOCITY);
                if (Canvas.GetTop(click) + click.Height <= 0 || double.IsNaN(Canvas.GetTop(click)))
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
            canvas.Background = ColourBank.KeyOverlayClick;

            Canvas.SetLeft(canvas, 0);
            Canvas.SetTop(canvas, column.ActualHeight - canvas.Height);

            column.Children.Add(canvas);

            return canvas;
        }

        private static void CreateHoldDurationUI(Thickness margin, int col)
        {
            RowDefinition holdRow = new RowDefinition();
            holdRow.MaxHeight = 200;
            holdRow.Height = GridLength.Auto;

            Canvas keyHoldUI = new Canvas();
            keyHoldUI.Opacity = 0.7;
            keyHoldUI.Margin = margin;
            keyHoldUI.Height = 200;
            keyHoldUI.Background = ColourBank.KeyOverlayButtonInactive;
            keyHoldUI.ClipToBounds = true;

            KeyOverlayWindow.RowDefinitions.Add(holdRow);
            KeyOverlayWindow.Children.Add(keyHoldUI);

            // row 0 is on the top part, col 0/1 is left/right side
            Grid.SetRow(keyHoldUI, 0);
            Grid.SetColumn(keyHoldUI, col);
        }

        private static void CreateKeyButtonUI(string keyName, Thickness margin, int col, double keyDiameter)
        {
            ColumnDefinition keyCol = new ColumnDefinition();
            keyCol.Width = GridLength.Auto;

            TextBlock key = new TextBlock();
            key.Width = keyDiameter;
            key.Height = keyDiameter;
            key.Text = keyName;
            key.Foreground = new SolidColorBrush(Colors.White);
            key.TextAlignment = TextAlignment.Center;
            key.Padding = new Thickness(0, 11.5, 0, 0);
            key.VerticalAlignment = VerticalAlignment.Bottom;

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

        private static void ChangeKeyButtonBackground(int index, SolidColorBrush color)
        {
            Border leftButton = KeyOverlayWindow.Children[index] as Border;
            leftButton.Background = color;
        }

        private static void AddKeyOverlayColumn(int colIndex, double paddingSize, double width)
        {
            KeyPressStates.Add(false);
            KeyPresses.Add(new List<Canvas>());
            CreateHoldDurationUI(new Thickness(0, 0, paddingSize, 0), colIndex);
            KeyColumns.Add(KeyOverlayWindow.Children[KeyOverlayWindow.Children.Count - 1] as Canvas);
            CreateKeyButtonUI($"K{colIndex + 1}", new Thickness(0, 0, paddingSize, 0), colIndex, width);
        }
    }
}
