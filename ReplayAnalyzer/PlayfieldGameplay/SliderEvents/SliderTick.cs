using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Diagnostics;
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
        private static Stopwatch StrictTrackingMissCooldown = new Stopwatch();

        public static void ResetFields()
        {
            TickIndex = 0;
            CurrentSlider = null;
        }

        /// <summary>
        /// This takes care of slider ticks and slider strict tracking if enabled.
        /// </summary>
        public static void UpdateSliderBodyEvents(bool updateAfterSeek = false)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                if (StrictTrackingMod.IsStrictTrackingEnabled == true)
                {
                    UpdateSliderStrictTracking();
                    UpdateSliderTickPreload();
                }
                else
                {
                    UpdateSliderTickPreload();
                }
            }
            else if (StrictTrackingMod.IsStrictTrackingEnabled == true)
            {
                UpdateSliderStrictTracking();
                UpdateSliderTicks(updateAfterSeek);
            }
            else
            {
                UpdateSliderTicks(updateAfterSeek);
            }
        }

        private static void UpdateSliderStrictTracking()
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = Slider.GetFirstSliderBySpawnTime();

                if (s == null)
                {
                    return;
                }

                if (CurrentSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    CurrentSlider = s;
                }

                double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                bool isReversed = IsSliderReversed(s, sliderPathDistance);
                
                double sliderBallPosition;
                if (s.RepeatCount == 1)
                {
                    sliderBallPosition = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
                }
                else
                {
                    sliderBallPosition = GetSliderBallPosition(s.SpawnTime, sliderPathDistance);
                }
                    
                if (sliderBallPosition >= 0 && sliderBallPosition <= 1)
                {
                    double osuScale = MainWindow.OsuPlayfieldObjectScale;

                    Vector2 ballCentre = GetSliderBallCanvaPosition(s, sliderBallPosition, osuScale);
                    double cursorPosition = GetCursorPosition(ballCentre, osuScale);
                    //                                  set diameter of slider ball hitbox
                    double circleRadius = Math.Pow((MainWindow.OsuPlayfieldObjectDiameter * 2.4) / 2, 2);

                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas; // maybe not needed? idk
                    if ((cursorPosition == -1 || cursorPosition > circleRadius && ball.Visibility == Visibility.Visible) && StrictTrackingMissCooldown.ElapsedMilliseconds > 100)
                    {
                        StrictTrackingMissCooldown.Reset();
                        ShowMiss(ballCentre, s);
                        StrictTrackingMissCooldown.Start();
                    }
                }
            }
        }

        private static void UpdateSliderTickPreload()
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

                double sliderBallPosition = GetSliderBallPosition(s.SpawnTime, sliderPathDistance);
                double tickPositionAt = GetSliderTickPosition(s, isReversed);

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
                    if (cursorPosition == -1 || cursorPosition > circleRadius)
                    {
                        HitJudgementManager.ApplyJudgement(null, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, -1);
                        SliderData slider = (SliderData)HitObjectManager.TransformHitObjectToDataObject(s);
                        slider.AllTicksHit = false;
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
            }
        }

        private static void UpdateSliderTicks(bool updateAfterSeek)
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

                double tickPositionAt = GetSliderTickPosition(s, isReversed);
                double sliderBallPosition = GetSliderBallPosition(s.SpawnTime, sliderPathDistance);
                if (sliderBallPosition < 0)
                {
                    return;
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
                    if (((cursorPosition == -1 || cursorPosition > circleRadius) && tick.Visibility == Visibility.Visible)
                    &&    updateAfterSeek == false) // <- this update is needed but also it breaks stuff i hate it here my code is trash
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
                if (updateAfterSeek == true && isReversed == false && TickIndex >= 0 && sliderBallPosition <= tickPositionAt)
                {
                    ShowCurrentSliderTick(s);
                    TickIndex--;
                    
                    if (TickIndex < 0)
                        TickIndex = 0;
                }
                if (updateAfterSeek == true && isReversed == true && TickIndex < s.SliderTicks.Length && sliderBallPosition <= tickPositionAt)
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

            // update tick index in lazy and inefficient way coz i can
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
            if (sliderBallPosition > 1 && (int)sliderBallPosition % 2 == 1)
            {
                while (sliderBallPosition > 1)
                {
                    sliderBallPosition = sliderBallPosition - 1;
                }
            }
            else if (sliderBallPosition > 1)
            {
                sliderBallPosition = sliderBallPosition - (int)sliderBallPosition;
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

                    double tickPos = 1 - s.SliderTicks[i - 3].PositionAt;
                    if (sliderBallPosition > tickPos)
                    {
                        tick.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private static double GetSliderTickPosition(Slider s, bool isReversed)
        {
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

                tickPositionAt = 1 - s.SliderTicks[TickIndex].PositionAt;
            }

            return tickPositionAt;
        }

        private static double GetSliderBallPosition(double sliderSpawnTime, double sliderPathDistance)
        {
            double sliderBallPosition = (GamePlayClock.TimeElapsed - sliderSpawnTime) / sliderPathDistance;

            if (sliderBallPosition > 1 && (int)sliderBallPosition % 2 == 1)
            {
                while (sliderBallPosition > 1)
                {
                    sliderBallPosition = sliderBallPosition - 1;
                }
            }
            else if (sliderBallPosition > 1)
            {
                sliderBallPosition = sliderBallPosition - (int)sliderBallPosition;
            }

            return sliderBallPosition;
        }

        private static bool IsSliderReversed(Slider s, double sliderPathDistance)
        {
            if (Math.Floor((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance) != 0)
            {
                return Math.Floor((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance) % 2 == 1 ? true : false;
            }

            return false;
        }

        private static Vector2 GetSliderTickPosition(Slider s, double osuScale)
        {
            Vector2 headPos = new Vector2((float)s.X, (float)s.Y);
            Vector2 tickPosInSlider = new Vector2((float)(s.SliderTicks[TickIndex].Position.X * osuScale), (float)(s.SliderTicks[TickIndex].Position.Y * osuScale));

            return headPos + tickPosInSlider;
        }

        private static Vector2 GetSliderBallCanvaPosition(Slider s, double sliderBallPosition, double osuScale)
        {
            Vector2 headPos = new Vector2((float)s.X, (float)s.Y);
            Vector2 sliderBallPosInSlider = s.Path.PositionAt(sliderBallPosition);

            Vector2 sliderBallPos = new Vector2((float)(sliderBallPosInSlider.X * osuScale), (float)(sliderBallPosInSlider.Y * osuScale));

            return headPos + sliderBallPos;
        }

        private static double GetCursorPosition(Vector2 objectCentre, double osuScale)
        {
            if (MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Click == 0
            ||  MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Click == Clicks.Smoke)
            {
                return -1;
            }

            double cursorX = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].X * osuScale - (Window.playfieldCursor.Width / 2);
            double cursorY = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Y * osuScale - (Window.playfieldCursor.Width / 2);
            
            double objectX = objectCentre.X;
            double objectY = objectCentre.Y;

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
