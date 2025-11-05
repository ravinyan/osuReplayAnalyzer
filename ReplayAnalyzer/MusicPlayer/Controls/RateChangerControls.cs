using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay;
using System.Windows;
using System.Windows.Controls;
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
        private static OsuMath Math = new OsuMath();

        public static void InitializeEvents()
        {
            CreateRateChangeWindow();

            Window.rateChangeButton.Click += RateChangeButtonClick;

            Window.rateChangeButton.MouseEnter += VolumeButtonMouseEnter;
            Window.rateChangeButton.MouseLeave += VolumeButtonMouseLeave;

            Window.rateChangeText.Text = "1x";
            RateChangeSlider.Value = 1;

            ChangeRate();
        }

        private static void CreateRateChangeWindow()
        {
            RateChangeWindow.Visibility = Visibility.Collapsed;
            RateChangeWindow.Width = 200;
            RateChangeWindow.Height = 40;
            RateChangeWindow.Background = new SolidColorBrush(Color.FromRgb(57, 42, 54));

            RowDefinition sl = new RowDefinition();

            RateChangeSlider.Orientation = Orientation.Horizontal;
            RateChangeSlider.Width = 180;
            RateChangeSlider.Minimum = 0.25;
            RateChangeSlider.Maximum = 2.00;
            RateChangeSlider.TickFrequency = 0.01;
            RateChangeSlider.IsSnapToTickEnabled = true;
            RateChangeSlider.VerticalAlignment = VerticalAlignment.Center;
            RateChangeSlider.HorizontalAlignment = HorizontalAlignment.Center;

            RateChangeSlider.ValueChanged += RateChangeSliderValueChanged;

            RateChangeWindow.RowDefinitions.Add(sl);
            RateChangeWindow.Children.Add(RateChangeSlider);
            Grid.SetRow(RateChangeSlider, 0);

            Window.ApplicationWindowUI.Children.Add(RateChangeWindow);
        }

        private static void RateChangeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Window.rateChangeText.Text = $"{(decimal)RateChangeSlider.Value}x";
            ChangeRate();
        }

        // changes all animation values... use for rate change stuff when i figure things out
        public static double fd = 0;
        public static double ar = 0;
        public static double RateChange = 1;
        private static void ChangeRate()
        {
            fd = Math.GetFadeInTiming(MainWindow.map.Difficulty.ApproachRate);
            ar = Math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
            
            //Random rng = new Random();
            //RateChange = Math.Clamp(rng.NextDouble() + rng.NextDouble(), 0.5, 2);

            RateChange = RateChangeSlider.Value;

            Window.musicPlayer.MediaPlayer!.SetRate((float)RateChange);
            MusicPlayer.Seek(GamePlayClock.TimeElapsed);
            
            double ms = Math.GetApproachRateTiming(MainWindow.map.Difficulty!.ApproachRate);
            ms = ms / RateChange;
            ar = ms;
            fd = ms * 0.66; // fade time is 2/3 of total ar time

            // math taken from osu lazer... what even is this monstrocity of math
            double newAr = System.Math.Sign(ms - 1200) == System.Math.Sign(450 - 1200)
                         ? (ms - 1200) / (450 - 1200) * 5 + 5
                         : (ms - 1200) / (1200 - 1800) * 5 + 5;
            /*
            //newMapDifficulty.ApproachRate = (decimal)newAr;

            //double greatHitWindow = math.GetOverallDifficultyHitWindow300(map.Difficulty.OverallDifficulty);
            //greatHitWindow = greatHitWindow / 1.5;
            //
            //double newOD = Math.Sign(greatHitWindow - 50) == Math.Sign(20 - 50)
            //             ? (greatHitWindow - 50) / (20 - 50) * 5 + 5
            //             : (greatHitWindow - 50) / (50 - 80) * 5 + 5;
            //newMapDifficulty.OverallDifficulty = (decimal)newOD;
            */

            foreach (var obj in Playfield.GetAliveHitObjects())
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
                        sbChild.Stop();
                        if (sbChild.Name == "FadeIn")
                        {
                            //sbChild.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds(fd));

                            sbChild.Children[0].SpeedRatio = RateChange;
                        }
                        else if (sbChild.Name == "ApproachCircle")
                        {
                           ///sbChild.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds(ar));
                           ///sbChild.Children[1].Duration = new Duration(TimeSpan.FromMilliseconds(ar));

                           sbChild.Children[0].SpeedRatio = RateChange;
                           sbChild.Children[1].SpeedRatio = RateChange;

                           // sbChild.SetSpeedRatio(RateChange);
                        }
                    }
                }
                else if (sb.Key.Contains("Slider"))
                {
                    foreach (var sbChild in sb.Value)
                    {
                        if (sbChild.Name == "FadeIn")
                        {
                            //sbChild.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds(fd));

                            sbChild.Children[0].SpeedRatio = RateChange;
                        }
                        else if (sbChild.Name == "ApproachCircle")
                        {
                            //sbChild.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds(ar));
                            //sbChild.Children[1].Duration = new Duration(TimeSpan.FromMilliseconds(ar));

                            sbChild.Children[0].SpeedRatio = RateChange;
                            sbChild.Children[1].SpeedRatio = RateChange;
                        }
                        else
                        {
                            // number 15 is coz of SliderHitObject(index here) name to only extract the index portion
                            Slider? s = OsuBeatmap.HitObjectDictByIndex[int.Parse(sb.Key.Substring(15))] as Slider;
                            //sbChild.Children[0].Duration = new Duration(TimeSpan.FromMilliseconds((s.EndTime - s.SpawnTime) / RateChange) / s.RepeatCount);
                            //var t = sbChild.Children[0].BeginTime / RateChange;
                            sbChild.Children[0].BeginTime = TimeSpan.FromMilliseconds(ar);

                            sbChild.Children[0].SpeedRatio = RateChange * s.RepeatCount;
                        }
                    }
                }
                else
                {
                    //sb.Value[0].Children[0].Duration = new Duration(TimeSpan.FromMilliseconds(ar));
                    //sb.Value[0].Children[1].Duration = new Duration(TimeSpan.FromMilliseconds(ar));

                    sb.Value[0].Children[0].SpeedRatio = RateChange;
                    sb.Value[0].Children[1].SpeedRatio = RateChange;
                }
            }

            HitObjectAnimations.Seek(Playfield.GetAliveHitObjects());
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
        private static void VolumeButtonMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Window.rateChangeText.Foreground = new SolidColorBrush(Color.FromRgb(57, 42, 54));
        }

        // hover effect for feedback
        private static void VolumeButtonMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Window.rateChangeText.Foreground = new SolidColorBrush(Colors.White);
        }
    }
}
