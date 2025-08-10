using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
using Circle = ReplayParsers.Classes.Beatmap.osu.Objects.Circle;
using Spinner = ReplayParsers.Classes.Beatmap.osu.Objects.Spinner;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Objects;

#nullable disable

namespace WpfApp1.Beatmaps
{
    public static class OsuBeatmap
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static int comboNumber = 0;

        public static void Create()
        {
            const double AspectRatio = 1.33;
            double height = Window.playfieldCanva.Height / AspectRatio;
            double width = Window.playfieldCanva.Width / AspectRatio;
            double osuScale = Math.Min(Window.playfieldCanva.Width / 512, Window.playfieldCanva.Height / 384);
            double radius = (double)((54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * osuScale) * 2;

            Stacking stacking = new Stacking();
            stacking.ApplyStacking(MainWindow.map);
            
            for (int i = 0; i < MainWindow.map.HitObjects.Count; i++)
            {
                if (MainWindow.map.HitObjects[i].Type.HasFlag(ObjectType.StartNewCombo))
                {
                    comboNumber = 1;
                }

                if (MainWindow.map.HitObjects[i] is Circle)
                {
                    Canvas circle = HitCircle.CreateCircle(MainWindow.map.HitObjects[i], radius, comboNumber, osuScale, i);
                    Window.playfieldCanva.Children.Add(circle);
                }
                else if (MainWindow.map.HitObjects[i] is Slider)
                {
                    Canvas slider = SliderObject.CreateSlider((Slider)MainWindow.map.HitObjects[i], radius, comboNumber, osuScale, i);
                    Window.playfieldCanva.Children.Add(slider);
                }
                else if (MainWindow.map.HitObjects[i] is Spinner)
                {
                    Canvas circle = HitCircle.CreateCircle(MainWindow.map.HitObjects[i], radius, comboNumber, osuScale, i);
                    Window.playfieldCanva.Children.Add(circle);
                }

                comboNumber++;
            }
        }
    }
}
