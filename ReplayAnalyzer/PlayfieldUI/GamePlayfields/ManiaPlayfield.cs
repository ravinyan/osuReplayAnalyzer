using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameplaySkin;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class ManiaPlayfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;
        public static Movable Playfield { get; private set; } = new Movable(Movable.Movables.ManiaPlayfieldPosition);

        public static void Dispose()
        {
            Playfield.Dispose();
        }

        public static void Create()
        {
            if (Window.ApplicationWindowUI.Children.Contains(Playfield))
            {
                Playfield.Dispose();
                Playfield = new Movable(Movable.Movables.ManiaPlayfieldPosition);
            }

            string stringWidth = SkinIniProperties.GetManiaPlayfieldWidth();
            string[] stringWidths = stringWidth.Split(",");

            //string[] testStringWidths = new string[9];
            //
            //for (int i = 0; i < testStringWidths.Length; i++)
            //{
            //    testStringWidths[i] = "52";
            //}
            //stringWidths = testStringWidths;

            int width = 0;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                width += int.Parse(stringWidths[i]);
            }

            // me thinks having same size always is good idea... i might change it to it has applied ScaleTransform but idk how to exactly
            width = 50 * stringWidths.Length;
            int singleButtonWidth = 50; // width / stringWidths.Length;

            //Playfield.ClipToBounds = true;
            //Playfield.Margin = new Thickness(100, 0, 0, 0);
            Playfield.Width = width;
            Playfield.Height = Window.playfieldGrid.ActualHeight;
            Canvas.SetTop(Playfield, 0);
            Canvas.SetLeft(Playfield, 200);

            Image stageLeft = new Image();
            stageLeft.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaStageLeft);
            stageLeft.Height = Window.playfieldGrid.ActualHeight;

            Canvas.SetTop(stageLeft, 0);
            Canvas.SetLeft(stageLeft, -singleButtonWidth - 4);
            Playfield.Children.Add(stageLeft);

            Image stageRight = new Image();
            stageRight.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaStageRight);
            stageRight.Height = Window.playfieldGrid.ActualHeight;

            Canvas.SetTop(stageRight, 0);
            Canvas.SetLeft(stageRight, width);
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
            double buttonXlocation = 81.5;
            bool columnColourSwitch = true; // true = white, false = pink, middle of odd column count = yellow
            // third iteration of trying to make correct loop and this looks so clean wow
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
            int lightingXlocation = -60;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                Image lightingOnClick = new Image();
                lightingOnClick.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaStageLight);
                lightingOnClick.Name = "lighting" + i;
                lightingOnClick.Width = singleButtonWidth;
                lightingOnClick.Height = Window.playfieldGrid.ActualHeight;
                lightingOnClick.Opacity = 0;

                Playfield.Children.Add(lightingOnClick);

                Canvas.SetTop(lightingOnClick, lightingXlocation);
                Canvas.SetLeft(lightingOnClick, singleButtonWidth * i);
            }

            Window.ApplicationWindowUI.Children.Add(Playfield);
        }

        public static void UpdateClickUI()
        {
            if (MainWindow.CurrentFrame == null)
            {
                return;
            }

            ReplayFrame frame = MainWindow.CurrentFrame;//MainWindow.replay.FramesDict[MainWindow.frameIndex - 1];
            int startIndex = 3;
            int k1Value = (int)Clicks.ManiaK1;
            int columnCount = (int)MainWindow.map.Difficulty.CircleSize;

            // wow this works perfectly lmao
            for (int i = 0; i < columnCount; i++)
            {
                int difference = i;
                if (frame.Clicks.Contains((Clicks)i + k1Value))
                {
                    Playfield.Children[startIndex + 2 * difference].Opacity = 1;
                    Playfield.Children[(int)(startIndex + (2 * columnCount)) + i - 1].Opacity = 1;
                }
                else
                {
                    Playfield.Children[startIndex + 2 * difference].Opacity = 0;
                    Playfield.Children[(int)(startIndex + (2 * columnCount)) + i - 1].Opacity = 0;
                }
            }
        }
  
        private static void CreateButton(SkinElement.SkinElements skinElementIdle, SkinElement.SkinElements skinElementActive, int width, double X, int i, Canvas maniaPlayfield)
        {
            Image idleButton = new();
            idleButton.Width = width;
            idleButton.Height = Window.playfieldGrid.ActualHeight;
            idleButton.Source = SkinElement.GetElement(skinElementIdle);
            idleButton.Name = "Idle" + i;
        
            Image activeButton = new Image();
            activeButton.Width = width;
            activeButton.Height = Window.playfieldGrid.ActualHeight;
            activeButton.Source = SkinElement.GetElement(skinElementActive);
            activeButton.Opacity = 0;
            activeButton.Name = "Active" + i;

            Canvas.SetTop(idleButton, X);// Window.Height - Playfield.Height + 2);// X;
            Canvas.SetLeft(idleButton, width * i);

            Canvas.SetTop(activeButton, X);// Window.Height - Playfield.Height + 2);// X;
            Canvas.SetLeft(activeButton, width * i);
        
            maniaPlayfield.Children.Add(idleButton);
            maniaPlayfield.Children.Add(activeButton);
        }
    }
}
