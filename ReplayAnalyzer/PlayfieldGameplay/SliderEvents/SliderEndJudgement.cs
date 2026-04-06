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
        private const int TAIL_JUDGEMENT_LENIENCY = 36;

        public static void ResetFields()
        {
            CurrentSlider = null;
        }

        // functions should give judgement but they wont coz it is not needed in this app (excluding miss in strict tracking)
        // just in case judgement for completion of a slider is added 36ms before it ends
        public static void UpdateSliderBodyEvents()
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
            Slider s = Slider.GetFirstSliderBySpawnTime();
            if (s == null)
            {
                CurrentSlider = null;
                return;
            }

            if (s != CurrentSlider)
            {
                CurrentSlider = s;

                // +16 is just +16ms as +1 frame to make sure this is correctly calculated
                if (GamePlayClock.TimeElapsed + 16 >= s.SliderEndJudgement.SpawnTime && IsSliderCurrentlyTracked(s) == true)
                {
                    IsTracking = true;
                }
                else
                {
                    IsTracking = false;
                }

                // when slider despawns the frame of the despawn gets saved
                // so Judgement SpawnTime SHOULD be always higher than EndTime by like >0ms and <1ms or just equal
                if (s.SliderEndJudgement.ObjectJudgement == HitObjectJudgement.SliderEndHit
                &&  s.SliderEndJudgement.SpawnTime >= s.DespawnTime)
                {
                    IsTracking = true;
                }
                else
                {
                    IsTracking = false;
                }
            }

            // if slider ball in this moment was tracked, then save IsTracking to be always true
            // but if it wasnt tracked, slider ball can still be tracked again at any time as long as slider is alive
            if ((int)s.EndTime - GamePlayClock.TimeElapsed < TAIL_JUDGEMENT_LENIENCY && IsTracking == true)
            {
                return;
            }

            IsTracking = !IsCursorOutsideBallHitbox(s);
        }
        
        private static void UpdateSliderStrictTracking(bool isPreloading = false)
        {
            Slider s = Slider.GetFirstSliderBySpawnTime();
            if (s == null)
            {
                CurrentSlider = null;
                return;
            }

            if (s != CurrentSlider)
            {
                CurrentSlider = s;

                // +16 is just +16ms as +1 frame to make sure this is correctly calculated
                if (GamePlayClock.TimeElapsed + 16 >= s.SliderEndJudgement.SpawnTime && IsSliderCurrentlyTracked(s) == true)
                {
                    IsTracking = true;
                }
                else
                {
                    IsTracking = false;
                }

                // when slider despawns the frame of the despawn gets saved
                // so Judgement SpawnTime SHOULD be always higher than EndTime by like >0ms and <1ms or just equal
                if (s.SliderEndJudgement.ObjectJudgement == HitObjectJudgement.SliderEndMiss
                && (GamePlayClock.TimeElapsed >= s.SliderEndJudgement.SpawnTime
                ||  s.SliderEndJudgement.SpawnTime >= s.DespawnTime))
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

            // need that slider ball position coz otherwise it will give miss when player hits circle early and
            // person seeks slowly into/from the slider
            double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            double sliderBallPosition = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
            if (sliderBallPosition > 0 && IsCursorOutsideBallHitbox(s) && IsTracking == true && IsJudged == false)
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

            IsTracking = !IsCursorOutsideBallHitbox(s);

            // reset judgement on backwards seeking
            if (GamePlayClock.TimeElapsed > s.SpawnTime && GamePlayClock.TimeElapsed < s.SliderEndJudgement.SpawnTime 
            &&  IsJudged == true)
            {
                IsJudged = false;
                IsTracking = true; // ball was tracked before the miss
            }
        }

        private static bool IsSliderCurrentlyTracked(Slider s)
        {
            bool isButtonHeld = false;
            OsuFileParsers.Classes.Replay.Clicks action = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Click;
            if (action != 0 && action != OsuFileParsers.Classes.Replay.Clicks.Smoke)
            {
                isButtonHeld = true;

                // set it to true to increase ball hitbox size since button was held when slider despawned
                IsTracking = true; 
            }

            if (isButtonHeld == true && IsCursorOutsideBallHitbox(s) == false)
            {
                return true;
            }

            return false;
        }
    }
}
