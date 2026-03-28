using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Numerics;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    // this is basically budget slider tick... ? i just want to reuse code without making it public
    // and repeating all function declaration everywhere like in other ways (interfaces, composition) coz its ugly
    public class SliderEndJudgement : SliderTick
    {
        private static Slider CurrentSlider = null;

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

        // this might be weird but strict tracking misses are slider end misses lol
        private static void UpdateSliderStrictTracking(bool isPreloading = false)
        {
            if (HitObjectManager.GetAliveHitObjects().Count == 0 || CurrentSlider == null)
            {
                return;
            }

            Slider s = CurrentSlider;

            double osuScale = MainWindow.OsuPlayfieldObjectScale;

            double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathDistance);
            if (IsCursorOutsideBallHitbox(s, sliderBallProgress, osuScale) && sliderBallProgress > 0
            &&  s.SliderEndJudgement.ObjectJudgement != HitObjectJudgement.None)
            {
                if (isPreloading == false)
                {
                    HitJudgementManager.ApplyJudgement(null, s.EndPosition, (long)GamePlayClock.TimeElapsed, -1);
                }
                else
                {
                    HitJudgementManager.ApplyJudgement(null, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, -1);
                }
            }
        }
    }
}
