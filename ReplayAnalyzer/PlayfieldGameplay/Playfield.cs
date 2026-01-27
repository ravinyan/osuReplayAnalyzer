using ReplayAnalyzer.PlayfieldGameplay.SliderEvents;

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
        Window.playfieldCanva.Children.Remove(middleHit);
    };

    Canvas.SetLeft(middleHit, (ballX - 5));
    Canvas.SetTop(middleHit, (ballY - 5));

    Canvas.SetZIndex(middleHit, 99999);

    Window.playfieldCanva.Children.Add(middleHit);

    Ellipse frick = new Ellipse();
    frick.Width = Window.playfieldCursor.Width;
    frick.Height = Window.playfieldCursor.Width;
    frick.Fill = System.Windows.Media.Brushes.Cyan;
    frick.Opacity = 0.5;

    frick.Loaded += async delegate (object sender, RoutedEventArgs e)
    {
        await Task.Delay(1000);
        Window.playfieldCanva.Children.Remove(frick);
    };

    Canvas.SetLeft(frick, cursorX - (0));
    Canvas.SetTop(frick, cursorY - (0));

    Window.playfieldCanva.Children.Add(frick);

    Ellipse hitbox = new Ellipse();
    hitbox.Width = diameter;
    hitbox.Height = diameter;
    hitbox.Fill = System.Windows.Media.Brushes.Red;
    hitbox.Opacity = 0.5;

    hitbox.Loaded += async delegate (object sender, RoutedEventArgs e)
    {
        await Task.Delay(1000);
        Window.playfieldCanva.Children.Remove(hitbox);
    };

    Canvas.SetLeft(hitbox, ballX - (diameter / 2));
    Canvas.SetTop(hitbox, ballY - (diameter / 2));

    Window.playfieldCanva.Children.Add(hitbox);

    //Ellipse tickBox = new Ellipse();
    //tickBox.Width = tick.Width;
    //tickBox.Height = tick.Width;
    //tickBox.Fill = System.Windows.Media.Brushes.Yellow;
    //tickBox.Opacity = 0.5;
    //
    //tickBox.Loaded += async delegate (object sender, RoutedEventArgs e)
    //{
    //    await Task.Delay(1000);
    //    Window.playfieldCanva.Children.Remove(tickBox);
    //};
    //
    //Canvas.SetLeft(tickBox, tickX - (0));
    //Canvas.SetTop(tickBox, tickY - (0));
    //
    //Window.playfieldCanva.Children.Add(tickBox);
}
//*/
