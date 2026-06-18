using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Numerics;

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    // just for fun experimenting to see if code looks nicer
    public class SliderEvent
    {
        protected static void ShowMiss(Vector2 objectPosition)
        {
            double missDiameter = MainWindow.OsuPlayfieldObjectDiameter * 0.2;

            float X = (float)(objectPosition.X - (missDiameter / 2));
            float Y = (float)(objectPosition.Y - (missDiameter / 2));

            HitJudgementManager.ApplyJudgement(null!, new Vector2(X, Y), (long)GamePlayClock.TimeElapsed, HitObjectJudgement.SliderTickMiss);
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

        private static double GetSliderBallProgressPosition(double sliderSpawnTime, double sliderPathDistance)
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

        private static Vector2 GetSliderBallPosition(Slider s, double sliderBallProgress, double osuScale)
        {
            Vector2 headPos = new Vector2((float)s.X, (float)s.Y);
            Vector2 sliderBallPosInSlider = Vector2.Multiply((float)osuScale, s.Path.PositionAt(sliderBallProgress));

            return headPos + sliderBallPosInSlider;
        }

        private static double GetCursorPositionInObject(Vector2 objectCentre, double osuScale)
        {
            ReplayFrame cursorFrame = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1];
            if (cursorFrame.Clicks.Count == 0 
            ||  cursorFrame.Clicks.Count == 1 && cursorFrame.Clicks.Contains(Clicks.Smoke))
            {
                return -1;
            }

            double cursorX = cursorFrame.X * osuScale;
            double cursorY = cursorFrame.Y * osuScale;

            // math.pow sucks for performance but at least it looks kinda clean... ?
            return Math.Pow(cursorX - objectCentre.X, 2) + Math.Pow(cursorY - objectCentre.Y, 2);
        }

        // i dont want to delete this my brain was overworking when i figured this out i hate math
        //protected static bool IsSliderReversed(int sliderSpawnTime, double sliderPathLength)
        //{
        //    return Math.Floor((GamePlayClock.TimeElapsed - sliderSpawnTime) / sliderPathLength) % 2 == 1;
        //}
    }
}
