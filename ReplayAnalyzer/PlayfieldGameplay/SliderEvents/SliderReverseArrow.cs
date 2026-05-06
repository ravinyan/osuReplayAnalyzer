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

        private static int ReverseArrowIndex = 1;

        private const double EPSILON = 1E-10;

        public static void ResetFields()
        {
            NextRepeatAt = 0;
            RepeatInterval = 0;
            CurrentReverseSlider = null;
            ReverseArrowIndex = 1;
        }

        public static void UpdateSliderRepeats(bool isPreloading = false)
        {
            Slider s = Slider.GetFirstSliderBySpawnTime();
            if (s == null || s.RepeatCount == 1)
            {
                CurrentReverseSlider = null;
                return;
            }

            if (CurrentReverseSlider != s || s.Visibility == Visibility.Collapsed)
            {
                InitializeFieldValues(s);
            }

            if (isPreloading == false && Slider.HeadHitCircleContainer(s).Visibility == Visibility.Visible)
            {
                // when slider head shows up hide all arrows beneath it
                HideReverseArrowsBeneathSliderHead(s);
            }
            
            double progress = (GamePlayClock.TimeElapsed - s.SpawnTime) / (s.EndTime - s.SpawnTime);
            if (progress > NextRepeatAt && NextRepeatAt >= 0 && progress >= 0 && progress <= 1)
            {
                if (NextRepeatAt > 0 && NextRepeatAt < EPSILON)
                {
                    // sometimes there is value like 1.1102230246251565E-16 caused by some floating point rounding error or whatever
                    // actually i dont remember if it broke something... i have feeling it did? if not bork dont fix
                    NextRepeatAt = RepeatInterval;
                    return;
                }

                if (IsCursorOutsideBallHitbox(s))
                {
                    if (isPreloading == false)
                    {
                        Vector2 missPosition = IsBallDirectionReversed()
                                             ? new Vector2((float)s.X, (float)s.Y)
                                             : s.EndPosition;
                        ShowMiss(missPosition);
                    }
                    else
                    {
                        HitJudgementManager.ApplyJudgement(null, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, HitObjectJudgement.SliderTickMiss);
                        SliderData slider = (SliderData)HitObjectManager.TransformHitObjectToDataObject(s);
                        slider.AllTicksHit = false;
                    }
                }

                if (isPreloading == false)
                {
                    ChangeCurrentReverseArrowVisibility(s, Visibility.Collapsed);
                }

                NextRepeatAt += RepeatInterval;
                ReverseArrowIndex++;

                // dont show ticks when spawning slider backwards
                // progress value should be higher than last reverse arrow and lower that 1
                // >>>>>>> this might be bad and have problems writing this just in case this has problems so i know <<<<<<<<
                if (progress < 1 - RepeatInterval / 3)
                {
                    ChangeSliderTickVisibility(s, Visibility.Visible);
                }
            }
            else if ((progress < NextRepeatAt - RepeatInterval) && NextRepeatAt >= 0 && progress > 0)
            {
                ChangeSliderTickVisibility(s, Visibility.Collapsed);

                NextRepeatAt -= RepeatInterval;
                ReverseArrowIndex = ReverseArrowIndex - 1 >= 1 // this should never reach 0
                                  ? ReverseArrowIndex - 1
                                  : 1;

                ChangeCurrentReverseArrowVisibility(s, Visibility.Visible);
            }
        }

        public static void UpdateReverseArrowsVisibility(Slider s)
        {
            // when seeking to new slider values need to be initalized anew to correctly update everything
            InitializeFieldValues(s);

            // make EVERYTHING collapsed
            Canvas head = Slider.Head(s);
            for(int i = 1; i < head.Children.Count; i++)
            {
                head.Children[i].Visibility = Visibility.Collapsed;
            }

            Canvas tail = Slider.Tail(s);
            for (int i = 0; i < tail.Children.Count; i++)
            {
                tail.Children[i].Visibility = Visibility.Collapsed;
            }

            // head container > reverse arrows
            int reverseArrowIndex = (int)Math.Ceiling(ReverseArrowIndex / 2.0);
            if (reverseArrowIndex < head.Children.Count)
            {
                head.Children[reverseArrowIndex].Visibility = Visibility.Visible;
            }
            
            reverseArrowIndex = (int)Math.Floor(ReverseArrowIndex / 2.0);
            if (reverseArrowIndex < tail.Children.Count)
            {
                tail.Children[reverseArrowIndex].Visibility = Visibility.Visible;
            }
        }

        private static void ChangeCurrentReverseArrowVisibility(Slider s, Visibility visibility)
        {
            if (IsBallDirectionReversed())
            {
                Canvas head = Slider.Head(s);

                // head container > reverse arrows
                int index = (int)Math.Ceiling(ReverseArrowIndex / 2.0);
                if (index + 1 < head.Children.Count)
                {
                    head.Children[index + 1].Visibility = visibility == Visibility.Collapsed
                                                       ? Visibility.Visible
                                                       : Visibility.Collapsed;
                }

                // just in case there might be something like progress(1) > NextRepeatAt(0.999999999998) which triggers exception
                // tho this exception kinda does nothing coz it doesnt crash the app and i dont think it lags it either
                if (index == head.Children.Count)
                {
                    index--;
                }

                head.Children[index].Visibility = visibility;
            }
            else
            {
                Canvas tail = Slider.Tail(s);

                int index = (int)Math.Floor(ReverseArrowIndex / 2.0);
                if (index + 1 < tail.Children.Count)
                {
                    tail.Children[index + 1].Visibility = visibility == Visibility.Collapsed 
                                                       ? Visibility.Visible
                                                       : Visibility.Collapsed;
                }
                if (index == tail.Children.Count)
                {
                    index--;
                }
                tail.Children[index].Visibility = visibility;
            }
        }

        private static void HideReverseArrowsBeneathSliderHead(Slider s)
        {
            Canvas head = Slider.Head(s);
            for (int i = 1; i < head.Children.Count; i++)
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
            int startIndex = (ticksInSlider * (ReverseArrowIndex - 1)) + 2;
            for (int i = startIndex; i < startIndex + ticksInSlider; i++)
            {
                if (i >= body.Children.Count)
                {// this might never be needed coz math says numbers will always be exact but i dont trust math
                    break;
                }

                body.Children[i].Visibility = visibility;
            }
        }

        private static bool IsBallDirectionReversed()
        {// 0(even) = going backwards (reversed), 1(odd) = going forwards
            return ReverseArrowIndex % 2 == 0;
        }

        private static void InitializeFieldValues(Slider s)
        {
            CurrentReverseSlider = s;

            ReverseArrowIndex = 1;

            RepeatInterval = 1 / (double)s.RepeatCount;
            NextRepeatAt = RepeatInterval;

            double progress = (GamePlayClock.TimeElapsed - s.SpawnTime) / (s.EndTime - s.SpawnTime);
            for (double i = RepeatInterval; i < progress; i += RepeatInterval)
            {
                NextRepeatAt += RepeatInterval;
                ReverseArrowIndex++;
            }
        }
    }
}
