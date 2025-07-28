using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.Skinning;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
namespace WpfApp1.Objects
{
    public static class SliderObject
    {
        private static string skinPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuFileParser\\WpfApp1\\Skins\\Komori - PeguLian II (PwV)";
        // https://osu.ppy.sh/wiki/en/Skinning/osu%21#slider
        // change grid body to canvas maybe? but then how to position stuff... pain
        public static Grid CreateSlider(Slider slider, double radius, int currentComboNumber, double osuScale)
        {
            Grid hitObject = new Grid();
            hitObject.DataContext = slider;
            hitObject.Width = radius;
            hitObject.Height = radius;

            // sliderb0.png                  slider ball
            // sliderb0@2x.png
            // sliderfollowcircle.png        slider circle ball
            // sliderfollowcircle@2x.png
            // 
            // 
            // and maybe 
            // sliderstartcircleoverlay.png
            // sliderscorepoint.png          slider tick
            // sliderpoint30.png
            // sliderpoint10.png
            Grid head = CreateSliderHead(slider, radius, currentComboNumber, osuScale);
            Grid body = CreateSliderBody(slider, radius, currentComboNumber, osuScale);
            Grid tail = CreateSliderTail(slider, radius, currentComboNumber, osuScale);

            Canvas.SetLeft(head, (31));
            Canvas.SetTop(head, (128));

            Canvas.SetLeft(tail, (162));
            Canvas.SetTop(tail, (71));

            hitObject.Children.Add(head);
            hitObject.Children.Add(body);
            //hitObject.Children.Add(tail);

            Canvas.SetLeft(head, (31));
            Canvas.SetTop(head, (128));

            Canvas.SetLeft(tail, (162));
            Canvas.SetTop(tail, (71));

            return hitObject;
        }

        public static Grid CreateSliderHead(HitObject slider, double radius, int currentComboNumber, double osuScale)
        {
            Grid head = new Grid();
            head.Width = radius;
            head.Height = radius;

            Color comboColor = Color.FromArgb(220, 24, 214);
            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle@2x.png"), comboColor);
            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png")),
            };

            StackPanel comboNumber = HitCircle.AddComboNumber(currentComboNumber, radius);

            Image approachCircle = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\approachcircle.png")),
                Name = "ApproachCircle",
            };

            head.Children.Add(hitCircle);
            head.Children.Add(hitCircleBorder2);
            head.Children.Add(comboNumber);
            head.Children.Add(approachCircle);

            Canvas.SetLeft(head, (31 * osuScale) - (radius / 2));
            Canvas.SetTop(head, (128 * osuScale) - (radius / 2));

            return head;
        }

        public static Grid CreateSliderTail(Slider slider, double radius, int currentComboNumber, double osuScale)
        {
            Grid tail = new Grid();
            tail.Width = radius;
            tail.Height = radius;

            Color comboColor = Color.FromArgb(220, 24, 214);

            Bitmap sliderEndCircle = new Bitmap($"{skinPath}\\sliderendcircle.png");

            Image hitCircle;
            if (sliderEndCircle.Width == 128)
            {
                hitCircle = SkinHitCircle.ApplyComboColourToHitObject(sliderEndCircle, comboColor);
            }
            else
            {
                hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle@2x.png"), comboColor);
            }
 
            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png")),
            };

            // reversearrow.png
            // reversearrow@2x.png
            // sliderendcircle.png (has 1 pixel i guess)
            tail.Children.Add(hitCircle);
            tail.Children.Add(hitCircleBorder2);

            Canvas.SetLeft(tail, (162 * osuScale) - (radius / 2));
            Canvas.SetTop(tail, (71 * osuScale) - (radius / 2));

            return tail;
        }

        public static Grid CreateSliderBody(Slider slider, double radius, int currentComboNumber, double osuScale)
        {
            Grid body = new Grid();

            Path sliderBodyPath = new Path();

            sliderBodyPath.Stroke = System.Windows.Media.Brushes.Cyan;
            sliderBodyPath.StrokeThickness = 5;

            sliderBodyPath.Data = CreateSliderPath(slider, osuScale, radius);

            body.Children.Add(sliderBodyPath);

            return body;
        }

       
        private static Geometry CreateSliderPath(Slider slider, double osuScale, double radius)
        {
            StringBuilder path = new StringBuilder();

            // M = start position
            path.Append($"M {(slider.SpawnPosition.X)},{(slider.SpawnPosition.Y)} L ");

            //switch (slider.CurveType)
            //{
            //    case CurveType.Bezier:
            //        path.Append(" B ");
            //        break;
            //    case CurveType.Centripetal:
            //        path.Append(" B ");
            //        break;
            //    case CurveType.Linear:
            //        path.Append(" B ");
            //        break;
            //    case CurveType.PerfectCirle:
            //        path.Append(" B ");
            //        break;
            //}

            for (int i = 0; i < slider.CurvePoints.Count; i++)
            {
                path.Append($"{(slider.CurvePoints[i].X)},{(slider.CurvePoints[i].Y)} ");
            }

            return Geometry.Parse(path.ToString());
        }
    }
}
