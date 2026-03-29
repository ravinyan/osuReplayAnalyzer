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
        public static bool IsTracking = false;
        private static bool IsJudged = false;

        public static void ResetFields()
        {
            CurrentSlider = null;
        }

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
                }

                if (s.EndTime - s.SpawnTime <= 36)
                {
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
                        return;
                    }

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale))
                    {
                        s.SliderEndJudgement.ObjectJudgement = HitObjectJudgement.SliderEndMiss;
                    }
                }
            }
        }

        // wait wat da... testing how strict tracking works and it looks like
        // if slider ball was never hold > strict tracking doesnt do anything
        // if slider ball was hold at ANYTIME > when getting outside of hitbox or stopping clicking button then it misses\
        // if slider ball gets to the end of slider without being click then at the end it causes a miss...
        // the judged object for missing is slider tail
        // time to check lazer code if i missed anything < no i didnt but they create new slider class for this and im too lazy for that
        // maybe put strict tracking to end judgement class

        // this might be weird but strict tracking misses are slider end misses lol
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
                IsJudged = false;
            }

            double osuScale = MainWindow.OsuPlayfieldObjectScale;

            double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathDistance);
            if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale) && sliderBallProgress > 0
            &&  IsTracking == true && IsJudged == false)
            {
                // if IsTracking is always false then miss at the end
                // if tracking was broke during the slider then miss when it was broke at the end
                if (isPreloading == false)
                {
                    IsJudged = true;
                    s.SliderEndJudgement = new HitJudgement(HitObjectJudgement.SliderEndMiss, (long)GamePlayClock.TimeElapsed);
                    ShowMiss(s.EndPosition);
                }
                else
                {
                    IsJudged = true;
                    HitJudgementManager.ApplyJudgement(s, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, -1);
                }
            }

            // if its in the hitbox then start tracking
            if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale) == false && IsTracking == false && IsJudged == false)
            {
                IsTracking = true;
            }

            // reset judgement on backwards seeking
            if (GamePlayClock.TimeElapsed < s.SliderEndJudgement.SpawnTime 
            && IsJudged == true)
            {// time here does not matter when its hit, only matters on miss
                //s.SliderEndJudgement = new HitJudgement(HitObjectJudgement.SliderEndHit, 0);
            }
        }
    }
}
