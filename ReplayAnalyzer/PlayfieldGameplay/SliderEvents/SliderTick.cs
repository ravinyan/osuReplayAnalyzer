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
        private static double TickPositionAt = 0;
        private static double BaseTickPosition = 0;
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
                    TickPositionAt = s.SliderTicks[0].PositionAt;
                    BaseTickPosition = s.SliderTicks[0].PositionAt;
                }

                double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                bool isReversed = IsSliderReversed(s, sliderPathDistance);
                double sliderCurrentPositionAt = GetCurrentSliderPosition(s, isReversed, sliderPathDistance, updateAfterSeek);

                // im losing my mind
                // when slider is not reversed:
                //  when going forward:
                //   slider ball position must be lower than slider tick position
                //   if ball position is higher then update tick position
                //   if ball is higher than last tick then count reverse arrow as slider tick at position 1
                //  when going backwards:
                //   i have no clue coz the numbers are never exact ffs

                double sliderBallPosition = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
                if (sliderBallPosition < 0)
                {
                    return;
                }

                if (sliderBallPosition >= 1)
                {
                    sliderBallPosition = sliderBallPosition - 1;
                }

                // maybe +- 1ms will be needed
                var time = (long)GamePlayClock.TimeElapsed;
                var a = s.SliderTicks;

                //                                        T H E V I S I O N I S R E A L
                //                                           nvm doesnt work
                //if (TickIndex < s.SliderTicks.Length && time == (long)s.SliderTicks[TickIndex].Time)
                //{
                //    
                //}


                //double tickPositionAt = 0;
                //if (TickIndex < s.SliderTicks.Length && sliderBallPosition > s.SliderTicks[TickIndex].PositionAt)
                //{
                //    tickPositionAt = s.SliderTicks[TickIndex].PositionAt;
                //}
                //else if (TickIndex >= 0 && TickIndex < s.SliderTicks.Length 
                //     &&  updateAfterSeek == true && sliderBallPosition < s.SliderTicks[TickIndex].PositionAt)
                //{
                //    TickIndex--;
                //
                //    if (TickIndex == s.SliderTicks.Length)
                //    {
                //        //tickPositionAt = 1; // reverse arrow
                //    }
                //    else
                //    {
                //        tickPositionAt = 1 - s.SliderTicks[TickIndex].PositionAt;
                //    }
                //}

                // maybe i shouldnt use animations for updating slider position but update it manually with saved positions...
                // how...

                double tickPositionAt = 0;
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
                        tickPositionAt = 1 - s.SliderTicks[TickIndex].PositionAt;
                    }
                    else if (TickIndex >= s.SliderTicks.Length - 1)
                    {
                        TickIndex = s.SliderTicks.Length;
                    }
                    else
                    {
                        tickPositionAt = 1 -s.SliderTicks[TickIndex].PositionAt;
                    }
                }

      

                // make reverse arrows count as slider ticks later < this is gonna be pain in the ass
                bool aaa = Math.Abs(sliderBallPosition - tickPositionAt) <= 0.001;
                if (TickIndex >= 0 && TickIndex < s.SliderTicks.Length
                &&  aaa == true)//sliderBallPosition >= TickPositionAt)
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
                else if (updateAfterSeek == true && isReversed == false && TickIndex >= 0 
                     &&  time < (long)s.SliderTicks[TickIndex].Time) // sliderBallPosition <= TickPositionAt)
                {
                    ShowCurrentSliderTick(s);
                    TickIndex--;
                    
                    if (TickIndex < 0)
                        TickIndex = 0;
                }
                else if (updateAfterSeek == true && isReversed == true 
                     &&  TickIndex < s.SliderTicks.Length && sliderBallPosition <= TickPositionAt)
                {
                    TickIndex++;
                    if (TickIndex >= s.SliderTicks.Length)
                        TickIndex = s.SliderTicks.Length - 1;
                    
                    ShowCurrentSliderTick(s);
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
                UpdateSliderTicks(true);
            }

            // reset visibility of all ticks to visible
            for (int i = 3; i < 3 + s.SliderTicks.Length; i++)
            {
                Image tick = body.Children[i] as Image;
                if (tick == null)
                {
                    continue;
                }
            
                //tick.Visibility = Visibility.Visible;
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
