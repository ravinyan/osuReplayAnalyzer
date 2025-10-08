using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp1.Skins;

namespace WpfApp1.Objects
{
    public class Spinner
    {
        private static MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Canvas CreateSpinner(SpinnerData spinner, double radius, int i)
        {
            Canvas spinnerObject = new Canvas();
            spinnerObject.DataContext = spinner;
            spinnerObject.Name = $"SpinnerHitObject{i}";

            spinnerObject.Width = Window.playfieldCanva.Width;
            spinnerObject.Height = Window.playfieldCanva.Height;

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

            //Animations.HitObjectAnimations.ApplySpinnerAnimations(spinnerObject);

            Canvas.SetLeft(approachCircle, (spinner.SpawnPosition.X) - (acRadius / 2));
            Canvas.SetTop(approachCircle, (spinner.SpawnPosition.Y) - (acRadius / 2));
            
            Canvas.SetLeft(rotatingBody, (spinner.SpawnPosition.X) - (rbRadius / 2));
            Canvas.SetTop(rotatingBody, (spinner.SpawnPosition.Y) - (rbRadius / 2));
            
            Canvas.SetLeft(spinnerObject, (spinner.SpawnPosition.X) - (Window.playfieldCanva.Width / 2));
            Canvas.SetTop(spinnerObject, (spinner.SpawnPosition.Y) - (Window.playfieldCanva.Height / 2));

            return spinnerObject;
        }
    }
}
