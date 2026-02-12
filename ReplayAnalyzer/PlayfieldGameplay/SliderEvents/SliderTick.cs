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
                Slider s = Slider.GetFirstSliderBySpawnTime();
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
                double sliderCurrentPositionAt = GetCurrentSliderPosition(s, isReversed, sliderPathDistance, true);

                // make reverse arrows count as slider ticks later < this is gonna be pain in the ass
                if (TickIndex >= 0 && TickIndex < s.SliderTicks.Length
                && (isReversed == false && sliderCurrentPositionAt >= s.SliderTicks[TickIndex].PositionAt
                ||  isReversed == true && sliderCurrentPositionAt <= s.SliderTicks[TickIndex].PositionAt))
                {
                    double osuScale = MainWindow.OsuPlayfieldObjectScale;

                    Vector2 tickCentre = GetSliderTickPosition(s, osuScale);
                    double cursorPosition = GetCursorPosition(tickCentre, osuScale);
                    //                      set diameter of slider ball hitbox
                    double circleRadius = Math.Pow((MainWindow.OsuPlayfieldObjectDiameter * 2.4) / 2, 2);
                    if (cursorPosition == -1 || cursorPosition > circleRadius)
                    {
                        HitJudgementManager.ApplyJudgement(null, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, -1);
                        SliderData slider = (SliderData)HitObjectManager.TransformHitObjectToDataObject(s);
                        slider.AllTicksHit = false;
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
            }
        }

        public static void UpdateSliderTicks(bool updateAfterSeek = false)
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = Slider.GetFirstSliderBySpawnTime();
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
                double sliderCurrentPositionAt = GetCurrentSliderPosition(s, isReversed, sliderPathDistance, updateAfterSeek);

                double sliderBallPosition = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
                if (sliderBallPosition < 0)
                {
                    return;
                }

                while (sliderBallPosition > 1)
                {
                    sliderBallPosition = sliderBallPosition - 1;
                }

                double tickPositionAt = 0;
                double reverseTickPositionAt = 0;
                if (isReversed == false)
                {
                    if (TickIndex == -1)
                    {
                        TickIndex = 0;
                        tickPositionAt = s.SliderTicks[TickIndex].PositionAt;
                    }
                    else if (TickIndex >= s.SliderTicks.Length)
                    {
                        TickIndex = s.SliderTicks.Length - 1;
                        tickPositionAt = s.SliderTicks[TickIndex].PositionAt;
                    }
                    else
                    {
                        tickPositionAt = s.SliderTicks[TickIndex].PositionAt;
                    }
                }
                else
                {
                    

                    if (TickIndex == -1)
                    {
                        TickIndex = 0;
                    }
                    else if (TickIndex >= s.SliderTicks.Length - 1)
                    {
                        TickIndex = s.SliderTicks.Length - 1;
                    }

                    int max = s.SliderTicks.Length - 1;
                    int cur = TickIndex;
                    int reverseCur = max - cur;
                    reverseTickPositionAt = 1 - s.SliderTicks[reverseCur].PositionAt;

                    tickPositionAt = 1 - s.SliderTicks[TickIndex].PositionAt;  
                }

                // make reverse arrows count as slider ticks later < this is gonna be pain in the ass < kill me
                if (TickIndex >= 0 && TickIndex < s.SliderTicks.Length
                && (isReversed == false && sliderBallPosition >= tickPositionAt
                ||  isReversed == true && sliderBallPosition >= tickPositionAt))
                {
                    double osuScale = MainWindow.OsuPlayfieldObjectScale;

                    Canvas body = s.Children[0] as Canvas;
                    if (TickIndex + 3 >= body.Children.Count)
                    {
                        return;
                    }
                     Image tick = body.Children[TickIndex + 3] as Image; // ticks are starting at [3]

                    Vector2 tickCentre = GetSliderTickPosition(s, osuScale);
                    double cursorPosition = GetCursorPosition(tickCentre, osuScale);
                    //                                  set diameter of slider ball hitbox
                    double circleRadius = Math.Pow((MainWindow.OsuPlayfieldObjectDiameter * 2.4) / 2, 2);
                    if ((cursorPosition == -1 || cursorPosition > circleRadius) && tick.Visibility == Visibility.Visible)
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
                if (updateAfterSeek == true && isReversed == false && TickIndex >= 0 
                &&  sliderBallPosition <= tickPositionAt)
                {
                    ShowCurrentSliderTick(s);
                    TickIndex--;
                    
                    if (TickIndex < 0)
                        TickIndex = 0;
                }
                if (updateAfterSeek == true && isReversed == true 
                &&  TickIndex < s.SliderTicks.Length && sliderBallPosition <= tickPositionAt)
                {
                    ShowCurrentSliderTick(s);

                    TickIndex++;
                    if (TickIndex >= s.SliderTicks.Length)
                        TickIndex = s.SliderTicks.Length - 1;
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

            // reset visibility of all ticks to visible
            for (int i = 3; i < 3 + s.SliderTicks.Length; i++)
            {
                Image tick = body.Children[i] as Image;
                if (tick == null)
                {
                    continue;
                }
            
                tick.Visibility = Visibility.Collapsed;
                UpdateSliderTicks(true);
                tick.Visibility = Visibility.Visible;
            }

            double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            bool isReversed = IsSliderReversed(s, sliderPathDistance);
            double sliderBallPosition = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
            if (sliderBallPosition >= 1)
            {
                sliderBallPosition = sliderBallPosition - 1;
            }

            if (isReversed == false)
            {
                for (int i = 3; i < 3 + s.SliderTicks.Length; i++)
                {
                    if (i > body.Children.Count - 1)
                    {
                        continue;
                    }
                    
                    if (sliderBallPosition > s.SliderTicks[i - 3].PositionAt)
                    {
                        Image tick = body.Children[i] as Image;
                        tick.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            { 
                for (int i = s.SliderTicks.Length + 2; i >= 3; i--)
                {
                    Image tick = body.Children[i] as Image;
                    if (tick == null)
                    {
                        continue;
                    }

                    if (sliderBallPosition < s.SliderTicks[i - 3].PositionAt)
                    {
                        tick.Visibility = Visibility.Collapsed;
                    }
                }
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

        private static double GetCurrentSliderPosition(Slider s, bool reversed, double sliderPathDistance, bool isSeeking)
        {
            // found one case where frame time was correct when seeking by frame and didnt see it being incorrect on maze map
            // but if slider tick has wrong misses when seeking by frame then this is the problem
            double time;
            if (isSeeking)
            {
                ReplayFrame f = MainWindow.replay.FramesDict.Values.FirstOrDefault(f => f.Time > GamePlayClock.TimeElapsed) ?? MainWindow.replay.FramesDict.Values.Last();
                time = f.Time;
            }
            else
            {
                time = GamePlayClock.TimeElapsed;
            }

            double sliderCurrentPositionAt;
            if (reversed == true)
            {
                double sliderProgress = (time - s.SpawnTime) / sliderPathDistance;
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
                sliderCurrentPositionAt = (time - s.SpawnTime) / sliderPathDistance;
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

        private static Vector2 GetSliderTickPosition(Slider s, double osuScale)
        {
            Vector2 headPos = new Vector2((float)s.X, (float)s.Y);
            Vector2 tickPosInSlider = new Vector2((float)(s.SliderTicks[TickIndex].Position.X * osuScale), (float)(s.SliderTicks[TickIndex].Position.Y * osuScale));

            return headPos + tickPosInSlider;
        }

        private static double GetCursorPosition(Vector2 tickCentre, double osuScale)
        {
            if (MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Click == 0
            ||  MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Click == OsuFileParsers.Classes.Replay.Clicks.Smoke)
            {
                return -1;
            }

            double cursorX = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].X * osuScale - (Window.playfieldCursor.Width / 2);
            double cursorY = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Y * osuScale - (Window.playfieldCursor.Width / 2);
            
            double objectX = tickCentre.X;
            double objectY = tickCentre.Y;

            return Math.Pow(cursorX - objectX, 2) + Math.Pow(cursorY - objectY, 2);
        }

        private static void ShowMiss(Vector2 tickCentre, Slider s)
        {
            double tickDiameter = MainWindow.OsuPlayfieldObjectDiameter * 0.2;

            float X = (float)(tickCentre.X - (tickDiameter / 2));
            float Y = (float)(tickCentre.Y - (tickDiameter / 2));

            HitJudgementManager.ApplyJudgement(null, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, -1);
        }

        private static void ShowCurrentSliderTick(Slider s)
        {
            Canvas body = s.Children[0] as Canvas;
            Canvas ball = body.Children[2] as Canvas;

            Image tick = body.Children[TickIndex + 3] as Image;

            // sometimes its null lol
            if (tick != null)
            {
                tick.Visibility = Visibility.Visible;
            }
        }
    }
}
