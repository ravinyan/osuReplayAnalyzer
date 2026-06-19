using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows.Controls;

namespace ReplayAnalyzer.GameplaySkin
{
    public class CursorSkin
    {
        private static readonly MainWindow Window = (MainWindow)System.Windows.Application.Current.MainWindow;

        private static Image Cursor = new Image();

        public static void Apply()
        {
            if (!OsuPlayfield.Playfield!.Children.Contains(OsuPlayfield.PlayfieldCursor))
            {
                OsuPlayfield.Playfield.Children.Add(OsuPlayfield.PlayfieldCursor);
            }

            OsuPlayfield.PlayfieldCursor!.Children.Clear();

            Cursor = new Image()
            {
                Width = OsuPlayfield.PlayfieldCursor.Width,
                Height = OsuPlayfield.PlayfieldCursor.Height,
                Source = SkinElement.GetElement(SkinElement.SkinElements.Cursor),
            };

            OsuPlayfield.PlayfieldCursor.Children.Add(Cursor);
        }

        // one day
        public static void ChangeCursorSize(double diameter)
        {

        }
    }
}
