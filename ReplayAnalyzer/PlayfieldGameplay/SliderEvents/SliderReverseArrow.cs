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
    public class SliderReverseArrow
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static double RepeatAt = 0;
        private static double RepeatInterval = 0;

        private static Slider CurrentReverseSlider = null;

        private static bool IsSliderReversed = false;

        private static int ReverseArrowIndex = 1;

        public static void ResetFields()
        {
            RepeatAt = 0;
            RepeatInterval = 0;
            CurrentReverseSlider = null;
            IsSliderReversed = false;
            ReverseArrowIndex = 1;
        }

        public static void UpdateSliderRepeatsPreload()
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = Slider.GetFirstSliderBySpawnTime();

                if (CurrentReverseSlider == null && s == null)
                {
                    return;
                }

                if (CurrentReverseSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    IsSliderReversed = false;

                    CurrentReverseSlider = s;

                    ReverseArrowIndex = 1;

                    RepeatInterval = s != null ? 1 / (double)s.RepeatCount : 1;
                    RepeatAt = RepeatInterval;

                    if (s == null)
                    {
                        return;
                    }
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

                        double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                        double sliderBallPosition = GetSliderBallPosition(s.SpawnTime, sliderPathDistance);
                        if (sliderBallPosition > 0)
                        {
                            Vector2 ballPosition = GetSliderBallCanvaPosition(s, sliderBallPosition, MainWindow.OsuPlayfieldObjectScale);
                            double cursorPosition = GetCursorPosition(ballPosition, MainWindow.OsuPlayfieldObjectScale);

                            double circleRadius = Math.Pow((MainWindow.OsuPlayfieldObjectDiameter * 2.4) / 2, 2); // * 2.4 is ball radius
                            if (cursorPosition == -1 || cursorPosition > circleRadius)
                            {
                                Vector2 missPosition = IsSliderReversed == true
                                                     ? new Vector2((float)s.X, (float)s.Y)
                                                     : s.EndPosition;
                                ShowMiss(missPosition, s);
                            }
                        }

                        RepeatAt += RepeatInterval;
                        ReverseArrowIndex++;

                        if (progress < 1 - RepeatInterval / 3)
                        {
                            ChangeSliderTickVisibility(s, Visibility.Visible);
                        }
                    }
                }
            }
        }

        public static void UpdateSliderRepeats()
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = Slider.GetFirstSliderBySpawnTime();
                
                if (CurrentReverseSlider == null && s == null)
                {
                    return;
                }

                if (CurrentReverseSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    IsSliderReversed = false;

                    CurrentReverseSlider = s;

                    ReverseArrowIndex = 1;

                    RepeatInterval = s != null ? 1 / (double)s.RepeatCount : 1;
                    RepeatAt = RepeatInterval;

                    if (s == null)
                    {
                        return;
                    }                 
                }

                if (s.RepeatCount > 1)
                {
                    if (Slider.HeadHitCircle(s).Visibility == Visibility.Visible)
                    {
                        // when slider head shows up hide all arrows beneath it
                        HideReverseArrowsBeneathSliderHead(s);
                    }

                    double progress = (GamePlayClock.TimeElapsed - s.SpawnTime) / (s.EndTime - s.SpawnTime);
                    if (progress > RepeatAt && RepeatAt >= 0 && progress >= 0)
                    {
                        if (RepeatAt == 0)
                        {
                            RepeatAt = RepeatInterval;
                            return;
                        }

                        double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                        double sliderBallPosition = GetSliderBallPosition(s.SpawnTime, sliderPathDistance);
                        if (sliderBallPosition > 0)
                        {
                            Vector2 ballPosition = GetSliderBallCanvaPosition(s, sliderBallPosition, MainWindow.OsuPlayfieldObjectScale);
                            double cursorPosition = GetCursorPosition(ballPosition, MainWindow.OsuPlayfieldObjectScale);

                            double circleRadius = Math.Pow((MainWindow.OsuPlayfieldObjectDiameter * 2.4) / 2, 2); // * 2.4 is ball radius
                            if ((cursorPosition == -1 || cursorPosition > circleRadius) && IsReverseArrowVisible(s) == true)
                            {
                                Vector2 missPosition = IsSliderReversed == true
                                                     ? new Vector2((float)s.X, (float)s.Y)
                                                     : s.EndPosition;
                                ShowMiss(missPosition, s);
                            }
                        }

                        HideReverseArrow(s);

                        RepeatAt += RepeatInterval;
                        ReverseArrowIndex++;
                        
                        // dont show ticks when spawning slider backwards
                        // progress value should be higher than last reverse arrow and lower that 1
                        if (progress < 1 - RepeatInterval / 3)
                        {
                            ChangeSliderTickVisibility(s, Visibility.Visible);
                        }
                    }
                    else if ((progress < RepeatAt - RepeatInterval) 
                    &&        RepeatAt >= 0 && progress >= 0)
                    {
                        ShowReverseArrow(s);

                        RepeatAt -= RepeatInterval;
                        ReverseArrowIndex--;

                        ChangeSliderTickVisibility(s, Visibility.Collapsed);
                    }
                }
            }
        }

        private static void HideReverseArrow(Slider s)
        {  
            if (IsSliderReversed == false)
            {
                Canvas tail = Slider.Tail(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex - 1) / 2.0);
                if (indx >= tail.Children.Count)
                {
                    indx = tail.Children.Count - 1;
                }

                tail.Children[indx].Visibility = Visibility.Collapsed;

                IsSliderReversed = true;
            }
            else
            {
                Canvas head = Slider.Head(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx == 0)
                {
                    indx = 1;
                }

                head.Children[head.Children.Count - indx].Visibility = Visibility.Collapsed;

                IsSliderReversed = false;
            }
        }

        private static void ShowReverseArrow(Slider s)
        {
            if (IsSliderReversed == false)
            {
                Canvas head = Slider.Head(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx > head.Children.Count - 4)
                {
                    // 4 index where child is reverse arrow, below are other always same circle children like approach circle
                    indx = head.Children.Count - 4;
                }

                head.Children[head.Children.Count - indx].Visibility = Visibility.Visible;

                IsSliderReversed = true;
            }
            else
            {
                Canvas tail = Slider.Tail(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx >= tail.Children.Count)
                {
                    indx = tail.Children.Count - 1;
                }

                tail.Children[indx].Visibility = Visibility.Visible;

                IsSliderReversed = false;
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
            Canvas body = Slider.Body(s);
            for (int i = 3; i < body.Children.Count; i++)
            {
                body.Children[i].Visibility = visibility;
            }
        }

        private static bool IsReverseArrowVisible(Slider s)
        {
            if (IsSliderReversed == false)
            {
                Canvas tail = Slider.Tail(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex - 1) / 2.0);
                if (indx >= tail.Children.Count)
                {
                    indx = tail.Children.Count - 1;
                }

                return tail.Children[indx].Visibility == Visibility.Visible;
            }
            else
            {
                Canvas head = Slider.Head(s);

                int indx = (int)Math.Ceiling((ReverseArrowIndex) / 2.0);
                if (indx == 0)
                {
                    indx = 1;
                }

                return head.Children[head.Children.Count - indx].Visibility == Visibility.Visible;
            }
        }

        // copied from SliderTick code maybe i will just add class so both classes can use it coz right now idk how to name it
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

        private static void ShowMiss(Vector2 missPosition, Slider s)
        {
            double missDiameter = MainWindow.OsuPlayfieldObjectDiameter * 0.2;

            float X = (float)(missPosition.X - (missDiameter / 2));
            float Y = (float)(missPosition.Y - (missDiameter / 2));

            HitJudgementManager.ApplyJudgement(null, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, -1);
        }
    }
}
