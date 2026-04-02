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

        // slider end will give max judgement before it ends completely, just like in game (if slider was tracked)
        public const int TAIL_JUDGEMENT_LENIENCY = 36;

        public static void ResetFields()
        {
            CurrentSlider = null;
        }

        // functions should give judgement but they wont coz it is not needed in this app (excluding miss in strict tracking)
        // just in case judgement for completion of a slider is added 36ms before it ends
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

        // after reading osu lazer code and testing stuff, checking replays to see differences between my results and lazer ones
        // i think its kind of impossible to make this completely correct? at least with replays being recorded in 60fps...
        // it still is worse(?) that osu lazer code but im just one skill issued guy so eh its fine i hope
        // additional thingy: i think lazer does one more update even if slider was tracked before 36ms mark and at 22ms mark
        // on heathens map it updated tracking again for slider... and in all these cases while playing replay normally (not seeking)
        // you get x300 and if you play replay seeking by frame, skip 1s/10s or use song progress for skipping, you get x100
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

                // if slider ball in this moment was tracked, then save IsTracking to be always true
                // but if it wasnt tracked, slider ball can still be tracked again at any time as long as slider is alive
                if ((int)s.EndTime - GamePlayClock.TimeElapsed < TAIL_JUDGEMENT_LENIENCY && IsTracking == true)
                {
                    return;
                }

                double sliderPathLength = (s.EndTime - s.SpawnTime) / s.RepeatCount;
                double sliderBallProgress = GetSliderBallProgressPosition(s.SpawnTime, sliderPathLength);

                IsTracking = !IsCursorOutsideBallHitbox(s, sliderBallProgress, MainWindow.OsuPlayfieldObjectScale);
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

            if ((int)s.EndTime - GamePlayClock.TimeElapsed < TAIL_JUDGEMENT_LENIENCY && IsTracking == true)
            {
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
