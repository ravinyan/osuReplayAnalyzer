using ReplayAnalyzer.AnalyzerTools.UIElements;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using ReplayAnalyzer.Skins;
using System.Windows;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class HitJudgementManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static List<HitJudgment> AliveHitJudgements = new List<HitJudgment>();

        public static void ResetFields()
        {
            AliveHitJudgements.Clear();
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

        public static HitJudgment Get300(double diameter)
        {
            JudgementCounter.Increment300();
            return new HitJudgment(SkinElement.Hit300(), diameter, diameter);
        }

        public static HitJudgment Get100(double diameter)
        {
            JudgementCounter.Increment100();
            return new HitJudgment(SkinElement.Hit100(), diameter, diameter);
        }

        public static HitJudgment Get50(double diameter)
        {
            JudgementCounter.Increment50();
            return new HitJudgment(SkinElement.Hit50(), diameter, diameter);
        }

        public static HitJudgment GetMiss(double diameter)
        {
            JudgementCounter.IncrementMiss();
            return new HitJudgment(SkinElement.HitMiss(), diameter, diameter);
        }

        public enum HitObjectJudgement
        {
            Max = 300,
            Ok = 100,
            Meh = 50,
            Miss = 0,
            None = -1,
        }
    }
}
