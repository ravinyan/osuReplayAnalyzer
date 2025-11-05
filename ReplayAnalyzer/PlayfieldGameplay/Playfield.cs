using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Analyser.UIElements;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using ReplayAnalyzer.Skins;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.Objects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
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

        public static void ResetVariables()
        {
            AliveHitObjects.Clear();
            AliveHitMarkers.Clear();
            AliveHitJudgements.Clear();
            HitObjectIndex = 0;
            HitObject = null!;
            CursorPositionIndex = 0;
            HitMarkerIndex = 0;
            CurrentFrame = MainWindow.replay.FramesDict[0];
            Marker = null;
            IsSliderEndHit = false;
            SliderReversed = false;
            TickIndex = 0;
            CurrentSlider = null;
            ReverseArrowTailIndex = 1;
            ReverseArrowHeadIndex = 1;
            RepeatAt = 0;
            RepeatInterval = 0;
            CurrentReverseSlider = null;
            CurrentSliderEndSlider = null;
        }

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

                AliveHitObjects.Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));
                if (AliveHitObjects.Count > 0)
                {
                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;

                    HitObject hitObject = FindCurrentlyHitObject(osuScale, diameter);
                    if (hitObject == null)
                    {
                        HitMarkerIndex++;
                        return;
                    }

                    HitObject blockedHitObject = FindBlockingHitObject(hitObject.SpawnTime);
                    if (blockedHitObject != null)
                    {
                        // its for osu lazer notelock system shake effect below is for osu stable
                        // shake the HITOBJECT ? or just something else to make it simpler? dont know
                    }
                    else if (blockedHitObject == null)
                    {
                        bool prevHitObjectExists = false;
                        string osuClient = SettingsOptions.config.AppSettings.Settings["OsuClient"].Value;
                        
                        if (hitObject is HitCircle && Marker.SpawnTime + 400 >= hitObject.SpawnTime && Marker.SpawnTime - 400 <= hitObject.SpawnTime)
                        {
                            ApplyNoteLockIfPossible(osuClient, hitObject, out prevHitObjectExists);

                            if (prevHitObjectExists == false)
                            {
                                if (hitObject.Visibility != Visibility.Collapsed)
                                {
                                    double judgementX = hitObject.X * osuScale - diameter / 2;
                                    double judgementY = hitObject.Y * osuScale - diameter;
                                    GetHitJudgment(hitObject, Marker, judgementX, judgementY, diameter);
                                }

                                AnnihilateHitObject(hitObject);

                                hitObject.HitAt = Marker.SpawnTime;
                                hitObject.IsHit = true;
                            }
                        }
                        else if (hitObject is Slider && Marker.SpawnTime + 400 >= hitObject.SpawnTime && Marker.SpawnTime - 400 <= hitObject.SpawnTime)
                        {
                            ApplyNoteLockIfPossible(osuClient, hitObject, out prevHitObjectExists);

                            Slider sHitObject = hitObject as Slider;
                            Canvas sliderHead = sHitObject.Children[1] as Canvas;

                            if (prevHitObjectExists == false)
                            {
                                if (sliderHead.Children[0].Visibility != Visibility.Collapsed)
                                {
                                    double judgementX = sHitObject.X * osuScale - diameter / 2;
                                    double judgementY = sHitObject.Y * osuScale - diameter;
                                    GetHitJudgment(sHitObject, Marker, judgementX, judgementY, diameter);
                                    RemoveSliderHead(sliderHead);
                                }

                                sHitObject.HitAt = Marker.SpawnTime;
                                sHitObject.IsHit = true;
                            }
                        }
                    }
                }

                HitMarkerIndex++;
            }
        }

        private static void ApplyNoteLockIfPossible(string osuClient, HitObject hitObject, out bool prevHitObjectExists)
        {
            prevHitObjectExists = false;

            switch (osuClient)
            {
                case "osu!lazer":
                    // if exists, miss all hitobjects before currently hit object for osu!lazer notelock behaviour
                    for (int i = 0; i < AliveHitObjects.Count; i++)
                    {
                        if (AliveHitObjects[i].SpawnTime >= hitObject.SpawnTime)
                        {
                            break;
                        }

                        HitObjectDespawnMiss(AliveHitObjects[i], SkinElement.HitMiss(), MainWindow.OsuPlayfieldObjectDiameter);

                        if (AliveHitObjects[i] is Slider)
                        {
                            RemoveSliderHead(AliveHitObjects[i].Children[1] as Canvas);
                        }
                        else if (AliveHitObjects[i] is HitCircle)
                        {
                            AnnihilateHitObject(AliveHitObjects[i]);
                        } 
                    }
                    break;
                case "osu!":
                    // osu! notelock behaviour locks hit object from being hit
                    if (hitObject is Slider)
                    {
                        Slider sHitObject = hitObject as Slider;
                        for (int i = 0; i < AliveHitObjects.Count; i++)
                        {
                            Slider s = AliveHitObjects[i] as Slider;
                            if (s == null || s.EndTime >= sHitObject.EndTime)
                            {
                                // circle shake
                                break;
                            }

                            prevHitObjectExists = true;
                        }
                    }
                    else if (hitObject is HitCircle)
                    {
                        for (int i = 0; i < AliveHitObjects.Count; i++)
                        {
                            if (AliveHitObjects[i].SpawnTime >= hitObject.SpawnTime)
                            {
                                // circle shake
                                break;
                            }

                            prevHitObjectExists = true;
                        }
                    }
                    break;
            }
        }

        // IF THERE IS EVER A BUG WITH HITBOXES/HITS BEING WORNG (like missing when there shouldnt be miss or
        // hitting object when there should be miss) THEN THIS IS THE REASON WHY
        private static HitObject FindCurrentlyHitObject(double osuScale, double diameter)
        {
            HitObject hitObject = null;
            for (int j = 0; j < AliveHitObjects.Count; j++)
            {
                hitObject = AliveHitObjects[j];

                // not deleting commented stuff here just in case im stupid
                /* old hitbox detection   
                //float X = (float)((hitObject.X * osuScale) - (((hitObject.Width * 1.0092f) * osuScale) / 2));
                //float Y = (float)((hitObject.Y * osuScale) - (((hitObject.Height * 1.0092f) * osuScale) / 2));
                //
                //// this Ellipse is hitbox area of circle and is created here coz actual circle cant have this as children
                //// then it check if Point (current hit marker location) is inside this Ellipse with Ellipse.Visible(pt)
                //System.Drawing.Drawing2D.GraphicsPath ellipse = new System.Drawing.Drawing2D.GraphicsPath();
                //ellipse.AddEllipse(X, Y, (float)(diameter * 1.0041f), (float)(diameter * 1.0041f));
                //
                //System.Drawing.PointF pt = new System.Drawing.PointF(
                //    (float)(Marker.Position.X * osuScale), (float)(Marker.Position.Y * osuScale));
                */
                /* losing my mind...   
                // just in case https://stackoverflow.com/questions/481144/equation-for-testing-if-a-point-is-inside-a-circle
                // what i have works so i dont think i will change it... but better to know of alternative than not
                // tested this ^ and it doesnt really work... i may be stupid tho
                // i was stupud adding "* osuScale" to hitObject positions worked
                // wait after testing more it seems like it works too well... im so stupid
                // wait no it doesnt slider in tetoris is still not being hit
                // WAIT IT WASNT EVEN SUPPOSED TO BE HIT
                // tested highest and lowest resolutions only and it works perfectly for pixel perfect hits
                */

                double cursorX = Marker.Position.X * osuScale;
                double cursorY = Marker.Position.Y * osuScale;
                
                double objectX = hitObject.X * osuScale;
                double objectY = hitObject.Y * osuScale;

                double hitPosition = Math.Pow(cursorX - objectX, 2) + Math.Pow(cursorY - objectY, 2);
                double circleRadius = Math.Pow(diameter * 1.00041f / 2, 2);

                // if cursor position is lower number then its inside the circle...
                // dont understand why or how it works, but thats what people who know math say...
                if (hitPosition < circleRadius)
                {
                    return hitObject;
                }
            }

            return hitObject = null;
        }

        private static HitObject FindBlockingHitObject(int spawnTime)
        {
            HitObject blockingObject = null;
            for (int j = 0; j < AliveHitObjects.Count; j++)
            {
                if (spawnTime <= AliveHitObjects[j].SpawnTime)
                {
                    break;
                }

                blockingObject = AliveHitObjects[j];
            }

            if (blockingObject != null && blockingObject.HitAt == -1 && GamePlayClock.TimeElapsed <= blockingObject.SpawnTime)
            {
                return blockingObject;
            }

            return blockingObject = null;
        }

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
                if (hitMarker.SpawnTime >= GamePlayClock.TimeElapsed || i == Analyser.Analyser.HitMarkers.Count - 1)
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

            // ok before my brain stops working
            // when adding 1.5 speed to gameplay clock using modified 10.3ar this wont work but using ar 9 would give correct ar speed
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
                for (int i = 0; i < hitObjects.Count; i++)
                {
                    KeyValuePair<long, HitObject> v = hitObjects[i];

                    // check to ignore looping when time it too high or too low
                    //if (time > hitObjects[hitObjects.Count - 1].Value.SpawnTime + od50Window
                    //||  time < hitObjects[0].Value.SpawnTime - od50Window)
                    //{
                    //    break;
                    //}

                    if (v.Value is Slider && GetEndTime(v.Value) > time)
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

                Canvas.SetLeft(Window.playfieldCursor, CurrentFrame.X * osuScale - Window.playfieldCursor.Width / 2);
                Canvas.SetTop(Window.playfieldCursor, CurrentFrame.Y * osuScale - Window.playfieldCursor.Width / 2);

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
                    if (isSeeking == true && toDelete.IsHit == true || elapsedTime <= toDelete.SpawnTime - math.GetApproachRateTiming(MainWindow.map.Difficulty.ApproachRate))
                    {
                        // here is for backwards seeking so it doesnt show misses
                        // nvm right now this is for backwards AND forward seeking
                        AnnihilateHitObject(toDelete);

                        if (toDelete is Slider)
                        {
                            Slider.ResetToDefault(toDelete);
                        }
                    }
                    else if (toDelete is HitCircle && toDelete.Visibility == Visibility.Visible && elapsedTime >= GetEndTime(toDelete))
                    {
                        HitObjectDespawnMiss(toDelete, SkinElement.HitMiss(), MainWindow.OsuPlayfieldObjectDiameter);

                        AnnihilateHitObject(toDelete);
                    }
                    else if (toDelete is Slider)
                    {
                        Slider s = toDelete as Slider;
                        if (IsSliderEndHit == false && elapsedTime >= (s.IsHit == true ? s.EndTime : s.DespawnTime))
                        {
                            HitObjectDespawnMiss(toDelete, SkinElement.SliderEndMiss(), MainWindow.OsuPlayfieldObjectDiameter * 0.2, true);
                            AnnihilateHitObject(toDelete);
                        }
                        else if (IsSliderEndHit == true && elapsedTime >= (s.IsHit == true ? s.EndTime : s.DespawnTime))
                        {
                            AnnihilateHitObject(toDelete);
                        }

                        Canvas sliderHead = toDelete.Children[1] as Canvas;
                        if (sliderHead.Children[0].Visibility == Visibility.Visible && s.IsHit == false 
                        &&  elapsedTime >= s.SpawnTime + math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty))
                        {
                            HitObjectDespawnMiss(toDelete, SkinElement.HitMiss(), MainWindow.OsuPlayfieldObjectDiameter);
                            RemoveSliderHead(toDelete.Children[1] as Canvas);
                        }
                    }
                    else if (toDelete is Spinner && elapsedTime >= GetEndTime(toDelete))
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
                Slider slider = hitObject as Slider;
                if (sliderEndMiss == true)
                {
                    missPosition = slider.RepeatCount % 2 == 0 ? slider.SpawnPosition : slider.EndPosition;
                }
                else
                {
                    missPosition = slider.SpawnPosition;
                }
            }

            double X = missPosition.X * MainWindow.OsuPlayfieldObjectScale - hitJudgment.Width / 2;
            double Y = missPosition.Y * MainWindow.OsuPlayfieldObjectScale - hitJudgment.Height;

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

        private static void RemoveSliderHead(Canvas sliderHead)
        {
            // all slider head children that are hit circle
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

        private static bool SliderReversed = false;
        private static int TickIndex = 0;
        private static Slider CurrentSlider = null;
        private static int ReverseArrowTailIndex = 1;
        private static int ReverseArrowHeadIndex = 1;
        // improve this function and change how hitboxes work after seeking for reverse arrows done
        public static void UpdateSliderTicks()
        {
            if (AliveHitObjects.Count > 0)
            {
                Slider s = GetFirstSliderDataBySpawnTime();
                if (s == null || s.SliderTicks == null)
                {
                    return;
                }

                if (CurrentSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    CurrentSlider = s;
                    TickIndex = 0;
                }

                double sliderPathDistance = (s.EndTime - s.SpawnTime) / s.RepeatCount;

                bool reversed = false;
                if (Math.Floor((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance) != 0)
                {
                    reversed = Math.Floor((GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance) % 2 == 1 ? true : false;
                }

                double sliderCurrentPositionAt;
                if (reversed == true)
                {
                    double sliderProgress = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
                    while (sliderProgress > 1)
                    {
                        sliderProgress -= 1;
                    }
                    sliderCurrentPositionAt = 1 - sliderProgress;

                    if (TickIndex == s.SliderTicks.Length && sliderCurrentPositionAt <= s.SliderTicks[TickIndex - 1].PositionAt)
                    {
                        TickIndex--;
                    }
                }
                else
                {
                    sliderCurrentPositionAt = (GamePlayClock.TimeElapsed - s.SpawnTime) / sliderPathDistance;
                    while (sliderCurrentPositionAt > 1)
                    {
                        sliderCurrentPositionAt -= 1;
                    }

                    if (TickIndex == -1 && sliderCurrentPositionAt >= s.SliderTicks[TickIndex + 1].PositionAt)
                    {
                        TickIndex = 0;
                    }
                }

                // make reverse arrows count as slider ticks later < this is gonna be pain in the ass
                if (TickIndex >= 0 && TickIndex < s.SliderTicks.Length 
                &&  (reversed == false && sliderCurrentPositionAt >= s.SliderTicks[TickIndex].PositionAt
                ||  reversed == true && sliderCurrentPositionAt <= s.SliderTicks[TickIndex].PositionAt))
                {
                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;
                    
                    // ticks are starting at [3]
                    Image tick = body.Children[TickIndex + 3] as Image;
                    tick.Visibility = Visibility.Collapsed;

                    double osuScale = MainWindow.OsuPlayfieldObjectScale;

                    Image hitboxBall = ball.Children[1] as Image;
                    double diameter = hitboxBall.Height * osuScale;

                    Point tickCentre = tick.TranslatePoint(new Point(tick.Width / 2, tick.Height / 2), Window.playfieldCanva);

                    double cursorX = MainWindow.replay.FramesDict[CursorPositionIndex - 1].X * osuScale;
                    double cursorY = MainWindow.replay.FramesDict[CursorPositionIndex - 1].Y * osuScale;

                    double objectX = tickCentre.X - (tick.Width / 2);
                    double objectY = tickCentre.Y - (tick.Height / 2);

                    double hitPosition = Math.Pow(cursorX - objectX, 2) + Math.Pow(cursorY - objectY, 2);
                    double circleRadius = Math.Pow(diameter / 2, 2);

                    if (hitPosition > circleRadius)
                    {
                        double tickDiameter = MainWindow.OsuPlayfieldObjectDiameter * 0.2;
                        HitJudgment hitJudgment = new HitJudgment(SkinElement.SliderTickMiss(), tickDiameter, tickDiameter);
                        
                        hitJudgment.SpawnTime = (long)GamePlayClock.TimeElapsed;
                        hitJudgment.EndTime = hitJudgment.SpawnTime + 600;

                        Canvas.SetLeft(hitJudgment, objectX - hitJudgment.Width / 2);
                        Canvas.SetTop(hitJudgment, objectY - hitJudgment.Width / 2);

                        Window.playfieldCanva.Children.Add(hitJudgment);
                        AliveHitJudgements.Add(hitJudgment);
                    }
                    
                    if (reversed == false && TickIndex < s.SliderTicks.Length)
                    {
                        TickIndex++;
                    }
                    else if (reversed == true)
                    {
                        TickIndex--;
                    }
                }
                else if (reversed == false && TickIndex > 0 && sliderCurrentPositionAt <= s.SliderTicks[TickIndex - 1].PositionAt)
                {
                    TickIndex--;

                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;
                    
                    Image tick = body.Children[TickIndex + 3] as Image;
                    tick.Visibility = Visibility.Visible;

                    if (TickIndex == 0)
                    {
                        TickIndex = -1;
                    }
                }
                else if (reversed == true && TickIndex + 1 < s.SliderTicks.Length && sliderCurrentPositionAt >= s.SliderTicks[TickIndex + 1].PositionAt)
                {
                    TickIndex++;
                    
                    Canvas body = s.Children[0] as Canvas;
                    Canvas ball = body.Children[2] as Canvas;
                    
                    Image tick = body.Children[TickIndex + 3] as Image;
                    tick.Visibility = Visibility.Visible;

                    if (TickIndex == 2)
                    {
                        TickIndex = 3;
                    }
                }
            }
        }

        private static double RepeatAt = 0;
        private static double RepeatInterval = 0;
        private static Slider CurrentReverseSlider = null;
        public static void UpdateSliderRepeats()
        {
            if (AliveHitObjects.Count > 0)
            {
                Slider s = GetFirstSliderDataBySpawnTime();
                if (s == null)
                {
                    return;
                }

                if (CurrentReverseSlider != s || s.Visibility == Visibility.Collapsed)
                {
                    SliderReversed = false;
                    CurrentReverseSlider = s;
                    ReverseArrowTailIndex = 1;
                    ReverseArrowHeadIndex = 1;

                    RepeatInterval = 1 / (double)s.RepeatCount;
                    RepeatAt = RepeatInterval;
                }

                if (s.RepeatCount > 1)
                {
                    double progress = (GamePlayClock.TimeElapsed - s.SpawnTime) / (s.EndTime - s.SpawnTime);
                    if (progress > RepeatAt && RepeatAt >= 0 && progress >= 0)
                    {
                        if (RepeatAt == 0)
                        {
                            RepeatAt = RepeatInterval;
                            return;
                        }

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
                    else if (progress < RepeatAt - RepeatInterval && RepeatAt >= 0 && progress >= 0)
                    {
                        if (SliderReversed == true)
                        {
                            Canvas tail = s.Children[2] as Canvas;
                            tail.Children[tail.Children.Count - ReverseArrowHeadIndex].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            Canvas head = s.Children[1] as Canvas;
                            head.Children[head.Children.Count - ReverseArrowTailIndex + 1].Visibility = Visibility.Visible;
                        }
                        
                        RepeatAt -= RepeatInterval;
                        
                        if (SliderReversed == true && ReverseArrowTailIndex > 1)
                        {
                            SliderReversed = false;
                            ReverseArrowTailIndex -= 1;
                        }
                        else if (SliderReversed == false && ReverseArrowHeadIndex > 1)
                        {
                            SliderReversed = true;
                            ReverseArrowHeadIndex -= 1;
                        }
                        
                        Canvas body = s.Children[0] as Canvas;
                        for (int i = 3; i < body.Children.Count; i++)
                        {
                            body.Children[i].Visibility = Visibility.Collapsed;
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
        private static Slider CurrentSliderEndSlider = null;
        public static void HandleSliderEndJudgement()
        {
            if (AliveHitObjects.Count > 0)
            {   
                Slider s = GetFirstSliderDataBySpawnTime();
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
                    double minPosForMaxJudgement = 1 - 36 / (s.EndTime - s.SpawnTime);
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
                    float cursorX = (float)(MainWindow.replay.FramesDict[CursorPositionIndex - 1].X * osuScale - Window.playfieldCursor.Width / 2);
                    float cursorY = (float)(MainWindow.replay.FramesDict[CursorPositionIndex - 1].Y * osuScale - Window.playfieldCursor.Width / 2);
                    System.Drawing.PointF pt = new System.Drawing.PointF(cursorX, cursorY);

                    if (ellipse.IsVisible(pt))
                    {
                        IsSliderEndHit = true;
                    }
                }
            }
        }

        private static Slider GetFirstSliderDataBySpawnTime()
        {
            Slider slider = null;

            foreach (HitObject obj in AliveHitObjects)
            {
                if (obj is not Slider)
                {
                    continue;
                }

                Slider s = obj as Slider;
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
            if (o is Slider sl)
            {
                return sl.EndTime;
            }
            else if (o is Spinner sp)
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
