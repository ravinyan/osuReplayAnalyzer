using System.Drawing;
using System.Numerics;
using System.Windows;
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

        public static Canvas CreateSlider(Slider slider, double diameter, int currentComboNumber, int index, Color comboColour)
        {
            // and maybe 
            // sliderstartcircleoverlay.png
            // sliderpoint30.png
            // sliderpoint10.png

            Canvas fullSlider = new Canvas();
            fullSlider.DataContext = slider;
            fullSlider.Name = $"SliderHitObject{index}";
            fullSlider.Width = diameter;
            fullSlider.Height = diameter;

            Canvas head = CreateSliderHead(slider, diameter, currentComboNumber, fullSlider.Name, comboColour);
            Canvas body = CreateSliderBody(slider, diameter);
            Canvas tail = CreateSliderTail(slider, diameter);

            fullSlider.Children.Add(body);
            fullSlider.Children.Add(head);
            fullSlider.Children.Add(tail);

            fullSlider.Visibility = System.Windows.Visibility.Collapsed;

            Canvas.SetZIndex(fullSlider, 0 - index);

            HitObjectAnimations.ApplySliderAnimations(fullSlider);

            return fullSlider;
        }

        private static Canvas CreateSliderHead(Slider slider, double diameter, int currentComboNumber, string name, Color comboColour)
        {
            Canvas head = new Canvas();
            head.Width = diameter;
            head.Height = diameter;
            head.Name = name;

            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap(SkinElement.HitCircle()), comboColour, diameter);
            Image hitCircleBorder2 = new Image()
            {
                Width = diameter,
                Height = diameter,
                Source = new BitmapImage(new Uri(SkinElement.HitCircleOverlay())),
            };

            Grid comboNumber = HitCircle.AddComboNumber(currentComboNumber, diameter);

            Image approachCircle = new Image()
            {
                Height = diameter,
                Width = diameter,
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
                        Width = diameter,
                        Height = diameter,
                        Source = new BitmapImage(new Uri(SkinElement.ReverseArrow())),
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new RotateTransform(GetReverseArrowAngle(slider, false)),
                    };

                    reverseArrow.Visibility = System.Windows.Visibility.Collapsed;
                    head.Children.Add(reverseArrow);

                    reverseArrowCount--;
                }
            }

            Canvas.SetLeft(head, (slider.X) - (diameter / 2));
            Canvas.SetTop(head, (slider.Y) - (diameter / 2));

            return head;
        }

        private static Canvas CreateSliderTail(Slider slider, double diameter)
        {
            Canvas tail = new Canvas();
            tail.Width = diameter;
            tail.Height = diameter;

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
                        Width = diameter,
                        Height = diameter,
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

            Canvas.SetLeft(tail, (slider.EndPosition.X) - (diameter / 2));
            Canvas.SetTop(tail, (slider.EndPosition.Y) - (diameter / 2));
            
            return tail;
        }

        private static Canvas CreateSliderBody(Slider slider, double diameter)
        {
            Canvas body = new Canvas();
            body.Width = 1;
            body.Height = 1;

            //body.CacheMode = new BitmapCache();

            Canvas.SetLeft(body, (slider.X));
            Canvas.SetTop(body, (slider.Y));

            Canvas.SetZIndex(body, -1);

            Path border = SliderBorder(slider, diameter);
            Path sliderBodyPath = SliderBody(slider, diameter);
            Canvas ball = SliderBall(slider, diameter);

            body.Children.Add(border);
            body.Children.Add(sliderBodyPath);
            body.Children.Add(ball);

            if (slider.SliderTicks != null)
            {
                AddSliderTicks(body, slider, diameter);
            }

            return body;
        }

        public static void ResetToDefault(Canvas slider)
        {
            for (int i = 0; i < slider.Children.Count; i++)
            {
                Canvas parent = slider.Children[i] as Canvas;

                if (parent.Visibility == Visibility.Collapsed)
                {
                    parent.Visibility = Visibility.Visible;
                }    

                for (int j = 0; j < parent.Children.Count; j++)
                {
                    // if its slider ball then make it collapsed and skip
                    if (i == 0 && j == 2)
                    {
                        parent.Children[j].Visibility = Visibility.Collapsed;
                        continue;
                    }

                    // if its reverse arrow on slider head then skip
                    if (i == 1 && j > 3)
                    {
                        if (parent.Children[j].Visibility == Visibility.Visible)
                        {
                            parent.Children[j].Visibility = Visibility.Collapsed;
                        }

                        continue;
                    }

                    if (parent.Children[j].Visibility == Visibility.Collapsed || parent.Children[j].Visibility == Visibility.Hidden)
                    {
                        parent.Children[j].Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private static PathGeometry CreateSliderPath(Slider slider)
        {
            // sliderscorepoint.png          slider tick
            List<Vector2> pathPoints = slider.Path.CalculatedPath();

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = new Point(pathPoints[0].X, pathPoints[0].Y);
            
            PointCollection myPointCollection = new PointCollection(slider.ControlPoints.Length);
            for (int i = 1; i < pathPoints.Count; i++)
            {
                myPointCollection.Add(new Point(pathPoints[i].X, pathPoints[i].Y));
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

        private static Canvas SliderBall(Slider slider, double diameter)
        {
            Canvas ball = new Canvas();
            ball.Width = diameter;
            ball.Height = diameter;

            Image sliderBall = new Image()
            {
                Width = diameter * 1.4,
                Height = diameter * 1.4,
                Source = new BitmapImage(new Uri(SkinElement.SliderBall())),
            };

            Image sliderBallCircle = new Image()
            {
                Width = diameter * 2.4,
                Height = diameter * 2.4,
                Source = new BitmapImage(new Uri(SkinElement.SliderBallCircle())), 
            };

            ball.Children.Add(sliderBall);
            ball.Children.Add(sliderBallCircle);

            Vector2 s = slider.Path.PositionAt(0);
            Canvas.SetLeft(ball, s.X - ((diameter * 1.4) / 2));
            Canvas.SetTop(ball, s.Y - ((diameter * 1.4) / 2));

            Canvas.SetLeft(sliderBallCircle, s.X - ((diameter) / 2));
            Canvas.SetTop(sliderBallCircle, s.Y - ((diameter) / 2));

            ball.Visibility = System.Windows.Visibility.Collapsed;

            return ball;
        }

        private static Path SliderBody(Slider slider, double diameter)
        {
            Path sliderBodyPath = new Path();
            sliderBodyPath.Data = CreateSliderPath(slider);
            sliderBodyPath.Stroke = new SolidColorBrush(System.Windows.Media.Color.FromRgb(3, 3, 12));
            sliderBodyPath.StrokeThickness = diameter * 0.85;
            sliderBodyPath.StrokeEndLineCap = PenLineCap.Round;
            sliderBodyPath.StrokeStartLineCap = PenLineCap.Round;
            /* funny
            my god and saviour... when he said not all is lost he was so right
            https://stackoverflow.com/questions/980798/wpf-bug-or-am-i-going-crazy
            in case stack overflow dies... i didnt even knew this existed i know im stupid but... im just stupid
            https://learn.microsoft.com/en-us/dotnet/api/system.windows.shapes.shape.strokelinejoin?view=windowsdesktop-9.0&redirectedfrom=MSDN#System_Windows_Shapes_Shape_StrokeLineJoin  */
            sliderBodyPath.StrokeLineJoin = PenLineJoin.Round;

            return sliderBodyPath;
        }

        private static Path SliderBorder(Slider slider, double diameter)
        {
            Path border = new Path();
            border.Data = CreateSliderPath(slider);
            border.StrokeThickness = diameter * 0.95;
            border.Stroke = Brushes.White;
            border.StrokeEndLineCap = PenLineCap.Round;
            border.StrokeStartLineCap = PenLineCap.Round;
            border.StrokeLineJoin = PenLineJoin.Round;

            return border;
        }

        private static void AddSliderTicks(Canvas body, Slider slider, double diameter)
        {
            for (int i = 0; i < slider.SliderTicks.Length; i++)
            {
                Image sliderTick = new Image()
                {
                    Source = new BitmapImage(new Uri(SkinElement.SliderTick())),
                    Width = diameter * 0.25,
                    Height = diameter * 0.25,
                };

                Canvas.SetLeft(sliderTick, slider.SliderTicks[i].Position.X - (sliderTick.Width / 2));
                Canvas.SetTop(sliderTick, slider.SliderTicks[i].Position.Y - (sliderTick.Width / 2));

                body.Children.Add(sliderTick);
            }
        }
    }
}
