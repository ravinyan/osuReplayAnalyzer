using ReplayAnalyzer.Analyser.UIElements;
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

namespace ReplayAnalyzer.PlayfieldGameplay
{
    // uhh does it make sense? i feel like it does... but does it?
    public class HitDetection : HitMarkerManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        private static OsuMath Math = new OsuMath();

        private static List<HitObject> AliveHitObjects
        {
            get
            {
                return Playfield.GetAliveHitObjects();
            }
        }

        public static void CheckIfObjectWasHit()
        {
            GetCurrentHitMarker(ref CurrentHitMarker, CurrentHitMarkerIndex);

            if (GamePlayClock.TimeElapsed >= CurrentHitMarker.SpawnTime && !AliveHitMarkers.Contains(CurrentHitMarker))
            {
                SpawnHitMarker(CurrentHitMarker);

                Playfield.GetAliveHitObjects().Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));
                if (Playfield.GetAliveHitObjects().Count > 0)
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
                                    double judgementX = hitObject.X * osuScale - diameter / 2;
                                    double judgementY = hitObject.Y * osuScale - diameter;
                                    GetHitJudgment(hitObject, CurrentHitMarker, judgementX, judgementY, diameter);
                                }

                                Playfield.AnnihilateHitObject(hitObject);

                                hitObject.HitAt = CurrentHitMarker.SpawnTime;
                                hitObject.IsHit = true;
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
                                    Playfield.RemoveSliderHead(sliderHead);
                                }

                                sHitObject.HitAt = CurrentHitMarker.SpawnTime;
                                sHitObject.IsHit = true;
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

                        Playfield.HitObjectDespawnMiss(AliveHitObjects[i], SkinElement.HitMiss(), MainWindow.OsuPlayfieldObjectDiameter);

                        if (AliveHitObjects[i] is Slider)
                        {
                            Playfield.RemoveSliderHead(AliveHitObjects[i].Children[1] as Canvas);
                        }
                        else if (AliveHitObjects[i] is HitCircle)
                        {
                            Playfield.AnnihilateHitObject(AliveHitObjects[i]);
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
            double H300 = Math.GetOverallDifficultyHitWindow300(MainWindow.map.Difficulty.OverallDifficulty);
            double H100 = Math.GetOverallDifficultyHitWindow100(MainWindow.map.Difficulty.OverallDifficulty);
            double H50 = Math.GetOverallDifficultyHitWindow50(MainWindow.map.Difficulty.OverallDifficulty);

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

            Playfield.AliveHitJudgements.Add(hitJudgment);

            Window.playfieldCanva.Children.Add(hitJudgment);

            Canvas.SetLeft(hitJudgment, X);
            Canvas.SetTop(hitJudgment, Y);
        }

      
    }
}
