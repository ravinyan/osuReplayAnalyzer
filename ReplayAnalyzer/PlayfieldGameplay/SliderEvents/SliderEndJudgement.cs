using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Numerics;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    // this is not made with adding score in mind (and i dont plan to add score calculation,
    // this is analyzer for misses and mistakes and not for score) so all code here ignores any scoring whatsoever
    // which is also why the judgement for slider end hit is kinda pointless too but meh whatever
    public class SliderEndJudgement : SliderTick
    {
        private static Slider CurrentSlider = null;
        public static bool IsTracking { get; private set; } = false;
        public static bool IsJudged { get; private set; } = false;

        // slider end will give max judgement before it ends completely, just like in game
        public const int TAIL_JUDGEMENT_LENIENCY = 36;

        public static void ResetFields()
        {
            CurrentSlider = null;
        }

        // functions should give judgement but they wont coz it is not needed in this app (excluding miss in strict tracking)
        // just in case judgement for completion of a slider is added like 3 frames before slider ends (or just 36ms)
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

                double sliderPathLeghtTotal = s.EndTime - s.SpawnTime;
                double sliderBallProgressTotal = GetSliderBallProgressPosition(s.SpawnTime, sliderPathLeghtTotal);

                double minPosForMaxJudgement = 1 - (TAIL_JUDGEMENT_LENIENCY / sliderPathLeghtTotal);
                if (sliderBallProgressTotal >= minPosForMaxJudgement && IsTracking == true)
                {// it will save the IsTracking status as true if it was true before reaching min posistion for non end miss
                    return;
                }

                double sliderPathLength = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathLength);

                double osuScale = MainWindow.OsuPlayfieldObjectScale;
                if (IsTracking == true && IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale))
                {// outside ball
                    IsTracking = false;
                }

                if (IsTracking == false && IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale) == false)
                {// inside ball
                     IsTracking = true;
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

            if (s.SpawnTime - s.EndTime <= TAIL_JUDGEMENT_LENIENCY)
            {
                IsJudged = true;
                return;
            }
            else
            {
                double sliderPathLeghtTotal = s.EndTime - s.SpawnTime;
                double sliderBallProgressTotal = GetSliderBallProgressPosition(s.SpawnTime, sliderPathLeghtTotal);

                double minPosForMaxJudgement = 1 - (TAIL_JUDGEMENT_LENIENCY / sliderPathLeghtTotal);
                if (sliderBallProgressTotal >= minPosForMaxJudgement && IsTracking == true)
                {// if it is here its basically max score judgement for tail
                    IsJudged = true;
                    return;
                }

                double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathDistance);

                double osuScale = MainWindow.OsuPlayfieldObjectScale;
                if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale) && IsTracking == true && IsJudged == false)
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
                }

                // reset judgement on backwards seeking
                if (GamePlayClock.TimeElapsed < s.SliderEndJudgement.SpawnTime && IsJudged == true)
                {
                    IsJudged = false;
                    IsTracking = true; // ball was tracked before the miss
                }
            }
        }
    }
}
