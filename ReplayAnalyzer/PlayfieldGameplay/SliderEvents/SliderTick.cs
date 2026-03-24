using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    public class SliderTick
    {
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
                    UpdateSliderTicks(updateAfterSeek, true);
                }
                else
                {
                    UpdateSliderTicks(updateAfterSeek, true);
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
                bool isReversed = IsSliderReversed(s.SpawnTime, sliderPathDistance);
                
                double sliderBallPosition;
                if (s.RepeatCount == 1)
                {
                    sliderBallPosition = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
                }
                else
                {
                    sliderBallPosition = GetSliderBallProgressPosition(s.SpawnTime, sliderPathDistance);
                }
                    
                if (sliderBallPosition >= 0 && sliderBallPosition <= 1)
                {
                    double osuScale = MainWindow.OsuPlayfieldObjectScale;

                    Vector2 ballCentre = GetSliderBallPosition(s, sliderBallPosition, osuScale);
                    double cursorPosition = GetCursorPosition(ballCentre, osuScale);
                    //                                  set diameter of slider ball hitbox
                    double circleRadius = Math.Pow(Slider.BallHitboxDiameter / 2, 2);

                    // change this to only 1 miss maybe... but this also shows more exact timing when someone misses... idk
                    if ((cursorPosition == -1 || cursorPosition > circleRadius) && Slider.BodyBall(s).Visibility == Visibility.Visible && StrictTrackingMissCooldown.ElapsedMilliseconds > 100)
                    {
                        StrictTrackingMissCooldown.Reset();
                        ShowMiss(ballCentre);
                        StrictTrackingMissCooldown.Start();
                    }
                }
            }
        }

        private static void UpdateSliderTicks(bool updateAfterSeek, bool isPreloading = false)
        {
            if (HitObjectManager.GetAliveHitObjects().Count == 0)
            {
                return;
            }

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

            double sliderPathLength = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            bool isReversed = IsSliderReversed(s.SpawnTime, sliderPathLength);

            //double tickPositionAt = GetSliderTickProgressPosition(s, isReversed);
            double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathLength);
            if (sliderBallProgress < 0)
            {
                return;
            }

            if (TickIndex >= 0 && TickIndex < s.SliderTicks.Count && GamePlayClock.TimeElapsed >= s.SliderTicks[TickIndex].Time)
            //if (TickIndex >= 0 && TickIndex < s.SliderTicks.Count && tickPositionAt != -1
            //&& (isReversed == false && sliderBallProgress >= tickPositionAt
            //||  isReversed == true && sliderBallProgress <= tickPositionAt))
            {
                Canvas body = Slider.Body(s);
                if (TickIndex + 3 >= body.Children.Count)
                {
                    return;
                }
                Image tick = body.Children[TickIndex + 3] as Image; // ticks are starting at [3]

                double osuScale = MainWindow.OsuPlayfieldObjectScale;
                if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale) == true && tick.Visibility == Visibility.Visible)
                {
                    if (isPreloading == false)
                    {
                        Vector2 tickCentre = GetSliderTickPosition(s, osuScale, MainWindow.OsuPlayfieldObjectDiameter);
                        ShowMiss(tickCentre);
                    }
                    else
                    {
                        HitJudgementManager.ApplyJudgement(null, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, -1);
                        SliderData slider = (SliderData)HitObjectManager.TransformHitObjectToDataObject(s);
                        slider.AllTicksHit = false; 
                    }
                }

                tick.Visibility = Visibility.Collapsed;

                //if (isReversed == false)
                //{
                    TickIndex++;
                //}
                //else if (isReversed == true)
                //{
                //    //TickIndex--;
                //}
            }

            if (TickIndex - 1 >= 0 && TickIndex - 1 < s.SliderTicks.Count
            &&  GamePlayClock.TimeElapsed <= s.SliderTicks[TickIndex - 1].Time)
            {
                Image tick = Slider.Body(s).Children[TickIndex - 1 + 3] as Image;
                tick.Visibility = Visibility.Visible;
                TickIndex--;
            }

            //if (updateAfterSeek == true && isReversed == false && TickIndex >= 0 && TickIndex < s.SliderTicks.Count
            //&&  sliderBallProgress <= tickPositionAt)
            //{
            //    ShowCurrentSliderTick(s);
            //    TickIndex--;
            //    
            //    if (TickIndex < 0)
            //    {
            //        TickIndex = 0;
            //    } 
            //}
            //else if (updateAfterSeek == true && isReversed == true && TickIndex < s.SliderTicks.Count && sliderBallProgress <= tickPositionAt)
            //{
            //    ShowCurrentSliderTick(s);
            //
            //    TickIndex++;
            //    if (TickIndex >= s.SliderTicks.Count)
            //    {
            //        TickIndex = s.SliderTicks.Count - 1;
            //    }  
            //}

            //if ((isReversed == false && sliderBallProgress >= tickPositionAt
            //||   isReversed == true && sliderBallProgress <= tickPositionAt))
            //{
            //    if (isReversed == false)
            //    {
            //        TickIndex++;
            //    }
            //    else if (isReversed == true)
            //    {
            //        TickIndex--;
            //    }
            //}
        }

        public static void HidePastTicks(Slider s)
        {
            if (s.SliderTicks == null)
            {
                return;
            }

            Canvas body = Slider.Body(s);

            // update tick index in lazy and inefficient way coz i can
            for (int i = 3; i < 3 + s.SliderTicks.Count; i++)
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
            bool isReversed = IsSliderReversed(s.SpawnTime, sliderPathDistance);
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
                for (int i = 3; i < 3 + s.SliderTicks.Count; i++)
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
                for (int i = s.SliderTicks.Count + 2; i >= 3; i--)
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

        private static bool IsCursorOutsideBallHitbox(Slider s, double sliderBallProgress, double osuScale)
        {
            Vector2 ballCentre = GetSliderBallPosition(s, sliderBallProgress, osuScale);
            double cursorPosition = GetCursorPosition(ballCentre, osuScale);

            double circleRadius = Math.Pow(Slider.BallHitboxDiameter / 2, 2);

            return cursorPosition == -1 || cursorPosition > circleRadius;
        }

        private static bool IsSliderReversed(int spawnTime, double sliderPathLength)
        {
            return Math.Floor((GamePlayClock.TimeElapsed - spawnTime) / sliderPathLength) % 2 == 1;
        }

        private static double GetSliderTickProgressPosition(Slider s, bool isReversed)
        {
            if (TickIndex == -1)
            {
                TickIndex = 0;
            }
            else if (TickIndex >= s.SliderTicks.Count)
            {
                TickIndex = s.SliderTicks.Count - 1;
            }

            try
            {
                return s.SliderTicks[TickIndex].PositionAt;
            }
            catch
            {
                return 0;
            }
        }

        private static double GetSliderBallProgressPosition(double sliderSpawnTime, double sliderPathDistance)
        {
            double sliderBallPosition = (GamePlayClock.TimeElapsed - sliderSpawnTime) / sliderPathDistance;

            double overflowPosition = sliderBallPosition - (int)sliderBallPosition;
            if ((int)sliderBallPosition % 2 == 1)
            {
                return sliderBallPosition = 1 - overflowPosition;
            }

            return overflowPosition;
        }

        private static Vector2 GetSliderTickPosition(Slider s, double osuScale, double diameter)
        {
            Vector2 headPos = new Vector2((float)(s.X), (float)(s.Y));
            Vector2 tickPosInSlider = new Vector2((float)(s.SliderTicks[TickIndex].Position.X * osuScale), (float)(s.SliderTicks[TickIndex].Position.Y * osuScale));

            return headPos + tickPosInSlider;
        }

        private static Vector2 GetSliderBallPosition(Slider s, double sliderBallProgress, double osuScale)
        {
            Vector2 headPos = new Vector2((float)s.X, (float)s.Y);

            Vector2 sliderBallPosInSlider = s.Path.PositionAt(sliderBallProgress);
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

            // so using diameter to get center of cursor position made cursor position in fact incorrect...
            // but i need to do that in cursor position to get correct position...
            // 
            // wat. < anyway i might need diameter
            double cursorX = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].X * osuScale;
            double cursorY = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Y * osuScale;

            return Math.Pow(cursorX - objectCentre.X, 2) + Math.Pow(cursorY - objectCentre.Y, 2);
        }

        private static void ShowMiss(Vector2 objectPosition)
        {
            double missDiameter = MainWindow.OsuPlayfieldObjectDiameter * 0.2;

            float X = (float)(objectPosition.X - (missDiameter / 2));
            float Y = (float)(objectPosition.Y - (missDiameter / 2));

            HitJudgementManager.ApplyJudgement(null, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, -1);
        }

        private static void ShowCurrentSliderTick(Slider s)
        {
            Image tick = Slider.Body(s).Children[TickIndex + 3] as Image;
            // sometimes its null lol < ok i learned AS gives null and (Image) gives exception... i rather have null
            if (tick != null)
            {
                tick.Visibility = Visibility.Visible;
            }
        }
    }
}
