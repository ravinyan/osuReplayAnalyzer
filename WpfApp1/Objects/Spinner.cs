using ReplayParsers.Classes.Beatmap.osu.Objects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfApp1.Skins;

namespace WpfApp1.Objects
{
    public class SpinnerObject
    {
        private static MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Canvas CreateSpinner(Spinner spinner, double radius, int i)
        {
            Canvas spinnerObject = new Canvas();
            spinnerObject.DataContext = spinner;
            //spinnerObject.Name = $"SpinnerHitObject{i}";

            double newRadius = Window.playfieldCanva.Width;

            spinnerObject.Width = newRadius;
            spinnerObject.Height = newRadius;
           

            Image approachCircle = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerApproachCircle())),
                Width = newRadius,
                Height = newRadius,
            };

            Image background = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerBackground())),
                Width = newRadius,
                Height = newRadius,
            };

            Image rotatingBody = new Image()
            {
                Source = new BitmapImage(new Uri(SkinElement.SpinnerCircle())),
                Width = newRadius,
                Height = newRadius,
            };

            spinnerObject.Visibility = Visibility.Collapsed;
            
            spinnerObject.Children.Add(rotatingBody);
            spinnerObject.Children.Add(background);
            spinnerObject.Children.Add(approachCircle);

            Canvas.SetLeft(spinnerObject, (spinner.SpawnPosition.X) - (newRadius / 2));
            Canvas.SetTop(spinnerObject, (spinner.SpawnPosition.Y) - (newRadius / 2));

            return spinnerObject;
        }
    }
}
