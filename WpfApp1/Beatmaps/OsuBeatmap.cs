using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
using Circle = ReplayParsers.Classes.Beatmap.osu.Objects.Circle;
using Spinner = ReplayParsers.Classes.Beatmap.osu.Objects.Spinner;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Objects;
using ReplayParsers.Classes.Beatmap.osu;
using System.Windows.Threading;

#nullable disable

namespace WpfApp1.Beatmaps
{
    public static class OsuBeatmap
    {
        private static int comboNumber = 0;

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
            
            for (int i = 0; i < map.HitObjects.Count; i++)
            {
                if (map.HitObjects[i].Type.HasFlag(ObjectType.StartNewCombo))
                {
                    comboNumber = 1;
                }

                if (map.HitObjects[i] is Circle)
                {
                    Canvas circle = HitCircle.CreateCircle(map.HitObjects[i], radius, comboNumber, osuScale, i);
                    playfieldCanva.Children.Add(circle);
                }
                else if (map.HitObjects[i] is Slider)
                {
                    Canvas slider = SliderObject.CreateSlider((Slider)map.HitObjects[i], radius, comboNumber, osuScale, i);
                    playfieldCanva.Children.Add(slider);
                }
                else if (map.HitObjects[i] is Spinner)
                {
                    Canvas circle = HitCircle.CreateCircle(map.HitObjects[i], radius, comboNumber, osuScale, i);
                    playfieldCanva.Children.Add(circle);
                }

                comboNumber++;
            }

            return hitObjects;
        }
    }
}
