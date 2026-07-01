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

        public TaikoSpinner(TaikoSpinnerData spinnerData)
        {
            SpawnTime = spinnerData.SpawnTime;
            Judgement = new HitJudgement((HitObjectJudgement)spinnerData.Judgement.Judgement, spinnerData.Judgement.SpawnTime);
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

            Image spinnerImage = new Image();
            spinnerImage.Width = 100;
            spinnerImage.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoSpinnerWarning);

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
