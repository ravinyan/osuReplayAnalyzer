using System.Drawing;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.Animations;
using WpfApp1.OsuMaths;
using WpfApp1.Skinning;
using WpfApp1.Skins;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;
//https://github.com/videolan/libvlcsharp

namespace WpfApp1.Objects
{
    public static class SliderObject
    {
        // https://osu.ppy.sh/wiki/en/Skinning/osu%21#slider

        public static Canvas CreateSlider(Slider slider, double radius, int currentComboNumber, double osuScale, int index, Color comboColour)
        {

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

            Canvas head = CreateSliderHead(slider, radius, currentComboNumber, osuScale, fullSlider.Name, comboColour);
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

        private static Canvas CreateSliderHead(Slider slider, double radius, int currentComboNumber, double osuScale, string name, Color comboColour)
        {
            Canvas head = new Canvas();
            head.Width = radius;
            head.Height = radius;
            head.Name = name;

            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap(SkinElement.HitCircle()), comboColour, radius);
            Image hitCircleBorder2 = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri(SkinElement.HitCircleOverlay())),
            };

            Grid comboNumber = HitCircle.AddComboNumber(currentComboNumber, radius);

            Image approachCircle = new Image()
            {
                Height = radius,
                Width = radius,
                Source = new BitmapImage(new Uri(SkinElement.ApproachCircle())),
                Name = "ApproachCircle",
            };

            head.Children.Add(hitCircle);
            head.Children.Add(hitCircleBorder2);
            head.Children.Add(comboNumber);
            head.Children.Add(approachCircle);

            // 1st one is nothing, 2nd one is slider end repeat
            if (slider.RepeatCount > 2)
            {
                double reverseArrowCount = Math.Round(slider.RepeatCount / 2.0, MidpointRounding.ToZero);
                
                if (slider.RepeatCount % 2 == 0)
                {
                    reverseArrowCount--;
                }

                while (reverseArrowCount != 0)
                {
                    Image reverseArrow = new Image()
                    {
                        Width = radius,
                        Height = radius,
                        Source = new BitmapImage(new Uri(SkinElement.ReverseArrow())),
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new RotateTransform(GetReverseArrowAngle(slider, false)),
                    };

                    reverseArrow.Visibility = System.Windows.Visibility.Collapsed;
                    head.Children.Add(reverseArrow);

                    reverseArrowCount--;
                }
            }

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

            // 1 is no repeats
            if (slider.RepeatCount > 1)
            {
                int reverseArrowCount = (int)Math.Floor(slider.RepeatCount / 2.0);

                // only first one should be visible and rest will become visible when first one gets hit
                bool isVisible = true;

                while (reverseArrowCount != 0)
                {
                    Image reverseArrow = new Image()
                    {
                        Width = radius,
                        Height = radius,
                        Source = new BitmapImage(new Uri(SkinElement.ReverseArrow())),
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new RotateTransform(GetReverseArrowAngle(slider, true)),
                    };

                    if (isVisible)
                    {
                        isVisible = false;
                        tail.Children.Add(reverseArrow);
                    }
                    else
                    {
                        reverseArrow.Visibility = System.Windows.Visibility.Collapsed;
                        tail.Children.Add(reverseArrow);
                    }

                    reverseArrowCount--;
                }
            }

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
            /* funny
            my god and saviour... when he said not all is lost he was so right
            https://stackoverflow.com/questions/980798/wpf-bug-or-am-i-going-crazy
            in case stack overflow dies... i didnt even knew this existed i know im stupid but... im just stupid
            https://learn.microsoft.com/en-us/dotnet/api/system.windows.shapes.shape.strokelinejoin?view=windowsdesktop-9.0&redirectedfrom=MSDN#System_Windows_Shapes_Shape_StrokeLineJoin  */
            sliderBodyPath.StrokeLineJoin = PenLineJoin.Round;

            Image sliderBall = new Image()
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri(SkinElement.SliderBall())),
                RenderTransform = new ScaleTransform(1.3, 1.3),
                RenderTransformOrigin = new Point(0.5, 0.5),
            };

            Image sliderBallCircle = new Image() 
            {
                Width = radius,
                Height = radius,
                Source = new BitmapImage(new Uri(SkinElement.SliderBallCircle())),
                RenderTransform = new ScaleTransform(2, 2),
                RenderTransformOrigin = new Point(0.5 ,0.5),
            };

            Canvas ball = new Canvas();
            ball.Width = radius;
            ball.Height = radius;

            ball.Children.Add(sliderBall);
            ball.Children.Add(sliderBallCircle);

            Vector2 s = slider.Path.PositionAt(0);
            Canvas.SetLeft(ball, s.X - (radius / 2));
            Canvas.SetTop(ball, s.Y - (radius / 2));

           // ball.Visibility = System.Windows.Visibility.Collapsed;

            body.Children.Add(border);
            body.Children.Add(sliderBodyPath);
            body.Children.Add(ball);

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

        private static double GetReverseArrowAngle(Slider slider, bool isRepeatAtEnd)
        {
            // i got math from osu lazer source code coz i hate math
            List<Vector2> path = new List<Vector2>();
            slider.Path.GetPathToProgress(path, 0, 1);

            Vector2 position = isRepeatAtEnd ? path.Last() : path.First();

            int searchStart = isRepeatAtEnd ? path.Count - 1 : 0;
            int direction = isRepeatAtEnd ? -1 : 1;

            Vector2 aimRotationVector = Vector2.Zero;

            for (int i = searchStart; i >= 0 && i < path.Count; i += direction)
            {
                if (OsuMath.AlmostEquals(path[i], position))
                {
                    continue;
                }

                aimRotationVector = path[i];
                break;
            }

            float aimRotation = float.RadiansToDegrees(MathF.Atan2(aimRotationVector.Y - position.Y, aimRotationVector.X - position.X));
            return aimRotation;
        }
    }
}
