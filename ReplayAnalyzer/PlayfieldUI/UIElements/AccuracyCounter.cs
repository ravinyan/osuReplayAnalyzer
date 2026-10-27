using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#nullable disable

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    public class AccuracyCounter
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        
        public static Canvas AccuracyCounterUI = new Movable(Movable.Movables.AccuracyCounter, true);
        private static TextBlock Accuracy = new TextBlock();
        
        // i would love to use bytes to use even less memory but oops cant
        // nvm i can use bytes and then multiply result by 10 but no point in doing that... well will see when i finish this at snail pace
        // instead of lists i can use maybe just normal numbers and incrementing them... but optimization will be after i figure out
        // acc formulas coz apparently osu one doesnt work...
        private static List<short> Counter320    = new List<short>();
        private static List<short> Counter300    = new List<short>();
        private static List<short> Counter200    = new List<short>();
        private static List<short> Counter150    = new List<short>(); // lazer slider end
        private static List<short> Counter100    = new List<short>();
        private static List<short> Counter50     = new List<short>();
        private static List<short> Counter30     = new List<short>(); // lazer tick (or maybe its 10?)
        private static List<short> Counter0      = new List<short>();
        private static List<short> Counter0Tails = new List<short>(); // lazer slider end misses
        private static List<short> Counter0Ticks = new List<short>(); // lazer slider tick misses


        // lazer uses current max possible score / current score for accuracy... but do i need to do that?
        // https://github.com/ppy/osu/blob/2de19558923160116a0139a955fa431008ef5d89/osu.Game/Rulesets/Scoring/ScoreProcessor.cs#L329-L371
        // i will just do my own thing i think and if its impossible then i will do this maybe? idk will see
        // for now i want to try doing this myself

        public static void ResetFields()
        {
            Counter320.Clear();
            Counter300.Clear();
            Counter200.Clear();
            Counter150.Clear();
            Counter100.Clear();
            Counter50.Clear();
            Counter30.Clear();
            Counter0.Clear();
            Counter0Tails.Clear();
            Counter0Ticks.Clear();
            Accuracy.Text = "100%";
        }
        
        // use combo elements later
        public static Canvas Create()
        {
            AccuracyCounterUI.Width = 40;
            AccuracyCounterUI.Height = 20;
        
            Accuracy.Width = 40;
            Accuracy.Height = 20;
            Accuracy.Text = "100%";
            Accuracy.Foreground = Brushes.White;
        
            AccuracyCounterUI.Children.Add(Accuracy);
        
            Canvas.SetLeft(AccuracyCounterUI, Window.ApplicationWindowUI.ActualWidth - Accuracy.Width);
            Canvas.SetTop(AccuracyCounterUI, 0 + Accuracy.Height);
            Canvas.SetZIndex(AccuracyCounterUI, 100);
        
            return AccuracyCounterUI;
        }
        
        private static double AccValue = 0;
        public static void UpdateAccuracyCounter()
        {
            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    if (ClassicMod.IsClassicEnabled || MainWindow.replay.IsLazer == false)
                    {// CORRECT
                        AccValue = (double)(300 * Counter300.Count + 100 * Counter100.Count + 50 * Counter50.Count)
                                 / (double)(300 * (Counter300.Count + Counter100.Count + Counter50.Count + Counter0.Count))
                                 * 100;
                    }
                    else 
                    {// CORRECT
                        // the total amount of slider ends and ticks is correct, the amount that should(?) be is not
                        // as in end result is wrong... what happens IN replay doesnt matter coz lazer cant even correctly play replays
            
                        // this formula IS correct, now figure out why judgement dont match...
                        // THERE IS POSSIBILITY THAT I CANT MAKE JUDGEMENTS CORRECT DUE TO HOW REPLAY FRAMES ARE STORED
                        // IN THAT CASE JUST DONT PAY ATTENTION TO THIS CODE AND LEAVE IT BE LET IT BE WRONG JUST LIKE LAZER IS WRONG
                        // OTHERWISE I WILL JUST WASTE TIME (not like it very much matters BUT IT DOESNT NEED TO BE PERFECT IF ITS NOT POSSIBLE)
                        AccValue = (double)(300 * Counter300.Count + 100 * Counter100.Count + 50 * Counter50.Count + 150 * Counter150.Count + 30 * Counter30.Count)
                                 / (double)(300 * (Counter300.Count + Counter100.Count + Counter50.Count + Counter0.Count)
                                          + 150 * (Counter150.Count + Counter0Tails.Count) + 30 * (Counter30.Count + Counter0Ticks.Count))
                                 * 100;

                        //var a = MainWindow.map.HitObjects.Where(h => h is OsuSliderData s && s.SliderEndJudgement.Judgement == 150);
                    }
                    break;
                case GameMode.OsuMania:
                    if (ScoreV2Mod.ManiaEnabled == true)
                    {// CORRECT (on lazer acc values on leaderboards ALWAYS use this formula even if score was set on stable with scorev1)
                     // but in this app im not doing that (for now at least need to think about it)
                        AccValue = (double)(305 * Counter320.Count + 300 * Counter300.Count + 200 * Counter200.Count + 100 * Counter100.Count + 50 * Counter50.Count)
                                 / (double)(305 * (Counter320.Count + Counter300.Count + Counter200.Count + Counter100.Count + Counter50.Count + Counter0.Count))
                                 * 100;
                    }
                    else
                    {// CORRECT
                        AccValue = (double)(300 * (Counter320.Count + Counter300.Count) + 200 * Counter200.Count + 100 * Counter100.Count + 50 * Counter50.Count)
                                 / (double)(300 * (Counter320.Count + Counter300.Count + Counter200.Count + Counter100.Count + Counter50.Count + Counter0.Count))
                                 * 100;
                    }
                    break;
                case GameMode.OsuTaiko:// CORRECT
                    AccValue = (double)(Counter300.Count + (Counter100.Count / 2.0))
                             / (double)(Counter300.Count + Counter100.Count + Counter0.Count)
                             * 100;
                    break;
                case GameMode.OsuCatch:// CORRECT (300 is ANY fruit/droplet/drop caught, 100 is droplet miss, 0 is fruit/drop miss)
                    AccValue = (double)(Counter300.Count)
                             / (double)(Counter300.Count + Counter100.Count + Counter0.Count)
                             * 100;
                    break; // small note: my 100 count is incorrect a bit so i need to investigate that
                default:   // welp that is interesting... playing replay frame by frame in lazer gives same results as mine, but otherwise it adds
                           // few droplets drops... and actually this not frame by frame behaviour is correct?
                           // in osu std it catches these drops and it is also correct...
                           // i need to analyze osu!std replay to see everything but thats when i have more time
                           //
                           // at 22s there is pixel perfect hit so i think my catcher width needs to be increased?
                           // ok the problem is not hitbox of catcher but... hitbox of fruits?
                           // actually fuck if i know what is happening stable replay ends with 98.84 and lazer with 97.98 AND CORRECT IS 97.88
                           // stable has wrong judgements too it has too little droplets dropped
                           // lazer is closer but has 3 misses and there should be 1 miss
                           // this is on replay with DT so that might be just DT issue... i will just ignore it for now
                           // the most important thing is that misses are correct
                           // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
                           // i think also how my droplets are created is wrong since there are misses when there shouldnt be on SS plays
                           // wow there is 99.98% score on aleph 0 but when whatching the replay it ends with SS (but result screen shows 99.98% anyway)
                           // do to do: save every judgement from preloading and use that... that will improve things maybe probably IDK
                           // ^ update: fuck osu catch i dont want to touch this game mode ever again after today
                           // ^ update2: as i thought the problem was with replay frames... i can simulate replay frames in 1k fps
                           //            and then everything just works (except DT)... the problem is amount of RAM app will use but that is not
                           //            really a problem? 100MB ram is not that much anyway 
                    throw new Exception("update THIS CATSGRAB");
            }

            //Accuracy.Text = Counter300.Count.ToString();

            Accuracy.Text = Math.Round(AccValue, 2).ToString() + "%";
        }

        public static void UpdateAccuracyAfterSeek(long currentTime)
        {
            // need to reset all counters before calculating accuracy again
            ResetFields();

            switch (MainWindow.replay.GameMode)
            {
                case GameMode.Osu:
                    UpdateOsu(currentTime);
                    break;
                case GameMode.OsuMania:
                    UpdateMania(currentTime);
                    break;
                case GameMode.OsuTaiko:
                    UpdateTaiko(currentTime);
                    break;
                case GameMode.OsuCatch:
                    UpdateCatch(currentTime);
                    break;
                default:
                    throw new Exception("game mode exists not");
            }
        }
        
        public static void Add(HitObjectJudgement judgement)
        {
            if (MainWindow.IsReplayPreloading)
            {
                return;
            }
        
            switch (judgement)
            {
                case HitObjectJudgement.Perfect:
                    Counter320.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.Great:
                    Counter300.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.Good:
                    Counter200.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.SliderEndHit:
                    Counter150.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.Ok:
                    Counter100.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.Meh:
                    Counter50.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.SliderTickHit:
                    Counter30.Add((short)judgement);
                    break;
                case HitObjectJudgement.Miss:
                    Counter0.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.SliderEndMiss:
                    Counter0Tails.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.SliderTickMiss:
                    Counter0Ticks.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.None: // dont do anything since some hit object can have this judgement and i dont want exception
                    break;
                default:
                    throw new Exception("judgement doesnt exist... it poof and existence no more");
            }
        }
        
        public static void UpdatePositionOnResize()
        {
            Canvas.SetLeft(AccuracyCounterUI, Window.ApplicationWindowUI.ActualWidth - Accuracy.Width);
            Canvas.SetTop(AccuracyCounterUI, 0 + Accuracy.Height);
        }

        // i wish there was a way to kill all these if {break} statements but sadly they are mandatory
        private static void UpdateOsu(long currentTime)
        {
            for (int i = 0; i < MainWindow.map.HitObjects.Count; i++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[i];
                if (hitObject is OsuCircleData)
                {
                    if (hitObject.Judgement.SpawnTime > currentTime)
                    {
                        break;
                    }
                    Add((HitObjectJudgement)hitObject.Judgement.Judgement);
                }
                else if (hitObject is OsuSliderData)
                {
                    OsuSliderData d = (OsuSliderData)hitObject;
                    if (hitObject.Judgement.SpawnTime > currentTime)
                    {
                        break;
                    }
                    if (ClassicMod.IsSliderHeadAccOn == true)
                    {
                        Add((HitObjectJudgement)d.Judgement.Judgement);
                    }
    
                    if (d.SliderTicks != null)// it can be null... i should change it one day to have 0 items... but i wont lol
                    {
                        for (int j = 0; j < d.SliderTicks.Count; j++)
                        {
                            if (d.SliderTicks[j].Judgement.SpawnTime > currentTime)
                            {
                                break;
                            }
                            Add((HitObjectJudgement)d.SliderTicks[j].Judgement.Judgement);
                        }
                    }

                    for (int j = 0; j < d.ReverseArrowJudgements.Length; j++)
                    {
                        if (d.ReverseArrowJudgements[j].SpawnTime > currentTime)
                        {
                            break;
                        }
                        Add((HitObjectJudgement)d.ReverseArrowJudgements[j].Judgement);
                    }

                    if (d.SliderEndJudgement.SpawnTime > currentTime)
                    {
                        break;
                    }
                    Add((HitObjectJudgement)d.SliderEndJudgement.Judgement);

                }
                else // spinner wont give judgements tho it should... but eh i will just set it to always give x300 IF i do anything at all
                {    // if i do it i need to give x300 judgement after each spinner tho... yea i will just ignore it for now

                }
            }
        }
        
        private static void UpdateMania(long currentTime)
        {
            for (int i = 0; i < MainWindow.map.HitObjects.Count; i++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[i];
                if (hitObject is ManiaNoteData)
                {
                    if (hitObject.Judgement.SpawnTime > currentTime)
                    {
                        break;
                    }
                    Add((HitObjectJudgement)hitObject.Judgement.Judgement);
                }
                else // long notes (covers sv1 and sv2)
                {
                    ManiaLongNoteData d = (ManiaLongNoteData)hitObject;
                    if (d.Judgement.SpawnTime > currentTime)// in sv this is judgement for the whole long note
                    {
                        break;
                    }
                    Add((HitObjectJudgement)d.Judgement.Judgement);

                    if (ScoreV2Mod.ManiaEnabled == true)
                    {
                        if (d.TailJudgement.SpawnTime > currentTime)
                        {
                            break;
                        }
                        Add((HitObjectJudgement)d.TailJudgement.Judgement);
                    }
                }
            }
        }

        private static void UpdateTaiko(long currentTime)
        {
            for (int i = 0; i < MainWindow.map.HitObjects.Count; i++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[i];
                if (hitObject.Judgement.SpawnTime > currentTime)
                {
                    break;
                }

                if (hitObject is TaikoHitCircleData) // nothing else gives accuracy in taiko
                {
                    Add((HitObjectJudgement)hitObject.Judgement.Judgement);
                }
            }
        }

        private static void UpdateCatch(long currentTime)
        {
            for (int i = 0; i < MainWindow.map.HitObjects.Count; i++)
            {
                HitObjectData hitObject = MainWindow.map.HitObjects[i];
                if (hitObject is CatchFruitData)
                {
                    if (hitObject.Judgement.SpawnTime > currentTime)
                    {
                        break;
                    }
                    Add((HitObjectJudgement)hitObject.Judgement.Judgement);
                }
                else if (hitObject is CatchJuiceStreamData)
                {
                    CatchJuiceStreamData d = (CatchJuiceStreamData)hitObject;
                    // Judgements here are everything: head, drops, droplets, reverse arrows and tails
                    for (int j = 0; j < d.Judgements.Count; j++)
                    {
                        if (d.Judgements[j].SpawnTime > currentTime)
                        {
                            break;
                        }
                        Add((HitObjectJudgement)d.Judgements[j].Judgement);
                    }
                }// no spinners coz they dont give accuracy
            }
        }
    }
}
