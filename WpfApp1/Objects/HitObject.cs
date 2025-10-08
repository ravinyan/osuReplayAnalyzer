using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.SliderPathMath;
using System.Drawing;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfApp1.Animations;
using WpfApp1.OsuMaths;
using WpfApp1.Skinning;
using WpfApp1.Skins;
using Image = System.Windows.Controls.Image;
using System.Windows;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Color = System.Drawing.Color;

namespace WpfApp1.Objects
{
    // i know its not really needed since there is DataContext in canvas classes, but i want to practice inheritance
    // and learn how to nicely use it and implement it instead of doing it messy... and maybe it will make code look nicer too
    public class HitObject : Canvas
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public int SpawnTime { get; set; }
        public int StackHeight { get; set; }
        public double HitAt { get; set; }
        public bool IsHit { get; set; }

        public static Grid AddComboNumber(int comboNumber, double diameter)
        {
            Grid grid = new Grid();
            grid.Width = diameter;
            grid.Height = diameter;

            StackPanel numberPanel = new StackPanel();
            numberPanel.Orientation = Orientation.Horizontal;
            numberPanel.HorizontalAlignment = HorizontalAlignment.Center;

            if (comboNumber <= 9)
            {
                Image hitCircleNumber = new Image()
                {
                    Height = (diameter / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(comboNumber))),
                };

                numberPanel.Children.Add(hitCircleNumber);
            }
            else if (comboNumber <= 99)
            {
                char[] number = comboNumber.ToString().ToCharArray();

                Image hitCircleNumber = new Image()
                {
                    Height = (diameter / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[0]))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = (diameter / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[1]))),
                };

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
            }
            else if (comboNumber <= 999)
            {
                char[] number = comboNumber.ToString().ToCharArray();

                Image hitCircleNumber = new Image()
                {
                    Height = (diameter / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[0]))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = (diameter / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[1]))),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = (diameter / 2) * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[2]))),
                };

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
                numberPanel.Children.Add(hitCircleNumber3);
            }
            else
            {
                Image hitCircleNumber = new Image()
                {
                    Height = (diameter / 2) * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(7))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = (diameter / 2) * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(2))),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = (diameter / 2) * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(7))),
                };

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
                numberPanel.Children.Add(hitCircleNumber3);
            }

            grid.Children.Add(numberPanel);

            return grid;
        }
    }

    public class Sliderr : HitObject
    {
        public Sliderr(SliderData sliderData)
        {
            X = sliderData.X;
            Y = sliderData.Y;
            SpawnPosition = sliderData.SpawnPosition;
            SpawnTime = sliderData.SpawnTime;
            StackHeight = sliderData.StackHeight;
            HitAt = sliderData.HitAt;
            IsHit = sliderData.IsHit;

            ControlPoints = sliderData.ControlPoints;
            Path = sliderData.Path;
            EndPosition = sliderData.EndPosition;
            RepeatCount = sliderData.RepeatCount;
            Length = sliderData.Length;
            EndTime = sliderData.EndTime;
            SliderTicks = sliderData.SliderTicks;

            OsuMath math = new OsuMath();
            if (EndTime - SpawnTime < math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty))
            {
                DespawnTime = SpawnTime + math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);
            }
            else
            {
                DespawnTime = EndTime;
            }
        }

        public PathControlPoint[] ControlPoints { get; set; }
        public SliderPath Path { get; set; }
        public Vector2 EndPosition { get; set; }
        public int RepeatCount { get; set; }
        public decimal Length { get; set; }
        public double EndTime { get; set; }
        public double DespawnTime { get; set; }
        public SliderTick[] SliderTicks { get; set; }

        public static Sliderr CreateSlider(SliderData slider, double diameter, int currentComboNumber, int index, Color comboColour)
        {
            // and maybe 
            // sliderstartcircleoverlay.png
            // sliderpoint30.png
            // sliderpoint10.png

            Sliderr fullSlider = new Sliderr(slider);
            fullSlider.Name = $"SliderHitObject{index}";
            fullSlider.Width = diameter;
            fullSlider.Height = diameter;

            Canvas head = CreateSliderHead(slider, diameter, currentComboNumber, fullSlider.Name, comboColour);
            Canvas body = CreateSliderBody(slider, diameter);
            Canvas tail = CreateSliderTail(slider, diameter);

            fullSlider.Children.Add(body);
            fullSlider.Children.Add(head);
            fullSlider.Children.Add(tail);

            fullSlider.Visibility = Visibility.Collapsed;

            Canvas.SetZIndex(fullSlider, 0 - index);

            HitObjectAnimations.ApplySliderAnimations(fullSlider);

            return fullSlider;
        }

        private static Canvas CreateSliderHead(SliderData slider, double diameter, int currentComboNumber, string name, Color comboColour)
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

        private static Canvas CreateSliderTail(SliderData slider, double diameter)
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
                        reverseArrow.Visibility = Visibility.Collapsed;
                        tail.Children.Add(reverseArrow);
                    }

                    reverseArrowCount--;
                }
            }

            Canvas.SetLeft(tail, (slider.EndPosition.X) - (diameter / 2));
            Canvas.SetTop(tail, (slider.EndPosition.Y) - (diameter / 2));

            return tail;
        }

        private static Canvas CreateSliderBody(SliderData slider, double diameter)
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

                if (parent.Visibility == Visibility.Collapsed || parent.Visibility == Visibility.Hidden)
                {
                    parent.Visibility = Visibility.Visible;
                }

                for (int j = 0; j < parent.Children.Count; j++)
                {
                    // if its slider ball then make it collapsed and skip
                    if (i == 0 && j == 2)
                    {
                        Canvas? ball = parent.Children[j] as Canvas;
                        ball!.Visibility = Visibility.Collapsed;

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

        private static PathGeometry CreateSliderPath(SliderData slider)
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

        private static double GetReverseArrowAngle(SliderData slider, bool isRepeatAtEnd)
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

        private static Canvas SliderBall(SliderData slider, double diameter)
        {
            Canvas ball = new Canvas();
            ball.Width = diameter * 2.4;
            ball.Height = diameter * 2.4;

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

            ball.Visibility = Visibility.Collapsed;

            return ball;
        }

        private static Path SliderBody(SliderData slider, double diameter)
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

        private static Path SliderBorder(SliderData slider, double diameter)
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

        private static void AddSliderTicks(Canvas body, SliderData slider, double diameter)
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

    public class Spinnerr : HitObject
    {
        public Spinnerr(SpinnerData spinnerData)
        {
            X = spinnerData.X;
            Y = spinnerData.Y;
            SpawnPosition = spinnerData.SpawnPosition;
            SpawnTime = spinnerData.SpawnTime;

            EndTime = spinnerData.EndTime;
        }

        public int EndTime { get; set; }

        private static MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Spinnerr CreateSpinner(SpinnerData spinner, double radius, int i)
        {
            Spinnerr spinnerObject = new Spinnerr(spinner);
            spinnerObject.Width = Window.playfieldCanva.Width;
            spinnerObject.Height = Window.playfieldCanva.Height;
            spinnerObject.Name = $"SpinnerHitObject{i}";

            double acRadius = radius * 6;
            Image approachCircle = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerApproachCircle())),
                Width = acRadius,
                Height = acRadius,
            };

            Image background = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerBackground())),
                Width = Window.playfieldCanva.Width,
                Height = Window.playfieldCanva.Height,
            };

            double rbRadius = radius * 3;
            Image rotatingBody = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerCircle())),
                Width = rbRadius,
                Height = rbRadius,
            };

            spinnerObject.Visibility = Visibility.Collapsed;

            spinnerObject.Children.Add(rotatingBody);
            spinnerObject.Children.Add(background);
            spinnerObject.Children.Add(approachCircle);

            HitObjectAnimations.ApplySpinnerAnimations(spinnerObject);

            Canvas.SetLeft(approachCircle, (spinner.SpawnPosition.X) - (acRadius / 2));
            Canvas.SetTop(approachCircle, (spinner.SpawnPosition.Y) - (acRadius / 2));

            Canvas.SetLeft(rotatingBody, (spinner.SpawnPosition.X) - (rbRadius / 2));
            Canvas.SetTop(rotatingBody, (spinner.SpawnPosition.Y) - (rbRadius / 2));

            Canvas.SetLeft(spinnerObject, (spinner.SpawnPosition.X) - (Window.playfieldCanva.Width / 2));
            Canvas.SetTop(spinnerObject, (spinner.SpawnPosition.Y) - (Window.playfieldCanva.Height / 2));

            return spinnerObject;
        }
    }

    public class HitCircle : HitObject
    {
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
            HitCircle hitObject = new HitCircle(circleData);
            hitObject.Width = diameter;
            hitObject.Height = diameter;

            Image hitCircle = SkinHitCircle.ApplyComboColourToHitObject(new Bitmap(SkinElement.HitCircle()), comboColour, diameter);

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

            Canvas.SetLeft(hitObject, (hitObject.X) - (diameter / 2));
            Canvas.SetTop(hitObject, (hitObject.Y) - (diameter / 2));

            // circles 1 2 3 were rendered so 3 was on top...
            // (0 - index) gives negative value so that 1 will be rendered on top
            // basically correct zindexing like it should be for every object
            Canvas.SetZIndex(hitObject, 0 - index);

            hitObject.Name = $"CircleHitObject{index}";

            hitObject.Visibility = Visibility.Collapsed;

            HitObjectAnimations.ApplyHitCircleAnimations(hitObject);

            return hitObject;
        }
    }
}
