using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.OsuMaths;
using System.Numerics;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class HardRockMod
    {
        public static bool IsHardRockEnabled { get; private set; } = false;

        public static void ResetFields()
        {
            IsHardRockEnabled = false;
        }

        public static void ApplyValues(bool isLazer)
        {
            IsHardRockEnabled = true;
            if (isLazer == false)
            {
                ApplyStable();
            }
            else
            {
                ApplyLazer();
            }
        }
        private static void ApplyStable()
        {
            Difficulty newMapDifficulty = MainWindow.map.Difficulty!;

            decimal newCS = Math.Min(newMapDifficulty.CircleSize * 1.3m, 10);
            decimal newOD = Math.Min(newMapDifficulty.OverallDifficulty * 1.4m, 10);

            if (MainWindow.replay.GameMode == OsuFileParsers.Classes.Replay.GameMode.OsuMania)
            {// and mania is very special...
                OsuMath.ManiaDifficultyMultiplier = 1.4;
            }
            else 
            {// mania doesnt like this
                newMapDifficulty.CircleSize = newCS;
                newMapDifficulty.OverallDifficulty = newOD;
            }

            decimal newAR = Math.Min(newMapDifficulty.ApproachRate * 1.4m, 10);
            newMapDifficulty.ApproachRate = newAR;

            decimal newHPDrain = Math.Min(newMapDifficulty.HPDrainRate * 1.4m, 10);
            newMapDifficulty.HPDrainRate = newHPDrain;

            if (MainWindow.replay.GameMode == OsuFileParsers.Classes.Replay.GameMode.Osu)
            {
                ApplyStandardHardRock();
            }

            MainWindow.map.Difficulty = newMapDifficulty;
        }

        private static void ApplyLazer()
        {
            // its the same as stable since there are no configs or anything like that
            ApplyStable();
        }

        private static void ApplyStandardHardRock()
        {
            // taken from osu lazer code in case there would be some big edge case (there wasnt)
            // for circles - changes vertical position of circle to be opposite of what it was
            // for sliders - same as circle + end position flip + flips control points and ticks
            for (int j = 0; j < MainWindow.map.HitObjects.Count; j++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[j];

                hitObject.BaseY = 384 - hitObject.BaseY;
                hitObject.BaseSpawnPosition = new Vector2((float)hitObject.BaseX, (float)hitObject.BaseY);

                if (hitObject is not OsuSliderData slider)
                {
                    continue;
                }

                slider.EndPosition = new Vector2(slider.EndPosition.X, 384 - slider.EndPosition.Y);

                for (int k = 0; k < slider.ControlPoints.Length; k++)
                {
                    slider.ControlPoints[k].Position = new Vector2(slider.ControlPoints[k].Position.X, -slider.ControlPoints[k].Position.Y);
                }
                slider.Path = new OsuFileParsers.SliderPathMath.SliderPath(slider);

                if (slider.SliderTicks != null)
                {
                    for (int k = 0; k < slider.SliderTicks.Count; k++)
                    {
                        slider.SliderTicks[k].Position = new Vector2(slider.SliderTicks[k].Position.X, -slider.SliderTicks[k].Position.Y);
                    }
                }
            }
        }
    }
}
