using ReplayAnalyzer.GameplaySkin;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class CursorSkin
    {
        private static readonly MainWindow Window = (MainWindow)System.Windows.Application.Current.MainWindow;

        private static Image Cursor = new Image();

        //public static void ChangeSkin()
        //{
        //    Cursor.Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.Cursor)));
        //}

        public static void ApplySkin()
        {
            if (!Window.playfieldCanva.Children.Contains(Window.playfieldCursor))
            {
                Window.playfieldCanva.Children.Add(Window.playfieldCursor);
            }

            Cursor = new Image()
            {
                Width = Window.playfieldCursor.Width,
                Height = Window.playfieldCursor.Height,
                Source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.Cursor))),
            };

            Window.playfieldCursor.Children.Add(Cursor);
        }

        // one day
        public static void ChangeCursorSize(double diameter)
        {

        }
    }
}
