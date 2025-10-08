using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using System.Drawing;
using System.Windows.Controls;
using WpfApp1.Objects;
using WpfApp1.OsuMaths;
using WpfApp1.Skins;
using CircleData = ReplayParsers.Classes.Beatmap.osu.Objects.CircleData;
using SliderData = ReplayParsers.Classes.Beatmap.osu.Objects.SliderData;
using SpinnerData = ReplayParsers.Classes.Beatmap.osu.Objects.SpinnerData;

#nullable disable

namespace WpfApp1.Beatmaps
{
    public static class OsuBeatmap
    {
        private static int comboNumber = 0;

        public static Dictionary<long, HitObject> HitObjectDictByTime = new Dictionary<long, HitObject>();
        public static Dictionary<int, HitObject> HitObjectDictByIndex = new Dictionary<int, HitObject>();

        public static Canvas[] Create(Canvas playfieldCanva, Beatmap map)
        {
            Canvas[] hitObjects = new Canvas[MainWindow.map.HitObjects.Count];

            double baseCircleRadius = (54.4 - 4.48 * (double)map.Difficulty.CircleSize) * 2;

            Stacking stacking = new Stacking();
            stacking.ApplyStacking(map);

            List<Color> colours = SkinIniProperties.GetComboColours();
            Color comboColour = Color.Transparent;
            
            for (int i = 0; i < map.HitObjects.Count; i++)
            {
                if (map.HitObjects[i].Type.HasFlag(ObjectType.StartNewCombo))
                {
                    if (comboColour != Color.Transparent)
                    {
                        comboColour = UpdateComboColour(comboColour, colours);
                    }
                    else
                    {
                        comboColour = colours[0];
                    }

                    comboNumber = 1;
                }

                if (map.HitObjects[i] is CircleData)
                {
                    HitCircle circle = HitCircle.CreateCircle((CircleData)map.HitObjects[i], baseCircleRadius, comboNumber, i, comboColour);

                    HitObjectDictByTime.Add(circle.SpawnTime, circle);
                    HitObjectDictByIndex.Add(i, circle);
                }
                else if (map.HitObjects[i] is SliderData)
                {
                    Sliderr slider = Sliderr.CreateSlider((SliderData)map.HitObjects[i], baseCircleRadius, comboNumber, i, comboColour);
                    HitObjectDictByTime.Add(slider.SpawnTime, slider);
                    HitObjectDictByIndex.Add(i, slider);
                }
                else if (map.HitObjects[i] is SpinnerData)
                {
                    Spinnerr spinner = Spinnerr.CreateSpinner((SpinnerData)map.HitObjects[i], baseCircleRadius, i);
                    HitObjectDictByTime.Add(spinner.SpawnTime, spinner);
                    HitObjectDictByIndex.Add(i, spinner);
                }

                comboNumber++;
            }

            return hitObjects;
        }

        public static void ModifyDifficultyValues(string modsUsed)
        {
            if (modsUsed == null)
            {
                return;
            }

            string[] modsSplit = modsUsed.Split(", ");
            Beatmap newMap = MainWindow.map;

            for (int i = 0; i < modsSplit.Length; i++)
            {
                if (modsSplit[i] == "HardRock")
                {
                    decimal newCS = MainWindow.map.Difficulty.CircleSize * 1.3m;
                    if (newCS > 10)
                    {
                        newCS = 10;
                    }
                    newMap.Difficulty.CircleSize = newCS;

                    decimal newAR = MainWindow.map.Difficulty.ApproachRate * 1.4m;
                    if (newAR > 10)
                    {
                        newAR = 10;
                    }
                    newMap.Difficulty.ApproachRate = newAR;

                    decimal newOD = MainWindow.map.Difficulty.OverallDifficulty * 1.4m;
                    if (newOD > 10)
                    {
                        newOD = 10;
                    }
                    newMap.Difficulty.OverallDifficulty = newOD;
                }

                if (modsSplit[i] == "DoubleTime" || modsSplit[i] == "Nightcore")
                {
                    OsuMath math = new OsuMath();

                    decimal ms = (decimal)math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
                    ms = ms / 1.5m;

                    decimal newAR = 0;
                    if (ms < 1200)
                    {
                        newAR = 11 - (ms - 300) / 150;
                    }
                    else
                    {
                        newAR = 11 - (ms - 300) / 120;
                    }

                    if (newAR > 11)
                    {
                        newAR = 11;
                    }
                    newMap.Difficulty.ApproachRate = newAR;

                    //decimal newOD = newMap.Difficulty.OverallDifficulty * 1.33m;
                    //if (newOD > 11.11m)
                    //{
                    //    newOD = 11.11m;
                    //}
                }

                if (modsSplit[i] == "Easy")
                {

                }

                if (modsSplit[i] == "HalfTime")
                {

                }
            }

            MainWindow.map = newMap;
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
