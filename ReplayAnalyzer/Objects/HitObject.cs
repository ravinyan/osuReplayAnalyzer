using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Skins;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static ReplayAnalyzer.PlayfieldGameplay.HitJudgementManager;
using Image = System.Windows.Controls.Image;

namespace ReplayAnalyzer.Objects
{
    // i know its not really needed since there is DataContext in canvas classes, but i want to practice inheritance
    // and learn how to nicely use it and implement it instead of doing it messy... and maybe it will make code look nicer too
    public class HitObject : Canvas, IDisposable
    {
        public double X { get; set; }
        public double Y { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public int SpawnTime { get; set; }
        public int StackHeight { get; set; }
        public double HitAt { get; set; }
        public bool IsHit { get; set; }
        public HitObjectJudgement Judgement { get; set; } = HitObjectJudgement.None;

        public static Grid AddComboNumber(int comboNumber, double diameter)
        {
            Grid grid = new Grid();
            grid.Width = diameter;
            grid.Height = diameter;

            StackPanel numberPanel = new StackPanel();
            numberPanel.Orientation = Orientation.Horizontal;
            numberPanel.HorizontalAlignment = HorizontalAlignment.Center;

            if (comboNumber <= 9)
            {
                Image hitCircleNumber = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(comboNumber))),
                };

                numberPanel.Children.Add(hitCircleNumber);
            }
            else if (comboNumber <= 99)
            {
                char[] number = comboNumber.ToString().ToCharArray();

                Image hitCircleNumber = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[0]))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[1]))),
                };

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
            }
            else if (comboNumber <= 999)
            {
                char[] number = comboNumber.ToString().ToCharArray();

                Image hitCircleNumber = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[0]))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[1]))),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = diameter / 2 * 0.8,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(number[2]))),
                };

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
                numberPanel.Children.Add(hitCircleNumber3);
            }
            else
            {
                Image hitCircleNumber = new Image()
                {
                    Height = diameter / 2 * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(7))),
                };

                Image hitCircleNumber2 = new Image()
                {
                    Height = diameter / 2 * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(2))),
                };

                Image hitCircleNumber3 = new Image()
                {
                    Height = diameter / 2 * 0.7,
                    Source = new BitmapImage(new Uri(SkinElement.ComboNumber(7))),
                };

                numberPanel.Children.Add(hitCircleNumber);
                numberPanel.Children.Add(hitCircleNumber2);
                numberPanel.Children.Add(hitCircleNumber3);
            }

            grid.Children.Add(numberPanel);

            return grid;
        }

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(disposing: true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SuppressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                //this.Children.Clear();
                //this.Children.Capacity = 0;
                //
                //LocalValueEnumerator locallySetProperties = this.GetLocalValueEnumerator();
                //while (locallySetProperties.MoveNext())
                //{
                //    DependencyProperty propertyToClear = locallySetProperties.Current.Property;
                //    if (!propertyToClear.ReadOnly) 
                //    { 
                //        this.ClearValue(propertyToClear); 
                //    }
                //}

                Dispatcher.Invoke(() =>
                {
                    for (int i = this.Children.Count - 1; i >= 0; i--)
                    {
                        if (this.Children[i] is Image)
                        {
                            var a = this.Children[i] as Image;
                            a.Source = null;
                            a.UpdateLayout();
                            this.Children[i] = a;
                        }

                        this.Children.Remove(Children[i]);
                        HitObjectAnimations.RemoveStoryboardFromDict(this);
                    }
                    this.Children.Capacity = 0;
                });

                // Note disposing has been done.
                disposed = true;
            }
        }


        ~HitObject()
        {
            Dispose(disposing: false);
        }
    }
}

