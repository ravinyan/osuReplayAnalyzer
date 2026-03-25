using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    // reverse arrow is basically slider tick with additional functionalities therefore doing this
    public class SliderReverseArrow : SliderTick
    {
        private static double NextRepeatAt = 0;
        private static double RepeatInterval = 0;

        private static Slider CurrentReverseSlider = null;

        private static bool IsSliderReversed = false;

        private static int ReverseArrowIndex = 1;

        public static void ResetFields()
        {
            NextRepeatAt = 0;
            RepeatInterval = 0;
            CurrentReverseSlider = null;
            IsSliderReversed = false;
            ReverseArrowIndex = 1;
        }

        public static void UpdateSliderRepeats(bool isPreloading = false)
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = Slider.GetFirstSliderBySpawnTime();
                if ((CurrentReverseSlider == null && s == null) || s == null)
                {
                    return;
                }

                if (CurrentReverseSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    IsSliderReversed = false;

                    CurrentReverseSlider = s;

                    ReverseArrowIndex = 1;

                    RepeatInterval = 1 / (double)s.RepeatCount;
                    NextRepeatAt = RepeatInterval;              
                }

                if (s.RepeatCount > 1)
                {
                    if (isPreloading == false && Slider.HeadHitCircle(s).Visibility == Visibility.Visible)
                    {
                        // when slider head shows up hide all arrows beneath it
                        HideReverseArrowsBeneathSliderHead(s);
                    }

                    double progress = (GamePlayClock.TimeElapsed - s.SpawnTime) / (s.EndTime - s.SpawnTime);
                    if (progress > NextRepeatAt && NextRepeatAt >= 0 && progress >= 0 && progress <= 1)
                    {
                        if (NextRepeatAt == 0)
                        {
                            NextRepeatAt = RepeatInterval;
                            return;
                        }

                        double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                        double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathDistance);
                        if (IsCursorOutsideBallHitbox(s, sliderBallProgress, MainWindow.OsuPlayfieldObjectScale) && IsReverseArrowVisible(s) == true)
                        {
                            if (isPreloading == false)
                            {
                                Vector2 missPosition = IsSliderReversed == true
                                                     ? new Vector2((float)s.X, (float)s.Y)
                                                     : s.EndPosition;
                                ShowMiss(missPosition);
                            }
                            else
                            {
                                HitJudgementManager.ApplyJudgement(null, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, -1);
                                SliderData slider = (SliderData)HitObjectManager.TransformHitObjectToDataObject(s);
                                slider.AllTicksHit = false;
                            }
                        }

                        if (isPreloading == false)
                        {
                            HideReverseArrow(s);
                        }

                        NextRepeatAt += RepeatInterval;
                        ReverseArrowIndex++;
                        
                        // dont show ticks when spawning slider backwards
                        // progress value should be higher than last reverse arrow and lower that 1
                        if (progress < 1 - RepeatInterval / 3)
                        {
                            ChangeSliderTickVisibility(s, Visibility.Visible);
                        }
                    }
                    else if ((progress < NextRepeatAt - RepeatInterval) && NextRepeatAt >= 0 && progress >= 0)
                    {
                        ShowReverseArrow(s);

                        ChangeSliderTickVisibility(s, Visibility.Collapsed);

                        NextRepeatAt -= RepeatInterval;
                        ReverseArrowIndex--;
                    }
                }
            }
        }

        private static void HideReverseArrow(Slider s)
        {  
            if (IsSliderReversed == false)
            {
                Canvas tail = Slider.Tail(s);
                
                int indx = (int)Math.Ceiling((ReverseArrowIndex - 1) / 2.0);
                if (indx >= tail.Children.Count)
                {
                    indx = tail.Children.Count - 1;
                }

                tail.Children[indx].Visibility = Visibility.Collapsed;

                IsSliderReversed = true;
            }
            else
            {
                Canvas head = Slider.Head(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx == 0)
                {
                    indx = 1;
                }

                head.Children[head.Children.Count - indx].Visibility = Visibility.Collapsed;

                IsSliderReversed = false;
            }
        }
        // this above and below maybe i can merge somehow if i improve the code into 1 good function
        private static void ShowReverseArrow(Slider s)
        {
            if (IsSliderReversed == false)
            {
                Canvas head = Slider.Head(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx > head.Children.Count - 4)
                {
                    // 4 index where child is reverse arrow, below are other always same circle children like approach circle
                    indx = head.Children.Count - 4;
                }

                head.Children[head.Children.Count - indx].Visibility = Visibility.Visible;

                IsSliderReversed = true;
            }
            else
            {
                Canvas tail = Slider.Tail(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx >= tail.Children.Count)
                {
                    indx = tail.Children.Count - 1;
                }

                tail.Children[indx].Visibility = Visibility.Visible;

                IsSliderReversed = false;
            }
        }

        private static void HideReverseArrowsBeneathSliderHead(Slider s)
        {
            Canvas head = Slider.Head(s);
            for (int i = 4; i < head.Children.Count; i++)
            {
                head.Children[i].Visibility = Visibility.Collapsed;
            }
        }

        private static void ChangeSliderTickVisibility(Slider s, Visibility visibility)
        {
            if (s.SliderTicks == null)
            {
                return;
            }

            Canvas body = Slider.Body(s);

            int ticksInSlider = s.SliderTicks.Count / s.RepeatCount;
            int startIndex = (ticksInSlider * (ReverseArrowIndex - 1)) + 3;
            for (int i = startIndex; i < startIndex + ticksInSlider; i++)
            {
                if (i >= body.Children.Count)
                {// this might never be needed coz math says numbers will always be exact but i dont trust math
                    break;
                }

                body.Children[i].Visibility = visibility;
            }
        }

        private static bool IsReverseArrowVisible(Slider s)
        {
            if (IsSliderReversed == false)
            {
                Canvas tail = Slider.Tail(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex - 1) / 2.0);
                if (indx >= tail.Children.Count)
                {
                    indx = tail.Children.Count - 1;
                }

                return tail.Children[indx].Visibility == Visibility.Visible;
            }
            else
            {
                Canvas head = Slider.Head(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx == 0)
                {
                    indx = 1;
                }

                return head.Children[head.Children.Count - indx].Visibility == Visibility.Visible;
            }
        }
    }
}
