using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.HitObjects.Taiko
{
    public class TaikoSpinner : HitObject
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public TaikoSpinner(TaikoSpinnerData noteData)
        {
            SpawnTime = noteData.SpawnTime;
            Judgement = new HitJudgement((HitObjectJudgement)noteData.Judgement.Judgement, noteData.Judgement.SpawnTime);
        }

        public static TaikoSpinner CreateSpinner(TaikoSpinnerData spinnerData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateSpinnerObject(spinnerData, index);
            }

            return CreateSpinnerPreload(spinnerData, index);
        }

        private static TaikoSpinner CreateSpinnerObject(TaikoSpinnerData spinnerData, int index)
        {
            TaikoSpinner spinner = new TaikoSpinner(spinnerData);

            // this will need to be coloured per wiki
            // Tinted red for "Don"(235, 69, 44)
            // Tinted blue for "Katsu"(68, 141, 171)

            Image spinnerImage = new Image();
            spinnerImage.Width = 50;
            spinnerImage.Source = SkinElement.GetElement(SkinElement.SkinElements.SpinnerCircle);

            spinner.Children.Add(spinnerImage);

            Canvas.SetLeft(spinner, Window.Width);
            Canvas.SetTop(spinner, 0);
            Canvas.SetZIndex(spinner, 1);

            spinner.Name = $"TaikoSpinnerObject{index}";

            return spinner;
        }

        private static TaikoSpinner CreateSpinnerPreload(TaikoSpinnerData spinnerData, int index)
        {
            TaikoSpinner spinner = new TaikoSpinner(spinnerData);

            Image spinnerImage = new Image();
            spinner.Children.Add(spinnerImage);

            Canvas.SetLeft(spinner, 0);
            Canvas.SetTop(spinner, 0);

            spinner.Name = $"TaikoSpinnerObject{index}";

            return spinner;
        }
    }
}
