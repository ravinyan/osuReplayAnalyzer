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

        public static void ResetFields()
        {
            NextRepeatAt = 0;
            RepeatInterval = 0;
            CurrentReverseSlider = null;
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

                    // maybe here update visibility of reverse arrows when using song slider seeking?
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
                                Vector2 missPosition = IsBallDirectionReversed()
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
                            ChangeCurrentReverseArrowVisibility(s, Visibility.Collapsed);
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
                        ChangeSliderTickVisibility(s, Visibility.Collapsed);

                        NextRepeatAt -= RepeatInterval;
                        ReverseArrowIndex--;

                        ChangeCurrentReverseArrowVisibility(s, Visibility.Visible);
                    }
                }
            }
        }

        private static void ChangeCurrentReverseArrowVisibility(Slider s, Visibility visibility)
        {
            if (IsBallDirectionReversed())
            {
                Canvas head = Slider.Head(s);

                // +3 is to offset 4 other elements in slider head
                // hit circle > border > combo number > approach circle > reverse arrows
                int indx = ((int)Math.Ceiling(ReverseArrowIndex / 2.0)) + 3;
                if (indx > head.Children.Count - 1)
                {
                    indx = head.Children.Count - 1;
                }
                else if (indx <= 3)// first if might never be reached but this sadly is
                {
                    return;
                }

                // hide previous arrow, show only current one
                if (indx + 1 < head.Children.Count)
                {
                    head.Children[indx + 1].Visibility = visibility == Visibility.Collapsed
                                                       ? Visibility.Visible
                                                       : Visibility.Collapsed;
                }

                head.Children[indx].Visibility = visibility;
            }
            else
            {
                Canvas tail = Slider.Tail(s);

                int indx = (int)Math.Floor((ReverseArrowIndex) / 2.0);
                if (indx >= tail.Children.Count)
                {
                    indx = tail.Children.Count - 1;
                }

                // hide previous arrow, show only current one
                if (indx + 1 < tail.Children.Count)
                {
                    tail.Children[indx + 1].Visibility = visibility == Visibility.Collapsed 
                                                       ? Visibility.Visible
                                                       : Visibility.Collapsed;
                }
                tail.Children[indx].Visibility = visibility;
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
            if (IsBallDirectionReversed())
            {
                Canvas head = Slider.Head(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx == 0)
                {
                    indx = 1;
                }

                return head.Children[head.Children.Count - indx].Visibility == Visibility.Visible;
            }
            else
            {
                Canvas tail = Slider.Tail(s);

                int indx = (int)Math.Floor((ReverseArrowIndex - 1) / 2.0);
                if (indx >= tail.Children.Count)
                {
                    indx = tail.Children.Count - 1;
                }

                return tail.Children[indx].Visibility == Visibility.Visible;
            }
        }

        private static bool IsBallDirectionReversed()
        {// 0(even) = going backwards (reversed), 1(odd) = going forwards
            return ReverseArrowIndex % 2 == 0;
        }
    }
}
