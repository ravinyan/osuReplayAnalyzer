using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Mania;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class ManiaPlayfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static Movable Playfield { get; private set; } = new Movable(Movable.Movables.ManiaPlayfieldPosition, false);
        public static int ColumnWidth { get; set; } = 50;
        public static int JudgementYPosition { get; set; } = 250;
        public static double NoteRenderSizeHeight { get; set; } = 0;
        
        // number in ms
        public static double ScrollSpeed { get; set; } = 700;

        public static bool[] ActiveClicks { get; set; }

        public static Vector2[] JudgementPos { get; private set; }

        public static bool Create()
        {
            if (Window.ApplicationWindowUI.Children.Contains(Playfield))
            {
                Playfield.Dispose();
                Playfield = new Movable(Movable.Movables.ManiaPlayfieldPosition, false);
            }

            string stringWidth = SkinIniProperties.GetManiaPlayfieldWidth();
            if (stringWidth == "")
            {
                MessageBox.Show($"Currently selected skin \"{SkinElement.CurrentSkinFolderPath.Split("\\").Last()}\" doesnt contain osu!mania skin.ini properties for \"ColumnWidth\"", "Incorrect skin selected");
                return false;
            }

            string[] stringWidths = stringWidth.Split(",");
            JudgementPos = new Vector2[stringWidths.Length];
            for (int i = 0; i < JudgementPos.Length; i++)
            {
                JudgementPos[i] = new Vector2(ColumnWidth * i, JudgementYPosition);
            }

            // me thinks having same size always is good idea... i might change it to it has applied ScaleTransform but idk how to exactly
            int width = ColumnWidth * stringWidths.Length;
            int singleButtonWidth = ColumnWidth;

            Playfield.Width = width;
            Playfield.Height = 450;
            Playfield.Clip = new RectangleGeometry(new Rect(-200, 0, Window.ActualWidth, Playfield.Height));

            Playfield.SetPositionToDefault();
            Canvas.SetZIndex(Playfield, 1);

            Image stageLeft = new Image();
            stageLeft.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaStageLeft);
            stageLeft.Height = Playfield.Height;

            Canvas.SetTop(stageLeft, 0);
            Canvas.SetLeft(stageLeft, -singleButtonWidth - 2);
            Canvas.SetZIndex(stageLeft, -1);
            Playfield.Children.Add(stageLeft);

            Image stageRight = new Image();
            stageRight.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaStageRight);
            stageRight.Height = Playfield.Height;

            Canvas.SetTop(stageRight, 0);
            Canvas.SetLeft(stageRight, width);
            Canvas.SetZIndex(stageRight, -1);
            Playfield.Children.Add(stageRight);

            /* Below is the default note image layout for each column, by key count.

                Keycount	Col 1	Col 2	Col 3	Col 4	Col 5	Col 6	Col 7	Col 8	Col 9
                1K	        S	        		    		    		    	
                2K	        1	        1	    		    		    		
                3K	        1	        S	    1	    		    			
                4K	        1	        2	    2	    1	    				
                5K	        1	        2	    S	    2	    1				
                6K	        1	        2	    1	    1	    2	    1	    		    
                7K	        1	        2	    1	    S	    1	    2	    1	    	
                8K	        1	        2	    1	    2	    2	    1	    2	    1	
                9K	        1	        2	    1	    2	    S	    2	    1	    2	    1
            */
            double buttonXlocation = 72.5;
            bool columnColourSwitch = true; // true = white, false = pink, middle of odd column count = yellow
            // third iteration of trying to make correct loop and this looks so clean wow
            Grid buttonGrid = new Grid();
            for (int i = 0; i < stringWidths.Length; i++)
            {
                // special middle button when number of columns is odd
                if (stringWidths.Length % 2 == 1 && i == stringWidths.Length / 2)
                {
                    columnColourSwitch = !columnColourSwitch;
                    CreateButton(SkinElement.SkinElements.ManiaKey3Idle, SkinElement.SkinElements.ManiaKey3Pressed
                                , singleButtonWidth, buttonXlocation, i, Playfield);
                }
                else
                {
                    // if middle point is reached then flip bool to colour order is mirrored
                    if (i == stringWidths.Length / 2)
                    {
                        columnColourSwitch = !columnColourSwitch;
                    }

                    if (columnColourSwitch == true)
                    {
                        columnColourSwitch = false;
                        CreateButton(SkinElement.SkinElements.ManiaKey1Idle, SkinElement.SkinElements.ManiaKey1Pressed
                                    , singleButtonWidth, buttonXlocation, i, Playfield);
                    }
                    else if (columnColourSwitch == false)
                    {
                        columnColourSwitch = true;
                        CreateButton(SkinElement.SkinElements.ManiaKey2Idle, SkinElement.SkinElements.ManiaKey2Pressed
                                    , singleButtonWidth, buttonXlocation, i, Playfield);
                    }
                }
            }

            // oh you need to be coloured... what a fucked up day (i might just not do that tho)
            int lightingXlocation = -56;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                Image lightingOnClick = new Image();

                var a = SkinElement.GetElement(SkinElement.SkinElements.ManiaStageLight);

                //BitmapImage myBitmapImage = new BitmapImage();
                //myBitmapImage.BeginInit();
                //myBitmapImage.UriSource = new Uri(SkinElement.GetElementPath(SkinElement.SkinElements.ManiaStageLight));
                //myBitmapImage.DecodePixelWidth = 1;//(int)(a.Width / 2);
                //myBitmapImage.DecodePixelHeight = 1;// (int)(a.Height / 2);
                //myBitmapImage.EndInit();

                //aa.DecodePixelHeight = (int)(a.Width / 1.5);
                //aa.DecodePixelHeight = (int)a.Height;

                // hmm i can do that... interesting... but that will change nothing since i want to reduce gpu usage
                //a.Freeze();

                lightingOnClick.Source = a;
                lightingOnClick.Name = "lighting" + i;
                lightingOnClick.Width = singleButtonWidth;
                lightingOnClick.Height = Playfield.Height;
                lightingOnClick.Visibility = Visibility.Collapsed;
            
                Playfield.Children.Add(lightingOnClick);
            
                Canvas.SetTop(lightingOnClick, lightingXlocation);
                Canvas.SetLeft(lightingOnClick, singleButtonWidth * i);
                Canvas.SetZIndex(lightingOnClick, -2); // notes are -1
            }

            ActiveClicks = new bool[stringWidths.Length];
            for (int i = 0; i < ActiveClicks.Length; i++)
            {
                ActiveClicks[i] = false;
            }

            Window.ApplicationWindowUI.Children.Add(Playfield);
            // this NEEDS to be here coz i need render sizes to correctly position playfield elements
            Window.UpdateLayout();

            return true;
        }

        public static void Dispose()
        {
            Playfield.Dispose();
            Playfield.Children.Clear();
        }

        public static void UpdateGameplayLoop(bool skip = false)
        {
            HitJudgementManager.HandleAliveHitJudgements();
            HitObjectManager.HandleVisibleHitObjects();
            ManiaClickManager.UpdatePlayfieldClicks(skip);
            HandleCollapsedHitObjects();
        }

        // this is for seeking backwards and correctly showing objects
        // this code sucks i will nuke it on all game modes and figure out some more optimized and clean way
        private static void HandleCollapsedHitObjects()
        {
            List<HitObject> hitObjects = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < hitObjects.Count; i++)
            {
                if (hitObjects[i].Visibility == Visibility.Collapsed)
                {
                    if (hitObjects[i] is ManiaLongNote)
                    {
                        ManiaLongNote ln = (ManiaLongNote)hitObjects[i];
                        if (ln.TailJudgement.SpawnTime > ManiaClickManager.ManiaFrame.Time
                        ||  ln.Judgement.SpawnTime > ManiaClickManager.ManiaFrame.Time)
                        {
                            hitObjects[i].Visibility = Visibility.Visible;
                        }
                    }
                    else if (hitObjects[i] is ManiaNote && hitObjects[i].Judgement.SpawnTime > ManiaClickManager.ManiaFrame.Time)
                    {
                        hitObjects[i].Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public static void PreloadReplay()
        {
            for (int i = 0; i < MainWindow.replay.FramesDict.Count; i++)
            {
                long time = MainWindow.replay.FramesDict[i].Time;
                GamePlayClock.Seek(time);
                HitObjectSpawner.UpdateHitObjects();
                HitObjectManager.HandleVisibleHitObjects();
                ManiaClickManager.UpdatePlayfieldClicks(false);
            }

            PlayfieldGameplay.Playfield.ResetPlayfieldFields();

            for (int i = Playfield.Children.Count - 1; i >= 0; i--)
            {
                if (Playfield.Children[i] is ManiaNote || Playfield.Children[i] is ManiaLongNote)
                {
                    Playfield.Children.Remove(Playfield.Children[i]);
                }
            }
        }

        public static void SeekGameplay(double direction, ReplayFrame f)
        {
            ManiaClickManager.UpdateIndexAfterSeek(f);
            ManiaLongNote.UpdateChildrenVisibility();
        }

        public static void Resize()
        {
            if (Playfield.Children.Count > 0)
            {
                // all these weird index numbers are needed in case user resizes app when there are notes on playfield
                int startIndex = 3;
                for (int column = 0; column < (int)MainWindow.map.Difficulty.CircleSize; column++)
                {
                    FrameworkElement? buttonIdle = Playfield.Children[startIndex + 2 * column - 1] as FrameworkElement;
                    FrameworkElement? buttonActive = Playfield.Children[startIndex + 2 * column] as FrameworkElement;
                    FrameworkElement? lighting = Playfield.Children[(startIndex + (2 * (int)MainWindow.map.Difficulty.CircleSize)) + column - 1] as FrameworkElement;

                    if (buttonIdle.RenderSize.Width < 50)
                    {
                        buttonIdle.LayoutTransform = new ScaleTransform(50 / buttonIdle.RenderSize.Width, 1);
                        Canvas.SetTop(buttonIdle, Playfield.Height - buttonIdle.RenderSize.Height);
                        Canvas.SetLeft(buttonIdle, (ColumnWidth * column) - (buttonIdle.RenderSize.Width / 2) + 2);
                    }
                    else
                    {
                        Canvas.SetTop(buttonIdle, Playfield.Height - buttonIdle.RenderSize.Height);
                        Canvas.SetLeft(buttonIdle, ColumnWidth * column);
                    }

                    if (buttonIdle.RenderSize.Width < 50)
                    {
                        buttonActive.LayoutTransform = new ScaleTransform(50 / buttonIdle.RenderSize.Width, 1);
                        Canvas.SetTop(buttonActive, Playfield.Height - buttonIdle.RenderSize.Height);
                        Canvas.SetLeft(buttonActive, (ColumnWidth * column) - (buttonIdle.RenderSize.Width / 2) + 2);
                    }
                    else
                    {
                        Canvas.SetTop(buttonActive, Playfield.Height - buttonIdle.RenderSize.Height);
                        Canvas.SetLeft(buttonActive, ColumnWidth * column);
                    }

                    Canvas.SetTop(lighting, Playfield.Height - lighting.RenderSize.Height);
                    Canvas.SetLeft(lighting, ColumnWidth * column);
                }

                UIElement stageRight = Playfield.Children[startIndex - 2];
                UIElement stageLeft = Playfield.Children[startIndex - 3];

                Canvas.SetTop(stageLeft, 0);
                Canvas.SetLeft(stageLeft, -stageLeft.RenderSize.Width);

                Canvas.SetTop(stageRight, 0);
                Canvas.SetLeft(stageRight, ColumnWidth * (int)MainWindow.map.Difficulty.CircleSize);
            }

            double scale = (Window.ApplicationWindowUI.ActualHeight - Window.musicControlUI.ActualHeight) / Playfield.Height;
            Playfield.RenderTransform = new ScaleTransform(scale, scale);

            Canvas.SetTop(Playfield, 0);//                                  7 is magic number to center the playfield
            Canvas.SetLeft(Playfield, ((Window.ApplicationWindowUI.ActualWidth / 2) - ((Playfield.Width * scale) / 2)) + 7);
        }

        private static void CreateButton(SkinElement.SkinElements skinElementIdle, SkinElement.SkinElements skinElementActive, int width, double X, int i, Canvas maniaPlayfield)
        {
            // ok so... how the fuck do i do keys here so that the size wont be too big or small on some skins
            // answer: i dont... i have no clue how... how... H O W... how myan...
            // why the images sizes can be so different and why im so stupid to not understand how to correctly use them

            BitmapSource skinKey = SkinElement.GetElement(skinElementIdle);
            // my lord and saviour https://osu.ppy.sh/community/forums/topics/1803158?n=6
            double h = 0;
            if (SkinElement.GetElementPath(skinElementIdle).Contains("@2x"))
            {
                h = Math.Round(skinKey.PixelHeight / 3.2);
            }
            else
            {
                h = Math.Round(skinKey.PixelHeight / 1.6);
            }

            Rect rect = new Rect(0, 0, ColumnWidth, h);
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(skinKey, rect);
            }
            RenderTargetBitmap resizedImage = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
            resizedImage.Render(drawingVisual);

            Image idleButton = new Image();
            idleButton.Opacity = 0.5;
            idleButton.Source = resizedImage;
            idleButton.Name = "Idle" + i;

            // for active key
            if (SkinElement.GetElementPath(skinElementIdle).Contains("@2x"))
            {
                h = Math.Round(skinKey.PixelHeight / 3.2);
            }
            else
            {
                h = Math.Round(skinKey.PixelHeight / 1.6);
            }

            rect = new Rect(0, 0, ColumnWidth, h);
            skinKey = SkinElement.GetElement(skinElementActive);
            drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawImage(skinKey, rect);
            }
            resizedImage = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96, 96, PixelFormats.Default);
            resizedImage.Render(drawingVisual);

            Image activeButton = new Image();
            activeButton.Source = resizedImage;
            activeButton.Opacity = 0.5;
            activeButton.Name = "Active" + i;

            maniaPlayfield.Children.Add(idleButton);
            maniaPlayfield.Children.Add(activeButton);
        }
    }
}
