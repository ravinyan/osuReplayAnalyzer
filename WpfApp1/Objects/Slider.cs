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
//https://github.com/videolan/libvlcsharp

namespace WpfApp1.Objects
{
    public static class SliderObject
    {
        private static string skinPath = FilePath.GetSkinPath();
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

            Canvas.SetZIndex(fullSlider, 0 - index);

            HitObjectAnimations.ApplySliderAnimations(fullSlider);

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
                Height = radius,
                Width = radius,
                Source = new BitmapImage(new Uri($"{skinPath}\\approachcircle.png")),
                Name = "ApproachCircle",
            };
            //Canvas.SetLeft(approachCircle, ((head.Height - approachCircle.Source.Width) / 2));
            //Canvas.SetTop(approachCircle, ((head.Width - approachCircle.Source.Height) / 2));

            head.Children.Add(hitCircle);
            head.Children.Add(hitCircleBorder2);
            head.Children.Add(comboNumber);
            head.Children.Add(approachCircle);

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

            if (!System.IO.File.Exists($"{skinPath}\\sliderendcircle.png"))
            {
                Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap($"{skinPath}\\hitcircle@2x.png"), comboColor, radius);

                Image hitCircleBorder2 = new Image()
                {
                    Width = radius,
                    Height = radius,
                    Source = new BitmapImage(new Uri($"{skinPath}\\hitcircleoverlay@2x.png")),
                };

                tail.Children.Add(hitCircle);
                tail.Children.Add(hitCircleBorder2);
            }

            // reversearrow.png
            // reversearrow@2x.png
            // sliderendcircle.png (has 1 pixel i guess)
            
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

            Canvas.SetZIndex(body, -1);
            
            Path border = new Path();
            border.Data = CreateSliderPath(slider, osuScale);
            border.StrokeThickness = radius * 0.95;
            border.Stroke = Brushes.White;
            border.StrokeEndLineCap = PenLineCap.Round;
            border.StrokeStartLineCap = PenLineCap.Round;
            border.StrokeLineJoin = PenLineJoin.Round;

            Path sliderBodyPath = new Path();
            sliderBodyPath.Data = CreateSliderPath(slider, osuScale);

            sliderBodyPath.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(3,3,12));
            sliderBodyPath.StrokeThickness = radius * 0.85;

            sliderBodyPath.StrokeEndLineCap = PenLineCap.Round;
            sliderBodyPath.StrokeStartLineCap = PenLineCap.Round;

            // my god and saviour... when he said not all is lost he was so right
            // https://stackoverflow.com/questions/980798/wpf-bug-or-am-i-going-crazy
            // in case stack overflow dies... i didnt even knew this existed i know im stupid but... im just stupid
            // https://learn.microsoft.com/en-us/dotnet/api/system.windows.shapes.shape.strokelinejoin?view=windowsdesktop-9.0&redirectedfrom=MSDN#System_Windows_Shapes_Shape_StrokeLineJoin
            sliderBodyPath.StrokeLineJoin = PenLineJoin.Round;

            body.Children.Add(border);
            body.Children.Add(sliderBodyPath);

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
