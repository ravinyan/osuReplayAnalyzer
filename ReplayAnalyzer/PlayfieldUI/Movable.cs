using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.SettingsMenu;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI
{
    // curiosity in its purest form, opinions of outsiders are rendered useless before it
    public class Movable : Canvas
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        private static double X = -1;
        private static double Y = -1;
        private static double W = -1;
        private static double H = -1;
        private static bool IsDragged = false;

        private static bool IsPositionAdjustable = true;

        private static List<Movable> AliveMovables = new List<Movable>();
        public Movables Type { get; private set; }

        public Movable(Movables movable, bool shouldPositionBeAdjustable)
        {
            for (int i = 0; i  < AliveMovables.Count; i++)
            {
                if (movable == AliveMovables[i].Type)
                {
                    throw new Exception("There can't be 2 same types of Movables - " + movable);
                }
            }

            AliveMovables.Add(this);
            Type = movable;
            IsPositionAdjustable = shouldPositionBeAdjustable;

            // VERY IMPORTANT for UI element to have correct hitbox... for some reason
            this.Background = Brushes.Transparent;
        }

        public static List<Movable> ToList()
        {
            return AliveMovables;
        }

        public void InitializeEvents()
        {
            AddEvents();
        }

        public void DeinitializeEvents()
        {
            RemoveEvents();
        }

        public void Dispose()
        {
            RemoveEvents();
            AliveMovables.Remove(this);
            
            Canvas? parent = this.Parent as Canvas;
            parent!.Children.Remove(this);
        }

        public void AdjustPositionOnResize()
        {
            string[] pos = SettingsOptions.GetConfigValue(this.Type.ToString()).Split(":");
            if (pos[0] == "" || IsPositionAdjustable == false)
            {
                SetPositionToDefault();
                return;
            }

            double x = double.Parse(pos[0]);
            double y = double.Parse(pos[1]);
            double w = double.Parse(pos[2]);
            double h = double.Parse(pos[3]);

            if (x < w * 0.70 && x > w * 0.30)
            {
                Canvas.SetLeft(this, ((Window.Width / 2) - (this.Width / 2)) + (x - (w / 2)));
            }
            else
            {
                if (w == -1)
                {
                    Canvas.SetLeft(this, x - this.Width / 2);
                }
                else
                {
                    Canvas.SetLeft(this, Window.Width - this.Width - (Window.Width - (Window.Width - x)));
                }
            }

            if (y < h * 0.70 && y > h * 0.30)
            {
                Canvas.SetTop(this, ((Window.Height / 2) - (this.Height / 2)) + (y - (h / 2)));
            }
            else
            {
                if (h == -1)
                {
                    Canvas.SetTop(this, y - this.Height / 2);
                }
                else
                {
                    Canvas.SetTop(this, Window.Height - this.Height - (Window.Height - (Window.Height - y)));
                }
            }
        }

        public void SetPositionToDefault()
        {
            if (SettingsOptions.GetConfigValue(this.Type.ToString()) != "")
            {
                SettingsOptions.SaveConfigOption(this.Type.ToString(), "");
            }

            switch (this.Type)
            {
                case Movables.URBarPosition:
                    Canvas.SetTop(this, (Window.Height - Window.musicControlUI.ActualHeight) - 50);
                    Canvas.SetLeft(this, (Window.Width / 2) - (this.Width / 2));
                    break;
                case Movables.HitMapPosition:
                    Canvas.SetTop(this, 35);
                    Canvas.SetLeft(this, Window.ActualWidth - this.Width - 35);
                    break;
                case Movables.KeyOverlayPosition:
                    Canvas.SetTop(this, Window.Height - Window.musicControlUI.ActualHeight - (this.Height + 50));
                    Canvas.SetLeft(this, Window.Width - this.Width - 20);
                    break;
                case Movables.ManiaPlayfieldPosition:
                    double scale2 = (Window.ApplicationWindowUI.ActualHeight) / 512;
                    this.RenderTransform = new ScaleTransform(scale2, scale2);

                    Canvas.SetTop(this, 0);//                                  7 is magic number to center the playfield
                    Canvas.SetLeft(this, ((Window.ApplicationWindowUI.ActualWidth / 2) - ((this.Width * scale2) / 2)) + 7);
                    break;
            }
        }
        
        public void ApplyStartingPosition()
        {
            if (SettingsOptions.GetConfigValue(this.Type.ToString()) != "")
            {
                AdjustPositionOnResize();
            }
            else
            {
                SetPositionToDefault();
            }
        }

        private void AddEvents()
        {
            this.MouseUp    += MovableMouseUp;
            this.MouseMove  += MovableMouseMove;
            this.MouseLeave += MovableMouseLeave;
        }

        private void RemoveEvents()
        {
            this.MouseUp    -= MovableMouseUp;
            this.MouseMove  -= MovableMouseMove;
            this.MouseLeave -= MovableMouseLeave;
        }

        private void MovableMouseLeave(object sender, MouseEventArgs e)
        {
            if (IsDragged == true && e.LeftButton == MouseButtonState.Released)
            {
                SettingsOptions.SaveConfigOption(this.Type.ToString(), $"{X}:{Y}:{W}:{H}");
                IsDragged = false;
            }
        }

        private void MovableMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDragged == true && e.LeftButton == MouseButtonState.Released)
            {
                SettingsOptions.SaveConfigOption(this.Type.ToString(), $"{X}:{Y}:{W}:{H}");
                IsDragged = false;
            }
        }

        private void MovableMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsDragged = true;

                Point pos = e.GetPosition(Window.ApplicationWindowUI);
                Canvas.SetLeft(this, pos.X - (this.Width  / 2));
                Canvas.SetTop(this,  pos.Y - (this.Height / 2));

                W = Window.Width;
                H = Window.Height;

                if (pos.X < W * 0.70 && pos.X > W * 0.30)
                {
                    X = pos.X;
                }
                else
                {
                    X = pos.X < W / 2 ? pos.X : W - pos.X - (this.Width / 2);
                    if (X == pos.X)
                    {
                        W = -1;
                    }
                }

                if (pos.Y < H * 0.70 && pos.Y > H * 0.30)
                {
                    Y = pos.Y;
                }
                else
                {
                    Y = pos.Y < H / 2 ? pos.Y : H - pos.Y - (this.Height / 2);
                    if (Y == pos.Y)
                    {
                        H = -1;
                    }
                }
            }
        }

        // names here need to be 1:1 with App.config names for options that store position of these elements
        // this is i guess only to set default positions of objects
        public enum Movables : byte // fun fact i can use this to have types be byte instead of default ints 
        {
            URBarPosition, // will leave this as movable but its pretty badly made so rip will improve this one day, maybe even try to do it like in osu!lazer? or something similar
            HitMapPosition,
            KeyOverlayPosition,
            ManiaPlayfieldPosition,
        }
    }
}
