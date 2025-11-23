using ReplayAnalyzer.AnalyzerTools;
using ReplayAnalyzer.AnalyzerTools.UIElements;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.SettingsMenu;
using ReplayAnalyzer.Skins;
using System.Windows;
using System.Windows.Controls;
using Slider = ReplayAnalyzer.Objects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
{
    // uhh does it make sense? i feel like it does... but does it?
    public class HitDetection : HitMarkerManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static OsuMath Math = new OsuMath();

        public static void CheckIfObjectWasHit(long t = 0)
        {
            GetCurrentHitMarker(ref CurrentHitMarker, CurrentHitMarkerIndex);
            // test
            long imLosingMySanity = 0;
            var a = CurrentHitMarker;
            if (t != 0)
            {
                //for (int i = 0; i < Analyzer.HitMarkers.Count; i++)
                //{
                //    HitMarker m = Analyzer.HitMarkers[i];
                //
                //    if (m.SpawnTime == t)
                //    {
                //        var p = "";
                //    }
                //}

                // its basically frame index coz its based on frames (ok this doesnt work i guess lol
                //t = CursorManager.CursorPositionIndex;

                HitMarker pain = Analyzer.HitMarkers.FirstOrDefault(
                    hm => hm.Value.SpawnTime == t).Value ?? null;

                

                if (pain != null)
                {
                    imLosingMySanity = t;
                    CurrentHitMarker = pain;
                }
                

                if (a != CurrentHitMarker)
                {
                    var s = "help me";
                }
            }

            //HitMarker painnn = Analyzer.HitMarkers.FirstOrDefault(
            //        hm => hm.Value.SpawnTime == MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex].Time).Value ?? null;
            ////GamePlayClock.TimeElapsed >= CurrentHitMarker.SpawnTime
            //
            //if (painnn == null)
            //{
            //    return;
            //}
            //CurrentHitMarker = painnn;

            if (imLosingMySanity == 0)
            {
                imLosingMySanity = (long)GamePlayClock.TimeElapsed;
            }

            if (imLosingMySanity >= CurrentHitMarker.SpawnTime && !AliveHitMarkers.Contains(CurrentHitMarker))
            {
                SpawnHitMarker(CurrentHitMarker);

                HitObjectManager.GetAliveHitObjects().Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));
                
                if (HitObjectManager.GetAliveHitObjects().Count == 0)
                {

                }

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
                        string osuClient = SettingsOptions.config.AppSettings.Settings["OsuClient"].Value;

                        if (hitObject is HitCircle && CurrentHitMarker.SpawnTime + 400 >= hitObject.SpawnTime && CurrentHitMarker.SpawnTime - 400 <= hitObject.SpawnTime)
                        {
                            ApplyNoteLockIfPossible(osuClient, hitObject, out prevHitObjectExists);

                            if (prevHitObjectExists == false)
                            {
                                if (hitObject.Visibility != Visibility.Collapsed)
                                {
                                    //HitMarkerManager.UpdateHitMarkerAfterSeek(1);
                                    double judgementX = hitObject.X * osuScale - diameter / 2;
                                    double judgementY = hitObject.Y * osuScale - diameter;
                                    GetHitJudgment(hitObject, CurrentHitMarker, judgementX, judgementY, diameter);
                                }
                                else
                                {

                                }

                                    HitObjectManager.AnnihilateHitObject(hitObject);

                                //if (isPreloading == true)
                                //{
                                    hitObject.HitAt = CurrentHitMarker.SpawnTime;
                                    hitObject.IsHit = true;
                                //}
                                
                            }
                            else
                            {

                            }
                        }
                        else if (hitObject is Slider && CurrentHitMarker.SpawnTime + 400 >= hitObject.SpawnTime && CurrentHitMarker.SpawnTime - 400 <= hitObject.SpawnTime)
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
                                    GetHitJudgment(sHitObject, CurrentHitMarker, judgementX, judgementY, diameter);
                                    HitObjectManager.RemoveSliderHead(sliderHead);
                                }

                                //if (isPreloading == true)
                                //{
                                    sHitObject.HitAt = CurrentHitMarker.SpawnTime;
                                    sHitObject.IsHit = true;
                                //}
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
                        
                        // sometimes this is true and it shouldnt and this is easy fix
                        // ok might not be needed but condition still stands if this happens SOMEHOW then continue
                        if (GamePlayClock.TimeElapsed > aliveHitObjects[i].SpawnTime)
                        {
                            continue;
                        }

                        HitObjectManager.HitObjectDespawnMiss(aliveHitObjects[i], SkinElement.HitMiss(), MainWindow.OsuPlayfieldObjectDiameter);

                        if (aliveHitObjects[i] is Slider)
                        {
                            HitObjectManager.RemoveSliderHead(aliveHitObjects[i].Children[1] as Canvas);
                        }
                        else if (aliveHitObjects[i] is HitCircle)
                        {
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

                double cursorX = CurrentHitMarker.Position.X * osuScale;
                double cursorY = CurrentHitMarker.Position.Y * osuScale;

                double objectX = hitObject.X * osuScale;
                double objectY = hitObject.Y * osuScale;

                double hitPosition = System.Math.Pow(cursorX - objectX, 2) + System.Math.Pow(cursorY - objectY, 2);
                double circleRadius = System.Math.Pow(diameter * 1.00041f / 2, 2);

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

        private static void GetHitJudgment(HitObject hitObject, HitMarker marker, double X, double Y, double diameter)
        {
            double H300 = Math.GetOverallDifficultyHitWindow300(MainWindow.map.Difficulty.OverallDifficulty);
            double H100 = Math.GetOverallDifficultyHitWindow100(MainWindow.map.Difficulty.OverallDifficulty);
            double H50 = Math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);

            HitJudgment hitJudgment;
            if (marker.SpawnTime <= hitObject.SpawnTime + H300 && marker.SpawnTime >= hitObject.SpawnTime - H300)
            {
                hitJudgment = HitJudgementManager.Get300(diameter);
            }
            else if (marker.SpawnTime <= hitObject.SpawnTime + H100 && marker.SpawnTime >= hitObject.SpawnTime - H100)
            {
                hitJudgment = HitJudgementManager.Get100(diameter);
            }
            else if (marker.SpawnTime <= hitObject.SpawnTime + H50 && marker.SpawnTime >= hitObject.SpawnTime - H50)
            {
                hitJudgment = HitJudgementManager.Get50(diameter);
            }
            else
            {
                // i mean if this condition hits something is wrong coz it should never hit it... but i guess something did
                return;
                //hitJudgment = HitJudgementManager.GetMiss(diameter);
            }

            hitJudgment.SpawnTime = marker.SpawnTime;
            hitJudgment.EndTime = hitJudgment.SpawnTime + 600;

            HitJudgementManager.AliveHitJudgements.Add(hitJudgment);

            Window.playfieldCanva.Children.Add(hitJudgment);

            Canvas.SetLeft(hitJudgment, X);
            Canvas.SetTop(hitJudgment, Y);
        }
    }
}
