using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Skinning;
using ReplayAnalyzer.Skins;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Image = System.Windows.Controls.Image;

namespace ReplayAnalyzer.Objects
{
    public class HitCircle : HitObject
    {

        private static HitCircle hitObject = new HitCircle();

        HitCircle() { }

        public HitCircle(CircleData circleData)
        {
            X = circleData.X;
            Y = circleData.Y;
            SpawnPosition = circleData.SpawnPosition;
            SpawnTime = circleData.SpawnTime;
            StackHeight = circleData.StackHeight;
            HitAt = circleData.HitAt;
            IsHit = circleData.IsHit;
        }

        public static HitCircle CreateCircle(CircleData circleData, double diameter, int currentComboNumber, int index, System.Drawing.Color comboColour)
        {
            hitObject = new HitCircle(circleData);
            hitObject.Width = diameter;
            hitObject.Height = diameter;

            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap(SkinElement.HitCircle()), comboColour, diameter);
            //Image hitCircle = new Image()
            //{
            //    Width = diameter,
            //    Height = diameter,
            //    Source = new BitmapImage(new Uri(SkinElement.HitCircle())),
            //};

            Image hitCircleBorder2 = new Image()
            {
                Width = diameter,
                Height = diameter,
                Source = new BitmapImage(new Uri(SkinElement.HitCircleOverlay())),
            };

            Grid comboNumber = AddComboNumber(currentComboNumber, diameter);

            Image approachCircle = new Image()
            {
                Height = diameter,
                Width = diameter,
                Source = new BitmapImage(new Uri(SkinElement.ApproachCircle())),
                RenderTransform = new ScaleTransform(),
            };

            hitObject.Children.Add(hitCircle);
            hitObject.Children.Add(hitCircleBorder2);
            hitObject.Children.Add(comboNumber);
            hitObject.Children.Add(approachCircle);

            Canvas.SetLeft(hitObject, hitObject.X - diameter / 2);
            Canvas.SetTop(hitObject, hitObject.Y - diameter / 2);

            // circles 1 2 3 were rendered so 3 was on top...
            // (0 - index) gives negative value so that 1 will be rendered on top
            // basically correct zindexing like it should be for every object
            SetZIndex(hitObject, 0 - index);

            hitObject.Name = $"CircleHitObject{index}";

            hitObject.Visibility = Visibility.Collapsed;

            HitObjectAnimations.ApplyHitCircleAnimations(hitObject);

            return hitObject;
        }
    }
}
