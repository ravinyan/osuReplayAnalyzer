using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameplaySkin;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.HitObjects
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

        public const int SpawnOffset = 400;

        public static Spinner CreateSpinner(SpinnerData spinner, double radius, int i)
        {
            Spinner spinnerObject = new Spinner(spinner);
            spinnerObject.Width = Window.playfieldCanva.Width;
            spinnerObject.Height = Window.playfieldCanva.Height;
            spinnerObject.Name = $"SpinnyHitObject{i}";

            double acRadius = radius * 6;
            Image approachCircle = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerApproachCircle())),
                Width = acRadius,
                Height = acRadius,
            };

            Image background = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerBackground())),
                Width = Window.playfieldCanva.Width,
                Height = Window.playfieldCanva.Height,
            };

            double rbRadius = radius * 3;
            Image rotatingBody = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerCircle())),
                Width = rbRadius,
                Height = rbRadius,
            };

            spinnerObject.Visibility = Visibility.Collapsed;

            spinnerObject.Children.Add(rotatingBody);
            spinnerObject.Children.Add(background);
            spinnerObject.Children.Add(approachCircle);

            HitObjectAnimations.ApplySpinnerAnimations(spinnerObject);

            Canvas.SetLeft(approachCircle, spinner.X - acRadius / 2);
            Canvas.SetTop(approachCircle, spinner.Y - acRadius / 2);
            
            Canvas.SetLeft(rotatingBody, spinner.X - rbRadius / 2);
            Canvas.SetTop(rotatingBody, spinner.Y - rbRadius / 2);
            
            Canvas.SetLeft(spinnerObject, spinner.X - Window.playfieldCanva.Width / 2);
            Canvas.SetTop(spinnerObject, spinner.Y - Window.playfieldCanva.Height / 2);

            return spinnerObject;
        }
    }
}
