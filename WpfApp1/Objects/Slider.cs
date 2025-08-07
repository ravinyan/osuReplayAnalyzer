using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.Animations;
using WpfApp1.Skinning;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
using Point = System.Windows.Point;
using Brushes = System.Windows.Media.Brushes;
using System.Numerics;

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

            fullSlider.Visibility = System.Windows.Visibility.Collapsed;

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
            body.Width = 1;
            body.Height = 1;

            Canvas.SetLeft(body, (slider.X * osuScale));
            Canvas.SetTop(body, (slider.Y * osuScale));
            
            Path sliderBodyPath = new Path();
            body.Children.Add(sliderBodyPath);
            sliderBodyPath.Data = CreateSliderPath(slider, osuScale);
            sliderBodyPath.Stroke = Brushes.Cyan;
            sliderBodyPath.StrokeThickness = radius;

            // my god and saviour... when he said not all is lost he was so right
            // https://stackoverflow.com/questions/980798/wpf-bug-or-am-i-going-crazy
            // in case stack overflow dies... i didnt even knew this existed i know im stupid but... im just stupid
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.shapes.shape.strokelinejoin?view=windowsdesktop-9.0&redirectedfrom=MSDN#System_Windows_Shapes_Shape_StrokeLineJoin
            sliderBodyPath.StrokeLineJoin = PenLineJoin.Round;

            return body;
        }

        private static PathGeometry CreateSliderPath(Slider slider, double osuScale)
        {
            List<Vector2> pathPoints = slider.Path.CalculatedPath();

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = new Point(pathPoints[0].X, pathPoints[0].Y);
            
            PointCollection myPointCollection = new PointCollection(slider.ControlPoints.Length);
            for (int i = 1; i < pathPoints.Count; i++)
            {
                myPointCollection.Add(new Point(pathPoints[i].X * osuScale,
                                                pathPoints[i].Y * osuScale));
            }

            PolyLineSegment polyLineSegment = new PolyLineSegment();
            polyLineSegment.Points = myPointCollection;
            
            PathSegmentCollection myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(polyLineSegment);
            myPathFigure.Segments = myPathSegmentCollection;
            
            PathFigureCollection myPathFigureCollection = new PathFigureCollection();
            myPathFigureCollection.Add(myPathFigure);

            PathGeometry myPathGeometry = new PathGeometry();
            myPathGeometry.Figures = myPathFigureCollection;
            
            return myPathGeometry;
        }
    }
}
