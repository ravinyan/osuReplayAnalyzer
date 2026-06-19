using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Numerics;
using Slider = ReplayAnalyzer.HitObjects.Osu.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay.SliderEvents
{
    public class SliderEndJudgement : SliderEvent
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
        
        // these functions just mark IsTracked and give misses when slider despawns in HitObjectManager
        // unless strict tracking where there is IsJudged to track and judgement can spawn once in the middle of the slider
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

                IsTracking = IsSliderCurrentlyTracked(s);
            }

            // if slider ball in this moment was tracked, then save IsTracking to be always true
            // but if it wasnt tracked, slider ball can still be tracked again at any time as long as slider is alive
            if ((int)s.EndTime - GamePlayClock.TimeElapsed < TAIL_JUDGEMENT_LENIENCY && IsTracking == true)
            {
                return;
            }
            
            // logic from osu lazer code... IF circle was hit late but cursor was inside
            // expanded slider ball radius, keep radius expanded and initialize IsTracking to true
            if (Slider.HeadHitCircleContainer(s).Visibility == System.Windows.Visibility.Visible
            &&  IsCursorInsideSliderHead(s) == true)
            {
                IsTracking = true;
            }
            else
            {
                IsTracking = !IsCursorOutsideBallHitbox(s);
            }
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

                IsTracking = IsSliderCurrentlyTracked(s);

                if (s.SliderEndJudgement.Judgement == HitObjectJudgement.SliderEndMiss
                &&  GamePlayClock.TimeElapsed >= s.SliderEndJudgement.SpawnTime)
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
                return;
            }

            // need that slider ball position coz otherwise it will give miss when player hits circle early and
            // person seeks slowly into/from the slider
            double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;
            double sliderBallPosition = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
            if (sliderBallPosition > 0 && IsCursorOutsideBallHitbox(s) && IsTracking == true && IsJudged == false)
            {
                // if IsTracking is always false then miss at the end
                // if tracking was broken during the slider then miss when it was broken only once (miss is on slider end)
                if (isPreloading == false)
                {
                    IsJudged = true;
                    ShowMiss(s.EndPosition);
                }
                else
                {
                    IsJudged = true;
                    HitJudgementManager.ApplyJudgement(s, new Vector2(0, 0), (long)GamePlayClock.TimeElapsed, HitObjectJudgement.SliderEndMiss);
                }
            }

            // logic from osu lazer code... IF circle was hit late but cursor was inside
            // expanded slider ball radius, keep radius expanded and initialize IsTracking to true
            if (Slider.HeadHitCircleContainer(s).Visibility == System.Windows.Visibility.Visible
            &&  IsCursorInsideSliderHead(s) == true)
            {
                IsTracking = true;
            }
            else
            {
                IsTracking = !IsCursorOutsideBallHitbox(s);
            }

            // reset judgement on backwards seeking
            if (GamePlayClock.TimeElapsed > s.SpawnTime && GamePlayClock.TimeElapsed < s.SliderEndJudgement.SpawnTime 
            &&  IsJudged == true)
            {
                IsJudged = false;
                // ball was tracked before the miss (function to be more accurate when song slider seeking coz sometimes it wasnt tracked)
                IsTracking = IsSliderCurrentlyTracked(s);
            }
        }

        private static bool IsSliderCurrentlyTracked(Slider s)
        {
            bool isButtonHeld = false;
            List<Clicks> actions = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1].Clicks;
            if (actions.Count != 0 && actions.Count == 1 && actions.Contains(Clicks.Smoke))
            {
                // set it to true to increase ball hitbox size since button was held when slider despawned
                IsTracking = true;
                isButtonHeld = true;
            }

            if (isButtonHeld == true && IsCursorOutsideBallHitbox(s) == false)
            {
                return true;
            }

            return false;
        }

        private static bool IsCursorInsideSliderHead(Slider s)
        {
            ReplayFrame cursorFrame = MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex - 1];
            double osuScale = MainWindow.OsuPlayfieldObjectScale;
            double cursorX = cursorFrame.X * osuScale;
            double cursorY = cursorFrame.Y * osuScale;

            double cursorPosition = Math.Pow(cursorX - s.X, 2) + Math.Pow(cursorY - s.Y, 2);
            double radius = MainWindow.OsuPlayfieldObjectDiameter / 2;
            return cursorPosition < radius * radius;
        }
    }
}
