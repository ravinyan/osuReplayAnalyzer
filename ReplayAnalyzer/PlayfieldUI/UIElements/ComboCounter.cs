using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    public class ComboCounter
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static Canvas ComboCounterUI = new Movable(Movable.Movables.ComboCounter, true);
        private static TextBlock Combo = new TextBlock();

        public static void ResetFields()
        {

        }

        // use combo elements later
        public static Canvas Create()
        {
            ComboCounterUI.Width = 50;
            ComboCounterUI.Height = 20;

            Combo.Width = 50;
            Combo.Height = 20;
            Combo.Text = "10000026";
            Combo.Foreground = Brushes.White;

            ComboCounterUI.Children.Add(Combo);

            Canvas.SetLeft(ComboCounterUI, 0);
            Canvas.SetTop(ComboCounterUI, Window.ApplicationWindowUI.ActualHeight - Combo.Height - Window.musicControlUI.ActualHeight);
            Canvas.SetZIndex(ComboCounterUI, 50);

            return ComboCounterUI;
        }

        public static void UpdateComboCounter()
        {

        }

        public static void UpdatePositionOnResize()
        {
            Canvas.SetLeft(ComboCounterUI, 0 - Combo.Width);
            Canvas.SetTop(ComboCounterUI, Window.ApplicationWindowUI.Height - Combo.Height);
        }
    }
}
