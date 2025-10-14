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
using WpfApp1.Skins;

#nullable disable

namespace WpfApp1.PlayfieldGameplay
{
    public static class Playfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static OsuMath math = new OsuMath();

        private static List<HitObject> AliveHitObjects = new List<HitObject>();
        private static List<HitMarker> AliveHitMarkers = new List<HitMarker>();
        private static List<HitJudgment> AliveHitJudgements = new List<HitJudgment>();

        private static int HitObjectIndex = 0;
        private static HitObject HitObject = null!;

        private static int CursorPositionIndex = 0;

        private static int HitMarkerIndex = 0;
        private static ReplayFrame CurrentFrame = MainWindow.replay.FramesDict[0];
        private static HitMarker Marker = null;

        private static bool IsSliderEndHit = false;

        public static void UpdateHitMarkers(bool isSeeking = false)
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

                if (AliveHitObjects.Count > 0)
                {
                    HitObject hitObject = AliveHitObjects.First();
                    for (int i = 0; i < AliveHitObjects.Count; i++)
                    {
                        HitObject temp = AliveHitObjects[i];
                        if (temp.SpawnTime < hitObject.SpawnTime)
                        {
                            hitObject = temp;
                        }
                    }

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;
                    float X = (float)((hitObject.X * osuScale)- (((hitObject.Width * 1.0092f) * osuScale) / 2));
                    float Y = (float)((hitObject.Y * osuScale) - (((hitObject.Height * 1.0092f) * osuScale) / 2));

                    // this Ellipse is hitbox area of circle and is created here coz actual circle cant have this as children
                    // then it check if Point (current hit marker location) is inside this Ellipse with Ellipse.Visible(pt)
                    System.Drawing.Drawing2D.GraphicsPath ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                    ellipse.AddEllipse(X, Y, (float)(diameter * 1.0041f), (float)(diameter * 1.0041f));

                    System.Drawing.PointF pt = new System.Drawing.PointF(
                        (float)(Marker.Position.X * osuScale), (float)(Marker.Position.Y * osuScale));
                    if (ellipse.IsVisible(pt))
                    {
                        /*
                        //if (isSeeking == true)
                        //{
                        //    Window.playfieldCanva.Children.Remove(hitObject);
                        //    AliveCanvasObjects.Remove(hitObject);
                        //    hitObject.Visibility = Visibility.Collapsed;
                        //
                        //    if (hitObject is Sliderr)
                        //    {
                        //        Canvas sliderHead = hitObject.Children[1] as Canvas;
                        //
                        //        if (sliderHead.Visibility != Visibility.Collapsed)
                        //        {
                        //            double judgementX = (hitObject.X * osuScale - (diameter / 2));
                        //            double judgementY = (hitObject.Y * osuScale - (diameter));
                        //            GetHitJudgment(hitObject, Marker, judgementX, judgementY, diameter);
                        //
                        //            // hide only hit circle elements index 4 is reverse arrow
                        //            for (int i = 0; i <= 3; i++)
                        //            {
                        //                sliderHead.Children[i].Visibility = Visibility.Collapsed;
                        //            }
                        //
                        //            // reverse arrow if exists will now be visible
                        //            if (sliderHead.Children.Count > 4)
                        //            {
                        //                sliderHead.Children[4].Visibility = Visibility.Visible;
                        //            }
                        //
                        //            sliderHead.Visibility = Visibility.Collapsed;
                        //        }
                        //    }
                        //
                        //    hitObject.HitAt = Marker.SpawnTime;
                        //    hitObject.IsHit = true;
                        //    return;
                        //}
                        */

                        // sliders have set end time no matter what i think but circles dont so when circle is hit then delete it
                        if (hitObject is HitCircle && (Marker.SpawnTime + 400 >= hitObject.SpawnTime && Marker.SpawnTime - 400 <= hitObject.SpawnTime))
                        {
                            if (hitObject.Visibility != Visibility.Collapsed)
                            {
                                double judgementX = (hitObject.X * osuScale - (diameter / 2));
                                double judgementY = (hitObject.Y * osuScale - (diameter));
                                GetHitJudgment(hitObject, Marker, judgementX, judgementY, diameter);
                            }

                            AnnihilateHitObject(hitObject);

                            hitObject.HitAt = Marker.SpawnTime;
                            hitObject.IsHit = true;
                        }
                        else if (hitObject is Sliderr && (Marker.SpawnTime + 400 >= hitObject.SpawnTime && Marker.SpawnTime - 400 <= hitObject.SpawnTime))
                        {
                            Canvas sliderHead = hitObject.Children[1] as Canvas;

                            if (sliderHead.Visibility != Visibility.Collapsed)
                            {
                                double judgementX = (hitObject.X * osuScale - (diameter / 2));
                                double judgementY = (hitObject.Y * osuScale - (diameter));
                                GetHitJudgment(hitObject, Marker, judgementX, judgementY, diameter);

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

                            hitObject.HitAt = Marker.SpawnTime;
                            hitObject.IsHit = true;
                        }
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
        private static void GetHitJudgment(HitObject hitObject, HitMarker marker, double X, double Y, double diameter)
        {
            double H300 = math.GetOverallDifficultyHitWindow300(MainWindow.map.Difficulty.OverallDifficulty);
            double H100 = math.GetOverallDifficultyHitWindow100(MainWindow.map.Difficulty.OverallDifficulty);
            double H50 = math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);

            HitJudgment hitJudgment;
            if (marker.SpawnTime <= hitObject.SpawnTime + H300 && marker.SpawnTime >= hitObject.SpawnTime - H300)
            {
                hitJudgment = new HitJudgment(SkinElement.Hit300(), diameter, diameter);
                JudgementCounter.Increment300();
            }
            else if (marker.SpawnTime <= hitObject.SpawnTime + H100 && marker.SpawnTime >= hitObject.SpawnTime - H100)
            {
                hitJudgment = new HitJudgment(SkinElement.Hit100(), diameter, diameter);
                JudgementCounter.Increment100();
            }
            else if (marker.SpawnTime <= hitObject.SpawnTime + H50 && marker.SpawnTime >= hitObject.SpawnTime - H50)
            {
                hitJudgment = new HitJudgment(SkinElement.Hit50(), diameter, diameter);
                JudgementCounter.Increment50();
            }
            else
            {
                hitJudgment = new HitJudgment(SkinElement.HitMiss(), diameter, diameter);
                JudgementCounter.IncrementMiss();
            }

            hitJudgment.SpawnTime = marker.SpawnTime;
            hitJudgment.EndTime = hitJudgment.SpawnTime + 600;

            AliveHitJudgements.Add(hitJudgment);

            Window.playfieldCanva.Children.Add(hitJudgment);
            
            Canvas.SetLeft(hitJudgment, X);
            Canvas.SetTop(hitJudgment, Y);
        }

        public static void HandleAliveHitJudgements()
        {
            for (int i = 0; i < AliveHitJudgements.Count; i++)
            {
                HitJudgment hitJudgment = AliveHitJudgements[i];
                if (GamePlayClock.TimeElapsed > hitJudgment.EndTime || GamePlayClock.TimeElapsed < hitJudgment.SpawnTime)
                {
                    AliveHitJudgements.Remove(hitJudgment);
                    Window.playfieldCanva.Children.Remove(hitJudgment);
                }
            }
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

        public static void UpdateHitObjects(bool isSeeking = false)
        {
            if (HitObjectIndex >= OsuBeatmap.HitObjectDictByIndex.Count)
            {
                return;
            }

            if (HitObject != OsuBeatmap.HitObjectDictByIndex[HitObjectIndex])
            {
                HitObject = OsuBeatmap.HitObjectDictByIndex[HitObjectIndex];
            }

            if (GamePlayClock.TimeElapsed > HitObject.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate)
            && !AliveHitObjects.Contains(HitObject))
            {
                AliveHitObjects.Add(HitObject);
                Window.playfieldCanva.Children.Add(OsuBeatmap.HitObjectDictByIndex[HitObjectIndex]);
                HitObject.Visibility = Visibility.Visible;

                HitObjectAnimations.Start(HitObject);

                if (GamePlayClock.IsPaused() || isSeeking == true)
                {
                    HitObjectAnimations.Seek(AliveHitObjects);
                }
                
                HitObjectIndex++;
            }
        }

        // fix seeking backwards for objects i hate this
        public static void UpdateHitObjectIndexAfterSeek(long time, double direction = 0)
        {
            List<KeyValuePair<long, HitObject>> hitObjects = OsuBeatmap.HitObjectDictByTime.ToList();
            
            // from naming yes it was supposed to be calculation for AR time... but it didnt work...
            // this works tho so uhhh wont complain LOL
            double arTime = math.GetFadeInTiming(MainWindow.map.Difficulty.ApproachRate);
            
            int idx;
            if (direction > 0) //forward
            {   
                KeyValuePair<long, HitObject> item = hitObjects.FirstOrDefault(
                    x => x.Key >= time + arTime, hitObjects.Last());

                idx = hitObjects.IndexOf(item);
                if (idx > HitObjectIndex || idx == 0)
                {
                    HitObjectIndex = idx;
                }
            }
            else //back
            {
                double od50Window = math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate);

                KeyValuePair<long, HitObject> curr = new KeyValuePair<long, HitObject>();
                for (int i = 0; i < hitObjects.Count - 1; i++)
                {
                    KeyValuePair<long, HitObject> v = hitObjects[i + 1];

                    // check to ignore looping when time it too high or too low
                    //if (time > hitObjects[hitObjects.Count - 1].Value.SpawnTime + od50Window
                    //||  time < hitObjects[0].Value.SpawnTime - od50Window)
                    //{
                    //    break;
                    //}

                    if (v.Value is Sliderr && GetEndTime(v.Value) > time)
                    {
                        if (v.Value.Visibility == Visibility.Collapsed)
                        {
                            curr = v;
                            break;
                        }

                        if (v.Value.IsHit == true && v.Value.HitAt > time)
                        {
                            curr = v;

                            Canvas sliderHead = curr.Value.Children[1] as Canvas;
                            for (int j = 0; j <= 3; j++)
                            {
                                sliderHead.Children[j].Visibility = Visibility.Visible;
                            }

                            if (sliderHead.Children.Count > 4)
                            {
                                sliderHead.Children[4].Visibility = Visibility.Collapsed;
                            }
                            sliderHead.Visibility = Visibility.Visible;
                        }

                        break;
                    }

                    if (v.Value.IsHit == false && GetEndTime(v.Value) > time)
                    {
                        curr = v;
                        break;
                    }

                    if (v.Value.IsHit == true && v.Value.HitAt > time)
                    {
                        curr = v;
                        break;
                    }
                }

                idx = hitObjects.IndexOf(curr);
                if (idx != -1)
                {
                    HitObjectIndex = idx;
                }
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

        public static void HandleVisibleHitObjects(bool isSeeking = false)
        {
            if (AliveHitObjects.Count > 0)
            {
                // need to check everything coz of sliders edge cases
                for (int i = 0; i < AliveHitObjects.Count; i++)
                {
                    HitObject toDelete = AliveHitObjects[i];

                    double elapsedTime = GamePlayClock.TimeElapsed;
                    if ((isSeeking == true && toDelete.IsHit == true) || elapsedTime <= toDelete.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate))
                    {
                        // here is for backwards seeking so it doesnt show misses
                        // nvm right now this is for backwards AND forward seeking
                        AnnihilateHitObject(toDelete);

                        if (toDelete is Sliderr)
                        {
                            Objects.Slider.ResetToDefault(toDelete);
                        }
                    }
                    else if (toDelete is HitCircle && toDelete.Visibility == Visibility.Visible && elapsedTime >= GetEndTime(toDelete))
                    {
                        HitObjectDespawnMiss(toDelete, SkinElement.HitMiss(), MainWindow.OsuPlayfieldObjectDiameter);

                        AnnihilateHitObject(toDelete);
                    }
                    else if (toDelete is Sliderr s && elapsedTime >= (s.IsHit == true ? s.EndTime : s.DespawnTime))
                    {
                        if (IsSliderEndHit == false)
                        {
                            HitObjectDespawnMiss(toDelete, SkinElement.SliderEndMiss(), MainWindow.OsuPlayfieldObjectDiameter * 0.2, true);
                        }

                        if (s.IsHit == false)
                        {
                            HitObjectDespawnMiss(toDelete, SkinElement.HitMiss(), MainWindow.OsuPlayfieldObjectDiameter);
                        }

                        AnnihilateHitObject(toDelete);
                    }
                    else if (toDelete is Spinnerr && elapsedTime >= GetEndTime(toDelete))
                    {
                        AnnihilateHitObject(toDelete);
                    }
                }
            }
        }

        private static void HitObjectDespawnMiss(HitObject hitObject, string missImageUri, double diameter, bool sliderEndMiss = false)
        {
            HitJudgment hitJudgment = new HitJudgment(missImageUri, diameter, diameter);
            hitJudgment.SpawnTime = (long)GamePlayClock.TimeElapsed;
            hitJudgment.EndTime = hitJudgment.SpawnTime + 600;

            Vector2 missPosition;
            if (hitObject is HitCircle)
            {
                missPosition = hitObject.SpawnPosition;
            }
            else
            {
                Sliderr slider = hitObject as Sliderr;
                if (sliderEndMiss == true)
                {
                    missPosition = slider.RepeatCount % 2 == 0 ? slider.SpawnPosition : slider.EndPosition;
                }
                else
                {
                    missPosition = slider.SpawnPosition;
                }
            }

            double X = (missPosition.X * MainWindow.OsuPlayfieldObjectScale) - (hitJudgment.Width / 2);
            double Y = (missPosition.Y * MainWindow.OsuPlayfieldObjectScale) - hitJudgment.Height;

            Canvas.SetLeft(hitJudgment, X);
            Canvas.SetTop(hitJudgment, Y);

            if (sliderEndMiss == false)
            {
                JudgementCounter.IncrementMiss();
            }

            Window.playfieldCanva.Children.Add(hitJudgment);
            AliveHitJudgements.Add(hitJudgment);
        }

        private static void AnnihilateHitObject(HitObject toDelete)
        {
            Window.playfieldCanva.Children.Remove(toDelete);
            toDelete.Visibility = Visibility.Collapsed;
            AliveHitObjects.Remove(toDelete);
            HitObjectAnimations.Remove(toDelete);
        }

        private static bool SliderReversed = false;
        private static int TickIndex = 0;
        private static Sliderr CurrentSlider = null;
        private static int ReverseArrowTailIndex = 1;
        private static int ReverseArrowHeadIndex = 1;
        public static void UpdateSliderTicks()
        {
            if (AliveHitObjects.Count > 0)
            {
                Sliderr s = GetFirstSliderDataBySpawnTime();
                if (s == null || s.SliderTicks == null)
                {
                    return;
                }

                if (CurrentSlider != s)
                {
                    CurrentSlider = s;
                    TickIndex = 0;
                }

                double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;

                bool reversed = false;
                if (Math.Floor(((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance)) != 0)
                {
                    reversed = Math.Floor(((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance)) % 2 == 1 ? true : false;
                }

                double sliderCurrentPositionAt;
                if (reversed == true)
                {
                    if (TickIndex == s.SliderTicks.Length)
                    {
                        TickIndex--;
                    }

                    double sliderProgress = ((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance);
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

                    sliderCurrentPositionAt = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
                    while (sliderCurrentPositionAt > 1)
                    {
                        sliderCurrentPositionAt -= 1;
                    }
                }

                // make reverse arrows count as slider ticks later
                if ((reversed == false && TickIndex < s.SliderTicks.Length && sliderCurrentPositionAt >= s.SliderTicks[TickIndex].PositionAt)
                ||  (reversed == true && TickIndex >= 0 && sliderCurrentPositionAt <= s.SliderTicks[TickIndex].PositionAt))
                {
                    Canvas body = s.Children[0] as Canvas;
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

                    if (!ellipse.IsVisible(pt) || Marker.Click == 0 || Marker.Click == Clicks.Smoke)
                    {
                        HitJudgment hitJudgment = new HitJudgment(SkinElement.SliderTickMiss(),
                            MainWindow.OsuPlayfieldObjectDiameter * 0.2, MainWindow.OsuPlayfieldObjectDiameter * 0.2);
                        hitJudgment.SpawnTime = (long)GamePlayClock.TimeElapsed;
                        hitJudgment.EndTime = hitJudgment.SpawnTime + 600;

                        Canvas.SetLeft(hitJudgment, ballX - (hitJudgment.Width / 2));
                        Canvas.SetTop(hitJudgment, ballY - (hitJudgment.Width / 2));

                        Window.playfieldCanva.Children.Add(hitJudgment);
                        AliveHitJudgements.Add(hitJudgment);
                    }
                    
                    if (reversed == false && TickIndex < s.SliderTicks.Length)
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
        private static Sliderr CurrentReverseSlider = null;
        public static void UpdateSliderRepeats()
        {
            if (AliveHitObjects.Count > 0)
            {
                Sliderr s = GetFirstSliderDataBySpawnTime();
                if (s == null)
                {
                    return;
                }

                if (CurrentReverseSlider != s)
                {
                    SliderReversed = false;
                    CurrentReverseSlider = s;
                    ReverseArrowTailIndex = 1;
                    ReverseArrowHeadIndex = 1;

                    RepeatInterval = ((double)1 / (double)s.RepeatCount);
                    RepeatAt = RepeatInterval;
                }

                if (s.RepeatCount > 1)
                {
                    double progress = (GamePlayClock.TimeElapsed - s.SpawnTime) / (s.EndTime - s.SpawnTime);
                    if (progress > RepeatAt && RepeatAt != 1)
                    {
                        if (SliderReversed == false)
                        {
                            Canvas tail = s.Children[2] as Canvas;
                            tail.Children[tail.Children.Count - ReverseArrowTailIndex].Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            Canvas head = s.Children[1] as Canvas;
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
                        Canvas body = s.Children[0] as Canvas;
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
        private static Sliderr CurrentSliderEndSlider = null;
        public static void HandleSliderEndJudgement()
        {
            if (AliveHitObjects.Count > 0)
            {
                Sliderr s = GetFirstSliderDataBySpawnTime();
                if (s == null)
                {
                    return;
                }

                if (s != CurrentSliderEndSlider)
                {
                    CurrentSliderEndSlider = s;
                    IsSliderEndHit = false;
                }

                if (s.EndTime - s.SpawnTime <= 36)
                {
                    IsSliderEndHit = true;
                    return;
                }
                else
                {
                    double minPosForMaxJudgement = 1 - ((36) / (s.EndTime - s.SpawnTime));
                    double currentSliderBallPosition = (GamePlayClock.TimeElapsed - s.SpawnTime) / (s.EndTime - s.SpawnTime);

                    // if current position is lower than minimum position to get x300 on slider end then leave
                    // or if its already confirmed that slider end is hit also leave
                    if (currentSliderBallPosition < minPosForMaxJudgement || IsSliderEndHit == true)
                    {
                        return;
                    }

                    Canvas body = s.Children[0] as Canvas;
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

        private static Sliderr GetFirstSliderDataBySpawnTime()
        {
            Sliderr slider = null;

            foreach (HitObject obj in AliveHitObjects)
            {
                if (obj is not Sliderr)
                {
                    continue;
                }

                Sliderr s = obj as Sliderr;
                if (slider == null || slider.SpawnTime > s.SpawnTime)
                {
                    slider = s;
                }
            }

            return slider;
        }

        public static int AliveHitObjectCount()
        {
            return AliveHitObjects.Count;
        }

        public static List<HitObject> GetAliveHitObjects()
        {
            return AliveHitObjects;
        }

        private static double GetEndTime(HitObject o)
        {
            if (o is Sliderr sl)
            {
                return sl.EndTime;
            }
            else if (o is Spinnerr sp)
            {
                return sp.EndTime;
            }
            else
            {
                return o.SpawnTime + math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);
            }
        }
    }
}

/* this could be used to show hitboxes for slider ball/tick/circles/sliderheads/hitmarker hits and other things i guess   
 * wont delete in case i might want to use that for testing or whatever
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
