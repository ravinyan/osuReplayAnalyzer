using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using OsuFileParsers.Decoders;
using OsuFileParsers.SliderPathMath;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Windows;

namespace ReplayAnalyzer.GameplayMods.Mods
{
    // i know im just copying formula of osu lazer but its still just too much effort for something i doubt much people will use... why am i even doing it?
    // day 2 - i regret everything and im putting everything in this class i dont care i wont care

    // basically all code here is taken from osu lazer OsuModRandom and this looks like abomination but code in lazer actually looks good
    // spoiler this didnt work... and im not fixing it i dont understand math behind this at all
    public class RandomMod
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

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

            WorkingObject? previous = null;
            List<WorkingObject> workingObjects = positions.Select(wo => new WorkingObject(wo)).ToList();

            // reposition objects
            for (int i = 0; i < workingObjects.Count; i++)
            {
                WorkingObject current = workingObjects[i];
                HitObjectData hitObject = current.HitObject;

                if (hitObject is SpinnerData)
                {
                    previous = current;
                    continue;
                }

                ComputeModifiedPosition(current, previous, i > 1 ? workingObjects[i - 2] : null);

                Vector2 shift = Vector2.Zero;
                switch (hitObject)
                {
                    case CircleData:
                        shift = ClampHitCircleToPlayfield(current); 
                        break;
                    case SliderData:
                        shift = ClampSliderToPlayfield(current);
                        break;
                }

                // idk osu lazer thing i just copy and i paste
                const int precedingHitobjectsToShift = 10;
                if (shift != Vector2.Zero)
                {
                    List<HitObjectData> toBeShifted = new List<HitObjectData>();

                    for (int j = i - 1; j >= i - precedingHitobjectsToShift && j >= 0; j--)
                    {
                        if (workingObjects[j].HitObject is not CircleData)
                        {
                            break;
                        }

                        toBeShifted.Add(workingObjects[j].HitObject);
                    }

                    if (toBeShifted.Count > 0)
                    {
                        ApplyDecreasingShift(toBeShifted, shift);
                    }
                }

                current.HitObject.BaseX = current.HitObject.BaseSpawnPosition.X;
                current.HitObject.BaseY = current.HitObject.BaseSpawnPosition.Y;

                previous = current;
            }

            MainWindow.map.HitObjects = workingObjects.Select(wo => wo.HitObject).ToList();
        }

        // WHEN WILL THIS END IT WILL PROBABLY NOT WORK AND I WONT FIX IT BUT JUST END FFS
        private static void ComputeModifiedPosition(WorkingObject current, WorkingObject? previous, WorkingObject? beforePrevious)
        {
            float previousAbsoluteAngle = 0f;
            Vector2 playfield_centre = new Vector2(512 / 2.0f, 384 / 2.0f);

            if (previous != null)
            {
                if (previous.HitObject is SliderData s)
                {
                    previousAbsoluteAngle = ObjectPositionInfo.GetSliderRotation(s);
                }
                else
                {
                    Vector2 earliestPosition;
                    if (beforePrevious != null)
                    {
                        Vector2? endPos = beforePrevious.HitObject is SliderData sd ? sd.EndPosition : beforePrevious.HitObject.BaseSpawnPosition;
                        earliestPosition = endPos ?? playfield_centre;
                    }
                    else
                    {  
                        earliestPosition = playfield_centre;
                    }

                    Vector2 relativePosition = previous.HitObject.BaseSpawnPosition - earliestPosition;
                    previousAbsoluteAngle = MathF.Atan2(relativePosition.Y, relativePosition.X);
                }
            }

            float absoluteAngle = previousAbsoluteAngle + current.PositionInfo.RelativeAngle;

            var posRelativeToPrev = new Vector2(
                current.PositionInfo.DistanceFromPrevious * MathF.Cos(absoluteAngle),
                current.PositionInfo.DistanceFromPrevious * MathF.Sin(absoluteAngle)
            );
            
            Vector2 lastEndPosition = previous?.EndPositionModified ?? playfield_centre;

            posRelativeToPrev = RotateAwayFromEdge(lastEndPosition, posRelativeToPrev);

            current.PositionModified = lastEndPosition + posRelativeToPrev;

            if (!(current.HitObject is SliderData slider))
                return;

            absoluteAngle = MathF.Atan2(posRelativeToPrev.Y, posRelativeToPrev.X);

            Vector2 centreOfMassOriginal = CalculateCentreOfMass(slider);
            Vector2 centreOfMassModified = RotateVector(centreOfMassOriginal, current.PositionInfo.Rotation + absoluteAngle - ObjectPositionInfo.GetSliderRotation(slider));
            centreOfMassModified = RotateAwayFromEdge(current.PositionModified, centreOfMassModified);

            float relativeRotation = MathF.Atan2(centreOfMassModified.Y, centreOfMassModified.X) - MathF.Atan2(centreOfMassOriginal.Y, centreOfMassOriginal.X);
            if (!Precision.AlmostEquals(relativeRotation, 0))
                RotateSlider(slider, relativeRotation);
        }

        private static Vector2 CalculateCentreOfMass(SliderData slider)
        {
            const double sample_step = 50;

            // just sample the start and end positions if the slider is too short
            if (slider.Path.Distance <= sample_step)
            {
                return Vector2.Divide(slider.Path.PositionAt(1), 2);
            }

            int count = 0;
            Vector2 sum = Vector2.Zero;
            double pathDistance = slider.Path.Distance;

            for (double i = 0; i < pathDistance; i += sample_step)
            {
                sum += slider.Path.PositionAt(i / pathDistance);
                count++;
            }

            return sum / count;
        }

        private static Vector2 RotateAwayFromEdge(Vector2 prevObjectPos, Vector2 posRelativeToPrev, float rotationRatio = 0.5f)
        {
            float relativeRotationDistance = 0f;

            // idk its const in lazer code so aaa
            const float playfieldEdgeRatio = 0.375f;

            Vector2 osuPlayfield = new Vector2(512, 384);
            Vector2 osuPlayfieldMiddle = new Vector2(512 / 2, 384 / 2);
            Vector2 borderDistance = new Vector2(osuPlayfield.X * playfieldEdgeRatio, osuPlayfield.Y * playfieldEdgeRatio);

            if (prevObjectPos.X < osuPlayfieldMiddle.X)
            {
                relativeRotationDistance = Math.Max(
                    (borderDistance.X - prevObjectPos.X) / borderDistance.X,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevObjectPos.X - (osuPlayfield.X - borderDistance.X)) / borderDistance.X,
                    relativeRotationDistance
                );
            }

            if (prevObjectPos.Y < osuPlayfieldMiddle.Y)
            {
                relativeRotationDistance = Math.Max(
                    (borderDistance.Y - prevObjectPos.Y) / borderDistance.Y,
                    relativeRotationDistance
                );
            }
            else
            {
                relativeRotationDistance = Math.Max(
                    (prevObjectPos.Y - (osuPlayfield.Y - borderDistance.Y)) / borderDistance.Y,
                    relativeRotationDistance
                );
            }

            return RotateVectorTowardsVector(
                posRelativeToPrev,
                osuPlayfieldMiddle - prevObjectPos,
                Math.Min(1, relativeRotationDistance * rotationRatio)
            );
        }

        private static Vector2 RotateVectorTowardsVector(Vector2 initial, Vector2 destination, float rotationRatio)
        {
            float initialAngleRad = MathF.Atan2(initial.Y, initial.X);
            float destAngleRad = MathF.Atan2(destination.Y, destination.X);

            float diff = destAngleRad - initialAngleRad;

            while (diff < -MathF.PI) diff += 2 * MathF.PI;

            while (diff > MathF.PI) diff -= 2 * MathF.PI;

            float finalAngleRad = initialAngleRad + rotationRatio * diff;

            return new Vector2(
                initial.Length() * MathF.Cos(finalAngleRad),
                initial.Length() * MathF.Sin(finalAngleRad)
            );
        }

        private static Vector2 ClampHitCircleToPlayfield(WorkingObject workingObject)
        {
            Vector2 previousPosition = workingObject.PositionModified;
            workingObject.PositionModified = ClampToPlayfieldWithPadding(workingObject.PositionModified);

            workingObject.HitObject.BaseSpawnPosition = workingObject.PositionModified;

            return workingObject.PositionModified - previousPosition;
        }

        private static Vector2 ClampSliderToPlayfield(WorkingObject workingObject)
        {
            SliderData slider = (SliderData)workingObject.HitObject;
            RectangleF possibleMovementBounds = CalculatePossibleMovementBounds(slider);

            // The slider rotation applied in computeModifiedPosition might make it impossible to fit the slider into the playfield
            // For example, a long horizontal slider will be off-screen when rotated by 90 degrees
            // In this case, limit the rotation to either 0 or 180 degrees
            if (possibleMovementBounds.Width < 0 || possibleMovementBounds.Height < 0)
            {
                float currentRotation = ObjectPositionInfo.GetSliderRotation(slider);
                float diff1 = GetAngleDifference(workingObject.RotationOriginal, currentRotation);
                float diff2 = GetAngleDifference(workingObject.RotationOriginal + MathF.PI, currentRotation);

                if (diff1 < diff2)
                {
                    RotateSlider(slider, workingObject.RotationOriginal - ObjectPositionInfo.GetSliderRotation(slider));
                }
                else
                {
                    RotateSlider(slider, workingObject.RotationOriginal + MathF.PI - ObjectPositionInfo.GetSliderRotation(slider));
                }

                possibleMovementBounds = CalculatePossibleMovementBounds(slider);
            }

            Vector2 previousPosition = workingObject.PositionModified;

            // Clamp slider position to the placement area
            // If the slider is larger than the playfield, at least make sure that the head circle is inside the playfield
            float newX = possibleMovementBounds.Width < 0
                ? Math.Clamp(possibleMovementBounds.Left, 0, 512)
                : Math.Clamp(previousPosition.X, possibleMovementBounds.Left, possibleMovementBounds.Right);

            float newY = possibleMovementBounds.Height < 0
                ? Math.Clamp(possibleMovementBounds.Top, 0, 384)
                : Math.Clamp(previousPosition.Y, possibleMovementBounds.Top, possibleMovementBounds.Bottom);

            slider.BaseSpawnPosition = workingObject.PositionModified = new Vector2(newX, newY);
            workingObject.EndPositionModified = slider.EndPosition;

            return workingObject.PositionModified - previousPosition;
        }

        private static void RotateSlider(SliderData slider, float rotation)
        {
            void rotateControlPoint(PathControlPoint point) => point.Position = RotateVector(point.Position, rotation);

            modifySlider(slider, rotateControlPoint);
        }

        private static void modifySlider(SliderData slider, Action<PathControlPoint> modifyControlPoint)
        {
            PathControlPoint[] controlPoints = slider.Path.ControlPoints.Select(p => new PathControlPoint(p.Position, p.Type)).ToArray();
            foreach (var point in controlPoints)
                modifyControlPoint(point);

            slider.Path = new SliderPath(controlPoints, slider.Path.ExpectedDistance);
        }

        private static Vector2 RotateVector(Vector2 vector, float rotation)
        {
            float angle = MathF.Atan2(vector.Y, vector.X) + rotation;
            float length = vector.Length();
            return new Vector2(
                length * MathF.Cos(angle),
                length * MathF.Sin(angle)
            );
        }

        private static RectangleF CalculatePossibleMovementBounds(SliderData slider)
        {
            List<Vector2> pathPositions = new List<Vector2>();
            slider.Path.GetPathToProgress(pathPositions, 0, 1);

            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;

            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            // Compute the bounding box of the slider.
            foreach (Vector2 pos in pathPositions)
            {
                minX = MathF.Min(minX, pos.X);
                maxX = MathF.Max(maxX, pos.X);

                minY = MathF.Min(minY, pos.Y);
                maxY = MathF.Max(maxY, pos.Y);
            }

            // Take the circle radius into account.
            float radius = GetRadius();

            minX -= radius;
            minY -= radius;

            maxX += radius;
            maxY += radius;

            // Given the bounding box of the slider (via min/max X/Y),
            // the amount that the slider can move to the left is minX (with the sign flipped, since positive X is to the right),
            // and the amount that it can move to the right is WIDTH - maxX.
            // Same calculation applies for the Y axis.
            float left = -minX;
            float right = 512 - maxX;
            float top = -minY;
            float bottom = 384 - maxY;

            return new RectangleF(left, top, right - left, bottom - top);
        }

        private static float GetAngleDifference(float angle1, float angle2)
        {
            float diff = MathF.Abs(angle1 - angle2) % (MathF.PI * 2);
            return MathF.Min(diff, MathF.PI * 2 - diff);
        }

        private static void ApplyDecreasingShift(List<HitObjectData> hitObjectsToShift, Vector2 shift)
        {
            for (int i = 0; i < hitObjectsToShift.Count; i++)
            {
                HitObjectData hitObject = hitObjectsToShift[i];
                // The first object is shifted by a vector slightly smaller than shift
                // The last object is shifted by a vector slightly larger than zero
                Vector2 position = hitObject.BaseSpawnPosition + shift * ((hitObjectsToShift.Count - i) / (float)(hitObjectsToShift.Count + 1));
            
                hitObject.BaseSpawnPosition = ClampToPlayfieldWithPadding(position);
            }
        }

        // this might not work if random is not the last mod in mod selection... if that will be the case i will just move it to be always last lol
        private static Vector2 ClampToPlayfieldWithPadding(Vector2 position)
        {
            float padding = GetRadius();

            return new Vector2(
                Math.Clamp(position.X, padding, 512 - padding),
                Math.Clamp(position.Y, padding, 384 - padding)
            );
        }

        // timing point fix
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
                List<TimingPoint> positiveBeatLengthTimingPoints = MainWindow.map.TimingPoints.Where(t => t.BeatLength > 0).ToList();
                TimingPoint timingPoint = BeatmapDecoder.BinarySearch(positiveBeatLengthTimingPoints, hitObject.SpawnTime);

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

        private static float GetRadius()
        {
            const double AspectRatio = 1.33;
            double height = (Window.ActualHeight - Window.musicControlUI.ActualHeight) / AspectRatio;
            double width = Window.ActualWidth / AspectRatio;
            double osuScale = Math.Min(height / 384, width / 512);
            float radius = (float)((54.4 - 4.48 * (double)MainWindow.map.Difficulty.CircleSize) * 2);

            return radius;
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

            public static float GetSliderRotation(SliderData slider)
            {
                Vector2 endPosVector = slider.Path.PositionAt(1);
                
                return MathF.Atan2(endPosVector.Y, endPosVector.X);
            }
        }

        private class WorkingObject
        {
            public float RotationOriginal { get; }
            public Vector2 PositionModified { get; set; }
            public Vector2 EndPositionModified { get; set; }

            public ObjectPositionInfo PositionInfo { get; }
            public HitObjectData HitObject => PositionInfo.HitObject!;

            public WorkingObject(ObjectPositionInfo positionInfo)
            {
                PositionInfo = positionInfo;
                RotationOriginal = HitObject is SliderData slider ? ObjectPositionInfo.GetSliderRotation(slider) : 0;
                PositionModified = HitObject.BaseSpawnPosition;
                EndPositionModified = HitObject is SliderData slider2 ? slider2.EndPosition : HitObject.BaseSpawnPosition;
            }
        }
    }
}
