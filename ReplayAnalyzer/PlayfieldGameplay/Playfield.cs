using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Osu;
using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public static class Playfield
    {
        public static void ResetPlayfieldFields()
        {
            SliderEndJudgement.ResetFields();
            SliderReverseArrow.ResetFields();
            SliderTick.ResetFields();
            CursorManager.ResetFields();
            HitJudgementManager.ResetFields();
            HitMarkerManager.ResetFields();
            HitObjectManager.ResetFields();
            HitObjectSpawner.ResetFields();
            FrameMarkerManager.ResetFields();
            CursorPathManager.ResetFields();
            CatchCatcherManager.ResetFields();
        }

        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        // for hitboxes coz slider ticks make my head hurt()
        public static void CreateHitBoxArea(double diameter, double X, double Y, SolidColorBrush colour, double offsetToCenterPos = 0)
        {
            Ellipse frick = new Ellipse();
            frick.Width = diameter;
            frick.Height = diameter;
            frick.Fill = colour;
            frick.Opacity = 0.5;

            frick.Loaded += async delegate (object sender, RoutedEventArgs e)
            {
                await Task.Delay(2000);
                OsuPlayfield.Playfield.Children.Remove(frick);
            };

            Canvas.SetLeft(frick, X - (offsetToCenterPos));
            Canvas.SetTop(frick, Y - (offsetToCenterPos));

            OsuPlayfield.Playfield.Children.Add(frick);
        }
    }
}

/* this could be used to show hitboxes for slider ball/tick/circles/sliderheads/hitmarker hits and other things i guess   
 * wont delete in case i might want to use that for testing or whatever
{
    Rectangle middleHit = new Rectangle();
    middleHit.Fill = Brushes.Cyan;
    middleHit.Width = 10;
    middleHit.Height = 10;

    middleHit.Loaded += async delegate (object sender, RoutedEventArgs e)
    {
        await Task.Delay(1000);
        OsuPlayfield.Playfield.Children.Remove(middleHit);
    };

    Canvas.SetLeft(middleHit, (ballX - 5));
    Canvas.SetTop(middleHit, (ballY - 5));

    Canvas.SetZIndex(middleHit, 99999);

    OsuPlayfield.Playfield.Children.Add(middleHit);

    Ellipse frick = new Ellipse();
    frick.Width = OsuPlayfield.PlayfieldCursor.Width;
    frick.Height = OsuPlayfield.PlayfieldCursor.Width;
    frick.Fill = System.Windows.Media.Brushes.Cyan;
    frick.Opacity = 0.5;

    frick.Loaded += async delegate (object sender, RoutedEventArgs e)
    {
        await Task.Delay(1000);
        OsuPlayfield.Playfield.Children.Remove(frick);
    };

    Canvas.SetLeft(frick, cursorX - (0));
    Canvas.SetTop(frick, cursorY - (0));

    OsuPlayfield.Playfield.Children.Add(frick);

    Ellipse hitbox = new Ellipse();
    hitbox.Width = diameter;
    hitbox.Height = diameter;
    hitbox.Fill = System.Windows.Media.Brushes.Red;
    hitbox.Opacity = 0.5;

    hitbox.Loaded += async delegate (object sender, RoutedEventArgs e)
    {
        await Task.Delay(1000);
        OsuPlayfield.Playfield.Children.Remove(hitbox);
    };

    Canvas.SetLeft(hitbox, ballX - (diameter / 2));
    Canvas.SetTop(hitbox, ballY - (diameter / 2));

    OsuPlayfield.Playfield.Children.Add(hitbox);

    //Ellipse tickBox = new Ellipse();
    //tickBox.Width = tick.Width;
    //tickBox.Height = tick.Width;
    //tickBox.Fill = System.Windows.Media.Brushes.Yellow;
    //tickBox.Opacity = 0.5;
    //
    //tickBox.Loaded += async delegate (object sender, RoutedEventArgs e)
    //{
    //    await Task.Delay(1000);
    //    OsuPlayfield.Playfield.Children.Remove(tickBox);
    //};
    //
    //Canvas.SetLeft(tickBox, tickX - (0));
    //Canvas.SetTop(tickBox, tickY - (0));
    //
    //OsuPlayfield.Playfield.Children.Add(tickBox);
}
//*/
