using OsuFileParsers.SliderPathMath;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using System.Drawing;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using SliderData = OsuFileParsers.Classes.Beatmap.osu.Objects.SliderData;
using SliderTick = ReplayAnalyzer.PlayfieldGameplay.SliderEvents.SliderTick;
using SliderTickData = OsuFileParsers.Classes.Beatmap.osu.Objects.SliderTick;

#nullable disable

namespace ReplayAnalyzer.HitObjects
{
    public class Slider : HitObject
    {
        private static OsuMath OsuMath = new OsuMath();

        public Slider(SliderData sliderData)
        {
            X = sliderData.X;
            Y = sliderData.Y;
            BaseSpawnPosition = sliderData.BaseSpawnPosition;
            SpawnTime = sliderData.SpawnTime;

            Path = sliderData.Path;
            EndPosition = sliderData.EndPosition;
            RepeatCount = sliderData.RepeatCount;
            EndTime = sliderData.EndTime;
            SliderTicks = sliderData.SliderTicks;
            AllTicksHit = sliderData.AllTicksHit;
            Judgement = new HitJudgement((HitObjectJudgement)sliderData.Judgement.Judgement, sliderData.Judgement.SpawnTime);
            SliderEndJudgement = new HitJudgement((HitObjectJudgement)sliderData.SliderEndJudgement.Judgement, sliderData.SliderEndJudgement.SpawnTime);

            if (EndTime - SpawnTime < OsuMath.GetJudgement50HitWindow())
            {
                DespawnTime = SpawnTime + OsuMath.GetJudgement50HitWindow();
            }
            else
            {
                DespawnTime = EndTime;
            }
        }

        public SliderPath Path { get; set; }
        public Vector2 EndPosition { get; set; }
        public int RepeatCount { get; set; }
        public double EndTime { get; set; }
        public double DespawnTime { get; set; }
        public List<SliderTickData> SliderTicks { get; set; }
        public HitJudgement SliderEndJudgement { get; set; } = new HitJudgement(HitObjectJudgement.None, 0);
        public bool AllTicksHit { get; set; } = true;

        public static double BallHitboxDiameter
        {
            get { return MainWindow.OsuPlayfieldObjectDiameter * 2.4; }
        }

        public static Slider CreateSlider(SliderData slider, double diameter, int currentComboNumber, int index, int comboColourIndex)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateSliderObject(slider, diameter, currentComboNumber, index, comboColourIndex);
            }

            return CreateSliderPreload(slider, diameter, index);
        }

        private static Slider CreateSliderObject(SliderData slider, double diameter, int currentComboNumber, int index, int comboColourIndex)
        {
            Slider fullSlider = new Slider(slider);
            fullSlider.Name = $"SliderHitObject{index}";
            fullSlider.Width = diameter;
            fullSlider.Height = diameter;

            Canvas head = CreateSliderHead(slider, diameter, currentComboNumber, fullSlider.Name, comboColourIndex);
            Canvas body = CreateSliderBody(slider, diameter);
            Canvas tail = CreateSliderTail(slider, diameter);

            fullSlider.Children.Add(head);
            fullSlider.Children.Add(body);
            fullSlider.Children.Add(tail);

            fullSlider.Visibility = Visibility.Collapsed;

            SetZIndex(fullSlider, 0 - index);

            // -17 (1 frame(16ms) + 1ms just in case) is coz if judgement spawns at the very end of a slider the time of it
            // have a bit more ms than it should have (like TimeElapsed is 1710 but Judgement.SpawnTime is 1718 coz
            // there is no additional replay frame for DespawnTime judgement and for EndTime its sometimes(always?) off coz of number rounding)
            if (GamePlayClock.TimeElapsed >= slider.Judgement.SpawnTime - 17)
            {
                // slider so short if it got missed head should never be removed since it means it was never clicked
                if (fullSlider.Judgement.Judgement != HitObjectJudgement.Miss
                &&  fullSlider.DespawnTime > fullSlider.EndTime)
                {
                    RemoveSliderHead(fullSlider); 
                }
                else if (fullSlider.DespawnTime == fullSlider.EndTime)
                {// normal size slider so just remove it if it got clicked or despawned (there 100% is edge case for this i swear to god)
                    RemoveSliderHead(fullSlider);
                }

                SliderReverseArrow.UpdateReverseArrowsVisibility(fullSlider);
                SliderTick.UpdateTicksVisibility(fullSlider);
            }

            return fullSlider;
        }

        private static Slider CreateSliderPreload(SliderData slider, double diameter, int index)
        {
            Slider fullSlider = new Slider(slider);
            fullSlider.Name = $"SliderHitObject{index}";
            fullSlider.Width = diameter;
            fullSlider.Height = diameter;

            Canvas head = new Canvas();

            Image hitCircle = new Image();
            Canvas hitCircleBorder2 = new Canvas();
            Canvas comboNumber = new Canvas();
            Image approachCircle = new Image();
            
            head.Children.Add(hitCircle);
            head.Children.Add(hitCircleBorder2);
            head.Children.Add(comboNumber);
            head.Children.Add(approachCircle);
            
            Canvas body = new Canvas();

            // add padding objects so preloading can work just like normal replay playing with ticks starting at index 2
            for (int i = 0; i < 2; i++)
            {
                body.Children.Add(new Canvas());
            }

            if (slider.SliderTicks != null)
            {
                AddSliderTicks(body, slider, diameter);
            }

            Canvas tail = new Canvas();

            fullSlider.Children.Add(head);
            fullSlider.Children.Add(body);
            fullSlider.Children.Add(tail);

            return fullSlider;
        }

        private static Canvas CreateSliderHead(SliderData slider, double diameter, int currentComboNumber, string name, int comboColourIndex)
        {
            Canvas head = new Canvas();
            head.Width = diameter;
            head.Height = diameter;
            head.Name = name;

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
                Height = diameter * 4,
                Width = diameter * 4,
                Source = new BitmapImage(new Uri(SkinElement.ApproachCircle())),
                Name = "ApproachCircle",
            };

            head.Children.Add(hitCircle);
            head.Children.Add(hitCircleBorder2);
            head.Children.Add(comboNumber);
            head.Children.Add(approachCircle);

            AddReverseArrowsToHead(slider, diameter, head);

            Canvas.SetLeft(head, slider.X - diameter / 2);
            Canvas.SetTop(head, slider.Y - diameter / 2);
            Canvas.SetZIndex(head, 3);

            return head;
        }

        private static void AddReverseArrowsToHead(SliderData slider, double diameter, Canvas head)
        {
            // 1st one is nothing, 2nd one is slider end repeat
            if (slider.RepeatCount <= 2)
            {
                return;
            }

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
                    Visibility = Visibility.Collapsed,
                };

                Canvas.SetZIndex(reverseArrow, 2);
                head.Children.Add(reverseArrow);

                reverseArrowCount--;
            }   
        }

        private static Canvas CreateSliderTail(SliderData slider, double diameter)
        {
            Canvas tail = new Canvas();
            tail.Width = diameter;
            tail.Height = diameter;

            AddReverseArrowsToTail(slider, diameter, tail);

            Canvas.SetLeft(tail, (slider.EndPosition.X * MainWindow.OsuPlayfieldObjectScale) - diameter / 2);
            Canvas.SetTop(tail, (slider.EndPosition.Y * MainWindow.OsuPlayfieldObjectScale) - diameter / 2);

            return tail;
        }

        private static void AddReverseArrowsToTail(SliderData slider, double diameter, Canvas tail)
        {
            // 1 is no repeats
            if (slider.RepeatCount == 1)
            {
                return;
            }

            int reverseArrowCount = (int)Math.Floor(slider.RepeatCount / 2.0);
            bool isFirst = true;
            while (reverseArrowCount != 0)
            {
                Image reverseArrow = new Image()
                {
                    Width = diameter,
                    Height = diameter,
                    Source = new BitmapImage(new Uri(SkinElement.ReverseArrow())),
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new RotateTransform(GetReverseArrowAngle(slider, true)),
                    Visibility = Visibility.Collapsed,
                };

                if (isFirst == true)
                {
                    isFirst = false;
                    reverseArrow.Visibility = Visibility.Visible;
                }
 
                Canvas.SetZIndex(reverseArrow, 2);
                tail.Children.Add(reverseArrow);

                reverseArrowCount--;
            }
        }

        private static Canvas CreateSliderBody(SliderData slider, double diameter)
        {
            Canvas body = new Canvas();
            body.Width = 1;
            body.Height = 1;

            Canvas.SetLeft(body, slider.X);
            Canvas.SetTop(body, slider.Y);

            SetZIndex(body, -1);

            Path border = SliderBorder(slider, diameter);
            Path sliderBodyPath = SliderBody(slider, diameter);
            Canvas ball = SliderBall(slider, diameter);

            Canvas bodyPaths = new Canvas();
            // give slight transparency to slider bodies to not block any objects under them 
            bodyPaths.Opacity = 0.8;

            bodyPaths.Children.Add(border);
            bodyPaths.Children.Add(sliderBodyPath);

            body.Children.Add(bodyPaths);
            body.Children.Add(ball);

            if (slider.SliderTicks != null)
            {
                AddSliderTicks(body, slider, diameter);
            }

            return body;
        }

        private static PathGeometry CreateSliderPath(SliderData slider)
        {
            List<Vector2> pathPoints = slider.Path.CalculatedPath();

            PathFigure myPathFigure = new PathFigure();
            myPathFigure.StartPoint = new Point(pathPoints[0].X, pathPoints[0].Y);

            PointCollection myPointCollection = new PointCollection(slider.ControlPoints.Length);
            for (int i = 1; i < pathPoints.Count; i++)
            {
                myPointCollection.Add(new Point(pathPoints[i].X * MainWindow.OsuPlayfieldObjectScale, pathPoints[i].Y * MainWindow.OsuPlayfieldObjectScale));
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
            myPathGeometry.Freeze();

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
                Width = BallHitboxDiameter,
                Height = BallHitboxDiameter,
                Source = new BitmapImage(new Uri(SkinElement.SliderBallCircle())),
            };

            ball.Children.Add(sliderBall);
            ball.Children.Add(sliderBallCircle);

            Vector2 s = slider.Path.PositionAt(0);
            Canvas.SetLeft(ball, s.X - diameter * 1.4 / 2);
            Canvas.SetTop(ball, s.Y - diameter * 1.4 / 2);
            
            Canvas.SetLeft(sliderBallCircle, s.X - diameter / 2);
            Canvas.SetTop(sliderBallCircle, s.Y - diameter / 2);

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
            for (int i = 0; i < slider.SliderTicks.Count; i++)
            {
                Image sliderTick = new Image()
                {
                    Source = new BitmapImage(new Uri(SkinElement.SliderTick())),
                    Width = diameter * 0.25,
                    Height = diameter * 0.25,
                };

                if (i >= slider.SliderTicks.Count / slider.RepeatCount)
                {
                    sliderTick.Visibility = Visibility.Collapsed;
                }

                Canvas.SetLeft(sliderTick, slider.SliderTicks[i].Position.X * MainWindow.OsuPlayfieldObjectScale - sliderTick.Width / 2);
                Canvas.SetTop(sliderTick, slider.SliderTicks[i].Position.Y * MainWindow.OsuPlayfieldObjectScale - sliderTick.Width / 2);

                body.Children.Add(sliderTick);
            }
        }

        public static void ResetToDefault(HitObject slider)
        {
            // yes this is horrible whatever
            // i 0 = head, 1 = body, 2 = tail
            // j are children of above objects
            for (int i = 0; i < slider.Children.Count; i++)
            {
                Canvas parent = slider.Children[i] as Canvas;
                if (parent.Visibility == Visibility.Collapsed)
                {
                    parent.Visibility = Visibility.Visible;
                }

                for (int j = 0; j < parent.Children.Count; j++)
                {
                    // collapse slider body ball and continue to not make them visible again
                    if (i == 1 && j == 2)
                    {
                        Canvas ball = parent.Children[j] as Canvas;
                        ball.Visibility = Visibility.Collapsed;

                        continue;
                    }

                    // make reverse arrows on slider head (index 4 and above) collapsed
                    if (i == 0 && j >= 4)
                    {
                        if (parent.Children[j].Visibility == Visibility.Visible)
                        {
                            parent.Children[j].Visibility = Visibility.Collapsed;
                        }

                        continue;
                    }

                    // in slider tail make FIRST reverse arrow visible and then everything after collapsed
                    if (i == 2)
                    {
                        parent.Children[j].Visibility = j == 0 
                                                      ? Visibility.Visible 
                                                      : Visibility.Collapsed;

                        continue;
                    }

                    // make all children visible for head, body and tail
                    if (parent.Children[j].Visibility == Visibility.Collapsed)
                    {
                        parent.Children[j].Visibility = Visibility.Visible;
                    }
                }
            }
        }

        // yes i throw all functions here instead of different class coz i cant come up with name of new class to put these in
        // and also i dont care
        public static void UpdateAliveSliderEvents()
        {
            if (HitObjectManager.GetAliveHitObjects().Count == 0)
            {
                return;
            }

            if (HitObjectManager.GetAliveHitObjects().First() is Slider slider)
            {
                if (slider is Slider s && s.EndTime >= GamePlayClock.TimeElapsed)
                {
                    UpdateCurrentSliderValues(s);
                }
            }
        }

        public static void HideHeadReverseArrows(Slider s)
        {
            Canvas head = Head(s);
            for (int i = head.Children.Count - 1; i >= 4; i--)
            {
                head.Children[i].Visibility = Visibility.Collapsed;
            }
        }

        public static void HideTailReverseArrows(Slider s)
        {
            Canvas tail = Tail(s);
            for (int i = tail.Children.Count - 1; i >= 0; i--)
            {
                tail.Children[i].Visibility = Visibility.Collapsed;
            }
        }

        public static void HideSliderTicks(Slider s)
        {
            if (s.SliderTicks != null)
            {
                Canvas body = Body(s);
                for (int i = 3; i < 3 + s.SliderTicks.Count; i++)
                {
                    Image tick = body.Children[i] as Image;
                    tick.Visibility = Visibility.Collapsed;
                }
            }
        }

        public static void HideAllSliderEvents(Slider s)
        {
            HideHeadReverseArrows(s);
            HideSliderTicks(s);
            HideTailReverseArrows(s);
        }

        public static void ShowSliderHead(Slider s)
        {
            Canvas sliderHead = Head(s);
            for (int j = 0; j <= 3; j++)
            {
                sliderHead.Children[j].Visibility = Visibility.Visible;
            }

            // hides reverse arrow
            if (sliderHead.Children.Count > 4)
            {
                sliderHead.Children[4].Visibility = Visibility.Collapsed;
            }
            sliderHead.Visibility = Visibility.Visible;
        }

        public static void RemoveSliderHead(Slider s)
        {
            Canvas head = Head(s);
            for (int i = 0; i <= 3; i++)
            {
                head.Children[i].Visibility = Visibility.Collapsed;
            }

            // reverse arrow if exists will now be visible
            if (head.Children.Count > 4)
            {
                head.Children[4].Visibility = Visibility.Visible;
            }
        }

        public static void UpdateCurrentSliderValues(Slider s)
        {
            ResetToDefault(s);

            // -17 (1 frame(16ms) + 1ms just in case) is for EXTREMELY short sliders that have judgements spawned at the
            // exact same time when they despawn, without that slider head will not reset in these sliders
            if (GamePlayClock.TimeElapsed >= s.Judgement.SpawnTime - 17)
            {
                RemoveSliderHead(s);
            }

            SliderReverseArrow.UpdateReverseArrowsVisibility(s);
            SliderTick.UpdateTicksVisibility(s);
        }

        public static Slider GetFirstSliderBySpawnTime()
        {
            Slider slider = null;
            foreach (HitObject obj in HitObjectManager.GetAliveHitObjects())
            {
                if (obj is not Slider)
                {
                    continue;
                }

                if (slider == null || slider.SpawnTime > obj.SpawnTime)
                {
                    slider = obj as Slider;
                }
            }

            return slider;
        }

        // maybe i should start using expression methods if return value is short coz it looks nice
        public static Canvas Head(Slider s) => s.Children[0] as Canvas;

        public static Image HeadHitCircle(Slider s) => Head(s).Children[0] as Image;

        public static Image HeadApproachCircle(Slider s) => Head(s).Children[3] as Image;

        public static Canvas Body(Slider s) => s.Children[1] as Canvas;

        public static Canvas BodyPath(Slider s) => Body(s).Children[0] as Canvas;

        public static Canvas BodyBall(Slider s) => Body(s).Children[1] as Canvas;

        public static Image BodyBallHitBox(Slider s) => BodyBall(s).Children[1] as Image;

        public static Canvas Tail(Slider s) => s.Children[2] as Canvas;
    }
}
