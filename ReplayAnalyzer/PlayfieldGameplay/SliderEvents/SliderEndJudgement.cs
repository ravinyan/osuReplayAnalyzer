using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    public class SliderEndJudgement
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static bool IsSliderEndHit = false;

        private static Slider CurrentSliderEndSlider = null;

        public static void ResetFields()
        {
            IsSliderEndHit = false;
            CurrentSliderEndSlider = null;
        }

        // this works but osu lazer doesnt have it done perfectly too... on seeking by frame it shows
        // slider end missed but while playing normally it wont show it... and it changes acc/combo too... its weird
        public static void HandleSliderEndJudgement()
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = HitObjectManager.GetFirstSliderDataBySpawnTime();
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

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;

                    double ballDiameter = 0;
                    Point ballCentre = GetSliderBallPosition(s, osuScale, out ballDiameter);

                    double cursorPosition = GetCursorPosition(ballCentre, osuScale);
                    double sliderBallRadius = Math.Pow(ballDiameter / 2, 2);

                    if (cursorPosition <= sliderBallRadius)
                    {
                        IsSliderEndHit = true;
                    }
                }
            }
        }

        private static Point GetSliderBallPosition(Slider s, double osuScale, out double ballDiameter)
        {
            Canvas body = s.Children[0] as Canvas;
            Canvas ball = body.Children[2] as Canvas;
            Image hitboxBall = ball.Children[1] as Image;

            double hitboxBallWidth = hitboxBall.Width;
            double hitboxBallHeight = hitboxBall.Height;

            ballDiameter = hitboxBallWidth * osuScale;

            return hitboxBall.TranslatePoint(new Point(hitboxBallWidth / 2, hitboxBallHeight / 2), Window.playfieldCanva);
        }

        private static double GetCursorPosition(Point ballCentre, double osuScale)
        {
            // cursor pos index - 1 coz its always ahead by one from incrementing at the end of cursor update
            double cursorX = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].X * osuScale - (Window.playfieldCursor.Width / 2);
            double cursorY = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Y * osuScale - (Window.playfieldCursor.Width / 2);

            return Math.Pow(cursorX - ballCentre.X, 2) + Math.Pow(cursorY - ballCentre.Y, 2);
        }
    }
}
