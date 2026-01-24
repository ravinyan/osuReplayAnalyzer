using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Slider = ReplayAnalyzer.HitObjects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
{
    // uhh does it make sense? i feel like it does... but does it?
    public class HitDetection : HitMarkerManager
    {
        public static void CheckIfObjectWasHit()
        {
            GetCurrentHitMarker(ref CurrentHitMarker, CurrentHitMarkerIndex);
            if (CurrentHitMarker == null)
            {
                return;
            }

            if (GamePlayClock.TimeElapsed >= CurrentHitMarker.SpawnTime && !AliveHitMarkersData.Contains(CurrentHitMarker))
            {
                SpawnHitMarker(CurrentHitMarker, CurrentHitMarkerIndex);
  
                HitObjectManager.GetAliveHitObjects().Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));

                if (HitObjectManager.GetAliveHitObjects().Count > 0)
                {
                    double osuScale = MainWindow.OsuPlayfieldObjectScale;
                    double diameter = MainWindow.OsuPlayfieldObjectDiameter;

                    HitObject hitObject = FindCurrentlyHitObject(osuScale, diameter);
                    if (hitObject == null)
                    {
                        CurrentHitMarkerIndex++;
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

                        if (hitObject is HitCircle && CurrentHitMarker.SpawnTime + 400 >= hitObject.SpawnTime && CurrentHitMarker.SpawnTime - 400 <= hitObject.SpawnTime)
                        {
                            ApplyNoteLockIfPossible(ClassicMod.NotelockClientType, hitObject, out prevHitObjectExists);
                        
                            if (prevHitObjectExists == false)
                            {
                                if (hitObject.Visibility != Visibility.Collapsed)
                                {
                                    float judgementX = (float)(hitObject.X - diameter / 2);
                                    float judgementY = (float)(hitObject.Y - diameter);
                                    GetHitJudgment(hitObject, CurrentHitMarker.SpawnTime, judgementX, judgementY);
                                }
                        
                                HitObjectManager.AnnihilateHitObject(hitObject);
                            }
                        }
                        else if (hitObject is Slider && CurrentHitMarker.SpawnTime + 400 >= hitObject.SpawnTime && CurrentHitMarker.SpawnTime - 400 <= hitObject.SpawnTime)
                        {
                            ApplyNoteLockIfPossible(ClassicMod.NotelockClientType, hitObject, out prevHitObjectExists);
                        
                            Slider sHitObject = hitObject as Slider;
                            Canvas sliderHead = sHitObject.Children[1] as Canvas;
                        
                            if (prevHitObjectExists == false)
                            {
                                if (sliderHead.Children[0].Visibility != Visibility.Collapsed)
                                {
                                    float judgementX = (float)(sHitObject.X - diameter / 2);
                                    float judgementY = (float)(sHitObject.Y - diameter);

                                    if (ClassicMod.IsSliderHeadAccOn == false)
                                    {
                                        URBar.ShowHit(hitObject.SpawnTime - CurrentHitMarker.SpawnTime, new SolidColorBrush(Color.FromRgb(138, 216, 255)));
                                        HitJudgementManager.ApplyJudgement(hitObject, new Vector2(judgementX, judgementY), CurrentHitMarker.SpawnTime, 300);
                                    }
                                    else
                                    {
                                        GetHitJudgment(sHitObject, CurrentHitMarker.SpawnTime, judgementX, judgementY);
                                    }
                                        
                                    HitObjectManager.RemoveSliderHead(sliderHead);
                                }
                            }
                        }
                    }

                    CurrentHitMarkerIndex++;
                }
            }
        }

        private static void ApplyNoteLockIfPossible(string osuClient, HitObject hitObject, out bool prevHitObjectExists)
        {
            prevHitObjectExists = false;
            List<HitObject> aliveHitObjects = HitObjectManager.GetAliveHitObjects();

            switch (osuClient)
            {
                case "osu!lazer":
                    // if exists, miss all hitobjects before currently hit object for osu!lazer notelock behaviour
                    for (int i = 0; i < aliveHitObjects.Count; i++)
                    {
                        if (aliveHitObjects[i].SpawnTime >= hitObject.SpawnTime)
                        {
                            break;
                        }

                        if (aliveHitObjects[i] is Slider)
                        {
                            Slider s = aliveHitObjects[i] as Slider;
                            Canvas head = s.Children[1] as Canvas;

                            // if its slider and slider then just give miss like code below this if statement
                            // if its circle that was hit second and slider before is alive and was hit or missed (collapsed visibility)
                            // then continue coz slider without slider head should not give miss anymore
                            if (hitObject is HitCircle && head.Children[0].Visibility == Visibility.Collapsed
                            ||  s.Judgement != HitJudgementManager.HitObjectJudgement.None)
                            {
                                continue;
                            }

                            HitObjectManager.HitObjectDespawnMiss(aliveHitObjects[i], MainWindow.OsuPlayfieldObjectDiameter);
                            HitObjectManager.RemoveSliderHead(head); 
                        }
                        else if (aliveHitObjects[i] is HitCircle)
                        {
                            HitObjectManager.HitObjectDespawnMiss(aliveHitObjects[i], MainWindow.OsuPlayfieldObjectDiameter);
                            HitObjectManager.AnnihilateHitObject(aliveHitObjects[i]);
                        }
                    }
                    break;
                case "osu!":
                    // osu! notelock behaviour locks hit object from being hit
                    if (hitObject is Slider)
                    {
                        Slider sHitObject = hitObject as Slider;
                        for (int i = 0; i < aliveHitObjects.Count; i++)
                        {
                            Slider s = aliveHitObjects[i] as Slider;
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
                        for (int i = 0; i < aliveHitObjects.Count; i++)
                        {
                            if (aliveHitObjects[i].SpawnTime >= hitObject.SpawnTime)
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

            List<HitObject> hitObjects = HitObjectManager.GetAliveHitObjects();
            for (int j = 0; j < hitObjects.Count; j++)
            {
                hitObject = hitObjects[j];

                double cursorX = CurrentHitMarker.Position.X;
                double cursorY = CurrentHitMarker.Position.Y;

                double objectX = hitObject.X;
                double objectY = hitObject.Y;

                double hitPosition = Math.Pow(cursorX - objectX, 2) + Math.Pow(cursorY - objectY, 2);
                double circleRadius = 0;

                // i think only circles get higher circle radius and sliders dont... which is weird but ok i have no way of testing anyway
                // wait they also have changed hitbox... im too tired for this aaaaa
                if (hitObject is HitCircle)
                {
                    // i think coz of how my circle size is calculated the osu formula doesnt work... please help
                    //                                 1.00041f
                    circleRadius = Math.Pow(diameter * 1.00030f / 2, 2);
                }
                else if (hitObject is Slider)
                {
                    circleRadius = Math.Pow(diameter * 1.00030f / 2, 2);

                    // special case if 2 sliders are on top of each other and 1 was hit.
                    // now the already hit slider will be skipped and slider below will be hit, causing miss or judgement just like in osu
                    Slider s = hitObject as Slider;
                    Canvas head = s.Children[1] as Canvas;
                    if (head.Children[0].Visibility == Visibility.Collapsed)
                    {
                        continue;
                    }
                }

                //Ellipse frick = new Ellipse();
                //frick.Width = diameter * 1.00030f;
                //frick.Height = diameter * 1.00030f;
                //frick.Fill = Brushes.Cyan;
                //frick.Opacity = 0.5;
                //
                //frick.Loaded += async delegate (object sender, RoutedEventArgs e)
                //{
                //    await Task.Delay(1000);
                //    Window.playfieldCanva.Children.Remove(frick);
                //};
                //
                //Canvas.SetLeft(frick, hitObject.X - (diameter * 1.00030f / 2));
                //Canvas.SetTop(frick, hitObject.Y - (diameter * 1.00030f / 2));
                //
                //Window.playfieldCanva.Children.Add(frick);

                // if cursor position is lower number then its inside the circle...
                // dont understand why or how it works, but thats what people who know math say...
                if (hitPosition <= circleRadius)
                {
                    return hitObject;
                }        
            }

            return hitObject = null;
        }

        private static HitObject FindBlockingHitObject(int spawnTime)
        {
            HitObject blockingObject = null;

            List<HitObject> hitObjects = HitObjectManager.GetAliveHitObjects();
            for (int j = 0; j < hitObjects.Count; j++)
            {
                if (spawnTime <= hitObjects[j].SpawnTime)
                {
                    break;
                }

                blockingObject = hitObjects[j];
            }

            if (blockingObject != null && GamePlayClock.TimeElapsed <= blockingObject.SpawnTime)
            {
                return blockingObject;
            }

            return blockingObject = null;
        }

        private static void GetHitJudgment(HitObject hitObject, long hitTime, float X, float Y)
        {
            OsuMath math = new OsuMath();
            double H300 = math.GetOverallDifficultyHitWindow300();
            double H100 = math.GetOverallDifficultyHitWindow100();
            double H50 = math.GetOverallDifficultyHitWindow50();

            double diff = Math.Abs(hitObject.SpawnTime - hitTime);
            HitObjectData hitObjectData = HitObjectManager.TransformHitObjectToDataObject(hitObject);
            if (hitObjectData.Judgement == (int)HitJudgementManager.HitObjectJudgement.Max
            || (diff <= H300 && diff >= -H300))
            {
                if (MainWindow.IsReplayPreloading == false)
                {
                    URBar.ShowHit(hitObject.SpawnTime - hitTime, new SolidColorBrush(Color.FromRgb(138, 216, 255)));
                }
                
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, 300);
            }
            else if (hitObjectData.Judgement == (int)HitJudgementManager.HitObjectJudgement.Ok
            ||      (diff <= H100 && diff >= -H100))
            {
                if (MainWindow.IsReplayPreloading == false)
                {
                    URBar.ShowHit(hitObjectData.SpawnTime - hitTime, new SolidColorBrush(Color.FromRgb(176, 192, 25)));
                }

                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, 100);
            }
            else if (hitObjectData.Judgement == (int)HitJudgementManager.HitObjectJudgement.Meh
            ||      (diff <= H50 && diff >= -H50))
            {
                if (MainWindow.IsReplayPreloading == false)
                {
                    URBar.ShowHit(hitObject.SpawnTime - hitTime, new SolidColorBrush(Color.FromRgb(255, 217, 61)));
                }

                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, 50);
            }
            else
            {
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, 0);
            }
        }
    }
}
