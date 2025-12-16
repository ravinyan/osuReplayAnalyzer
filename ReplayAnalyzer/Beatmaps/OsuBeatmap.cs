using OsuFileParsers.Classes.Beatmap.osu;
using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.Skins;
using System.Drawing;
using System.Numerics;
using System.Windows.Controls;
using CircleData = OsuFileParsers.Classes.Beatmap.osu.Objects.CircleData;
using SliderData = OsuFileParsers.Classes.Beatmap.osu.Objects.SliderData;
using SpinnerData = OsuFileParsers.Classes.Beatmap.osu.Objects.SpinnerData;

#nullable disable

namespace ReplayAnalyzer.Beatmaps
{
    public static class OsuBeatmap
    {
        // put this later for hit data objects
        //Stacking stacking = new Stacking();
        //stacking.ApplyStacking(map);

        public static void ModifyDifficultyValues(string modsUsed)
        {
            if (modsUsed == null)
            {
                return;
            }

            Difficulty newMapDifficulty = new Difficulty(MainWindow.map.Difficulty);

            string[] modsSplit = modsUsed.Split(", ");
            for (int i = 0; i < modsSplit.Length; i++)
            {
                if (modsSplit[i] == "HardRock")
                {
                    newMapDifficulty = ModifyHRValues(newMapDifficulty);
                    continue;
                }

                if (modsSplit[i] == "Easy")
                {
                    newMapDifficulty = ModifyEZValues(newMapDifficulty);
                    continue;
                }

                if (modsSplit[i] == "DoubleTime" || modsSplit[i] == "Nightcore")
                {
                    // handled by rate change, still want functions here dont delete
                    //newMapDifficulty = ModifyDTValues(newMapDifficulty);
                    continue;
                }

                if (modsSplit[i] == "HalfTime" || modsSplit[i] == "Daycore")
                {
                    // handled by rate change, still want functions here dont delete
                    //newMapDifficulty = ModifyHTValues(newMapDifficulty);
                    continue;
                }
            }

            MainWindow.map.Difficulty = newMapDifficulty;
        }

        private static Difficulty ModifyHRValues(Difficulty newMapDifficulty)
        {
            decimal newCS = Math.Min(newMapDifficulty.CircleSize * 1.3m, 10);
            newMapDifficulty.CircleSize = newCS;

            decimal newAR = Math.Min(newMapDifficulty.ApproachRate * 1.4m, 10);
            newMapDifficulty.ApproachRate = newAR;

            decimal newOD = Math.Min(newMapDifficulty.OverallDifficulty * 1.4m, 10);
            newMapDifficulty.OverallDifficulty = newOD;

            decimal newHPDrain = Math.Min(newMapDifficulty.HPDrainRate * 1.4m, 10);
            newMapDifficulty.HPDrainRate = newHPDrain;

            // taken from osu lazer code in case there would be some big edge case (there wasnt)
            // for circles - changes vertical position of circle to be opposite of what it was
            // for sliders - same as circle + end position flip + flips control points and ticks
            for (int j = 0; j < MainWindow.map.HitObjects.Count; j++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[j];

                hitObject.Y = 384 - hitObject.SpawnPosition.Y;
                hitObject.SpawnPosition = new Vector2((float)hitObject.X, (float)hitObject.Y);

                if (hitObject is not SliderData slider)
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
                    for (int k = 0; k < slider.SliderTicks.Length; k++)
                    {
                        slider.SliderTicks[k].Position = new Vector2(slider.SliderTicks[k].Position.X, -slider.SliderTicks[k].Position.Y);
                    }
                }
            }

            return newMapDifficulty;
        }

        private static Difficulty ModifyEZValues(Difficulty newMapDifficulty)
        {
            newMapDifficulty.CircleSize = newMapDifficulty.CircleSize * 0.5m;
            newMapDifficulty.ApproachRate = newMapDifficulty.ApproachRate * 0.5m;
            newMapDifficulty.OverallDifficulty = newMapDifficulty.OverallDifficulty * 0.5m;
            newMapDifficulty.HPDrainRate = newMapDifficulty.HPDrainRate * 0.5m;

            return newMapDifficulty;
        }

        private static Difficulty ModifyDTValues(Difficulty newMapDifficulty)
        {
            OsuMath math = new OsuMath();
            double ms = math.GetApproachRateTiming(newMapDifficulty.ApproachRate);
            ms = ms / 1.5;

            // math taken from osu lazer... what even is this monstrocity of math
            double newAr = Math.Sign(ms - 1200) == Math.Sign(450 - 1200)
                         ? (ms - 1200) / (450 - 1200) * 5 + 5
                         : (ms - 1200) / (1200 - 1800) * 5 + 5;
            // for custom speed changes it actually breaks AR values,
            // therefore AR will be always set by speed changing method
            //newMapDifficulty.ApproachRate = (decimal)newAr;

            double greatHitWindow = math.GetOverallDifficultyHitWindow300(newMapDifficulty.OverallDifficulty);
            greatHitWindow = greatHitWindow / 1.5;

            double newOD = Math.Sign(greatHitWindow - 50) == Math.Sign(20 - 50)
                         ? (greatHitWindow - 50) / (20 - 50) * 5 + 5
                         : (greatHitWindow - 50) / (50 - 80) * 5 + 5;
            // ACTUALLY OD might also be affected but will comment it out when testing
            newMapDifficulty.OverallDifficulty = (decimal)newOD;

            return newMapDifficulty;
        }

        private static Difficulty ModifyHTValues(Difficulty newMapDifficulty)
        {
            OsuMath math = new OsuMath();
            double ms = math.GetApproachRateTiming(newMapDifficulty.ApproachRate);
            ms = ms / 0.75;

            // math taken from osu lazer... what even is this monstrocity of math
            double newAr = Math.Sign(ms - 1200) == Math.Sign(450 - 1200)
                         ? (ms - 1200) / (450 - 1200) * 5 + 5
                         : (ms - 1200) / (1200 - 1800) * 5 + 5;
            newMapDifficulty.ApproachRate = (decimal)newAr;

            double greatHitWindow = math.GetOverallDifficultyHitWindow300(newMapDifficulty.OverallDifficulty);
            greatHitWindow = greatHitWindow / 0.75;

            double newOD = Math.Sign(greatHitWindow - 50) == Math.Sign(20 - 50)
                         ? (greatHitWindow - 50) / (20 - 50) * 5 + 5
                         : (greatHitWindow - 50) / (50 - 80) * 5 + 5;
            newMapDifficulty.OverallDifficulty = (decimal)newOD;

            return newMapDifficulty;
        }

        private static Color UpdateComboColour(Color comboColour, List<Color> colours)
        {
            int currentColourIndex = colours.IndexOf(comboColour);

            if (currentColourIndex + 1 > colours.Count - 1)
            {
                currentColourIndex = -1;
            }

            currentColourIndex++;
            return colours[currentColourIndex];
        }
    }
}
