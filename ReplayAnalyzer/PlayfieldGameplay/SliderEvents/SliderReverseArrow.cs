using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    public class SliderReverseArrow
    {
        private static double RepeatAt = 0;
        private static double RepeatInterval = 0;

        private static Slider CurrentReverseSlider = null;

        private static bool IsSliderReversed = false;

        private static int ReverseArrowIndex = 1;

        public static void ResetFields()
        {
            RepeatAt = 0;
            RepeatInterval = 0;
            CurrentReverseSlider = null;
            IsSliderReversed = false;
            ReverseArrowIndex = 1;
        }

        public static void UpdateSliderRepeats()
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = Slider.GetFirstSliderBySpawnTime();
                
                if (CurrentReverseSlider == null && s == null)
                {
                    return;
                }

                if (CurrentReverseSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    IsSliderReversed = false;

                    CurrentReverseSlider = s;

                    ReverseArrowIndex = 1;

                    RepeatInterval = s != null ? 1 / (double)s.RepeatCount : 1;
                    RepeatAt = RepeatInterval;

                    if (s == null)
                    {
                        return;
                    }                 
                }

                if (s.RepeatCount > 1)
                {
                    Canvas head = s.Children[1] as Canvas;
                    if (head.Children[1].Visibility == Visibility.Visible)
                    {
                        // when slider head shows up hide all arrows beneath it
                        HideReverseArrowsBeneathSliderHead(s);
                    }

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
                        ReverseArrowIndex++;
                        
                        // dont show ticks when spawning slider backwards
                        // progress value should be higher than last reverse arrow and lower that 1
                        if (progress < 1 - RepeatInterval / 3)
                        {
                            ChangeSliderTickVisibility(s, Visibility.Visible);
                        }
                    }
                    else if ((progress < RepeatAt - RepeatInterval) 
                         &&  RepeatAt >= 0 && progress >= 0)
                    {
                        ShowReverseArrow(s);

                        RepeatAt -= RepeatInterval;
                        ReverseArrowIndex--;

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
                Canvas head = s.Children[1] as Canvas;

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx == 0)
                {
                    indx = 1;
                }

                head.Children[head.Children.Count - indx].Visibility = Visibility.Collapsed;

                IsSliderReversed = false;
            }
        }

        private static void ShowReverseArrow(Slider s)
        {
            if (IsSliderReversed == false)
            {
                Canvas head = s.Children[1] as Canvas;

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
                Canvas tail = s.Children[2] as Canvas;

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
            Canvas head = s.Children[1] as Canvas;
            for (int i = 4; i < head.Children.Count; i++)
            {
                head.Children[i].Visibility = Visibility.Collapsed;
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
