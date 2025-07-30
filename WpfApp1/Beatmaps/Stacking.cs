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

                double stackTreshold = (double)(math.GetApproachRateTiming(map.Difficulty.ApproachRate) * map.General.StackLeniency);

                if (objectI is Circle)
                {
                    while (--n >= 0)
                    {
                        HitObject objectN = map.HitObjects[n];

                        if (objectI is Spinner)
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

                        if (objectN is Slider && GetDistance(objectN, objectI.SpawnPosition) < StackDistance)
                        {
                            int offset = objectI.StackHeight - (objectN.StackHeight + 1);

                            for (int j = n + 1; j <= i; j++)
                            {
                                HitObject objectJ = map.HitObjects[j];
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
                else if (objectI is Slider)
                {
                    while (--n >= startIndex)
                    {
                        HitObject objectN = map.HitObjects[n];

                        if (objectN is Spinner)
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
                HitObject currHitObject = map.HitObjects[i];

                if (currHitObject.StackHeight != 0 && !(currHitObject is Slider))
                {
                    continue;
                }

                double startTime = math.GetSliderEndTime(currHitObject, map.Difficulty.SliderMultiplier);
                int sliderStack = 0;

                for (int j = i + 1; j < map.HitObjects.Count; j++)
                {
                    HitObject hitObjectJ = map.HitObjects[j];
                    double stackTreshold = (double)(math.GetApproachRateTiming(map.Difficulty.ApproachRate) * map.General.StackLeniency);

                    if (hitObjectJ.SpawnTime - stackTreshold > startTime)
                    {
                        break;
                    }

                    Vector2 position2 = currHitObject is Slider currSlider
                        ? currSlider.SpawnPosition + GetEndPosition(currSlider)
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

        private float GetDistance(HitObject o1, Vector2 o2)
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
            if (slider.RepeatCount % 2 == 1)
            {
                return slider.CurvePoints[0];
            }
            else
            {
                return slider.CurvePoints[slider.CurvePoints.Count - 1];
            }  
        }
    }
}
