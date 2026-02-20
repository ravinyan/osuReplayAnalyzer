using ReplayAnalyzer.GameplaySkin;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class CursorSkin
    {
        private static readonly MainWindow Window = (MainWindow)System.Windows.Application.Current.MainWindow;

        public static void ApplySkin()
        {
            Image cursor = new Image()
            {
                Width = Window.playfieldCursor.Width,
                Height = Window.playfieldCursor.Height,
                Source = new BitmapImage(new Uri(SkinElement.Cursor())),
            };

            Window.playfieldCursor.Children.Add(cursor);
        }

        // one day
        public static void ChangeCursorSize(double diameter)
        {

        }
    }
}
