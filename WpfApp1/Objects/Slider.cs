using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using System;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.Animations;
using WpfApp1.Objects.SliderPathMath;
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

        public static Canvas CreateSlider(Slider slider, double radius, int currentComboNumber, double osuScale, int index)
        {
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

            Canvas fullSlider = new Canvas();
            fullSlider.DataContext = slider;
            fullSlider.Name = $"HitObject{index}";

            Canvas head = CreateSliderHead(slider, radius, currentComboNumber, osuScale, fullSlider.Name);
            Canvas body = CreateSliderBody(slider, radius, osuScale);
            Canvas tail = CreateSliderTail(slider, radius, osuScale);

            fullSlider.Children.Add(head);
            fullSlider.Children.Add(body);
            fullSlider.Children.Add(tail);

            return fullSlider;
        }

        private static Canvas CreateSliderHead(Slider slider, double radius, int currentComboNumber, double osuScale, string name)
        {
            Canvas head = new Canvas();
            head.Width = radius;
            head.Height = radius;
            head.Name = name;

            Color comboColor = Color.FromArgb(220, 24, 214);
            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle@2x.png"), comboColor, radius);
            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png")),
            };

            Grid comboNumber = HitCircle.AddComboNumber(currentComboNumber, radius);

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

            HitCircleAnimation.ApplyHitCircleAnimations(head);

            Canvas.SetLeft(head, (slider.X * osuScale) - (radius / 2));
            Canvas.SetTop(head, (slider.Y * osuScale) - (radius / 2));

            return head;
        }

        private static Canvas CreateSliderTail(Slider slider, double radius, double osuScale)
        {
            Canvas tail = new Canvas();
            tail.Width = radius;
            tail.Height = radius;

            Color comboColor = Color.FromArgb(220, 24, 214);

            Bitmap sliderEndCircle = new Bitmap($"{skinPath}\\sliderendcircle.png");

            Image hitCircle;
            if (sliderEndCircle.Width == 128) // testing but change to 1 or something idk
            {
                hitCircle = SkinHitCircle.ApplyComboColourToHitObject(sliderEndCircle, comboColor, radius);
            }
            else
            {
                hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle@2x.png"), comboColor, radius);
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

            Canvas.SetLeft(tail, (slider.EndPosition.X * osuScale) - (radius / 2));
            Canvas.SetTop(tail, (slider.EndPosition.Y * osuScale) - (radius / 2));

            return tail;
        }

        private static Canvas CreateSliderBody(Slider slider, double radius, double osuScale)
        {
            Canvas body = new Canvas();

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

            Vector2[] vertices = new Vector2[slider.CurvePoints.Count];

            for (int i = 0; i < slider.CurvePoints.Count; i++)
            {
                vertices[i].X = slider.CurvePoints[i].X;
                vertices[i].Y = slider.CurvePoints[i].Y;
            }


            List<Vector2> a;
            if (slider.CurveType == CurveType.Catmull)
            {
                 a = PathApproximator.CatmullToPiecewiseLinear(vertices);
            }
            else if (slider.CurveType == CurveType.Linear)
            {
                 a = PathApproximator.LinearToPiecewiseLinear(vertices);
            }
            else if (slider.CurveType == CurveType.PerfectCirle)
            {
                 a = PathApproximator.CircularArcToPiecewiseLinear(vertices);
            }
            else
            {
                 a = PathApproximator.BSplineToPiecewiseLinear(vertices, vertices.Length);
            }

                // M = start position
                path.Append($"M {Math.Ceiling(a[0].X * osuScale)},{Math.Ceiling(a[0].Y * osuScale)} L ");

            for (int i = 1; i < a.Count; i++)
            {
                path.Append($"{Math.Ceiling(a[i].X * osuScale)},{Math.Ceiling(a[i].Y * osuScale)} ");
            }

            return Geometry.Parse(path.ToString());
        }
    }
}
