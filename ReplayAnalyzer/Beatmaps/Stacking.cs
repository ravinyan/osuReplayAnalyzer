using OsuFileParsers.Classes.Beatmap.osu;
using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.OsuMaths;
using System.Numerics;
using ReplayAnalyzer;

#nullable disable

namespace ReplayAnalyzer.Beatmaps
{
    public class Stacking
    {
        OsuMath math = new OsuMath();
        double StackDistance = 3;

        public void ApplyStacking(Beatmap map)
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
                    float scale = (float)(1.0f - 0.7f * (((float)MainWindow.map.Difficulty.CircleSize - 5 ) / 5)) / 2 * 1.00041f;
        
                    Vector2 stackOFfset = new Vector2(hitObject.StackHeight * scale * -6.4f);
                    hitObject.X += Math.Ceiling(stackOFfset.X);
                    hitObject.Y += Math.Ceiling(stackOFfset.Y);
                }
            }
        }

        // trying to understand and do this
        // https://github.com/ppy/osu/blob/master/osu.Game.Rulesets.Osu/Beatmaps/OsuBeatmapProcessor.cs
        void ApplyStackingNew(Beatmap map, List<HitObjectData> objects)
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

                double stackTreshold = math.GetApproachRateTiming() * (double)map.General.StackLeniency;

                if (objectI is CircleData)
                {
                    while (--n >= 0)
                    {
                        HitObjectData objectN = map.HitObjects[n];

                        if (objectI is SpinnerData)
                        {
                            continue;
                        }

                        double endTime = math.GetSliderEndTime(objectN, map.Difficulty.SliderMultiplier);

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

        private void ApplyStackingOld(Beatmap map)
        {
            for (int i = 0; i < map.HitObjects.Count; i++)
            {
                HitObjectData currHitObject = map.HitObjects[i];

                if (currHitObject.StackHeight != 0 && !(currHitObject is SliderData))
                {
                    continue;
                }

                double startTime = math.GetSliderEndTime(currHitObject, map.Difficulty.SliderMultiplier);
                int sliderStack = 0;

                for (int j = i + 1; j < map.HitObjects.Count; j++)
                {
                    HitObjectData hitObjectJ = map.HitObjects[j];
                    double stackTreshold = math.GetApproachRateTiming() * (double)map.General.StackLeniency;

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

        private float GetDistance(HitObjectData o1, Vector2 o2)
        {
            if (o1 is SliderData)
            {
                SliderData s = o1 as SliderData;
                Vector2 ep = s.EndPosition;
                
                return MathF.Sqrt((o2.X - ep.X) * (o2.X - ep.X) + (o2.Y - ep.Y) * (o2.Y - ep.Y));
            }

            return MathF.Sqrt((float)((o2.X - o1.X) * (o2.X - o1.X) + (o2.Y - o1.Y) * (o2.Y - o1.Y)));
        }
    }
}
