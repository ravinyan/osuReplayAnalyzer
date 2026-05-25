using ReplayAnalyzer.GameplaySkin;
using System.Windows.Controls;

namespace ReplayAnalyzer.PlayfieldGameplay
{
    public class CursorSkin
    {
        private static readonly MainWindow Window = (MainWindow)System.Windows.Application.Current.MainWindow;

        private static Image Cursor = new Image();

        public static void InitializeCursor()
        {
            if (!Window.playfieldCanva.Children.Contains(Window.playfieldCursor))
            {
                Window.playfieldCanva.Children.Add(Window.playfieldCursor);
            }

            Window.playfieldCursor.Children.Clear();

            Cursor = new Image()
            {
                Width = Window.playfieldCursor.Width,
                Height = Window.playfieldCursor.Height,
                Source = SkinElement.GetElement(SkinElement.SkinElements.Cursor),
            };

            Window.playfieldCursor.Children.Add(Cursor);
        }

        // one day
        public static void ChangeCursorSize(double diameter)
        {

        }
    }
}
