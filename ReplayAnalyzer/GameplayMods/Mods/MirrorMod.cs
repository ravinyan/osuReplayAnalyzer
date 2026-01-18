using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using System.Numerics;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    public class MirrorMod
    {
        public static void ApplyValues(bool isLazer)
        {
            if (isLazer == true)
            {
                ApplyLazer();
            }
        }

        private static void ApplyLazer()
        {
            LazerMod mirror = MainWindow.replay.LazerMods.Where(mod => mod.Acronym == "MR").First();

            // would use switch but coz of needed count == 0 i dont feel like it
            if (mirror.Settings.Count == 0)
            {
                HorizontalMirror();
            }
            else if (mirror.Settings.ContainsValue("1"))
            {
                VerticalMirror();
            }
            else if (mirror.Settings.ContainsValue("2"))
            {
                VerticalAndHorizontalMirror();
            }
        }

        private static void HorizontalMirror()
        {
            for (int j = 0; j < MainWindow.map.HitObjects.Count; j++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[j];

                hitObject.BaseX = 512 - hitObject.BaseX;
                hitObject.BaseSpawnPosition = new Vector2((float)hitObject.BaseX, (float)hitObject.BaseY);

                if (hitObject is not SliderData slider)
                {
                    continue;
                }

                slider.EndPosition = new Vector2(512 - slider.EndPosition.X, slider.EndPosition.Y);

                for (int k = 0; k < slider.ControlPoints.Length; k++)
                {
                    slider.ControlPoints[k].Position = new Vector2(-slider.ControlPoints[k].Position.X, slider.ControlPoints[k].Position.Y);
                }
                slider.Path = new OsuFileParsers.SliderPathMath.SliderPath(slider);

                if (slider.SliderTicks != null)
                {
                    for (int k = 0; k < slider.SliderTicks.Length; k++)
                    {
                        slider.SliderTicks[k].Position = new Vector2(-slider.SliderTicks[k].Position.X, slider.SliderTicks[k].Position.Y);
                    }
                }
            }
        }

        private static void VerticalMirror()
        {
            for (int j = 0; j < MainWindow.map.HitObjects.Count; j++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[j];

                hitObject.BaseY = 384 - hitObject.BaseY;
                hitObject.BaseSpawnPosition = new Vector2((float)hitObject.BaseX, (float)hitObject.BaseY);

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
        }

        private static void VerticalAndHorizontalMirror()
        {
            for (int j = 0; j < MainWindow.map.HitObjects.Count; j++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[j];

                hitObject.BaseY = 384 - hitObject.BaseY;
                hitObject.BaseX = 512 - hitObject.BaseX;
                hitObject.BaseSpawnPosition = new Vector2((float)hitObject.BaseX, (float)hitObject.BaseY);

                if (hitObject is not SliderData slider)
                {
                    continue;
                }

                slider.EndPosition = new Vector2(512 - slider.EndPosition.X, 384 - slider.EndPosition.Y);

                for (int k = 0; k < slider.ControlPoints.Length; k++)
                {
                    slider.ControlPoints[k].Position = new Vector2(-slider.ControlPoints[k].Position.X, -slider.ControlPoints[k].Position.Y);
                }
                slider.Path = new OsuFileParsers.SliderPathMath.SliderPath(slider);

                if (slider.SliderTicks != null)
                {
                    for (int k = 0; k < slider.SliderTicks.Length; k++)
                    {
                        slider.SliderTicks[k].Position = new Vector2(-slider.SliderTicks[k].Position.X, -slider.SliderTicks[k].Position.Y);
                    }
                }
            }
        }
    }
}
