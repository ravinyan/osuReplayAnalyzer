using OsuFileParsers.Classes.Beatmap.osu;
using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using System.Numerics;

#nullable disable

namespace OsuFileParsers.Stacking
{
    public class Stacking
    {
        private static double StackDistance = 3;

        public static void ApplyStacking(Beatmap map)
        {
            List<HitObjectData> hitObjects = new List<HitObjectData>();

            if (map.FileVersion >= 6)
            {
                ApplyStackingNew(map, hitObjects);
            }
            else
            {
                ApplyStackingOld(map);
            }

            foreach (HitObjectData hitObject in map.HitObjects)
            {
                if (hitObject.StackHeight > 0)
                {
                    // math from osu lazer
                    float scale = (float)(1.0f - 0.7f * (((float)map.Difficulty.CircleSize - 5) / 5)) / 2;

                    Vector2 stackOFfset = new Vector2(hitObject.StackHeight * scale * -6.4f);
                    hitObject.BaseX += Math.Ceiling(stackOFfset.X);
                    hitObject.BaseY += Math.Ceiling(stackOFfset.Y);
                }
            }
        }

        // trying to understand and do this
        // https://github.com/ppy/osu/blob/master/osu.Game.Rulesets.Osu/Beatmaps/OsuBeatmapProcessor.cs
        private static void ApplyStackingNew(Beatmap map, List<HitObjectData> objects)
        {
            int startIndex = 0;
            int endIndex = map.HitObjects.Count - 1;
            int extendedEndIndex = endIndex;
            int extendedStartIndex = startIndex;

            for (int i = extendedEndIndex; i > startIndex; i--)
            {
                int n = i;

                HitObjectData objectI = map.HitObjects[i];

                if (objectI.StackHeight == 0 && objectI is SpinnerData)
                {
                    continue;
                }

                double stackTreshold = GetApproachRateTiming(map.Difficulty.ApproachRate) * (double)map.General.StackLeniency;

                if (objectI is CircleData)
                {
                    while (--n >= 0)
                    {
                        HitObjectData objectN = map.HitObjects[n];

                        if (objectI is SpinnerData)
                        {
                            continue;
                        }

                        double endTime = GetSliderEndTime(objectN, map.Difficulty.SliderMultiplier);

                        if (objectI.SpawnTime - endTime > stackTreshold)
                        {
                            break;
                        }

                        if (n < extendedStartIndex)
                        {
                            objectN.StackHeight = 0;
                            extendedStartIndex = n;
                        }

                        if (objectN is SliderData && GetDistance(objectN, objectI.SpawnPosition) < StackDistance)
                        {
                            int offset = objectI.StackHeight - (objectN.StackHeight + 1);

                            for (int j = n + 1; j <= i; j++)
                            {
                                HitObjectData objectJ = map.HitObjects[j];
                                if (GetDistance(objectN, objectJ.SpawnPosition) < StackDistance)
                                {
                                    objectJ.StackHeight -= offset;
                                }
                            }

                            break;
                        }

                        if (GetDistance(objectN, objectI.SpawnPosition) < StackDistance)
                        {
                            objectN.StackHeight = objectI.StackHeight + 1;
                            objectI = objectN;
                        }
                    }
                }
                else if (objectI is SliderData)
                {
                    while (--n >= startIndex)
                    {
                        HitObjectData objectN = map.HitObjects[n];

                        if (objectN is SpinnerData)
                        {
                            continue;
                        }

                        if (objectI.SpawnTime - objectN.SpawnTime > stackTreshold)
                        {
                            break;
                        }

                        if (GetDistance(objectN, objectI.SpawnPosition) < StackDistance)
                        {
                            objectN.StackHeight = objectI.StackHeight + 1;
                            objectI = objectN;
                        }
                    }
                }
            }
        }

        private static void ApplyStackingOld(Beatmap map)
        {
            for (int i = 0; i < map.HitObjects.Count; i++)
            {
                HitObjectData currHitObject = map.HitObjects[i];

                if (currHitObject.StackHeight != 0 && !(currHitObject is SliderData))
                {
                    continue;
                }

                double startTime = GetSliderEndTime(currHitObject, map.Difficulty.SliderMultiplier);
                int sliderStack = 0;

                for (int j = i + 1; j < map.HitObjects.Count; j++)
                {
                    HitObjectData hitObjectJ = map.HitObjects[j];
                    double stackTreshold = GetApproachRateTiming(map.Difficulty.ApproachRate) * (double)map.General.StackLeniency;

                    if (hitObjectJ.SpawnTime - stackTreshold > startTime)
                    {
                        break;
                    }

                    Vector2 position2 = currHitObject is SliderData currSlider
                        ? currSlider.SpawnPosition + currSlider.Path.PositionAt(1)
                        : currHitObject.SpawnPosition;

                    if (GetDistance(hitObjectJ, currHitObject.SpawnPosition) < StackDistance)
                    {
                        currHitObject.StackHeight++;
                        startTime = hitObjectJ.SpawnTime;
                    }
                    else if (GetDistance(hitObjectJ, position2) < StackDistance)
                    {
                        sliderStack++;
                        hitObjectJ.StackHeight -= sliderStack;
                        startTime = hitObjectJ.SpawnTime;
                    }
                }
            }
        }

        private static float GetDistance(HitObjectData o1, Vector2 o2)
        {
            if (o1 is SliderData)
            {
                SliderData s = o1 as SliderData;
                Vector2 ep = s.EndPosition;

                return MathF.Sqrt((o2.X - ep.X) * (o2.X - ep.X) + (o2.Y - ep.Y) * (o2.Y - ep.Y));
            }

            return MathF.Sqrt((float)((o2.X - o1.BaseX) * (o2.X - o1.BaseX) + (o2.Y - o1.BaseY) * (o2.Y - o1.BaseY)));
        }

        private static double GetApproachRateTiming(decimal approachRate)
        {
            if (approachRate < 5)
            {
                return (double)(1200 + 600 * (5 - approachRate) / 5);
            }
            else if (approachRate == 5)
            {
                return 1200;
            }
            else
            {
                return (double)(1200 - 750 * (approachRate - 5) / 5);
            }
        }

        private static double GetSliderEndTime(HitObjectData hitObject, decimal sliderMultiplayer)
        {
            if (hitObject is SliderData)
            {
                SliderData a = hitObject as SliderData;
                int repeats = a.RepeatCount + 1;
                return (double)(a.SpawnTime + repeats * a.Length / sliderMultiplayer);
            }

            return hitObject.SpawnTime;
        }
    }
}
