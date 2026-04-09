using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    public class SliderTick
    {
        private static int TickIndex = 0;
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
                if (TickIndex + 3 >= body.Children.Count)
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
                        SliderData slider = (SliderData)HitObjectManager.TransformHitObjectToDataObject(s);
                        slider.AllTicksHit = false; 
                    }
                }

                Image tick = body.Children[TickIndex + 3] as Image; // ticks are starting at [3]
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
                body.Children[i + 3].Visibility = Visibility.Collapsed;
            }

            int ticksInSlider = s.SliderTicks.Count / s.RepeatCount;
            int reverseArrowIndex = 1;
            for (int i = TickIndex; i > 3; i -= ticksInSlider)
            {
                reverseArrowIndex++;
            }

            int remainder = Math.Abs(TickIndex - (reverseArrowIndex * ticksInSlider));
            for (int i = TickIndex; i < TickIndex + remainder; i++)
            {
                if (i + 3 >= body.Children.Count)
                {// this might never be needed coz math says numbers will always be exact but i dont trust math
                    break;
                }

                body.Children[i + 3].Visibility = Visibility.Visible;
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
            Image tick = Slider.Body(s).Children[(TickIndex - 1) + 3] as Image;
            tick.Visibility = Visibility.Visible;
        }

        // here are functions for this and other slider events... wanted to make separate class for this but this makes more
        // sense for me since slider tick is correlated to reverse arrows and slider ends (as in how all things are calculated)
        protected static void ShowMiss(Vector2 objectPosition)
        {
            double missDiameter = MainWindow.OsuPlayfieldObjectDiameter * 0.2;

            float X = (float)(objectPosition.X - (missDiameter / 2));
            float Y = (float)(objectPosition.Y - (missDiameter / 2));

            HitJudgementManager.ApplyJudgement(null, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, HitObjectJudgement.SliderTickMiss);
        }

        protected static double GetSliderBallProgressPosition(double sliderSpawnTime, double sliderPathDistance)
        {
            double sliderBallPosition = (GamePlayClock.TimeElapsed - sliderSpawnTime) / sliderPathDistance;
            if (sliderBallPosition < 0)
            {
                sliderBallPosition = 0;
            }

            double overflowPosition = sliderBallPosition - (int)sliderBallPosition;
            if ((int)sliderBallPosition % 2 == 1)
            {
                return sliderBallPosition = 1 - overflowPosition;
            }

            return overflowPosition;
        }

        protected static bool IsCursorOutsideBallHitbox(Slider s)
        {
            double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathDistance);

            double osuScale = MainWindow.OsuPlayfieldObjectScale;
            Vector2 ballCentre = GetSliderBallPosition(s, sliderBallProgress, osuScale);
            double cursorPosition = GetCursorPositionInObject(ballCentre, osuScale);

            // slider ball when it is not tracked have smaller hitbox (as in just normal hit circle hitbox)
            // when it is tracked then it expands its radius and when tracking is lost then it gets smaller again
            double ballRadius = SliderEndJudgement.IsTracking == true
                              ? Slider.BallHitboxDiameter / 2
                              : MainWindow.OsuPlayfieldObjectDiameter / 2;

            return cursorPosition == -1 || cursorPosition > (ballRadius * ballRadius);
        }

        protected static Vector2 GetSliderBallPosition(Slider s, double sliderBallProgress, double osuScale)
        {
            Vector2 headPos = new Vector2((float)s.X, (float)s.Y);
            Vector2 sliderBallPosInSlider = Vector2.Multiply((float)osuScale, s.Path.PositionAt(sliderBallProgress));

            return headPos + sliderBallPosInSlider;
        }

        protected static double GetCursorPositionInObject(Vector2 objectCentre, double osuScale)
        {
            ReplayFrame cursorFrame = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1];
            if (cursorFrame.Click == 0 || cursorFrame.Click == Clicks.Smoke)
            {
                return -1;
            }

            double cursorX = cursorFrame.X * osuScale;
            double cursorY = cursorFrame.Y * osuScale;

            // math.pow sucks for performance but at least it looks kinda clean... ?
            return Math.Pow(cursorX - objectCentre.X, 2) + Math.Pow(cursorY - objectCentre.Y, 2);
        }
    }
}

// i dont want to delete this my brain was overworking when i figured this out i hate math
//protected static bool IsSliderReversed(int sliderSpawnTime, double sliderPathLength)
//{
//    return Math.Floor((GamePlayClock.TimeElapsed - sliderSpawnTime) / sliderPathLength) % 2 == 1;
//}
