using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class OsuPlayfield
    {
        public static Border PlayfieldBorder { get; private set; } = new Border();
        public static Canvas Playfield { get; private set; } = new Canvas();
        public static Canvas PlayfieldCursor { get; private set; } = new Canvas();

        public static void Create()
        {
            // osu playfield never changes so it can be created only once
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            if (window.playfieldGrid.Children.Contains(PlayfieldBorder))
            {
                return;
            }

            CreateBorder();
            CreatePlayfield();
            CreateCursor();

            window.playfieldGrid.Children.Add(PlayfieldBorder);
        }

        public static void Dispose()
        {
            //Playfield = null!;
            //PlayfieldBorder = null!;
            //PlayfieldCursor = null!;
        }

        private static void CreateCursor()
        {
            if (PlayfieldCursor == null)
            {
                PlayfieldCursor = new Canvas();
            }

            PlayfieldCursor.Name = "PlayfieldCursor";
            PlayfieldCursor.Width = 35;
            PlayfieldCursor.Height = 35;
            Canvas.SetZIndex(PlayfieldCursor, 99);
        }

        private static void CreatePlayfield()
        {
            if (Playfield == null)
            {
                Playfield = new Canvas();
            }

            Playfield.Name = "OsuPlayfield";
            Playfield.Width = 512;
            Playfield.Height = 384;
            Playfield.ClipToBounds = false;

            PlayfieldBorder!.Child = Playfield;
        }

        private static void CreateBorder()
        {
            if (PlayfieldBorder == null)
            {
                PlayfieldBorder = new Border();
            }

            PlayfieldBorder.Name = "OsuPlayfieldBorder";
            PlayfieldBorder.Visibility = Visibility.Visible;
            PlayfieldBorder.BorderBrush = Brushes.White;
            // in MainWindow.xaml there is grid and this playfield is in this position
            Grid.SetColumn(PlayfieldBorder, 1);
            Grid.SetRow(PlayfieldBorder, 1);
        }
    }
}
