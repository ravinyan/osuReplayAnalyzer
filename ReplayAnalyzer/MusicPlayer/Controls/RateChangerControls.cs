using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Slider = ReplayAnalyzer.HitObjects.Slider;

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

        public static void ResetFields()
        {
            RateChange = 1;
            Window.rateChangeText.Text = "1x";
            RateChangeSlider.Value = 1;
        }

        public static void InitializeEvents()
        {
            CreateRateChangeWindow();
            
            Window.rateChangeButton.Click += RateChangeButtonClick;
            
            Window.rateChangeButton.MouseEnter += VolumeButtonMouseEnter;
            Window.rateChangeButton.MouseLeave += VolumeButtonMouseLeave;

            RateChangeSlider.MouseEnter += delegate (object sender, MouseEventArgs e)
            {
                RateChangeSlider.Focusable = true;
                RateChangeSlider.Focus();
            };

            RateChangeSlider.MouseLeave += delegate (object sender, MouseEventArgs e)
            {
                RateChangeSlider.Focusable = false;
            };

            ChangeBaseRate(1);
        }

        public static void ChangeBaseRate(double value)
        {
            RateChangeSlider.Value = value;
            Window.rateChangeText.Text = $"{value}x";

            ChangeRate();
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

            Canvas.SetZIndex(RateChangeWindow, 10000);

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

            MusicPlayer.Seek(GamePlayClock.TimeElapsed);
            MusicPlayer.ChangeMusicRate((float)RateChange);

            foreach (HitObject obj in HitObjectManager.GetAliveHitObjects())
            {
                HitObjectAnimations.Remove(obj);
                HitObjectAnimations.RemoveStoryboardFromDict(obj);

                if (obj is HitCircle)
                {
                    HitObjectAnimations.ApplyHitCircleAnimations(obj as HitCircle);
                }
                else if (obj is Slider)
                {
                    HitObjectAnimations.ApplySliderAnimations(obj as Slider);
                }
                else
                {
                    HitObjectAnimations.ApplySpinnerAnimations(obj as Spinner);
                }
                    
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
                        if (sbChild.Children == null)
                        {
                            return;
                        }

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
                            //Slider? s = OsuBeatmap.HitObjectDictByIndex[int.Parse(sb.Key.Substring(15))] as Slider;
                            Slider? s = HitObjectManager.GetAliveHitObjects().First(o => o.Name == sb.Key) as Slider;

                            sbChild.Children[0].BeginTime = TimeSpan.FromMilliseconds(arMs);
                            sbChild.Children[0].SpeedRatio = RateChange * s.RepeatCount;
                        }
                    }
                }
                else // spinny (aka spinner but with 6 letters to match circle and slider length)
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
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;

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
            RateChangeSlider.SmallChange = 0.01;
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
