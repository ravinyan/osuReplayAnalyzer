using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Numerics;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    public class SliderEndJudgement : SliderTick
    {
        private static Slider CurrentSlider = null;
        public static bool IsTracking { get; private set; } = false;
        public static bool IsJudged { get; private set; } = false;

        public static void ResetFields()
        {
            CurrentSlider = null;
        }

        // both functions in this class should not give any judgements, only mark if slider ball was/is tracked
        // then use IsTracked and IsJudged values to apply judgement when slider despawns
        public static void UpdateSliderBodyEvents(bool updateAfterSeek = false)
        {
            if (StrictTrackingMod.IsStrictTrackingEnabled == true)
            {
                UpdateSliderStrictTracking(MainWindow.IsReplayPreloading);
            }
            else
            {
                HandleSliderEndJudgement(MainWindow.IsReplayPreloading);
            }
        }

        // this works but osu lazer doesnt have it done perfectly too... on seeking by frame it shows
        // slider end missed but while playing normally it wont show it... and it changes acc/combo too... its weird
        private static void HandleSliderEndJudgement(bool isPreloading = false)
        {
            if (HitObjectManager.GetAliveHitObjects().Count > 0)
            {
                Slider s = Slider.GetFirstSliderBySpawnTime();
                if (s == null)
                {
                    return;
                }

                if (s != CurrentSlider)
                {
                    CurrentSlider = s;
                    IsTracking = false;
                }

                if (s.EndTime - s.SpawnTime <= 36)
                {// if slider is super short then it automatically gives best judgement
                    IsTracking = true;
                    return;
                }
                else
                {
                    double minPosForMaxJudgement = 1 - (36 / (s.EndTime - s.SpawnTime));

                    double sliderPathLength = s.EndTime - s.SpawnTime;
                    double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathLength);

                    // if current position is lower than minimum position to get x300 on slider end then leave
                    // or if its already confirmed that slider end is hit also leave
                    if (sliderBallProgress == 0 
                    || (sliderBallProgress >= minPosForMaxJudgement || s.SliderEndJudgement.ObjectJudgement != HitObjectJudgement.None))
                    {
                        IsTracking = true;
                        return;
                    }

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    // outside ball
                    if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale))
                    {
                        IsTracking = false;
                    }

                    // inside ball
                    if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale) == false)
                    {
                        IsTracking = true;
                    }
                }
            }
        }
        
        private static void UpdateSliderStrictTracking(bool isPreloading = false)
        {
            if (HitObjectManager.GetAliveHitObjects().Count == 0)
            {
                return;
            }

            Slider s = Slider.GetFirstSliderBySpawnTime();
            if (s == null)
            {
                return;
            }

            if (s != CurrentSlider)
            {
                CurrentSlider = s;
                IsTracking = false;

                if (s.SliderEndJudgement.ObjectJudgement == HitObjectJudgement.SliderEndMiss)
                {
                    IsJudged = true;
                }
                else
                {
                    IsJudged = false;
                }
            }

            double osuScale = MainWindow.OsuPlayfieldObjectScale;

            double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathDistance);
            if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale) && sliderBallProgress > 0
            &&  IsTracking == true && IsJudged == false)
            {
                // if IsTracking is always false then miss at the end
                // if tracking was broken during the slider then miss when it was broken (miss is on slider end)
                if (isPreloading == false)
                {
                    IsJudged = true;
                    ShowMiss(s.EndPosition);
                }
                else
                {// this is only code in this class that should give judgements and only in preload
                    IsJudged = true;
                    HitJudgementManager.ApplyJudgement(s, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, -2);
                }
            }

            // if its in the hitbox then start tracking, if it was never tracked then miss will be shown when slider despawns
            if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale) == false && IsTracking == false && IsJudged == false)
            {
                IsTracking = true;
                //if (s.SliderEndJudgement.ObjectJudgement == HitObjectJudgement.None)
                //{
                //    HitJudgementManager.ApplyJudgement(s, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, 150);
                //}
            }

            // reset judgement on backwards seeking
            if (GamePlayClock.TimeElapsed < s.SliderEndJudgement.SpawnTime 
            &&  IsJudged == true)
            {
                IsJudged = false;
                IsTracking = true; // ball was tracked before the miss
            }
        }
    }
}
