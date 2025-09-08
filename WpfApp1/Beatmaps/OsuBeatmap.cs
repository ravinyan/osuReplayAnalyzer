using NAudio.Mixer;
using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Drawing;
using System.Windows.Controls;
using WpfApp1.Objects;
using WpfApp1.Skins;
using Circle = ReplayParsers.Classes.Beatmap.osu.Objects.Circle;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
using Spinner = ReplayParsers.Classes.Beatmap.osu.Objects.Spinner;

#nullable disable

namespace WpfApp1.Beatmaps
{
    public static class OsuBeatmap
    {
        private static int comboNumber = 0;

        public static Dictionary<long, Canvas> HitObjectDictByTime = new Dictionary<long, Canvas>();
        public static Dictionary<int, Canvas> HitObjectDictByIndex = new Dictionary<int, Canvas>();

        public static Canvas[] Create(Canvas playfieldCanva, Beatmap map)
        {
            Canvas[] hitObjects = new Canvas[MainWindow.map.HitObjects.Count];

            const double AspectRatio = 1.33;
            double height = playfieldCanva.Height / AspectRatio;
            double width = playfieldCanva.Width / AspectRatio;
            double osuScale = Math.Min(playfieldCanva.Width / 512, playfieldCanva.Height / 384);
            double radius = (double)((54.4 - 4.48 * (double)map.Difficulty.CircleSize) * osuScale) * 2;

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

                if (map.HitObjects[i] is Circle)
                {
                    Canvas circle = HitCircle.CreateCircle(map.HitObjects[i], radius, comboNumber, i, comboColour);
                    HitObjectDictByTime.Add(map.HitObjects[i].SpawnTime, circle);
                    HitObjectDictByIndex.Add(i, circle);
                }
                else if (map.HitObjects[i] is Slider)
                {
                    Canvas slider = SliderObject.CreateSlider((Slider)map.HitObjects[i], radius, comboNumber, i, comboColour);
                    HitObjectDictByTime.Add(map.HitObjects[i].SpawnTime, slider);
                    HitObjectDictByIndex.Add(i, slider);
                }
                else if (map.HitObjects[i] is Spinner)
                {
                    Canvas circle = HitCircle.CreateCircle(map.HitObjects[i], radius, comboNumber, i, comboColour);
                    HitObjectDictByTime.Add(map.HitObjects[i].SpawnTime, circle);
                    HitObjectDictByIndex.Add(i, circle);
                }

                comboNumber++;
            }

            return hitObjects;
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
