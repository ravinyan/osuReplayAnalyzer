using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.HitObjects.Osu
{
    public class Spinner : HitObject
    {
        public Spinner(SpinnerData spinnerData)
        {
            X = spinnerData.X;
            Y = spinnerData.Y;
            BaseSpawnPosition = new System.Numerics.Vector2((float)spinnerData.X, (float)spinnerData.Y);
            SpawnTime = spinnerData.SpawnTime - SpawnOffset;
            EndTime = spinnerData.EndTime;
        }

        public int EndTime { get; set; }

        private static MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public const int SpawnOffset = 375;

        public static Spinner CreateSpinner(SpinnerData spinner, double radius, int i)
        {
            Spinner spinnerObject = new Spinner(spinner);
            spinnerObject.Width = OsuPlayfield.Playfield.Width;
            spinnerObject.Height = OsuPlayfield.Playfield.Height;
            spinnerObject.Name = $"SpinnyHitObject{i}";

            double acRadius = radius * 6;
            Image approachCircle = new Image()
            {
                Source = SkinElement.GetElement(SkinElement.SkinElements.SpinnerApproachCircle),
                Width = acRadius,
                Height = acRadius,
            };

            Image background = new Image()
            {
                Source = SkinElement.GetElement(SkinElement.SkinElements.SpinnerBackground),
                Width = OsuPlayfield.Playfield.Width,
                Height = OsuPlayfield.Playfield.Height,
            };

            double rbRadius = radius * 3;
            Image rotatingBody = new Image()
            {
                Source = SkinElement.GetElement(SkinElement.SkinElements.SpinnerCircle),
                Width = rbRadius,
                Height = rbRadius,
            };

            spinnerObject.Visibility = Visibility.Collapsed;

            spinnerObject.Children.Add(rotatingBody);
            spinnerObject.Children.Add(background);
            spinnerObject.Children.Add(approachCircle);

            SetLeft(approachCircle, spinner.X - acRadius / 2);
            SetTop(approachCircle, spinner.Y - acRadius / 2);
            
            SetLeft(rotatingBody, spinner.X - rbRadius / 2);
            SetTop(rotatingBody, spinner.Y - rbRadius / 2);
            
            SetLeft(spinnerObject, spinner.X - OsuPlayfield.Playfield.Width / 2);
            SetTop(spinnerObject, spinner.Y - OsuPlayfield.Playfield.Height / 2);

            return spinnerObject;
        }

        public static Image ApproachCircle(Spinner s)
        {
            return (Image)s.Children[2];
        }
    }
}
