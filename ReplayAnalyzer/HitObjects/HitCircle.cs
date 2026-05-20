using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

#nullable disable

namespace ReplayAnalyzer.HitObjects
{
    public class HitCircle : HitObject
    {
        public HitCircle(CircleData circleData)
        {
            X = circleData.X;
            Y = circleData.Y;
            BaseSpawnPosition = circleData.BaseSpawnPosition;
            SpawnTime = circleData.SpawnTime;
            Judgement = new HitJudgement((HitObjectJudgement)circleData.Judgement.Judgement, circleData.Judgement.SpawnTime);
        }

        public static HitCircle CreateCircle(CircleData circleData, double diameter, int currentComboNumber, int index, int comboColourIndex)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateCircleObject(circleData, diameter, currentComboNumber, index, comboColourIndex);
            }

            return CreateCirclePreload(circleData, diameter, index);
        }

        private static HitCircle CreateCircleObject(CircleData circleData, double diameter, int currentComboNumber, int index, int comboColourIndex)
        {
            HitCircle hitObject = new HitCircle(circleData);
            hitObject.Width = diameter;
            hitObject.Height = diameter;

            Image hitCircle = new Image()
            {
                Width = diameter,
                Height = diameter,
                Source = SkinElement.GetElement(SkinElement.SkinElements.HitCircle, $"{comboColourIndex}"),
            };

            Image hitCircleBorder = new Image()
            {
                Width = diameter,
                Height = diameter,
                Source = SkinElement.GetElement(SkinElement.SkinElements.HitCircleOverlay),
            };

            Grid comboNumber = AddComboNumber(currentComboNumber, diameter);

            string approachCirclePath = SkinElement.GetElementPath(SkinElement.SkinElements.ApproachCircle);
            BitmapSource approachCircleBitmap = SkinElement.GetElement(SkinElement.SkinElements.ApproachCircle);
            double scale = 1.0;
            if (approachCirclePath.Substring(approachCirclePath.Length - 7).Contains("@2x"))
            {
                scale = approachCircleBitmap.PixelWidth / 256.0;
            }
            else
            {
                scale = approachCircleBitmap.PixelWidth / 128.0;
            }
            
            Image approachCircle = new Image()
            {
                Height = (diameter * scale) * 4,
                Width = (diameter * scale) * 4,
                Source = approachCircleBitmap,
                DataContext = scale, // for approach circle animation
            };

            hitObject.Children.Add(hitCircle);
            hitObject.Children.Add(hitCircleBorder);
            hitObject.Children.Add(comboNumber);
            hitObject.Children.Add(approachCircle);

            Canvas.SetLeft(hitObject, (hitObject.X - diameter / 2));
            Canvas.SetTop(hitObject, (hitObject.Y - diameter / 2));

            // circles 1 2 3 were rendered so 3 was on top...
            // (0 - index) gives negative value so that 1 will be rendered on top
            // basically correct zindexing like it should be for every object
            SetZIndex(hitObject, 0 - index);

            hitObject.Name = $"CircleHitObject{index}";

            hitObject.Visibility = Visibility.Collapsed;

            return hitObject;
        }

        private static HitCircle CreateCirclePreload(CircleData circleData, double diameter, int index)
        {
            HitCircle hitObject = new HitCircle(circleData);
            hitObject.Width = diameter;
            hitObject.Height = diameter;

            Image hitCircle = new Image();
            Canvas hitCircleBorder2 = new Canvas();
            Canvas comboNumber = new Canvas();
            Image approachCircle = new Image();

            hitObject.Children.Add(hitCircle);
            hitObject.Children.Add(hitCircleBorder2);
            hitObject.Children.Add(comboNumber);
            hitObject.Children.Add(approachCircle);

            Canvas.SetLeft(hitObject, hitObject.X - diameter / 2);
            Canvas.SetTop(hitObject, hitObject.Y - diameter / 2);

            hitObject.Name = $"CircleHitObject{index}";

            return hitObject;
        }

        public static Image Circle(HitCircle c)
        {
            return c.Children[0] as Image;
        }

        public static Image ApproachCircle(HitCircle c)
        {
            return c.Children[3] as Image;
        }
    }
}
