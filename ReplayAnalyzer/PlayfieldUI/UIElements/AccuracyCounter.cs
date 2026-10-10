using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    public class AccuracyCounter
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static Canvas AccuracyCounterUI = new Movable(Movable.Movables.AccuracyCounter, true);
        private static TextBlock Accuracy = new TextBlock();

        // i would love to use bytes to use even less memory but oops cant
        // nvm i can use bytes and then multiply result by 10 but no point in doing that... well will see when i finish this at snail pace
        // instead of lists i can use maybe just normal numbers and incrementing them... but optimization will be after i figure out
        // acc formulas coz apparently osu one doesnt work...
        private static List<short> Counter320 = new List<short>();
        private static List<short> Counter300 = new List<short>();
        private static List<short> Counter200 = new List<short>();
        private static List<short> Counter150 = new List<short>(); // lazer slider end
        private static List<short> Counter100 = new List<short>();
        private static List<short> Counter50  = new List<short>();
        private static List<short> Counter30  = new List<short>(); // lazer tick
        private static List<short> Counter0   = new List<short>();

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
                case GameMode.Osu: //98.79 but i have 98.91
                    if (ClassicMod.IsClassicEnabled || MainWindow.replay.IsLazer == false)
                    {// this is slightly incorrect... need to factor sliders i guess somehow? like slider ends but idk how
                        // nvm fixedddd... nvm x2 it is not fully correct but its problem in hit judgement manager
                        AccValue = (double)(300 * Counter300.Count + 100 * Counter100.Count + 50 * Counter50.Count)
                                 / (double)(300 * (Counter300.Count + Counter100.Count + Counter50.Count + Counter0.Count))
                                 * 100;
                    }
                    else // lezeeer i actually dont really understand how to do it im beyond horrible at math
                    {    // acc is based on total score sooooo idk how to exactly do it? will try some stuff like counting all ticks etc
                        AccValue = (double)(300 * Counter300.Count + 100 * Counter100.Count + 50 * Counter50.Count)// + 150 * Counter150.Count + 30 * Counter30.Count)
                                 / (double)(300 * (Counter300.Count + Counter100.Count + Counter50.Count + Counter0.Count))
                                 * 100;
                    }
                    break;
                case GameMode.OsuMania:
                    if (ScoreV2Mod.ManiaEnabled == true)
                    {
                        AccValue = (double)(305 * Counter320.Count + 300 * Counter300.Count + 200 * Counter200.Count + 100 * Counter100.Count + 50 * Counter50.Count)
                                 / (double)(305 * (Counter320.Count + Counter300.Count + Counter200.Count + Counter100.Count + Counter50.Count + Counter0.Count))
                                 * 100;
                    }
                    else
                    {
                        AccValue = (double)(300 * (Counter320.Count + Counter300.Count) + 200 * Counter200.Count + 100 * Counter100.Count + 50 * Counter50.Count)
                                 / (double)(300 * (Counter320.Count + Counter300.Count + Counter200.Count + Counter100.Count + Counter50.Count + Counter0.Count))
                                 * 100;
                    }
                    break;
                case GameMode.OsuTaiko:
                    AccValue = (double)(Counter300.Count + (Counter100.Count / 2)) // apparently it is 0.5n good (x100 here)
                             / (double)(Counter300.Count + Counter100.Count + Counter0.Count)
                             * 100;
                    break;
                case GameMode.OsuCatch:// 300 is ANY fruit/droplet/drop caught, 100 is droplet miss, 0 is fruit/drop miss
                    AccValue = (double)(Counter300.Count)
                             / (double)(Counter300.Count + Counter100.Count + Counter0.Count)
                             * 100;
                    break;
                default:
                    throw new Exception("update THIS CATSGRAB");
            }

            Accuracy.Text = Math.Round(AccValue, 2).ToString() + "%";
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
                //case HitObjectJudgement.a:
                //    Counter30.Add((short)judgement);
                //    UpdateAccuracyCounter();
                //    break;
                case HitObjectJudgement.Miss:
                case HitObjectJudgement.SliderTickMiss:
                    Counter0.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.SliderEndMiss:
                    Counter0.Add((short)judgement);
                    UpdateAccuracyCounter();
                    break;
            }
        }

        public static void UpdatePositionOnResize()
        {
            Canvas.SetLeft(AccuracyCounterUI, Window.ApplicationWindowUI.ActualWidth - Accuracy.Width);
            Canvas.SetTop(AccuracyCounterUI, 0 + Accuracy.Height);
        }
    }
}
