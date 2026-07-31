using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using OsuFileParsers.SliderPathMath;
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

            if (mirror.Settings.Count == 0)
            {
                // taiko doesnt have one + only horizontal flip exists for non osu game modes
                if (MainWindow.replay.GameMode == GameMode.Osu)
                {
                    HorizontalMirror();
                }
                else if (MainWindow.replay.GameMode == GameMode.OsuMania)
                {
                    ManiaHorizontalMirror();
                }
                else if (MainWindow.replay.GameMode == GameMode.OsuCatch)
                {
                    CatchHorizontalMirror();
                }
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

        private static void ManiaHorizontalMirror()
        {
            for (int j = 0; j < MainWindow.map.HitObjects.Count; j++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[j];
                if (hitObject is ManiaNoteData)
                {
                    ManiaNoteData n = (ManiaNoteData)hitObject;
                    n.ColumnIndex = (int)(MainWindow.map.Difficulty.CircleSize - 1) - n.ColumnIndex;
                }
                else if (hitObject is ManiaLongNoteData)
                {
                    ManiaLongNoteData ln = (ManiaLongNoteData)hitObject;
                    ln.ColumnIndex = (int)(MainWindow.map.Difficulty.CircleSize - 1) - ln.ColumnIndex;
                }
            }
        }

        private static void CatchHorizontalMirror()
        {
            for (int j = 0; j < MainWindow.map.HitObjects.Count; j++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[j];

                if (hitObject is CatchFruitData)
                {
                    CatchFruitData fruit = (CatchFruitData)hitObject;
                    fruit.X = 512 - fruit.X;

                    continue;
                }
                
                if (hitObject is CatchJuiceStreamData)
                {
                    CatchJuiceStreamData slider = (CatchJuiceStreamData)hitObject;

                    slider.X = 512 - slider.X;
                    slider.EndXPosition = -slider.EndXPosition;

                    var controlPoints = slider.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
                    for (int k = 0; k < slider.Path.ControlPoints.Count; k++)
                    {
                        controlPoints[k].Position = new Vector2(-controlPoints[k].Position.X, controlPoints[k].Position.Y);
                    }
                    slider.Path = new SliderPath(controlPoints, slider.Path.ExpectedDistance);

                    if (slider.Drops != null)
                    {
                        for (int k = 0; k < slider.Drops.Count; k++)
                        {
                            slider.Drops[k].Position = new Vector2(-slider.Drops[k].Position.X, slider.Drops[k].Position.Y);
                        }
                    }
                }
            }
        }

        private static void HorizontalMirror()
        {
            for (int j = 0; j < MainWindow.map.HitObjects.Count; j++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[j];

                hitObject.BaseX = 512 - hitObject.BaseX;
                hitObject.BaseSpawnPosition = new Vector2((float)hitObject.BaseX, (float)hitObject.BaseY);

                if (hitObject is not OsuSliderData slider)
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
                    for (int k = 0; k < slider.SliderTicks.Count; k++)
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

        private static void VerticalAndHorizontalMirror()
        {
            for (int j = 0; j < MainWindow.map.HitObjects.Count; j++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[j];

                hitObject.BaseY = 384 - hitObject.BaseY;
                hitObject.BaseX = 512 - hitObject.BaseX;
                hitObject.BaseSpawnPosition = new Vector2((float)hitObject.BaseX, (float)hitObject.BaseY);

                if (hitObject is not OsuSliderData slider)
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
                    for (int k = 0; k < slider.SliderTicks.Count; k++)
                    {
                        slider.SliderTicks[k].Position = new Vector2(-slider.SliderTicks[k].Position.X, -slider.SliderTicks[k].Position.Y);
                    }
                }
            }
        }
    }
}
