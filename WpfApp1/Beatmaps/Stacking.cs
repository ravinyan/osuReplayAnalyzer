using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Numerics;
using WpfApp1.OsuMaths;
#nullable disable

namespace WpfApp1.Beatmaps
{
    public class Stacking
    {
        OsuMath math = new OsuMath();
        int StackDistance = 3;

        public void ApplyStacking(Beatmap map)
        {
            List<HitObject> hitObjects = new List<HitObject>();

            if (map.FileVersion >= 6)
            {
                ApplyStackingNew(map, hitObjects);
            }
            else
            {
                ApplyStackingOld(map);
            }

            foreach (HitObject hitObject in map.HitObjects)
            {
                if (hitObject.StackHeight > 0)
                {
                    float scale = math.CalculateScaleFromCircleSize(map.Difficulty.CircleSize);
                    Vector2 stackOFfset = new Vector2(hitObject.StackHeight * scale * -6.4f);

                    hitObject.X -= (int)Math.Floor((decimal)stackOFfset.X);
                    hitObject.Y -= (int)Math.Floor((decimal)stackOFfset.Y);
                }
            }
        }

        // trying to understand and do this
        // https://github.com/ppy/osu/blob/master/osu.Game.Rulesets.Osu/Beatmaps/OsuBeatmapProcessor.cs
        void ApplyStackingNew(Beatmap map, List<HitObject> objects)
        {
            int startIndex = 0;
            int endIndex = map.HitObjects.Count - 1;
            int extendedEndIndex = endIndex;
            int extendedStartIndex = startIndex;

            for (int i = extendedEndIndex; i > startIndex; i--)
            {
                int n = i;

                HitObject objectI = map.HitObjects[i];

                if (objectI.StackHeight == 0 && objectI is Spinner)
                {
                    continue;
                }

                decimal stackTreshold = math.GetApproachRateTiming(map.Difficulty.ApproachRate) * map.General.StackLeniency;

                if (objectI is Circle)
                {
                    while (--n >= 0)
                    {
                        HitObject objectN = map.HitObjects[n];

                        if (objectI is Spinner)
                        {
                            continue;
                        }

                        decimal endTime = GetEndTime(objectN, map);

                        if (objectI.Time - endTime > stackTreshold)
                        {
                            break;
                        }

                        if (n < extendedStartIndex)
                        {
                            objectN.StackHeight = 0;
                            extendedStartIndex = n;
                        }

                        if (objectN is Slider && GetDistance((Slider)objectN, objectI) < StackDistance)
                        {
                            int offset = objectI.StackHeight - (objectN.StackHeight + 1);

                            for (int j = n + 1; j <= i; j++)
                            {
                                HitObject objectJ = map.HitObjects[j];
                                if (GetDistance((Slider)objectN, objectJ) < StackDistance)
                                {
                                    objectJ.StackHeight -= offset;
                                }
                            }

                            break;
                        }

                        if (GetDistance(objectN, objectI) < StackDistance)
                        {
                            objectN.StackHeight = objectI.StackHeight + 1;
                            objectI = objectN;
                        }
                    }
                }
                else if (objectI is Slider)
                {
                    while (--n >= startIndex)
                    {
                        HitObject objectN = map.HitObjects[n];

                        if (objectN is Spinner)
                        {
                            continue;
                        }

                        if (objectI.Time - objectN.Time > stackTreshold)
                        {
                            break;
                        }

                        if (GetDistance((Slider)objectN, objectI) < StackDistance)
                        {
                            objectN.StackHeight = objectI.StackHeight + 1;
                            objectI = objectN;
                        }
                    }
                }
            }
            
        }

        // will do later
        private void ApplyStackingOld(Beatmap map)
        {

        }

        private float GetDistance(HitObject o1, HitObject o2)
        {
            if (o1 is Slider)
            {
                Vector2 ep = GetEndPosition(o1 as Slider);
                
                return MathF.Sqrt((o2.X - ep.X) * (o2.X - ep.X) + (o2.Y - ep.Y) * (o2.Y - ep.Y));
            }

            return MathF.Sqrt((o2.X - o1.X) * (o2.X - o1.X) + (o2.Y - o1.Y) * (o2.Y - o1.Y));
        }

        private Vector2 GetEndPosition(Slider slider)
        {
            if (slider.Slides % 2 == 1)
            {
                return slider.CurvePoints[0];
            }
            else
            {
                return slider.CurvePoints[slider.CurvePoints.Count - 1];
            }  
        }

        private decimal GetEndTime(HitObject hitObject, Beatmap map)
        {
            if (hitObject is Slider)
            {
                Slider a = hitObject as Slider;
                int repeats = a.Slides + 1;
                return a.Time + (repeats * a.Length) / map.Difficulty.SliderMultiplier;
            }

            return hitObject.Time;
        }
    }
}
