using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.HitObjects.Osu.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    public class SliderTick : SliderEvent
    {
        private static int TickIndex = 0;
        // this is where ticks are starting and i found it annoying changing it from 3 to 2
        // so i added this even tho i will never need to change this again... currently body path[0] > ball[1] > ticks[2..]
        private static int TickStartIndex = 2;
        private static Slider CurrentSlider = null;

        public static void ResetFields()
        {
            TickIndex = 0;
            CurrentSlider = null;
        }

        public static void UpdateSliderTicks(bool isPreloading = false)
        {
            Slider s = Slider.GetFirstSliderBySpawnTime();
            if (s == null || s.SliderTicks == null)
            {
                CurrentSlider = null;
                return;
            }

            if (CurrentSlider != s || s.Visibility == Visibility.Collapsed)
            {
                CurrentSlider = s;
                TickIndex = 0;
            }

            if (TickIndex >= 0 && TickIndex < s.SliderTicks.Count && GamePlayClock.TimeElapsed >= s.SliderTicks[TickIndex].Time)
            {
                Canvas body = Slider.Body(s);
                if (TickIndex + TickStartIndex >= body.Children.Count)
                {
                    return;
                }

                if (IsCursorOutsideBallHitbox(s))
                {
                    if (isPreloading == false)
                    {
                        Vector2 tickCentre = GetSliderTickPosition(s);
                        ShowMiss(tickCentre);
                    }
                    else
                    {
                        HitJudgementManager.ApplyJudgement(null, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, HitObjectJudgement.SliderTickMiss);
                        OsuSliderData slider = (OsuSliderData)HitObjectManager.TransformHitObjectToDataObject(s);
                        slider.AllTicksHit = false; 
                    }
                }

                Image tick = body.Children[TickIndex + TickStartIndex] as Image; // ticks are starting at [2]
                tick.Visibility = Visibility.Collapsed;

                TickIndex++;
            }

            if (TickIndex - 1 >= 0 && TickIndex - 1 < s.SliderTicks.Count
            &&  GamePlayClock.TimeElapsed <= s.SliderTicks[TickIndex - 1].Time)
            {
                ShowPreviousSliderTick(s);
                TickIndex--;
            }
        }

        public static void UpdateTicksVisibility(Slider s)
        {
            if (s.SliderTicks == null)
            {
                return;
            }

            TickIndex = 0;
            while (TickIndex >= 0 && TickIndex < s.SliderTicks.Count && GamePlayClock.TimeElapsed >= s.SliderTicks[TickIndex].Time)
            {
                TickIndex++;
            }

            CurrentSlider = s;

            // make everything collapsed then make only what is needed visible
            Canvas body = Slider.Body(s);
            for (int i = 0; i < s.SliderTicks.Count; i++)
            {
                body.Children[i + TickStartIndex].Visibility = Visibility.Collapsed;
            }

            int ticksInSlider = s.SliderTicks.Count / s.RepeatCount;
            int reverseArrowIndex = 1;
            for (int i = TickIndex; i > TickStartIndex; i -= ticksInSlider)
            {
                reverseArrowIndex++;
            }

            int remainder = Math.Abs(TickIndex - (reverseArrowIndex * ticksInSlider));
            for (int i = TickIndex; i < TickIndex + remainder; i++)
            {
                if (i + TickStartIndex >= body.Children.Count)
                {// this might never be needed coz math says numbers will always be exact but i dont trust math
                    break;
                }

                body.Children[i + TickStartIndex].Visibility = Visibility.Visible;
            }
        }

        private static Vector2 GetSliderTickPosition(Slider s)
        {
            Vector2 headPos = new Vector2((float)s.X, (float)s.Y);
            Vector2 tickPosInSlider = Vector2.Multiply((float)MainWindow.OsuPlayfieldObjectScale, s.SliderTicks[TickIndex].Position); 

            return headPos + tickPosInSlider;
        }

        private static void ShowPreviousSliderTick(Slider s)
        {
            Image tick = Slider.Body(s).Children[(TickIndex - 1) + TickStartIndex] as Image;
            tick.Visibility = Visibility.Visible;
        }
    }
}
