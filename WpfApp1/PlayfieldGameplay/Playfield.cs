using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.OsuMaths;
using WpfApp1.PlayfieldUI.UIElements;
using Slider = ReplayParsers.Classes.Beatmap.osu.Objects.Slider;

#nullable disable

namespace WpfApp1.PlayfieldGameplay
{
    public static class Playfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static OsuMath math = new OsuMath();

        private static List<Canvas> AliveCanvasObjects = new List<Canvas>();
        public static List<Canvas> AliveHitMarkers = new List<Canvas>();

        private static int HitObjectIndex = 0;
        private static Canvas HitObject = null!;
        private static HitObject HitObjectProperties = null!;

        private static int CursorPositionIndex = 0;

        private static int HitMarkerIndex = 0;
        private static ReplayFrame CurrentFrame = MainWindow.replay.FramesDict[0];
        private static Canvas Marker = null;
        private static ReplayFrame MarkerFrame = MainWindow.replay.FramesDict[0];

        public static void UpdateHitMarkers()
        {
            if (HitMarkerIndex >= Analyser.Analyser.HitMarkers.Count)
            {
                return;
            }

            if (HitMarkerIndex < Analyser.Analyser.HitMarkers.Count && Marker != Analyser.Analyser.HitMarkers[HitMarkerIndex])
            {
                Marker = Analyser.Analyser.HitMarkers[HitMarkerIndex];
                MarkerFrame = Marker.DataContext as ReplayFrame;
            }

            if (GamePlayClock.TimeElapsed >= MarkerFrame.Time && !Window.playfieldCanva.Children.Contains(Marker))
            {
                CurrentFrame = MarkerFrame;
                Window.playfieldCanva.Children.Add(Marker);
                HitMarkerAnimation.Start(Marker);
                AliveHitMarkers.Add(Marker);

                if (AliveCanvasObjects.Count > 0)
                {
                    Canvas objectToHit = AliveCanvasObjects.First();
                    HitObject prop = objectToHit.DataContext as HitObject;

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;
                    float X = (float)((prop.X * osuScale) - (diameter / 2));
                    float Y = (float)((prop.Y * osuScale) - (diameter / 2));

                    // this Ellipse is hitbox area of circle and is created here coz actual circle cant have this as children
                    // then it check if Point (current hit marker location) is inside this Ellipse with Ellipse.Visible(pt)
                    System.Drawing.Drawing2D.GraphicsPath ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                    ellipse.AddEllipse(X, Y, (float)(diameter), (float)(diameter));
                    
                    System.Drawing.PointF pt = new System.Drawing.PointF((float)(MarkerFrame.X * osuScale), (float)(MarkerFrame.Y * osuScale));
                    if (ellipse.IsVisible(pt))
                    {
                        // sliders have set end time no matter what i think but circles dont so when circle is hit then delete it
                        if (prop is Circle && (MarkerFrame.Time + 400 >= prop.SpawnTime && MarkerFrame.Time - 400 <= prop.SpawnTime))
                        {
                            if (objectToHit.Visibility != Visibility.Collapsed)
                            {
                                double judgementX = (prop.X * osuScale - (diameter / 2));
                                double judgementY = (prop.Y * osuScale - (diameter));
                                GetHitJudgment(prop, MarkerFrame, judgementX, judgementY, diameter);
                            }

                            Window.playfieldCanva.Children.Remove(objectToHit);
                            AliveCanvasObjects.Remove(objectToHit);
                            objectToHit.Visibility = Visibility.Collapsed;
                        }
                        else if (prop is Slider && (MarkerFrame.Time + 400 >= prop.SpawnTime && MarkerFrame.Time - 400 <= prop.SpawnTime))
                        {
                            Canvas sliderHead = objectToHit.Children[1] as Canvas;

                            if (sliderHead.Visibility != Visibility.Collapsed)
                            {
                                double judgementX = (prop.X * osuScale - (diameter / 2));
                                double judgementY = (prop.Y * osuScale - (diameter));
                                GetHitJudgment(prop, MarkerFrame, judgementX, judgementY, diameter);

                                // hide only hit circle elements index 4 is reverse arrow
                                for (int i = 0; i <= 3; i++)
                                {
                                    sliderHead.Children[i].Visibility = Visibility.Collapsed;
                                }
                                
                                // reverse arrow if exists will now be visible
                                if (sliderHead.Children.Count > 4)
                                {
                                    sliderHead.Children[4].Visibility = Visibility.Visible;
                                }
                            }
                        }
                    }
                }

                HitMarkerIndex++;
            }
            else if (GamePlayClock.TimeElapsed < MarkerFrame.Time && Window.playfieldCanva.Children.Contains(Marker))
            {
                Window.playfieldCanva.Children.Remove(Marker);
                AliveHitMarkers.Remove(Marker);
            
                if (HitMarkerIndex > 0)
                {
                    HitMarkerIndex--;
                }

                Canvas newMarker = Analyser.Analyser.HitMarkers[HitMarkerIndex];
                ReplayFrame newFrame = Marker.DataContext as ReplayFrame;
                CurrentFrame = newFrame;
            }
        }

        private static void GetHitJudgment(HitObject prop, ReplayFrame frame, double X, double Y, double diameter)
        {
            double H300 = math.GetOverallDifficultyHitWindow300(MainWindow.map.Difficulty.OverallDifficulty);
            double H100 = math.GetOverallDifficultyHitWindow100(MainWindow.map.Difficulty.OverallDifficulty);
            double H50 = math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);

            Image img;
            if (frame.Time <= prop.SpawnTime + H300 && frame.Time >= prop.SpawnTime - H300)
            {
                img = Analyser.UIElements.HitJudgment.Image300();
                JudgementCounter.Increment300();
            }
            else if (frame.Time <= prop.SpawnTime + H100 && frame.Time >= prop.SpawnTime - H100)
            {
                img = Analyser.UIElements.HitJudgment.Image100();
                JudgementCounter.Increment100();
            }
            else if (frame.Time <= prop.SpawnTime + H50 && frame.Time >= prop.SpawnTime - H50)
            {
                img = Analyser.UIElements.HitJudgment.Image50();
                JudgementCounter.Increment50();
            }
            else
            {
                img = Analyser.UIElements.HitJudgment.ImageMiss();
                JudgementCounter.IncrementMiss();
            }

            img.Width = diameter;
            img.Height = diameter;

            Window.playfieldCanva.Children.Add(img);

            Canvas.SetLeft(img, X);
            Canvas.SetTop(img, Y);

            img.Loaded += async delegate (object sender, RoutedEventArgs e)
            {
                await Task.Delay(800);
                Window.playfieldCanva.Children.Remove(img);
            };
        }

        public static void UpdateHitMarkerIndexAfterSeek(ReplayFrame frame)
        {
            int i;
            bool found = false;
            for (i = 0; i < Analyser.Analyser.HitMarkers.Count; i++)
            {
                ReplayFrame hitMarkFrame = Analyser.Analyser.HitMarkers[i].DataContext as ReplayFrame;

                if (hitMarkFrame.Time >= frame.Time || i == Analyser.Analyser.HitMarkers.Count - 1)
                {
                    found = true;
                    break;
                }
            }

            if (found == true)
            {
                HitMarkerIndex = i;
            }
        }

        public static void UpdateHitObjects()
        {
            if (HitObjectIndex >= OsuBeatmap.HitObjectDictByIndex.Count)
            {
                return;
            }    

            if (HitObject != OsuBeatmap.HitObjectDictByIndex[HitObjectIndex])
            {
                HitObject = OsuBeatmap.HitObjectDictByIndex[HitObjectIndex];
                HitObjectProperties = (HitObject)HitObject.DataContext;
            }

            if (GamePlayClock.TimeElapsed > HitObjectProperties.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
            && !AliveCanvasObjects.Contains(HitObject))
            {
                AliveCanvasObjects.Add(HitObject);
                Window.playfieldCanva.Children.Add(OsuBeatmap.HitObjectDictByIndex[HitObjectIndex]);
                HitObject.Visibility = Visibility.Visible;

                HitObjectAnimations.Start(HitObject);

                HitObjectIndex++;
            }
        }

        public static void UpdateHitObjectIndexAfterSeek(long time)
        {
            double ArTime = math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);
            List<KeyValuePair<long, Canvas>> hitObjects = OsuBeatmap.HitObjectDictByTime.ToList();

            List<KeyValuePair<long, Canvas>> aliveHitObjects = hitObjects.Where(
                x => x.Key - ArTime < time 
                && GetEndTime(x.Value) > time && x.Value.Visibility != Visibility.Visible).ToList();

            bool found = false;
            int i;
            for (i = 0; i < hitObjects.Count; i++)
            {
                if (aliveHitObjects.Count > 0 && hitObjects[i].Value == aliveHitObjects[0].Value)
                {
                    found = true;
                    break;
                }
            }

            if (found == true)
            {
                HitObjectIndex = i;
            }
            else
            {
                var item = OsuBeatmap.HitObjectDictByTime.FirstOrDefault(
                    x => x.Key > time, OsuBeatmap.HitObjectDictByTime.Last());
                HitObjectIndex = hitObjects.IndexOf(item);
            }
        }

        public static void UpdateCursor()
        {
            if (CursorPositionIndex < MainWindow.replay.FramesDict.Count
            &&  CurrentFrame != MainWindow.replay.FramesDict[CursorPositionIndex])
            {
                CurrentFrame = MainWindow.replay.FramesDict[CursorPositionIndex];
            }

            // if statement works now just fine but just in case while is better i guess
            while (CursorPositionIndex < MainWindow.replay.FramesDict.Count && GamePlayClock.TimeElapsed >= CurrentFrame.Time)
            {
                double osuScale = MainWindow.OsuPlayfieldObjectScale;

                Canvas.SetLeft(Window.playfieldCursor, (CurrentFrame.X * osuScale) - (Window.playfieldCursor.Width / 2));
                Canvas.SetTop(Window.playfieldCursor, (CurrentFrame.Y * osuScale) - (Window.playfieldCursor.Width / 2));

                CursorPositionIndex++;
                CurrentFrame = CursorPositionIndex < MainWindow.replay.FramesDict.Count
                    ? MainWindow.replay.FramesDict[CursorPositionIndex]
                    : MainWindow.replay.FramesDict[MainWindow.replay.FramesDict.Count - 1];
            }
        }

        public static void UpdateCursorPositionAfterSeek(ReplayFrame frame)
        {
            CursorPositionIndex = MainWindow.replay.Frames.IndexOf(frame);
        }

        public static void HandleVisibleCircles()
        {
            if (AliveCanvasObjects.Count > 0)
            {
                // for first and last circle validation coz im stupid
                for (int i = AliveCanvasObjects.Count == 1 ? 1 : 0; i < 2; i++)
                {
                    Canvas toDelete;
                    if (i == 1)
                    {
                        toDelete = AliveCanvasObjects.First();
                    }
                    else
                    {
                        toDelete = AliveCanvasObjects.Last();
                    }

                    HitObject dc = (HitObject)toDelete.DataContext;

                    long elapsedTime = GamePlayClock.TimeElapsed;
                    if (elapsedTime >= GetEndTime(toDelete)
                    || elapsedTime <= dc.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate))
                    {
                        if (dc is Circle && toDelete.Visibility == Visibility.Visible)
                        {
                            MainWindow window = (MainWindow)Application.Current.MainWindow;

                            Image miss = Analyser.UIElements.HitJudgment.ImageMiss();
                            miss.Width = MainWindow.OsuPlayfieldObjectDiameter;
                            miss.Height = MainWindow.OsuPlayfieldObjectDiameter;

                            miss.Loaded += async delegate (object sender, RoutedEventArgs e)
                            {
                                await Task.Delay(800);
                                window.playfieldCanva.Children.Remove(miss);
                            };

                            double X = (dc.X * MainWindow.OsuPlayfieldObjectScale) - (MainWindow.OsuPlayfieldObjectDiameter / 2);
                            double Y = (dc.Y * MainWindow.OsuPlayfieldObjectScale) - MainWindow.OsuPlayfieldObjectDiameter;

                            Canvas.SetLeft(miss, X);
                            Canvas.SetTop(miss, Y);

                            JudgementCounter.IncrementMiss();
                            window.playfieldCanva.Children.Add(miss);
                        }

                        Window.playfieldCanva.Children.Remove(toDelete);
                        toDelete.Visibility = Visibility.Collapsed;
                        AliveCanvasObjects.Remove(toDelete);
                    }
                }
            }
        }

        private static bool SliderReversed = false;
        private static int TickIndex = 0;
        private static Slider CurrentSlider = null;
        private static int ReverseArrowTailIndex = 1;
        private static int ReverseArrowHeadIndex = 1;
        public static void UpdateSliderTicks()
        {
            // change to loop through every slider for aspire maps
            if (AliveCanvasObjects.Count > 0 && AliveCanvasObjects.First().DataContext is Slider)
            {
                Slider dc = AliveCanvasObjects.First().DataContext as Slider;

                if (dc.SliderTicks == null)
                {
                    return;
                }

                if (CurrentSlider != dc)
                {
                    CurrentSlider = dc;
                    TickIndex = 0;
                }

                // need to fix this so reverse sliders work properly with this...
                // sliders have ticks but they only work as if slider was just long without repeats
                double sliderPathDistance = (dc.EndTime - dc.SpawnTime) / dc.RepeatCount;

                bool reversed = false;
                if (Math.Floor(((GamePlayClock.TimeElapsed - dc.SpawnTime) / sliderPathDistance)) != 0)
                {
                    reversed = Math.Floor(((GamePlayClock.TimeElapsed - dc.SpawnTime) / sliderPathDistance)) % 2 == 1 ? true : false;
                }

                double howtomath;
                if (reversed == true)
                {
                    if (TickIndex == dc.SliderTicks.Length)
                    {
                        TickIndex--;
                    }

                    double sliderProgress = ((GamePlayClock.TimeElapsed - dc.SpawnTime) / sliderPathDistance);
                        
                    while (sliderProgress > 1)
                    {
                        sliderProgress -= 1;
                    }

                    howtomath = 1 - (sliderProgress);
                }
                else
                {
                    if (TickIndex == -1)
                    {
                        TickIndex = 0;
                    }

                    howtomath = (GamePlayClock.TimeElapsed - dc.SpawnTime) / sliderPathDistance;

                    while (howtomath > 1)
                    {
                        howtomath -= 1;
                    }
                }

                if (TickIndex == -1)
                {
                    return;
                }

                Canvas slider2 = AliveCanvasObjects.First();
                Canvas body2 = slider2.Children[0] as Canvas;
                Canvas ball2 = body2.Children[2] as Canvas;

                if ("a" == "b")
                {
                    Image hitboxBall2 = ball2.Children[1] as Image;

                    double osuScale2 = MainWindow.OsuPlayfieldObjectScale;
                    double hitboxBallWidth2 = hitboxBall2.Width * osuScale2;
                    double hitboxBallHeight2 = hitboxBall2.Height * osuScale2;

                    Point ballCentre2 = hitboxBall2.TranslatePoint(new Point(hitboxBallWidth2 / 2, hitboxBallHeight2 / 2), Window.playfieldCanva);


                    float cursorX2 = (float)((MainWindow.replay.FramesDict[CursorPositionIndex - 1].X * osuScale2));
                    float cursorY2 = (float)((MainWindow.replay.FramesDict[CursorPositionIndex - 1].Y * osuScale2));
                    System.Drawing.PointF pt2 = new System.Drawing.PointF(cursorX2, cursorY2);
                }



                if ((reversed == false && TickIndex < dc.SliderTicks.Length && howtomath >= dc.SliderTicks[TickIndex].PositionAt)
                ||  (reversed == true && TickIndex >= 0 && howtomath <= dc.SliderTicks[TickIndex].PositionAt))
                {
                    Canvas slider = AliveCanvasObjects.First();
                    Canvas body = slider.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;
                    
                    // ticks are starting at [3]
                    Image tick = body.Children[TickIndex + 3] as Image;
                    tick.Visibility = Visibility.Collapsed;
                    
                    Image hitboxBall = ball.Children[1] as Image;

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    double hitboxBallWidth = hitboxBall.Width;
                    double hitboxBallHeight = hitboxBall.Height;

                    Point ballCentre = hitboxBall.TranslatePoint(new Point(hitboxBallWidth / 2, hitboxBallHeight / 2), Window.playfieldCanva);

                    double diameter = hitboxBallHeight * osuScale;
                    double ballX = ballCentre.X - (diameter / 2);
                    double ballY = ballCentre.Y - (diameter / 2);



                    //Point tickCentre = tick.TranslatePoint(new Point((tick.Width * sliderScale.CenterX) / 2, (tick.Width * sliderScale.CenterX) / 2), Window.playfieldCanva);
                    //double tickX = (tickCentre.X - ((tick.Width * sliderScale.CenterX) / 2));
                    //double tickY = (tickCentre.Y - ((tick.Width * sliderScale.CenterX) / 2));

                    Rectangle middleHit = new Rectangle();
                    middleHit.Fill = Brushes.Cyan;
                    middleHit.Width = 10;
                    middleHit.Height = 10;

                    middleHit.Loaded += async delegate (object sender, RoutedEventArgs e)
                    {
                        await Task.Delay(1000);
                        Window.playfieldCanva.Children.Remove(middleHit);
                    };

                    double ballX2 = ballCentre.X;
                    double ballY2 = ballCentre.Y;

                    Canvas.SetLeft(middleHit, (ballX2 - 5));
                    Canvas.SetTop(middleHit, (ballY2 - 5));

                    Canvas.SetZIndex(middleHit, 99999);

                    Window.playfieldCanva.Children.Add(middleHit);

                    System.Drawing.Drawing2D.GraphicsPath ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                    ellipse.AddEllipse((float)(ballX2 - diameter / 2), (float)(ballY2 - diameter / 2), (float)diameter, (float)diameter);

                    // cursor pos index - 1 coz its always ahead by one from incrementing at the end of cursor update
                    float cursorX = (float)((MainWindow.replay.FramesDict[CursorPositionIndex - 1].X * osuScale) );//- (Window.playfieldCursor.Width / 2));
                    float cursorY = (float)((MainWindow.replay.FramesDict[CursorPositionIndex - 1].Y * osuScale));//- (Window.playfieldCursor.Width / 2));
                    System.Drawing.PointF pt = new System.Drawing.PointF(cursorX, cursorY);


                    //* ellipse for test hopefully wont need it aaaaa
                    {
                        Ellipse frick = new Ellipse();
                        frick.Width = Window.playfieldCursor.Width;
                        frick.Height = Window.playfieldCursor.Width;
                        frick.Fill = System.Windows.Media.Brushes.Cyan;
                        frick.Opacity = 0.5;

                        frick.Loaded += async delegate (object sender, RoutedEventArgs e)
                        {
                            await Task.Delay(1000);
                            Window.playfieldCanva.Children.Remove(frick);
                        };

                        Canvas.SetLeft(frick, cursorX - (0));
                        Canvas.SetTop(frick, cursorY - (0));

                        Window.playfieldCanva.Children.Add(frick);

                        Ellipse hitbox = new Ellipse();
                        hitbox.Width = diameter;
                        hitbox.Height = diameter;
                        hitbox.Fill = System.Windows.Media.Brushes.Red;
                        hitbox.Opacity = 0.5;

                        hitbox.Loaded += async delegate (object sender, RoutedEventArgs e)
                        {
                            await Task.Delay(1000);
                            Window.playfieldCanva.Children.Remove(hitbox);
                        };

                        Canvas.SetLeft(hitbox, ballX2 - (diameter / 2));
                        Canvas.SetTop(hitbox, ballY2 - (diameter / 2));

                        Window.playfieldCanva.Children.Add(hitbox);

                        //Ellipse tickBox = new Ellipse();
                        //tickBox.Width = tick.Width;
                        //tickBox.Height = tick.Width;
                        //tickBox.Fill = System.Windows.Media.Brushes.Yellow;
                        //tickBox.Opacity = 0.5;
                        //
                        //tickBox.Loaded += async delegate (object sender, RoutedEventArgs e)
                        //{
                        //    await Task.Delay(1000);
                        //    Window.playfieldCanva.Children.Remove(tickBox);
                        //};
                        //
                        //Canvas.SetLeft(tickBox, tickX - (0));
                        //Canvas.SetTop(tickBox, tickY - (0));
                        //
                        //Window.playfieldCanva.Children.Add(tickBox);
                    }
                    //*/

                    if (!ellipse.IsVisible(pt) || MarkerFrame.Click == 0 || MarkerFrame.Click == Clicks.Smoke)
                    {
                        Image miss = Analyser.UIElements.HitJudgment.ImageMiss();
                        miss.Width = MainWindow.OsuPlayfieldObjectDiameter;
                        miss.Height = MainWindow.OsuPlayfieldObjectDiameter;
                    
                        miss.Loaded += async delegate (object sender, RoutedEventArgs e)
                        {
                            await Task.Delay(800);
                            Window.playfieldCanva.Children.Remove(miss);
                        };
                    
                        Canvas.SetLeft(miss, ballX2 - (miss.Width / 2));
                        Canvas.SetTop(miss, ballY2 - (miss.Width / 2));
                    
                        JudgementCounter.IncrementMiss();
                        Window.playfieldCanva.Children.Add(miss);
                    }
                    
                    if (reversed == false && TickIndex < dc.SliderTicks.Length )
                    {
                        TickIndex++;
                    }
                    else if (reversed == true && TickIndex >= 0)
                    {
                        TickIndex--;
                    }
                }
            }
        }

        private static double RepeatAt = 0;
        private static double RepeatInterval = 0;
        private static Slider CurrentReverseSlider = null;
        public static void UpdateSliderRepeats()
        {
            if (AliveCanvasObjects.Count > 0 && AliveCanvasObjects.First().DataContext is Slider)
            {
                Canvas slider = AliveCanvasObjects.First();
                Slider dc = AliveCanvasObjects.First().DataContext as Slider;

                if (CurrentReverseSlider != dc)
                {
                    SliderReversed = false;
                    CurrentReverseSlider = dc;
                    ReverseArrowTailIndex = 1;
                    ReverseArrowHeadIndex = 1;

                    RepeatInterval = ((double)1 / (double)dc.RepeatCount);
                    RepeatAt = RepeatInterval;
                }

                if (dc.RepeatCount > 1)
                {
                    double progress = (GamePlayClock.TimeElapsed - dc.SpawnTime) / (dc.EndTime - dc.SpawnTime);
                    if (progress > RepeatAt && RepeatAt != 1)
                    {
                        if (SliderReversed == false)
                        {
                            Canvas tail = slider.Children[2] as Canvas;
                            tail.Children[tail.Children.Count - ReverseArrowTailIndex].Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            Canvas head = slider.Children[1] as Canvas;
                            head.Children[head.Children.Count - ReverseArrowHeadIndex].Visibility = Visibility.Collapsed;
                        }

                        RepeatAt += RepeatInterval;
 
                        if (SliderReversed == false)
                        {
                            SliderReversed = true;
                            ReverseArrowTailIndex += 1;
                        }
                        else
                        {
                            SliderReversed = false;
                            ReverseArrowHeadIndex += 1;
                        }

                        // this is for resetting slider ticks once repeat arrow is hit
                        // tick children in slider body start at index 3
                        Canvas body = slider.Children[0] as Canvas;
                        for (int i = 3; i < body.Children.Count; i++)
                        {
                            body.Children[i].Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        public static int AliveHitObjectCount()
        {
            return AliveCanvasObjects.Count;
        }

        public static List<Canvas> GetAliveHitObjects()
        {
            return AliveCanvasObjects;
        }

        private static double GetEndTime(Canvas o)
        {
            if (o.DataContext is Slider)
            {
                Slider obj = o.DataContext as Slider;
                return (int)obj.EndTime;
            }
            else if (o.DataContext is Spinner)
            {
                Spinner obj = o.DataContext as Spinner;
                return obj.EndTime;
            }
            else
            {
                Circle obj = o.DataContext as Circle;
                return obj.SpawnTime + math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);
            }
        }
    }
}
