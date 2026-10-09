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
        public static List<short> Counter320 = new List<short>();
        public static List<short> Counter300 = new List<short>();
        public static List<short> Counter200 = new List<short>();
        public static List<short> Counter150 = new List<short>(); // lazer slider end miss?
        public static List<short> Counter100 = new List<short>();
        public static List<short> Counter50  = new List<short>();
        public static List<short> Counter0   = new List<short>();

        // lazer uses current max possible score / current score for accuracy... but do i need to do that?
        // https://github.com/ppy/osu/blob/2de19558923160116a0139a955fa431008ef5d89/osu.Game/Rulesets/Scoring/ScoreProcessor.cs#L329-L371
        // i will just do my own thing i think and if its impossible then i will do this maybe? idk will see
        // for now i want to try doing this myself


        // use combo elements later
        public static Canvas Create()
        {
            AccuracyCounterUI.Width = 40;
            AccuracyCounterUI.Height = 20;

            Accuracy.Width = 40;
            Accuracy.Height = 20;
            Accuracy.Text = "99.99%";
            Accuracy.Foreground = Brushes.White;

            AccuracyCounterUI.Children.Add(Accuracy);

            Canvas.SetLeft(AccuracyCounterUI, Window.ApplicationWindowUI.ActualWidth - Accuracy.Width);
            Canvas.SetTop(AccuracyCounterUI, 0 + Accuracy.Height);
            Canvas.SetZIndex(AccuracyCounterUI, 100);

            return AccuracyCounterUI;
        }

        public static void UpdateAccuracyCounter()
        {

        }

        public static void Add(HitObjectJudgement judgement, short value)
        {
            switch (judgement)
            {
                case HitObjectJudgement.Perfect:
                    Counter320.Add(value);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.Great:
                    Counter320.Add(value);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.Good:
                    Counter320.Add(value);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.Ok:
                    Counter320.Add(value);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.Meh:
                    Counter320.Add(value);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.Miss:
                case HitObjectJudgement.SliderTickMiss:
                    Counter320.Add(value);
                    UpdateAccuracyCounter();
                    break;
                    Counter320.Add(value);
                    UpdateAccuracyCounter();
                    break;
                case HitObjectJudgement.SliderEndMiss:
                    Counter320.Add(value);
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
