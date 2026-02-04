using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Slider = ReplayAnalyzer.HitObjects.Slider;

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

        public static void UpdateSliderTickPreload()
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
                double sliderCurrentPositionAt = GetCurrentSliderPosition(s, false, sliderPathDistance);

                // make reverse arrows count as slider ticks later < this is gonna be pain in the ass
                if (TickIndex >= 0 && TickIndex < s.SliderTicks.Length
                &&  sliderCurrentPositionAt >= s.SliderTicks[TickIndex].PositionAt)
                {
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;
                    double osuScale = MainWindow.OsuPlayfieldObjectScale;

                    Image tick = new Image()
                    {
                        Width = diameter * 0.25,
                        Height = diameter * 0.25,
                    };
                    Canvas.SetLeft(tick, s.SliderTicks[TickIndex].Position.X * osuScale - tick.Width / 2);
                    Canvas.SetTop(tick, s.SliderTicks[TickIndex].Position.Y * osuScale - tick.Width / 2);

                    var ball = s.Children[0] as Canvas;
                    ball.Width = 1;
                    ball.Height = 1;
                    ball.Children.Add(tick);

                    var a = Canvas.GetLeft(tick);
                    var b = Canvas.GetTop(tick);

                    // figure out how to count slider ticks on pre load and dont use UI elements if possible to make it fast\
                    // WHY PLAYFIELDCANVA NO WORKS AAAAAAAA
                    Window.playfieldCanva.UpdateLayout();
                    Point tickCentre = tick.TranslatePoint(new Point(tick.Width / 2, tick.Height / 2), Window.playfieldCanva);

                    double cursorPosition = GetCursorPosition(tickCentre, tick.Width, osuScale);
                    //                      set diameter of slider ball hitbox
                    double circleRadius = Math.Pow((diameter * 2.4) / 2, 2);
                    if (cursorPosition > circleRadius && tick.Visibility == Visibility.Visible)
                    {
                        ShowMiss(tickCentre, s);
                    }

                    tick.Visibility = Visibility.Collapsed;

                    if (TickIndex < s.SliderTicks.Length)
                    {
                        TickIndex++;
                    }
                }
            }
        }

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

                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;

                    // ticks are starting at [3]
                    Image tick = body.Children[TickIndex + 3] as Image;

                    var a = Canvas.GetLeft(tick);
                    var b = Canvas.GetTop(tick);

                    // figure out how to count slider ticks on pre load and dont use UI elements if possible to make it fast
                    double tickWidth = 0;
                    double ballDiameter = 0;
                    Point tickCentre = GetSliderTickPosition(s, osuScale, ball, tick, out tickWidth, out ballDiameter);
                    
                    double cursorPosition = GetCursorPosition(tickCentre, tickWidth, osuScale);
                    double circleRadius = Math.Pow(ballDiameter / 2, 2);
                    if (cursorPosition > circleRadius && tick.Visibility == Visibility.Visible)
                    {
                        ShowMiss(tickCentre, s);
                    }

                    tick.Visibility = Visibility.Collapsed;

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

            // i know this is a bit stupid but it works for now
            // update TickIndex to be correct
            for (int i = 0; i < s.SliderTicks.Length - 1; i++)
            {
                UpdateSliderTicks();
            }

            // reset visibility of all ticks to visible
            for (int i = 3; i < 3 + s.SliderTicks.Length; i++)
            {
                Image tick = body.Children[i] as Image;
                if (tick == null)
                {
                    continue;
                }
            
                tick.Visibility = Visibility.Visible;
            }

            double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            bool isReversed = IsSliderReversed(s, sliderPathDistance);

            if (isReversed == false)
            {
                for (int i = 3; i < TickIndex + 3; i++)
                {
                    if (i > body.Children.Count - 1)
                    {
                        continue;
                    }

                    Image tick = body.Children[i] as Image;
                    tick.Visibility = Visibility.Collapsed;
                }
            }
            else
            { 
                for (int i = s.SliderTicks.Length + 2; i > 3 + TickIndex; i--)
                {
                    Image tick = body.Children[i] as Image;
                    if (tick == null)
                    {
                        continue;
                    }

                    tick.Visibility = Visibility.Collapsed;
                }
            }
        }

        // i do not know what im doing

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

        private static Point GetSliderTickPosition(Slider s, double osuScale, Canvas ball, Image tick, out double tickDiameter, out double ballDiameter)
        {
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

        private static void ShowMiss(Point tickCentre, Slider s)
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
