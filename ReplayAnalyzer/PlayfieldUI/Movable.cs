using ReplayAnalyzer.SettingsMenu;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReplayAnalyzer.PlayfieldUI
{
    // curiosity in its purest form
    public class Movable : Canvas
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static double X = -1;
        private static double Y = -1;
        private static double W = -1;
        private static double H = -1;
        private static bool IsDragged = false;

        private static List<Movable> UIElements = new List<Movable>();

        public static int Length => UIElements.Count;
            
        public Movable()
        {
            UIElements.Add(this);
        }

        public Movable this[int index] => UIElements[index];

        public void AddMovableEvents()
        {
            this.MouseUp += HitMapUI_MouseUp;
            this.MouseMove += HitMapUI_MouseMove;
            this.MouseLeave += HitMapUI_MouseLeave;
        }

        public void RemoveMovableEvents()
        {
            this.MouseUp -= HitMapUI_MouseUp;
            this.MouseMove -= HitMapUI_MouseMove;
            this.MouseLeave -= HitMapUI_MouseLeave;
        }

        public void Resize()
        {
            string[] pos = SettingsOptions.GetConfigValue("HitMapPosition").Split(":");
            if (pos[0] == "")
            {
                return;
            }

            double x = double.Parse(pos[0]);
            double y = double.Parse(pos[1]);
            double w = double.Parse(pos[2]);
            double h = double.Parse(pos[3]);

            if (w == -1)
            {
                Canvas.SetLeft(this, x - this.Width / 2);
            }
            else
            {
                Canvas.SetLeft(this, Window.Width - this.Width - (Window.Width - (Window.Width - x)));
            }

            if (h == -1)
            {
                Canvas.SetTop(this, y - this.Height / 2);
            }
            else
            {
                Canvas.SetTop(this, Window.Height - this.Height - (Window.Height - (Window.Height - y)));
            }
        }

        private void HitMapUI_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsDragged == true && e.LeftButton == MouseButtonState.Released)
            {
                SettingsOptions.SaveConfigOption("HitMapPosition", $"{X}:{Y}:{W}:{H}");
                IsDragged = false;
            }
        }

        private void HitMapUI_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDragged == true && e.LeftButton == MouseButtonState.Released)
            {
                SettingsOptions.SaveConfigOption("HitMapPosition", $"{X}:{Y}:{W}:{H}");
                IsDragged = false;
            }
        }

        private void HitMapUI_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsDragged = true;

                Point pos = e.GetPosition(Window.ApplicationWindowUI);
                Canvas.SetLeft(this, pos.X - (this.Width / 2));
                Canvas.SetTop(this, pos.Y - (this.Height / 2));

                W = Window.Width;
                H = Window.Height;
                X = pos.X < W / 2 ? pos.X : W - pos.X - (this.Width / 2);
                Y = pos.Y < H / 2 ? pos.Y : H - pos.Y - (this.Height / 2);
                if (X == pos.X)
                {
                    W = -1;
                }
                if (Y == pos.Y)
                {
                    H = -1;
                }
            }
        }

        public void Dispose()
        {
            UIElements.Remove(this);
        }
    }
}
