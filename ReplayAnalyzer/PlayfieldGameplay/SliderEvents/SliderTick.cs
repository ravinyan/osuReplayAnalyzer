using ReplayAnalyzer.AnalyzerTools.UIElements;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Skins;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.Objects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    public class SliderTick
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static int TickIndex = 0;
        private static Slider CurrentSlider = null;
        
        public static void ResetFields()
        {
            TickIndex = 0;
            CurrentSlider = null;
        }

        // YOU ARE NEXT I WILL FIX YOU OR I DIE TRYING
        // slider ticks on seeking by slider and seeking by frame when there is i think(?) one tick have problems
        public static void UpdateSliderTicks()
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = HitObjectManager.GetFirstSliderDataBySpawnTime();
                if (s == null || s.SliderTicks == null)
                {
                    return;
                }

                if (CurrentSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    CurrentSlider = s;
                    TickIndex = 0;
                }

                double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;

                bool isReversed = IsSliderReversed(s, sliderPathDistance);
                double sliderCurrentPositionAt = GetCurrentSliderPosition(s, isReversed, sliderPathDistance);

                // make reverse arrows count as slider ticks later < this is gonna be pain in the ass
                if (TickIndex >= 0 && TickIndex < s.SliderTicks.Length
                && (isReversed == false && sliderCurrentPositionAt >= s.SliderTicks[TickIndex].PositionAt
                ||  isReversed == true && sliderCurrentPositionAt <= s.SliderTicks[TickIndex].PositionAt))
                {
                    double osuScale = MainWindow.OsuPlayfieldObjectScale;

                    double tickWidth = 0;
                    double ballDiameter = 0;
                    Point tickCentre = GetSliderTickPosition(s, osuScale, out tickWidth, out ballDiameter);

                    double cursorPosition = GetCursorPosition(tickCentre, tickWidth, osuScale);
                    double circleRadius = Math.Pow(ballDiameter / 2, 2);

                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;

                    // ticks are starting at [3]
                    Image tick = body.Children[TickIndex + 3] as Image;

                    if (cursorPosition > circleRadius && tick.Visibility == Visibility.Visible)
                    {
                        ShowMiss(tickCentre);
                    }

                    if (isReversed == false && TickIndex < s.SliderTicks.Length)
                    {
                        TickIndex++;
                    }
                    else if (isReversed == true)
                    {
                        TickIndex--;
                    }
                }
                else if (isReversed == false && TickIndex > 0 && sliderCurrentPositionAt <= s.SliderTicks[TickIndex - 1].PositionAt)
                {
                    TickIndex--;

                    ShowCurrentSliderTick(s);

                    if (TickIndex == 0)
                    {
                        TickIndex = -1;
                    }
                }
                else if (isReversed == true && TickIndex + 1 < s.SliderTicks.Length && sliderCurrentPositionAt >= s.SliderTicks[TickIndex + 1].PositionAt)
                {
                    TickIndex++;

                    ShowCurrentSliderTick(s);

                    if (TickIndex == 2)
                    {
                        TickIndex = 3;
                    }
                }
            }
        }

        public static void HidePastTicks(Slider s)
        {
            if (s.SliderTicks == null)
            {
                return;
            }

            Canvas body = s.Children[0] as Canvas;
            Canvas ball = body.Children[2] as Canvas;

            for (int i = s.SliderTicks.Count() - 1; i > TickIndex; i--)
            {
                Image tick = body.Children[i + 3] as Image;
                tick.Visibility = Visibility.Visible;
            }

            for (int i = TickIndex + 1; i >= 3; i--)
            {
                Image tick = body.Children[i] as Image;
                tick.Visibility = Visibility.Collapsed;
            }
        }

        private static bool IsSliderReversed(Slider s, double sliderPathDistance)
        {
            if (Math.Floor((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance) != 0)
            {
                return Math.Floor((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance) % 2 == 1 ? true : false;
            }

            return false;
        }

        private static double GetCurrentSliderPosition(Slider s, bool reversed, double sliderPathDistance)
        {
            double sliderCurrentPositionAt;
            if (reversed == true)
            {
                double sliderProgress = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
                while (sliderProgress > 1)
                {
                    sliderProgress -= 1;
                }
                sliderCurrentPositionAt = 1 - sliderProgress;

                if (TickIndex == s.SliderTicks.Length && sliderCurrentPositionAt <= s.SliderTicks[TickIndex - 1].PositionAt)
                {
                    TickIndex--;
                }
            }
            else
            {
                sliderCurrentPositionAt = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
                while (sliderCurrentPositionAt > 1)
                {
                    sliderCurrentPositionAt -= 1;
                }

                if (TickIndex == -1 && sliderCurrentPositionAt >= s.SliderTicks[TickIndex + 1].PositionAt)
                {
                    TickIndex = 0;
                }
            }

            return sliderCurrentPositionAt;
        }

        private static Point GetSliderTickPosition(Slider s, double osuScale, out double tickDiameter, out double ballDiameter)
        {
            Canvas body = s.Children[0] as Canvas;
            Canvas ball = body.Children[2] as Canvas;

            // ticks are starting at [3]
            Image tick = body.Children[TickIndex + 3] as Image;
            tick.Visibility = Visibility.Collapsed;

            Image hitboxBall = ball.Children[1] as Image;
            double diameter = hitboxBall.Height * osuScale;

            tickDiameter = tick.Width;
            ballDiameter = diameter;

            return tick.TranslatePoint(new Point(tick.Width / 2, tick.Height / 2), Window.playfieldCanva);
        }

        private static double GetCursorPosition(Point tickCentre, double tickDiameter, double osuScale)
        {
            double cursorX = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].X * osuScale - (Window.playfieldCursor.Width / 2);
            double cursorY = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Y * osuScale - (Window.playfieldCursor.Width / 2);
            
            double objectX = tickCentre.X - (tickDiameter / 2);
            double objectY = tickCentre.Y - (tickDiameter / 2);

            return Math.Pow(cursorX - objectX, 2) + Math.Pow(cursorY - objectY, 2);
        }

        private static void ShowMiss(Point tickCentre)
        {
            double tickDiameter = MainWindow.OsuPlayfieldObjectDiameter * 0.2;

            float X = (float)(tickCentre.X - (tickDiameter / 2));
            float Y = (float)(tickCentre.Y - (tickDiameter / 2));

            HitJudgementManager.ApplyJudgement(null, new System.Numerics.Vector2(X, Y), (long)GamePlayClock.TimeElapsed, -1);
        }

        private static void ShowCurrentSliderTick(Slider s)
        {
            Canvas body = s.Children[0] as Canvas;
            Canvas ball = body.Children[2] as Canvas;

            Image tick = body.Children[TickIndex + 3] as Image;
            tick.Visibility = Visibility.Visible;
        }
    }
}
