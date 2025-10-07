using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Replay;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.Analyser.UIElements;
using WpfApp1.Animations;
using WpfApp1.Beatmaps;
using WpfApp1.GameClock;
using WpfApp1.Objects;
using WpfApp1.OsuMaths;
using WpfApp1.PlayfieldUI.UIElements;
using SliderData = ReplayParsers.Classes.Beatmap.osu.Objects.SliderData;

#nullable disable

namespace WpfApp1.PlayfieldGameplay
{
    public static class Playfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static OsuMath math = new OsuMath();

        private static List<Canvas> AliveCanvasObjects = new List<Canvas>();
        public static List<HitMarker> AliveHitMarkers = new List<HitMarker>();

        private static int HitObjectIndex = 0;
        private static Canvas HitObject = null!;
        private static HitObject HitObjectProperties = null!;

        private static int CursorPositionIndex = 0;

        private static int HitMarkerIndex = 0;
        private static ReplayFrame CurrentFrame = MainWindow.replay.FramesDict[0];
        private static HitMarker Marker = null;

        private static bool IsSliderEndHit = false;

        public static void UpdateHitMarkers()
        {
            if (HitMarkerIndex >= Analyser.Analyser.HitMarkers.Count)
            {
                return;
            }

            if (HitMarkerIndex < Analyser.Analyser.HitMarkers.Count && Marker != Analyser.Analyser.HitMarkers[HitMarkerIndex])
            {
                Marker = Analyser.Analyser.HitMarkers[HitMarkerIndex];
            }

            if (GamePlayClock.TimeElapsed >= Marker.SpawnTime && !AliveHitMarkers.Contains(Marker))
            {
                Window.playfieldCanva.Children.Add(Marker);
                AliveHitMarkers.Add(Marker);

                if (AliveCanvasObjects.Count > 0)
                {
                    Canvas objectToHit = AliveCanvasObjects.First();
                    HitObject prop = objectToHit.DataContext as HitObject;
                    for (int i = 0; i < AliveCanvasObjects.Count; i++)
                    {
                        var a = AliveCanvasObjects[i];
                        var b = a.DataContext as HitObject;

                        if (b.SpawnTime < prop.SpawnTime)
                        {
                            objectToHit = a;
                            prop = b;
                        }
                    }

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;
                    float X = (float)((prop.X * osuScale)- (((objectToHit.Width * 1.0092f) * osuScale) / 2));
                    float Y = (float)((prop.Y * osuScale) - (((objectToHit.Height * 1.0092f) * osuScale) / 2));

                    // this Ellipse is hitbox area of circle and is created here coz actual circle cant have this as children
                    // then it check if Point (current hit marker location) is inside this Ellipse with Ellipse.Visible(pt)
                    System.Drawing.Drawing2D.GraphicsPath ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                    ellipse.AddEllipse(X, Y, (float)(diameter * 1.0041f), (float)(diameter * 1.0041f));

                    System.Drawing.PointF pt = new System.Drawing.PointF(
                        (float)(Marker.Position.X * osuScale), (float)(Marker.Position.Y * osuScale));
                    if (ellipse.IsVisible(pt))
                    {
                        // sliders have set end time no matter what i think but circles dont so when circle is hit then delete it
                        if (prop is CircleData && (Marker.SpawnTime + 400 >= prop.SpawnTime && Marker.SpawnTime - 400 <= prop.SpawnTime))
                        {
                            if (objectToHit.Visibility != Visibility.Collapsed)
                            {
                                double judgementX = (prop.X * osuScale - (diameter / 2));
                                double judgementY = (prop.Y * osuScale - (diameter));
                                GetHitJudgment(prop, Marker, judgementX, judgementY, diameter);
                            }

                            Window.playfieldCanva.Children.Remove(objectToHit);
                            AliveCanvasObjects.Remove(objectToHit);
                            objectToHit.Visibility = Visibility.Collapsed;
                        }
                        else if (prop is SliderData && (Marker.SpawnTime + 400 >= prop.SpawnTime && Marker.SpawnTime - 400 <= prop.SpawnTime))
                        {
                            Canvas sliderHead = objectToHit.Children[1] as Canvas;

                            if (sliderHead.Visibility != Visibility.Collapsed)
                            {
                                double judgementX = (prop.X * osuScale - (diameter / 2));
                                double judgementY = (prop.Y * osuScale - (diameter));
                                GetHitJudgment(prop, Marker, judgementX, judgementY, diameter);

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

                                sliderHead.Visibility = Visibility.Collapsed;
                            }
                        }

                        prop.HitAt = Marker.SpawnTime;
                        prop.IsHit = true;
                    }
                }

                HitMarkerIndex++;
            }
        }

        public static void HandleAliveHitMarkers()
        {
            for (int i = 0; i < AliveHitMarkers.Count; i++)
            {
                HitMarker marker = AliveHitMarkers[i];
                if (GamePlayClock.TimeElapsed > marker.EndTime || GamePlayClock.TimeElapsed < marker.SpawnTime)
                {
                    AliveHitMarkers.Remove(marker);
                    Window.playfieldCanva.Children.Remove(marker);
                }
            }
        }

        // in tetoris slider only there are 2 fast short sliders and they gave 50 when seeking backwards
        // and culprit is this function since its the only thing that gives x50 judgement
        // investigate this and send the bug to jail... or something
        // ^ this was not the cause of the problem... anyway fixed lol
        private static void GetHitJudgment(HitObject prop, HitMarker marker, double X, double Y, double diameter)
        {
            double H300 = math.GetOverallDifficultyHitWindow300(MainWindow.map.Difficulty.OverallDifficulty);
            double H100 = math.GetOverallDifficultyHitWindow100(MainWindow.map.Difficulty.OverallDifficulty);
            double H50 = math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);

            Image img;
            if (marker.SpawnTime <= prop.SpawnTime + H300 && marker.SpawnTime >= prop.SpawnTime - H300)
            {
                img = HitJudgment.Image300();
                JudgementCounter.Increment300();
            }
            else if (marker.SpawnTime <= prop.SpawnTime + H100 && marker.SpawnTime >= prop.SpawnTime - H100)
            {
                img = HitJudgment.Image100();
                JudgementCounter.Increment100();
            }
            else if (marker.SpawnTime <= prop.SpawnTime + H50 && marker.SpawnTime >= prop.SpawnTime - H50)
            {
                img = HitJudgment.Image50();
                JudgementCounter.Increment50();
            }
            else
            {
                img = HitJudgment.ImageMiss();
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

        public static void UpdateHitMarkerIndexAfterSeek(ReplayFrame frame, double direction)
        {
            int i;
            bool found = false;
            for (i = 0; i < Analyser.Analyser.HitMarkers.Count; i++)
            {
                HitMarker hitMarker = Analyser.Analyser.HitMarkers[i];

                long time = direction > 0 ? hitMarker.SpawnTime : hitMarker.EndTime;
                if (time >= frame.Time || i == Analyser.Analyser.HitMarkers.Count - 1)
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

        public static void UpdateHitObjectIndexAfterSeek(long time, double direction = 0)
        {
            List<KeyValuePair<long, Canvas>> hitObjects = OsuBeatmap.HitObjectDictByTime.ToList();
            
            // from naming yes it was supposed to be calculation for AR time... but it didnt work...
            // this works tho so uhhh wont complain LOL
            double arTime = math.GetFadeInTiming(MainWindow.map.Difficulty.ApproachRate);
            
            int idx;
            if (direction > 0) //forward
            {   
                KeyValuePair<long, Canvas> item = hitObjects.FirstOrDefault(x => x.Key >= time + arTime
                                                                            , hitObjects.First());

                idx = hitObjects.IndexOf(item);
                if (idx > HitObjectIndex || idx == 0)
                {
                    HitObjectIndex = idx;
                }
            }
            else //back
            {
                HitObject itemem = MainWindow.map.HitObjects.FirstOrDefault(t => t.SpawnTime >= time
                                                                            , hitObjects.First().Value.DataContext as HitObject);

                KeyValuePair<long, Canvas> item;
                if (itemem is SliderData)
                {
                    item = hitObjects.FirstOrDefault(x => x.Key - arTime <= time
                                                     && GetEndTime(x.Value) > time, hitObjects.First());
                    HitObject dc = item.Value.DataContext as HitObject;

                    // reset slider head before slider was hit (if it was hit)
                    if (dc.IsHit == true && dc.HitAt > time)
                    {
                        Canvas sliderHead = item.Value.Children[1] as Canvas;
                        // hide only hit circle elements index 4 is reverse arrow
                        for (int i = 0; i <= 3; i++)
                        {
                            sliderHead.Children[i].Visibility = Visibility.Visible;
                        }
                        
                        // reverse arrow if exists will now be visible
                        if (sliderHead.Children.Count > 4)
                        {
                            sliderHead.Children[4].Visibility = Visibility.Collapsed;
                        }
                        
                        sliderHead.Visibility = Visibility.Visible;
                        dc.IsHit = false;
                    }
                }
                else
                {
                    item = hitObjects.FirstOrDefault(x => x.Key >= time
                                                     , hitObjects.First());
                }
                    
                idx = hitObjects.IndexOf(item);
                if (itemem.IsHit == true && itemem.HitAt <= time)
                {
                    idx++;
                }
                else
                {
                    itemem.IsHit = false;
                }

                HitObjectIndex = idx;   
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
                // need to check everything coz of sliders edge cases
                for (int i = 0; i < AliveCanvasObjects.Count; i++)
                {
                    Canvas toDelete = AliveCanvasObjects[i];
                    HitObject dc = toDelete.DataContext as HitObject;

                    double elapsedTime = GamePlayClock.TimeElapsed;
                    if (dc is CircleData && toDelete.Visibility == Visibility.Visible && elapsedTime >= GetEndTime(toDelete))
                    {
                        HitObjectDespawnMiss(dc, HitJudgment.ImageMiss(), MainWindow.OsuPlayfieldObjectDiameter);

                        Window.playfieldCanva.Children.Remove(toDelete);
                        toDelete.Visibility = Visibility.Collapsed;
                        AliveCanvasObjects.Remove(toDelete);
                    }
                    else if (dc is SliderData dcs && elapsedTime >= (dcs.IsHit == true ? dcs.EndTime : dcs.DespawnTime))
                    {
                        if (IsSliderEndHit == false)
                        {
                            HitObjectDespawnMiss(dc, HitJudgment.ImageSliderEndMiss(), MainWindow.OsuPlayfieldObjectDiameter * 0.2, true);
                        }

                        if (dcs.IsHit == false)
                        {
                            HitObjectDespawnMiss(dc, HitJudgment.ImageMiss(), MainWindow.OsuPlayfieldObjectDiameter);
                        }
                        
                        Window.playfieldCanva.Children.Remove(toDelete);
                        toDelete.Visibility = Visibility.Collapsed;
                        AliveCanvasObjects.Remove(toDelete);
                    }
                    else if (elapsedTime <= dc.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate))
                    {
                        // here is for backwards seeking so it doesnt show misses
                        Window.playfieldCanva.Children.Remove(toDelete);
                        toDelete.Visibility = Visibility.Collapsed;
                        AliveCanvasObjects.Remove(toDelete);

                        if (dc is SliderData)
                        {
                            Objects.Slider.ResetToDefault(toDelete);
                        }
                    }
                }
            }
        }

        private static void HitObjectDespawnMiss(HitObject dc, Image missImage, double diameter, bool sliderEndMiss = false)
        {
            MainWindow window = (MainWindow)Application.Current.MainWindow;

            Image miss = missImage;
            miss.Width = diameter;
            miss.Height = diameter;

            miss.Loaded += async delegate (object sender, RoutedEventArgs e)
            {
                await Task.Delay(800);
                window.playfieldCanva.Children.Remove(miss);
            };

            Vector2 missPosition;
            if (dc is CircleData)
            {
                missPosition = dc.SpawnPosition;
            }
            else
            {
                SliderData dcs = dc as SliderData;
                if (sliderEndMiss == true)
                {
                    missPosition = dcs.RepeatCount % 2 == 0 ? dcs.SpawnPosition : dcs.EndPosition;
                }
                else
                {
                    missPosition = dcs.SpawnPosition;
                }
            }

            double X = (missPosition.X * MainWindow.OsuPlayfieldObjectScale) - (miss.Width / 2);
            double Y = (missPosition.Y * MainWindow.OsuPlayfieldObjectScale) - miss.Height;

            Canvas.SetLeft(miss, X);
            Canvas.SetTop(miss, Y);

            JudgementCounter.IncrementMiss();
            window.playfieldCanva.Children.Add(miss);
        }

        private static bool SliderReversed = false;
        private static int TickIndex = 0;
        private static SliderData CurrentSlider = null;
        private static int ReverseArrowTailIndex = 1;
        private static int ReverseArrowHeadIndex = 1;
        public static void UpdateSliderTicks()
        {
            if (AliveCanvasObjects.Count > 0)
            {
                (Canvas slider, SliderData dc) = GetFirstSliderDataBySpawnTime();
                if (dc == null || dc.SliderTicks == null)
                {
                    return;
                }

                if (CurrentSlider != dc)
                {
                    CurrentSlider = dc;
                    TickIndex = 0;
                }

                double sliderPathDistance = (dc.EndTime - dc.SpawnTime) / dc.RepeatCount;

                bool reversed = false;
                if (Math.Floor(((GamePlayClock.TimeElapsed - dc.SpawnTime) / sliderPathDistance)) != 0)
                {
                    reversed = Math.Floor(((GamePlayClock.TimeElapsed - dc.SpawnTime) / sliderPathDistance)) % 2 == 1 ? true : false;
                }

                double sliderCurrentPositionAt;
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
                    sliderCurrentPositionAt = 1 - (sliderProgress);
                }
                else
                {
                    if (TickIndex == -1)
                    {
                        TickIndex = 0;
                    }

                    sliderCurrentPositionAt = (GamePlayClock.TimeElapsed - dc.SpawnTime) / sliderPathDistance;
                    while (sliderCurrentPositionAt > 1)
                    {
                        sliderCurrentPositionAt -= 1;
                    }
                }

                // make reverse arrows count as slider ticks later
                if ((reversed == false && TickIndex < dc.SliderTicks.Length && sliderCurrentPositionAt >= dc.SliderTicks[TickIndex].PositionAt)
                ||  (reversed == true && TickIndex >= 0 && sliderCurrentPositionAt <= dc.SliderTicks[TickIndex].PositionAt))
                {
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

                    double ballX = ballCentre.X;
                    double ballY = ballCentre.Y;

                    System.Drawing.Drawing2D.GraphicsPath ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                    ellipse.AddEllipse((float)(ballX - diameter / 2), (float)(ballY - diameter / 2), (float)diameter, (float)diameter);

                    // cursor pos index - 1 coz its always ahead by one from incrementing at the end of cursor update
                    float cursorX = (float)((MainWindow.replay.FramesDict[CursorPositionIndex - 1].X * osuScale) - (Window.playfieldCursor.Width / 2));
                    float cursorY = (float)((MainWindow.replay.FramesDict[CursorPositionIndex - 1].Y * osuScale) - (Window.playfieldCursor.Width / 2));
                    System.Drawing.PointF pt = new System.Drawing.PointF(cursorX, cursorY);

                    /* maybe will use this to make hit markers for slider ticks  
                    {
                        Rectangle middleHit = new Rectangle();
                        middleHit.Fill = Brushes.Cyan;
                        middleHit.Width = 10;
                        middleHit.Height = 10;

                        middleHit.Loaded += async delegate (object sender, RoutedEventArgs e)
                        {
                            await Task.Delay(1000);
                            Window.playfieldCanva.Children.Remove(middleHit);
                        };

                        Canvas.SetLeft(middleHit, (ballX - 5));
                        Canvas.SetTop(middleHit, (ballY - 5));

                        Canvas.SetZIndex(middleHit, 99999);

                        Window.playfieldCanva.Children.Add(middleHit);

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

                        Canvas.SetLeft(hitbox, ballX - (diameter / 2));
                        Canvas.SetTop(hitbox, ballY - (diameter / 2));

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

                    if (!ellipse.IsVisible(pt) || Marker.Click == 0 || Marker.Click == Clicks.Smoke)
                    {
                        Image miss = HitJudgment.ImageSliderTickMiss();
                        miss.Width = MainWindow.OsuPlayfieldObjectDiameter * 0.2;
                        miss.Height = MainWindow.OsuPlayfieldObjectDiameter * 0.2;
                    
                        miss.Loaded += async delegate (object sender, RoutedEventArgs e)
                        {
                            await Task.Delay(800);
                            Window.playfieldCanva.Children.Remove(miss);
                        };
                    
                        Canvas.SetLeft(miss, ballX - (miss.Width / 2));
                        Canvas.SetTop(miss, ballY - (miss.Width / 2));
                    
                        JudgementCounter.IncrementMiss();
                        Window.playfieldCanva.Children.Add(miss);
                    }
                    
                    if (reversed == false && TickIndex < dc.SliderTicks.Length)
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
        private static SliderData CurrentReverseSlider = null;
        public static void UpdateSliderRepeats()
        {
            if (AliveCanvasObjects.Count > 0)
            {
                (Canvas slider, SliderData dc) = GetFirstSliderDataBySpawnTime();
                if (dc == null)
                {
                    return;
                }

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

        /*  noted on how this works in case i will want to make it like its in osu stable (i wont)  
            in osu lazer - slider end is just like tick but doesnt kill combo completely and you lose more accuracy
            and there wont be any accuracy in this app
            is osu - when hitting head but no ticks and tail you get x50 at the end of slider
            depending on how many ticks you get AND dont hit tail you get x50 or x100
            if you hit all ticks AND miss tail you get x100
            if hit all ticks and tail you get x300
            so if what i tested in game is correct then example with 10 ticks:
            4/10 = x50, 5/10 = x50, 6/10 = x100, in short half + 1 = x100 and HEAD counts as tick while TAIL does not
            in short: <= half = x50, > half && < all = x100, all = x300
            for now this app mimics only osu lazer replays so will do that... also its way simpler lol
           
            info for me
            https://www.reddit.com/r/osugame/comments/9rki8o/how_are_slider_judgements_calculated/
            Slider end leniency is now more lenient
            On very fast sliders, you now only need to be tracking somewhere in the last 36 ms,
            rather than at the point 36 ms before the slider end
            (more details needed at https://www.youtube.com/watch?v=SlWKKA-ltZY)
            ok in lazer no matter what slider length slider have perfect acc when cursor is in slider ball 36ms before end
            just in case osu stable if slider is less than 72ms then divide by 2 is the window for perfect acc
        */

        // this works but osu lazer doesnt have it done perfectly too... on seeking by frame it shows
        // slider end missed but while playing normally it wont show it... and it changes acc/combo too... its weird
        private static SliderData CurrentSliderEndSlider = null;
        public static void HandleSliderEndJudgement()
        {
            if (AliveCanvasObjects.Count > 0)
            {
                (Canvas slider, SliderData dc) = GetFirstSliderDataBySpawnTime();
                if (dc == null)
                {
                    return;
                }

                if (dc != CurrentSliderEndSlider)
                {
                    CurrentSliderEndSlider = dc;
                    IsSliderEndHit = false;
                }

                if (dc.EndTime - dc.SpawnTime <= 36)
                {
                    IsSliderEndHit = true;
                    return;
                }
                else
                {
                    double minPosForMaxJudgement = 1 - ((36) / (dc.EndTime - dc.SpawnTime));
                    double currentSliderBallPosition = (GamePlayClock.TimeElapsed - dc.SpawnTime) / (dc.EndTime - dc.SpawnTime);

                    // if current position is lower than minimum position to get x300 on slider end then leave
                    // or if its already confirmed that slider end is hit also leave
                    if (currentSliderBallPosition < minPosForMaxJudgement || IsSliderEndHit == true)
                    {
                        return;
                    }

                    Canvas body = slider.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;
                    Image hitboxBall = ball.Children[1] as Image;

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    double hitboxBallWidth = hitboxBall.Width;
                    double hitboxBallHeight = hitboxBall.Height;

                    Point ballCentre = hitboxBall.TranslatePoint(new Point(hitboxBallWidth / 2, hitboxBallHeight / 2), Window.playfieldCanva);
                    double diameter = hitboxBallHeight * osuScale;

                    double ballX = ballCentre.X;
                    double ballY = ballCentre.Y;

                    System.Drawing.Drawing2D.GraphicsPath ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                    ellipse.AddEllipse((float)(ballX - diameter / 2), (float)(ballY - diameter / 2), (float)diameter, (float)diameter);

                    // cursor pos index - 1 coz its always ahead by one from incrementing at the end of cursor update
                    float cursorX = (float)((MainWindow.replay.FramesDict[CursorPositionIndex - 1].X * osuScale) - (Window.playfieldCursor.Width / 2));
                    float cursorY = (float)((MainWindow.replay.FramesDict[CursorPositionIndex - 1].Y * osuScale) - (Window.playfieldCursor.Width / 2));
                    System.Drawing.PointF pt = new System.Drawing.PointF(cursorX, cursorY);

                    if (ellipse.IsVisible(pt))
                    {
                        IsSliderEndHit = true;
                    }
                }
            }
        }

        private static (Canvas, SliderData) GetFirstSliderDataBySpawnTime()
        {
            Canvas slider = null;
            SliderData dc = null;

            foreach (var c in AliveCanvasObjects)
            {
                if (c.DataContext is not SliderData)
                {
                    continue;
                }

                if (dc == null)
                {
                    slider = c;
                    dc = c.DataContext as SliderData;
                }

                SliderData tempDc = c.DataContext as SliderData;
                if (dc.SpawnTime > tempDc.SpawnTime)
                {
                    slider = c;
                    dc = tempDc;
                }
            }

            return (slider, dc);
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
            if (o.DataContext is SliderData)
            {
                SliderData obj = o.DataContext as SliderData;
                return (int)obj.EndTime;
            }
            else if (o.DataContext is SpinnerData)
            {
                SpinnerData obj = o.DataContext as SpinnerData;
                return obj.EndTime;
            }
            else
            {
                CircleData obj = o.DataContext as CircleData;
                return obj.SpawnTime + math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);
            }
        }
    }
}
