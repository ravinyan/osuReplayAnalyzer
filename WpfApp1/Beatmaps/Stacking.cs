using NAudio.Wave;
using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Windows;
using WpfApp1.OsuMaths;
#nullable disable

namespace WpfApp1.Beatmaps
{
    public class Stacking
    {
        OsuMath math = new OsuMath();
        int StackDistance = 3;

        private Dictionary<string, int> stackingHeights = new Dictionary<string, int>();

        void ApplyStacking(Beatmap map)
        {
            List<HitObject> hitObjects = new List<HitObject>();

            if (hitObjects.Count > 0 )
            {

            }
        }

        // trying to understand and do this
        // https://github.com/ppy/osu/blob/master/osu.Game.Rulesets.Osu/Beatmaps/OsuBeatmapProcessor.cs
        void ApplyStackingNew(Beatmap map)
        {
            int startIndex = 0;
            int endIndex = map.HitObjects.Count - 1;
            int extendedEndIndex = endIndex;
            int extendedStartIndex = startIndex;

            for (int i = extendedEndIndex; i > startIndex; i--)
            {
                int n = i;

                HitObject objectI = map.HitObjects[i];

                // stackheight != 0 ||
                if (objectI.Type.HasFlag(ObjectType.Spinner))
                {
                    continue;
                }

                decimal stackTreshold = math.GetApproachRateTiming(map.Difficulty.ApproachRate) * map.General.StackLeniency;

                if (objectI.Type.HasFlag(ObjectType.HitCircle))
                {
                    while (--n >= 0)
                    {
                        HitObject objectN = map.HitObjects[n];

                        if (objectI.Type.HasFlag(ObjectType.Spinner))
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
                            //objectN stackHeight = 0;
                            extendedStartIndex = n;
                        }

                        if (objectN is Slider && GetDistance((Slider)objectN, objectI) < StackDistance)
                        {

                        }
                    }
                }
            }
            
        }

        private float GetDistance(HitObject o1, HitObject o2)
        {
            if (o1 is Slider)
            {
                o1 = (Slider)o1;
                
            }

            return MathF.Sqrt((o2.X - o1.X) * (o2.X - o1.X) + (o2.Y - o1.Y) * (o2.Y - o1.Y));
        }

        private int GetEndPosition(Slider slider)
        {
            return slider.CurvePoints[1];
        }

        private static decimal GetEndTime(HitObject hitObject, Beatmap map)
        {
            if (hitObject is Slider)
            {
                Slider a = hitObject as Slider;
                int repeats = a.Slides + 1;
                return a.Time + (repeats * a.Length) / map.Difficulty.SliderMultiplier;
            }
            else if (hitObject is Spinner)
            {
                Spinner a = hitObject as Spinner;
                return a.EndTime;
            }

            return hitObject.Time;
        }
    }
}
