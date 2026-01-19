using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using System.Globalization;
using System.Numerics;
using System.Windows;

namespace ReplayAnalyzer.GameplayMods
{
    // i know im just copying formula of osu lazer but its still just too much effort for something i doubt much people will use... why am i even doing it?
    public class RandomMod
    {
        private static Random rng;

        public static void ApplyValues(bool isLazer)
        {
            if (isLazer == true)
            {
                ApplyLazer();
            }
        }

        private static void ApplyLazer()
        {
            LazerMod random = MainWindow.replay.LazerMods.Where(mod => mod.Acronym == "RD").First();

            // code from osu lazer with some comments just in case
            int seedValue = int.Parse((string)random.Settings["seed"]);
            rng = new Random(seedValue);

            double angleSharpness;
            if (random.Settings.ContainsKey("angle_sharpness"))
            {
                angleSharpness = double.Parse((string)random.Settings["angle_sharpness"], CultureInfo.InvariantCulture.NumberFormat);
            }
            else
            {   // default
                angleSharpness = 7;
            }

            List<ObjectPositionInfo> positions = ObjectPositionInfo.GeneratePositionInfos(MainWindow.map.HitObjects);

            // Whether the angles are positive or negative (clockwise or counter-clockwise flow).
            bool flowDirection = false;
            float sectionOffset = 0;

            for (int i = 0; i < positions.Count; i++)
            {
                if (ShouldStartNewSection(positions, i))
                {
                    sectionOffset = GetRandomOffset(0.0008f, angleSharpness);
                    flowDirection = !flowDirection;
                }

                if ("object is slider" == "" && rng.NextDouble() < 0.5)
                {
                    FlipSliderHorizontally(new SliderData());
                }

                if (i == 0)
                {   //                                                       base playfield height
                    positions[i].DistanceFromPrevious = (float)(rng.NextDouble() * 384 / 2);
                    positions[i].RelativeAngle = (float)(rng.NextDouble() * 2 * Math.PI - Math.PI);
                }
                else
                {
                    // Offsets only the angle of the current hit object if a flow change occurs.
                    float flowChangeOffset = 0;

                    // Offsets only the angle of the current hit object.
                    float oneTimeOffset = GetRandomOffset(0.002f, angleSharpness);

                    if (ShouldStartNewSection(positions, i) == true)
                    {
                        flowChangeOffset = GetRandomOffset(0.002f, angleSharpness);
                        flowDirection = !flowDirection;
                    }

                    // what the hell is this even
                                        // sectionOffset and oneTimeOffset should mainly affect patterns with large spacing.
                    float totalOffset = (sectionOffset + oneTimeOffset) * positions[i].DistanceFromPrevious +
                                        // flowChangeOffset should mainly affect streams.
                                        flowChangeOffset * (640.995056f - positions[i].DistanceFromPrevious);
                    // some math thing in osu lazer  ^  named playfield_diagonal and its calculated from BASE playfield size that never changes

                    positions[i].RelativeAngle = GetRelativeTargetAngle(positions[i].DistanceFromPrevious, totalOffset, flowDirection, angleSharpness);
                }
            }

            // reposition objects
            foreach (HitObjectData hitObject in MainWindow.map.HitObjects)
            {

            }
        }

        // wat to do
        private static bool ShouldStartNewSection(List<ObjectPositionInfo> positions, int i)
        {
            if (i == 0)
                return true;
            
            // Exclude new-combo-spam and 1-2-combos.
            bool previousObjectStartedCombo = positions[Math.Max(0, i - 2)].HitObject.ComboNumber > 1 &&
                                              positions[i - 1].HitObject.Type.HasFlag(ObjectType.StartNewCombo);
            bool previousObjectWasOnDownbeat = IsHitObjectOnBeat(positions[i - 1].HitObject, true);
            bool previousObjectWasOnBeat = IsHitObjectOnBeat(positions[i - 1].HitObject);
            
            return (previousObjectStartedCombo && rng.NextDouble() < 0.6f) ||
                   previousObjectWasOnDownbeat ||
                   (previousObjectWasOnBeat && rng.NextDouble() < 0.4f);

            bool IsHitObjectOnBeat(HitObjectData hitObject, bool downbeatsOnly = false)
            {
                TimingPoint timingPoint = MainWindow.map.TimingPoints.FirstOrDefault(t => t.Time == hitObject.SpawnTime) ?? null;

                if (timingPoint == null)
                {
                    MessageBox.Show("you stupid");
                    return false;
                }

                double timeSinceTimingPoint = hitObject.SpawnTime - (double)timingPoint.Time;

                double beatLength = timingPoint.BeatLength;

                if (downbeatsOnly)
                {
                    // i think its hard coded for this and if they 
                    beatLength *= timingPoint.Meter;
                }
                    
                // Ensure within 1ms of expected location.
                return Math.Abs(timeSinceTimingPoint + 1) % beatLength < 2;
            }
        }

        private static float GetRandomOffset(float stdDev, double angleSharpness)
        {
            float customMultiplayer = (float)(1.5f * 10 - angleSharpness) / (1.5f * 10 - 7);

            // Generate 2 random numbers in the interval (0,1].
            // x1 must not be 0 since log(0) = undefined.
            Random rng = new Random();
            double x1 = 1 - rng.NextDouble();
            double x2 = 1 - rng.NextDouble();

            double stdNormal = Math.Sqrt(-2 * Math.Log(x1)) * Math.Sin(2 * Math.PI * x2);
            return 0 + (stdDev * customMultiplayer) * (float)stdNormal;
        }

        // im not even writing this out i hate math what even is this
        private static float GetRelativeTargetAngle(float targetDistance, float offset, bool flowDirection, double curAngleSharpness)
        {
            // Range: [0.1, 1]
            float angleSharpness = (float)(curAngleSharpness / 10.0);
            // Range: [0, 0.9]
            float angleWideness = 1 - angleSharpness;

            // Range: [-60, 30]
            float customOffsetX = angleSharpness * 100 - 70;
            // Range: [-0.075, 0.15]
            float customOffsetY = angleWideness * 0.25f - 0.075f;

            targetDistance += customOffsetX;
            float angle = (float)(2.16 / (1 + 200 * Math.Exp(0.036 * (targetDistance - 310 + customOffsetX))) + 0.5);
            angle += offset + customOffsetY;

            float relativeAngle = (float)Math.PI - angle;

            return flowDirection ? -relativeAngle : relativeAngle;
        }

        private static void FlipSliderHorizontally(SliderData slider)
        {

        }

        private class ObjectPositionInfo
        {
            public ObjectPositionInfo(HitObjectData hitObject)
            {
                HitObject = hitObject;
            }

            public float RelativeAngle { get; set; }
            public float DistanceFromPrevious { get; set; }
            public float Rotation { get; set; }
            public HitObjectData? HitObject { get; set; }

            public static List<ObjectPositionInfo> GeneratePositionInfos(List<HitObjectData> hitObjects)
            {
                List<ObjectPositionInfo> positionInfos = new List<ObjectPositionInfo>();

                // center of playfield as first pos
                Vector2 previousPos = new Vector2(512 / 2, 384 / 2);
                float previousAngle = 0;

                foreach (HitObjectData hitObject in hitObjects)
                {
                    Vector2 relativePos = hitObject.BaseSpawnPosition = previousPos;
                    float absoluteAngle = MathF.Atan2(relativePos.Y, relativePos.X);
                    float relativeAngle = absoluteAngle - previousAngle;

                    ObjectPositionInfo positionInfo;
                    positionInfos.Add(positionInfo = new ObjectPositionInfo(hitObject)
                    {
                        RelativeAngle = relativeAngle,
                        DistanceFromPrevious = relativePos.Length(),
                    });

                    if (hitObject is SliderData slider)
                    {
                        float absoluteRotation = GetSliderRotation(slider);
                        positionInfo.Rotation = absoluteRotation - absoluteAngle;
                        absoluteAngle = absoluteRotation;

                        previousPos = slider.EndPosition;
                    }
                    else
                    {
                        previousPos = hitObject.BaseSpawnPosition;
                    }

                    previousAngle = absoluteAngle;
                }

                return positionInfos;
            }

            private static float GetSliderRotation(SliderData slider)
            {
                Vector2 endPosVector = slider.Path.PositionAt(1);
                
                return MathF.Atan2(endPosVector.Y, endPosVector.X);
            }
        }
    }
}
