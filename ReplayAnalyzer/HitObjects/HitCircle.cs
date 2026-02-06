using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.SliderPathMath;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace ReplayAnalyzer.HitObjects
{
    public class HitCircle : HitObject
    {
        public HitCircle() { }

        public HitCircle(CircleData circleData)
        {
            X = circleData.X;
            Y = circleData.Y;
            BaseSpawnPosition = circleData.BaseSpawnPosition;
            SpawnTime = circleData.SpawnTime;
            StackHeight = circleData.StackHeight;
            Judgement = new HitJudgement((HitObjectJudgement)circleData.Judgement.HitJudgement, circleData.Judgement.SpawnTime);
        }

        public static HitCircle CreateCircle(CircleData circleData, double diameter, int currentComboNumber, int index, int comboColourIndex)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateCircleObject(circleData, diameter, currentComboNumber, index, comboColourIndex);
            }

            return CreateCirclePreload(circleData, diameter, index);
        }

        public static HitCircle CreateCircleObject(CircleData circleData, double diameter, int currentComboNumber, int index, int comboColourIndex)
        {
            HitCircle hitObject = new HitCircle(circleData);
            hitObject.Width = diameter;
            hitObject.Height = diameter;

            Image hitCircle = ApplyComboColourToHitCircle(new Bitmap(SkinElement.HitCircle()), comboColourIndex, diameter);

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

            Canvas.SetLeft(hitObject, (hitObject.X - diameter / 2) + circleData.StackOffset * MainWindow.OsuPlayfieldObjectScale);
            Canvas.SetTop(hitObject, (hitObject.Y - diameter / 2) + circleData.StackOffset * MainWindow.OsuPlayfieldObjectScale);

            // circles 1 2 3 were rendered so 3 was on top...
            // (0 - index) gives negative value so that 1 will be rendered on top
            // basically correct zindexing like it should be for every object
            SetZIndex(hitObject, 0 - index);

            hitObject.Name = $"CircleHitObject{index}";

            hitObject.Visibility = Visibility.Collapsed;

            HitObjectAnimations.ApplyHitCircleAnimations(hitObject);

            return hitObject;
        }

        private static HitCircle CreateCirclePreload(CircleData circleData, double diameter, int index)
        {
            HitCircle hitObject = new HitCircle(circleData);
            hitObject.Width = diameter;
            hitObject.Height = diameter;

            Canvas hitCircle = new Canvas();
            Canvas hitCircleBorder2 = new Canvas();
            Canvas comboNumber = new Canvas();
            Canvas approachCircle = new Canvas();

            hitObject.Children.Add(hitCircle);
            hitObject.Children.Add(hitCircleBorder2);
            hitObject.Children.Add(comboNumber);
            hitObject.Children.Add(approachCircle);

            Canvas.SetLeft(hitObject, hitObject.X - diameter / 2);
            Canvas.SetTop(hitObject, hitObject.Y - diameter / 2);

            hitObject.Name = $"CircleHitObject{index}";

            return hitObject;
        }
    }
}
