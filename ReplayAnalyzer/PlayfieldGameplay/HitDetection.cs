using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.SettingsMenu;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Slider = ReplayAnalyzer.Objects.Slider;

#nullable disable

namespace ReplayAnalyzer.PlayfieldGameplay
{
    // uhh does it make sense? i feel like it does... but does it?
    public class HitDetection : HitMarkerManager
    {
        public static void CheckIfObjectWasHit()
        {
            GetCurrentHitMarker(ref CurrentHitMarker, CurrentHitMarkerIndex);

            if (GamePlayClock.TimeElapsed >= CurrentHitMarker.SpawnTime && !AliveHitMarkers.Contains(CurrentHitMarker))
            {
                SpawnHitMarker(CurrentHitMarker);

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
                        string osuClient = SettingsOptions.config.AppSettings.Settings["OsuClient"].Value;

                        if (hitObject is HitCircle && CurrentHitMarker.SpawnTime + 400 >= hitObject.SpawnTime && CurrentHitMarker.SpawnTime - 400 <= hitObject.SpawnTime)
                        {
                            ApplyNoteLockIfPossible(osuClient, hitObject, out prevHitObjectExists);

                            if (prevHitObjectExists == false)
                            {
                                if (hitObject.Visibility != Visibility.Collapsed)
                                {
                                    float judgementX = (float)(hitObject.X * osuScale - diameter / 2);
                                    float judgementY = (float)(hitObject.Y * osuScale - diameter);
                                    GetHitJudgment(hitObject, CurrentHitMarker.SpawnTime, judgementX, judgementY, diameter);
                                }

                                HitObjectManager.AnnihilateHitObject(hitObject);
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
                                    float judgementX = (float)(sHitObject.X * osuScale - diameter / 2);
                                    float judgementY = (float)(sHitObject.Y * osuScale - diameter);
                                    GetHitJudgment(sHitObject, CurrentHitMarker.SpawnTime, judgementX, judgementY, diameter);
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
                            if (head.Children[0].Visibility == Visibility.Visible)
                            {
                                HitObjectManager.HitObjectDespawnMiss(aliveHitObjects[i], MainWindow.OsuPlayfieldObjectDiameter);
                                HitObjectManager.RemoveSliderHead(head); 
                            }
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

                double cursorX = CurrentHitMarker.Position.X * osuScale;
                double cursorY = CurrentHitMarker.Position.Y * osuScale;

                double objectX = hitObject.X * osuScale;
                double objectY = hitObject.Y * osuScale;

                double hitPosition = Math.Pow(cursorX - objectX, 2) + Math.Pow(cursorY - objectY, 2);
                // 1.00(0)41f from osu lazer here additional 0 doesnt work tho
                double circleRadius = 0;

                var x1c = 415.78486003572976;
                var y1c = 100.33069300273108;
                var x1o = 452.5049603174603;
                var y1o = 105.75396825396825;
                var cr1 = Math.Pow(74.190476190476176 * 1.00030f / 2, 2);
                var hp1 = Math.Pow(x1c - x1o, 2) + Math.Pow(y1c - y1o, 2);
                bool bish1 = hp1 <= cr1;
                var bishh1 = hp1 - cr1;
                // ^ NOT HIT SHOULD BE

                var x2c = 226.87388563913012;
                var y2c = 235.00880627405076;
                var x2o = 204.38988095238093;
                var y2o = 263.36805555555554;
                var cr2 = Math.Pow(72.368253968253953 * 1.00030f / 2, 2);
                var hp2 = Math.Pow(x2c - x2o, 2) + Math.Pow(y2c - y2o, 2);
                bool bish2 = hp2 <= cr2;
                var bishh2ss = hp2 - cr2;
                // ^ YES HIT SHOULD BE

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
                }
                // 54 > 70 = 16 = 124 + 16 = 140 | 157 > 163 = 6
                if (hitPosition - circleRadius > 0 && hitPosition - circleRadius < 5)
                {
                    var aaa = hitObject.SpawnTime / 1000.0;
                    var min = (int)aaa / 60;
                    var sec = aaa % 60;
                    var a = GamePlayClock.TimeElapsed;

                    var s = "";
                    // god help me before i go insane
                }                // if cursor position is lower number then its inside the circle...
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

        private static void GetHitJudgment(HitObject hitObject, long hitTime, float X, float Y, double diameter)
        {
            OsuMath math = new OsuMath();
            double H300 = math.GetOverallDifficultyHitWindow300();
            double H100 = math.GetOverallDifficultyHitWindow100();
            double H50 = math.GetOverallDifficultyHitWindow50();

            double diff = Math.Abs(hitObject.SpawnTime - hitTime);                             
            if (hitObject.Judgement == HitJudgementManager.HitObjectJudgement.Max || (diff <= H300 && diff >= -H300))
            {
                URBar.ShowHit(hitObject.SpawnTime - hitTime, new SolidColorBrush(Color.FromRgb(138, 216, 255)));
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, 300);
            }
            else if (hitObject.Judgement == HitJudgementManager.HitObjectJudgement.Ok || (diff <= H100 && diff >= -H100))
            {
                URBar.ShowHit(hitObject.SpawnTime - hitTime, new SolidColorBrush(Color.FromRgb(176, 192, 25)));
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, 100);
            }
            else if (hitObject.Judgement == HitJudgementManager.HitObjectJudgement.Meh || (diff <= H50 && diff >= -H50))
            {
                URBar.ShowHit(hitObject.SpawnTime - hitTime, new SolidColorBrush(Color.FromRgb(255, 217, 61)));
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, 50);
            }
            else
            {
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, 0);
            }
        }
    }
}
