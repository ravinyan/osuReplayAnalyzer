using ReplayAnalyzer.GameClock;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.Objects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    public class SliderReverseArrow
    {
        private static double RepeatAt = 0;
        private static double RepeatInterval = 0;

        private static Slider CurrentReverseSlider = null;

        private static bool IsSliderReversed = false;

        private static int ReverseArrowTailIndex = 1;
        private static int ReverseArrowHeadIndex = 1;

        public static void ResetFields()
        {
            RepeatAt = 0;
            RepeatInterval = 0;
            CurrentReverseSlider = null;
            IsSliderReversed = false;
            ReverseArrowTailIndex = 1;
            ReverseArrowHeadIndex = 1;
        }

        public static void UpdateSliderRepeats()
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = HitObjectManager.GetFirstSliderDataBySpawnTime();
                if (s == null)
                {
                    return;
                }

                if (CurrentReverseSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    IsSliderReversed = false;

                    CurrentReverseSlider = s;

                    ReverseArrowTailIndex = 1;
                    ReverseArrowHeadIndex = 1;

                    RepeatInterval = 1 / (double)s.RepeatCount;
                    RepeatAt = RepeatInterval;
                }

                if (s.RepeatCount > 1)
                {
                    double progress = (GamePlayClock.TimeElapsed - s.SpawnTime) / (s.EndTime - s.SpawnTime);
                    if (progress > RepeatAt && RepeatAt >= 0 && progress >= 0)
                    {
                        if (RepeatAt == 0)
                        {
                            RepeatAt = RepeatInterval;
                            return;
                        }

                        HideReverseArrow(s);

                        RepeatAt += RepeatInterval;

                        ChangeSliderTickVisibility(s, Visibility.Visible);
                    }
                    else if (progress < RepeatAt - RepeatInterval && RepeatAt >= 0 && progress >= 0)
                    {
                        ShowReverseArrow(s);

                        RepeatAt -= RepeatInterval;

                        ChangeSliderTickVisibility(s, Visibility.Collapsed);
                    }
                }
            }
        }

        private static void HideReverseArrow(Slider s)
        {
            if (IsSliderReversed == false)
            {
                Canvas tail = s.Children[2] as Canvas;
                tail.Children[tail.Children.Count - ReverseArrowTailIndex].Visibility = Visibility.Collapsed;

                IsSliderReversed = true;
                ReverseArrowTailIndex += 1;
            }
            else
            {
                Canvas head = s.Children[1] as Canvas;
                head.Children[head.Children.Count - ReverseArrowHeadIndex].Visibility = Visibility.Collapsed;

                IsSliderReversed = false;
                ReverseArrowHeadIndex += 1;
            }
        }

        private static void ShowReverseArrow(Slider s)
        {
            if (IsSliderReversed == false)
            {
                Canvas head = s.Children[1] as Canvas;
                try
                {
                    head.Children[head.Children.Count - ReverseArrowTailIndex + 1].Visibility = Visibility.Visible;
                }
                catch { }// something borker when seeking backwards on repeat slider on toho map }

                if (ReverseArrowHeadIndex > 1)
                {
                    IsSliderReversed = true;
                    ReverseArrowHeadIndex -= 1;
                }
            }
            else
            {
                Canvas tail = s.Children[2] as Canvas;
                tail.Children[tail.Children.Count - ReverseArrowHeadIndex].Visibility = Visibility.Visible;

                if (ReverseArrowTailIndex > 1)
                {
                    IsSliderReversed = false;
                    ReverseArrowTailIndex -= 1;
                }
            }
        }

        private static void ChangeSliderTickVisibility(Slider s, Visibility visibility)
        {
            Canvas body = s.Children[0] as Canvas;
            for (int i = 3; i < body.Children.Count; i++)
            {
                body.Children[i].Visibility = visibility;
            }
        }
    }
}
