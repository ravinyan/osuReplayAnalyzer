using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Slider = ReplayAnalyzer.Objects.Slider;

namespace ReplayAnalyzer.MusicPlayer.Controls
{
    public class RateChangerControls
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static Grid RateChangeWindow = new Grid();

        // unknown if wanted slider or just increments of 0.25x... depends on bugs i guess lol
        // but anyway min value will be 0.25x and max will be 2x
        private static System.Windows.Controls.Slider RateChangeSlider = new System.Windows.Controls.Slider();
        public static double RateChange = 1;

        private static OsuMath Math = new OsuMath();

        public static void InitializeEvents()
        {
            CreateRateChangeWindow();
            
            Window.rateChangeButton.Click += RateChangeButtonClick;
            
            Window.rateChangeButton.MouseEnter += VolumeButtonMouseEnter;
            Window.rateChangeButton.MouseLeave += VolumeButtonMouseLeave;

            ChangeBaseRate();
        }

        public static void ChangeBaseRate()
        {
            double modRateChange = 1;
            if (MainWindow.replay.ModsUsed.HasFlag(OsuFileParsers.Classes.Replay.Mods.DoubleTime))
            {
                modRateChange = 1.5;
            }
            else if (MainWindow.replay.ModsUsed.HasFlag(OsuFileParsers.Classes.Replay.Mods.HalfTime))
            {
                modRateChange = 0.75;
            }

            RateChangeSlider.Value = modRateChange;
            Window.rateChangeText.Text = $"{modRateChange}x";
        }

        public static void ChangeRateShortcut(int direction)
        {
            if (direction > 0)
            {
                RateChangeSlider.Value += 0.25;
            }
            else
            {
                RateChangeSlider.Value -= 0.25;
            }
        }

        private static void CreateRateChangeWindow()
        {
            RateChangeWindow.Visibility = Visibility.Collapsed;
            RateChangeWindow.Width = 200;
            RateChangeWindow.Height = 50;
            RateChangeWindow.Background = new SolidColorBrush(Color.FromRgb(57, 42, 54));

            CreateText();
            ApplyPropertiesToSlider();

            Window.ApplicationWindowUI.Children.Add(RateChangeWindow);
        }

        private static void RateChangeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Window.rateChangeText.Text = $"{(decimal)RateChangeSlider.Value}x";
            ChangeRate();
        }

        private static void ChangeRate()
        {
            RateChange = RateChangeSlider.Value;

            double ms = Math.GetApproachRateTiming();
            ms = ms / RateChange;
            double arMs = ms;

            Window.musicPlayer.MediaPlayer!.SetRate((float)RateChange);
            // without gives no audio and delayed audio change which is annoying
            // with hurts ears but works correctly...
            MusicPlayer.Seek(GamePlayClock.TimeElapsed);
                  
            foreach (var obj in HitObjectManager.GetAliveHitObjects())
            {
                HitObjectAnimations.Remove(obj);
                HitObjectAnimations.Start(obj);
            }

            foreach (var sb in HitObjectAnimations.sbDict)
            {
                if (sb.Key.Contains("Circle"))
                {
                    foreach (var sbChild in sb.Value)
                    {
                        if (sbChild.Name == "FadeIn")
                        {
                            sbChild.Children[0].SpeedRatio = RateChange;
                        }
                        else if (sbChild.Name == "ApproachCircle")
                        {
                           sbChild.Children[0].SpeedRatio = RateChange;
                           sbChild.Children[1].SpeedRatio = RateChange;
                        }
                    }
                }
                else if (sb.Key.Contains("Slider"))
                {
                    foreach (var sbChild in sb.Value)
                    {
                        if (sbChild.Name == "FadeIn")
                        {
                            sbChild.Children[0].SpeedRatio = RateChange;
                        }
                        else if (sbChild.Name == "ApproachCircle")
                        {
                            sbChild.Children[0].SpeedRatio = RateChange;
                            sbChild.Children[1].SpeedRatio = RateChange;
                        }
                        else
                        {
                            // number 15 is coz of SliderHitObject(index here) name to only extract the index portion
                            Slider? s = OsuBeatmap.HitObjectDictByIndex[int.Parse(sb.Key.Substring(15))] as Slider;

                            sbChild.Children[0].BeginTime = TimeSpan.FromMilliseconds(arMs);
                            sbChild.Children[0].SpeedRatio = RateChange * s.RepeatCount;
                        }
                    }
                }
                else
                {
                    sb.Value[0].Children[0].SpeedRatio = RateChange;
                    sb.Value[0].Children[1].SpeedRatio = RateChange;
                }
            }

            HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());  
        }

        private static void RateChangeButtonClick(object sender, RoutedEventArgs e)
        {
            if (RateChangeWindow.Visibility == Visibility.Visible)
            {
                RateChangeWindow.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (VolumeControls.VolumeWindow.Visibility == Visibility.Visible)
                {
                    VolumeControls.VolumeWindow.Visibility = Visibility.Collapsed;
                }

                Canvas.SetTop(RateChangeWindow, Window.Height - 140);
                Canvas.SetLeft(RateChangeWindow, Window.Width - 268);

                RateChangeWindow.Visibility = Visibility.Visible;
            }
        }

        // hover effect for feedback
        private static void VolumeButtonMouseLeave(object sender, MouseEventArgs e)
        {
            Window.rateChangeText.Foreground = new SolidColorBrush(Color.FromRgb(57, 42, 54));
        }

        // hover effect for feedback
        private static void VolumeButtonMouseEnter(object sender, MouseEventArgs e)
        {
            Window.rateChangeText.Foreground = new SolidColorBrush(Colors.White);
        }

        private static void CreateText()
        {
            RowDefinition text = new RowDefinition();
            text.Height = new GridLength(20);

            TextBlock textBlock = new TextBlock();
            textBlock.Height = 20;
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.Margin = new Thickness(9, 5, 0, 0);
            textBlock.Text = "Playback speed";

            RateChangeWindow.RowDefinitions.Add(text);
            RateChangeWindow.Children.Add(textBlock);
            Grid.SetRow(textBlock, 0);
        }

        private static void ApplyPropertiesToSlider()
        {
            RowDefinition slider = new RowDefinition();
            slider.Height = new GridLength(25);

            RateChangeSlider.Orientation = Orientation.Horizontal;
            RateChangeSlider.Width = 180;
            RateChangeSlider.Minimum = 0.25;
            RateChangeSlider.Maximum = 2.00;
            RateChangeSlider.TickFrequency = 0.01;
            RateChangeSlider.IsSnapToTickEnabled = true;
            RateChangeSlider.VerticalAlignment = VerticalAlignment.Center;
            RateChangeSlider.HorizontalAlignment = HorizontalAlignment.Center;
            RateChangeSlider.Margin = new Thickness(0, 3, 0, 0);
            RateChangeSlider.Style = Window.Resources["OptionsSliderStyle"] as Style;

            RateChangeSlider.ValueChanged += RateChangeSliderValueChanged;

            RateChangeWindow.RowDefinitions.Add(slider);
            RateChangeWindow.Children.Add(RateChangeSlider);
            Grid.SetRow(RateChangeSlider, 1);
        }
    }
}
