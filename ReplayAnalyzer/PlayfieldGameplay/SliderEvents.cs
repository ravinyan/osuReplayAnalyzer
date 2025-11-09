using ReplayAnalyzer.Analyser.UIElements;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.Skins;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.Objects.Slider;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    // maybe make the code look nicer later for this coz right now i feel like it looks horrible
    public class SliderEvents
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        /*
        public static void RemoveSliderHead(Canvas sliderHead)
        {
            // all slider head children that are hit circle
            for (int i = 0; i <= 3; i++)
            {
                sliderHead.Children[i].Visibility = Visibility.Collapsed;
            }

            // reverse arrow if exists will now be visible
            if (sliderHead.Children.Count > 4)
            {
                sliderHead.Children[4].Visibility = Visibility.Visible;
            }
        }

        private static bool SliderReversed = false;
        private static int TickIndex = 0;
        private static Slider CurrentSlider = null;
        private static int ReverseArrowTailIndex = 1;
        private static int ReverseArrowHeadIndex = 1;
        // improve this function and change how hitboxes work after seeking for reverse arrows done
        public static void UpdateSliderTicks()
        {
            if (AliveHitObjects.Count > 0)
            {
                Slider s = GetFirstSliderDataBySpawnTime();
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

                bool reversed = false;
                if (Math.Floor((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance) != 0)
                {
                    reversed = Math.Floor((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance) % 2 == 1 ? true : false;
                }

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

                // make reverse arrows count as slider ticks later < this is gonna be pain in the ass
                if (TickIndex >= 0 && TickIndex < s.SliderTicks.Length
                && (reversed == false && sliderCurrentPositionAt >= s.SliderTicks[TickIndex].PositionAt
                || reversed == true && sliderCurrentPositionAt <= s.SliderTicks[TickIndex].PositionAt))
                {
                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;

                    // ticks are starting at [3]
                    Image tick = body.Children[TickIndex + 3] as Image;
                    tick.Visibility = Visibility.Collapsed;

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;

                    Image hitboxBall = ball.Children[1] as Image;
                    double diameter = hitboxBall.Height * osuScale;

                    Point tickCentre = tick.TranslatePoint(new Point(tick.Width / 2, tick.Height / 2), Window.playfieldCanva);

                    double cursorX = MainWindow.replay.FramesDict[CursorPositionIndex - 1].X * osuScale;
                    double cursorY = MainWindow.replay.FramesDict[CursorPositionIndex - 1].Y * osuScale;

                    double objectX = tickCentre.X - (tick.Width / 2);
                    double objectY = tickCentre.Y - (tick.Height / 2);

                    double hitPosition = Math.Pow(cursorX - objectX, 2) + Math.Pow(cursorY - objectY, 2);
                    double circleRadius = Math.Pow(diameter / 2, 2);

                    if (hitPosition > circleRadius)
                    {
                        double tickDiameter = MainWindow.OsuPlayfieldObjectDiameter * 0.2;
                        HitJudgment hitJudgment = new HitJudgment(SkinElement.SliderTickMiss(), tickDiameter, tickDiameter);

                        hitJudgment.SpawnTime = (long)GamePlayClock.TimeElapsed;
                        hitJudgment.EndTime = hitJudgment.SpawnTime + 600;

                        Canvas.SetLeft(hitJudgment, objectX - hitJudgment.Width / 2);
                        Canvas.SetTop(hitJudgment, objectY - hitJudgment.Width / 2);

                        Window.playfieldCanva.Children.Add(hitJudgment);
                        AliveHitJudgements.Add(hitJudgment);
                    }

                    if (reversed == false && TickIndex < s.SliderTicks.Length)
                    {
                        TickIndex++;
                    }
                    else if (reversed == true)
                    {
                        TickIndex--;
                    }
                }
                else if (reversed == false && TickIndex > 0 && sliderCurrentPositionAt <= s.SliderTicks[TickIndex - 1].PositionAt)
                {
                    TickIndex--;

                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;

                    Image tick = body.Children[TickIndex + 3] as Image;
                    tick.Visibility = Visibility.Visible;

                    if (TickIndex == 0)
                    {
                        TickIndex = -1;
                    }
                }
                else if (reversed == true && TickIndex + 1 < s.SliderTicks.Length && sliderCurrentPositionAt >= s.SliderTicks[TickIndex + 1].PositionAt)
                {
                    TickIndex++;

                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;

                    Image tick = body.Children[TickIndex + 3] as Image;
                    tick.Visibility = Visibility.Visible;

                    if (TickIndex == 2)
                    {
                        TickIndex = 3;
                    }
                }
            }
        }

        private static double RepeatAt = 0;
        private static double RepeatInterval = 0;
        private static Slider CurrentReverseSlider = null;
        public static void UpdateSliderRepeats()
        {
            if (AliveHitObjects.Count > 0)
            {
                Slider s = GetFirstSliderDataBySpawnTime();
                if (s == null)
                {
                    return;
                }

                if (CurrentReverseSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    SliderReversed = false;
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

                        if (SliderReversed == false)
                        {
                            Canvas tail = s.Children[2] as Canvas;
                            tail.Children[tail.Children.Count - ReverseArrowTailIndex].Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            Canvas head = s.Children[1] as Canvas;
                            head.Children[head.Children.Count - ReverseArrowHeadIndex].Visibility = Visibility.Collapsed;
                        }

                        RepeatAt += RepeatInterval;

                        if (SliderReversed == false)
                        {
                            SliderReversed = true;
                            ReverseArrowTailIndex += 1;
                        }
                        else
                        {
                            SliderReversed = false;
                            ReverseArrowHeadIndex += 1;
                        }

                        // this is for resetting slider ticks once repeat arrow is hit
                        // tick children in slider body start at index 3
                        Canvas body = s.Children[0] as Canvas;
                        for (int i = 3; i < body.Children.Count; i++)
                        {
                            body.Children[i].Visibility = Visibility.Visible;
                        }
                    }
                    else if (progress < RepeatAt - RepeatInterval && RepeatAt >= 0 && progress >= 0)
                    {
                        if (SliderReversed == true)
                        {
                            Canvas tail = s.Children[2] as Canvas;
                            tail.Children[tail.Children.Count - ReverseArrowHeadIndex].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            Canvas head = s.Children[1] as Canvas;
                            try
                            {
                                head.Children[head.Children.Count - ReverseArrowTailIndex + 1].Visibility = Visibility.Visible;
                            }
                            catch { }// something borker when seeking backwards on repeat slider on toho map }

                        }

                        RepeatAt -= RepeatInterval;

                        if (SliderReversed == true && ReverseArrowTailIndex > 1)
                        {
                            SliderReversed = false;
                            ReverseArrowTailIndex -= 1;
                        }
                        else if (SliderReversed == false && ReverseArrowHeadIndex > 1)
                        {
                            SliderReversed = true;
                            ReverseArrowHeadIndex -= 1;
                        }

                        Canvas body = s.Children[0] as Canvas;
                        for (int i = 3; i < body.Children.Count; i++)
                        {
                            body.Children[i].Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        // this works but osu lazer doesnt have it done perfectly too... on seeking by frame it shows
        // slider end missed but while playing normally it wont show it... and it changes acc/combo too... its weird
        private static Slider CurrentSliderEndSlider = null;
        public static void HandleSliderEndJudgement()
        {
            if (AliveHitObjects.Count > 0)
            {
                Slider s = GetFirstSliderDataBySpawnTime();
                if (s == null)
                {
                    return;
                }

                if (s != CurrentSliderEndSlider)
                {
                    CurrentSliderEndSlider = s;
                    IsSliderEndHit = false;
                }

                if (s.EndTime - s.SpawnTime <= 36)
                {
                    IsSliderEndHit = true;
                    return;
                }
                else
                {
                    double minPosForMaxJudgement = 1 - 36 / (s.EndTime - s.SpawnTime);
                    double currentSliderBallPosition = (GamePlayClock.TimeElapsed - s.SpawnTime) / (s.EndTime - s.SpawnTime);

                    // if current position is lower than minimum position to get x300 on slider end then leave
                    // or if its already confirmed that slider end is hit also leave
                    if (currentSliderBallPosition < minPosForMaxJudgement || IsSliderEndHit == true)
                    {
                        return;
                    }

                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;
                    Image hitboxBall = ball.Children[1] as Image;

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    double hitboxBallWidth = hitboxBall.Width;
                    double hitboxBallHeight = hitboxBall.Height;

                    Point ballCentre = hitboxBall.TranslatePoint(new Point(hitboxBallWidth / 2, hitboxBallHeight / 2), Window.playfieldCanva);
                    double diameter = hitboxBallHeight * osuScale;

                    double ballX = ballCentre.X;
                    double ballY = ballCentre.Y;

                    System.Drawing.Drawing2D.GraphicsPath ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                    ellipse.AddEllipse((float)(ballX - diameter / 2), (float)(ballY - diameter / 2), (float)diameter, (float)diameter);

                    // cursor pos index - 1 coz its always ahead by one from incrementing at the end of cursor update
                    float cursorX = (float)(MainWindow.replay.FramesDict[CursorPositionIndex - 1].X * osuScale - Window.playfieldCursor.Width / 2);
                    float cursorY = (float)(MainWindow.replay.FramesDict[CursorPositionIndex - 1].Y * osuScale - Window.playfieldCursor.Width / 2);
                    System.Drawing.PointF pt = new System.Drawing.PointF(cursorX, cursorY);

                    if (ellipse.IsVisible(pt))
                    {
                        IsSliderEndHit = true;
                    }
                }
            }
        }

        private static Slider GetFirstSliderDataBySpawnTime()
        {
            Slider slider = null;

            foreach (HitObject obj in AliveHitObjects)
            {
                if (obj is not Slider)
                {
                    continue;
                }

                Slider s = obj as Slider;
                if (slider == null || slider.SpawnTime > s.SpawnTime)
                {
                    slider = s;
                }
            }

            return slider;
        }
        */
    }
}
