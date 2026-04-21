using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Numerics;
using System.Windows;
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

                if (HitObjectManager.GetAliveHitObjects().Count == 0)
                {
                    return;
                }
                HitObjectManager.GetAliveHitObjects().Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));

                double osuScale = MainWindow.OsuPlayfieldObjectScale;
                double diameter = MainWindow.OsuPlayfieldObjectDiameter;

                HitObject clickedHitObject = FindCurrentlyHitObject(osuScale, diameter);
                if (clickedHitObject == null)
                {
                    CurrentHitMarkerIndex++;
                    return;
                }

                HitObject blockedHitObject = FindBlockingHitObject(clickedHitObject.SpawnTime);
                if (blockedHitObject != null)
                {
                    ApplyNotelockEffect(clickedHitObject);
                }
                else if (blockedHitObject == null)
                {
                    bool prevHitObjectExists = false;

                    if (clickedHitObject is HitCircle && CurrentHitMarker.SpawnTime + 400 >= clickedHitObject.SpawnTime 
                    &&  CurrentHitMarker.SpawnTime - 400 <= clickedHitObject.SpawnTime)
                    {
                        ApplyNoteLockIfPossible(ClassicMod.NotelockClientType, clickedHitObject, out prevHitObjectExists);
                    
                        if (prevHitObjectExists == false)
                        {
                            if (clickedHitObject.Visibility != Visibility.Collapsed)
                            {
                                float judgementX = (float)(clickedHitObject.X - diameter / 2);
                                float judgementY = (float)(clickedHitObject.Y - diameter);
                                GetHitJudgment(clickedHitObject, CurrentHitMarker.SpawnTime, judgementX, judgementY);
                            }
                    
                            HitObjectManager.AnnihilateHitObject(clickedHitObject);
                        }
                    }
                    else if (clickedHitObject is Slider && CurrentHitMarker.SpawnTime + 400 >= clickedHitObject.SpawnTime 
                    &&       CurrentHitMarker.SpawnTime - 400 <= clickedHitObject.SpawnTime)
                    {
                        ApplyNoteLockIfPossible(ClassicMod.NotelockClientType, clickedHitObject, out prevHitObjectExists);
                     
                        if (prevHitObjectExists == false)
                        {
                            Slider slider = clickedHitObject as Slider;
                            if (Slider.HeadHitCircle(slider).Visibility != Visibility.Collapsed)
                            {
                                float judgementX = (float)(slider.X - diameter / 2);
                                float judgementY = (float)(slider.Y - diameter);

                                if (ClassicMod.IsSliderHeadAccOn == false)
                                {// force x300 judgement here
                                    URBar.ShowHit(HitObjectJudgement.Max, clickedHitObject.SpawnTime - CurrentHitMarker.SpawnTime);
                                    HitJudgementManager.ApplyJudgement(clickedHitObject, new Vector2(judgementX, judgementY)
                                                                      ,CurrentHitMarker.SpawnTime, HitObjectJudgement.Max);
                                }
                                else
                                {
                                    GetHitJudgment(slider, CurrentHitMarker.SpawnTime, judgementX, judgementY);
                                }
                                    
                                Slider.RemoveSliderHead(slider);
                            }
                        }
                    }
                }

                CurrentHitMarkerIndex++;
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

                        if (aliveHitObjects[i] is Slider slider)
                        {
                            // if its slider and slider then just give miss like code below this if statement
                            // if its circle that was hit second and slider before is alive and was hit or missed (collapsed visibility)
                            // then continue coz slider without slider head should not give miss anymore
                            if (hitObject is HitCircle && Slider.HeadHitCircle(slider).Visibility == Visibility.Collapsed
                            ||  slider.Judgement.Judgement != HitObjectJudgement.None)
                            {
                                continue;
                            }

                            HitObjectManager.HitObjectDespawnMiss(aliveHitObjects[i], MainWindow.OsuPlayfieldObjectDiameter);
                            Slider.RemoveSliderHead(slider); 
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
                            if (aliveHitObjects[i] is Spinner)
                            {
                                continue;
                            }

                            if (prevHitObjectExists == false && aliveHitObjects[i].SpawnTime >= hitObject.SpawnTime)
                            {
                                break;
                            }

                            Slider s = aliveHitObjects[i] as Slider;
                            if (s == null || s.EndTime >= sHitObject.EndTime)
                            {
                                ApplyNotelockEffect(s);
                                break;
                            }

                            prevHitObjectExists = true;
                        }
                    }
                    else if (hitObject is HitCircle)
                    {
                        for (int i = 0; i < aliveHitObjects.Count; i++)
                        {
                            if (aliveHitObjects[i] is Spinner)
                            {
                                continue;
                            }

                            if (prevHitObjectExists == false && aliveHitObjects[i].SpawnTime >= hitObject.SpawnTime)
                            {
                                break;
                            }

                            if (prevHitObjectExists == true && aliveHitObjects[i].SpawnTime >= hitObject.SpawnTime)
                            {
                                ApplyNotelockEffect(hitObject);
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
                    if (Slider.HeadHitCircle(s).Visibility == Visibility.Collapsed)
                    {
                        continue;
                    }
                }

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
            double H300 = math.GetJudgement300HitWindow();
            double H100 = math.GetJudgement100HitWindow();
            double H50 = math.GetJudgement50HitWindow();

            double diff = Math.Abs(hitObject.SpawnTime - hitTime);
            HitObjectData hitObjectData = HitObjectManager.TransformHitObjectToDataObject(hitObject);
            if (hitObjectData.Judgement.Judgement == (int)HitObjectJudgement.Max || (diff <= H300 && diff >= -H300))
            {
                URBar.ShowHit(HitObjectJudgement.Max, hitObject.SpawnTime - hitTime);
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Max);
            }
            else if (hitObjectData.Judgement.Judgement == (int)HitObjectJudgement.Ok || (diff <= H100 && diff >= -H100))
            {
                URBar.ShowHit(HitObjectJudgement.Ok, hitObjectData.SpawnTime - hitTime);
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Ok);
            }
            else if (hitObjectData.Judgement.Judgement == (int)HitObjectJudgement.Meh || (diff <= H50 && diff >= -H50))
            {
                URBar.ShowHit(HitObjectJudgement.Meh, hitObject.SpawnTime - hitTime);
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Meh);
            }
            else
            {
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Miss);
            }
        }

        private static void ApplyNotelockEffect(HitObject hitObject)
        {
            if (MainWindow.IsReplayPreloading == true)
            {
                return;
            }

            if (hitObject is Slider)
            {
                int index = SkinIniProperties.GetComboColours().Count;
                HitObject.SetColour(Slider.HeadHitCircle(hitObject as Slider), index);
            }
            else if (hitObject is HitCircle)
            {
                int index = SkinIniProperties.GetComboColours().Count;
                HitObject.SetColour(HitCircle.Circle(hitObject as HitCircle), index);
            }
        }
    }
}
