using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Mania;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI.GamePlayfields
{
    public class ManiaPlayfield
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static Movable Playfield { get; private set; } = new Movable(Movable.Movables.ManiaPlayfieldPosition, false);
        public static int ColumnWidth = 50;
        public static int JudgementYPosition = 250;
        
        // number in ms
        public static double ScrollSpeed { get; set; } = 700;

        public static bool[] ActiveClicks { get; set; }

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

            // me thinks having same size always is good idea... i might change it to it has applied ScaleTransform but idk how to exactly
            int width = ColumnWidth * stringWidths.Length;
            int singleButtonWidth = ColumnWidth;

            Playfield.Width = width;
            Playfield.Height = 450;
            Playfield.Clip = new RectangleGeometry(new Rect(-200, 0, Window.ActualWidth, Playfield.Height));

            Playfield.SetPositionToDefault();

            Image stageLeft = new Image();
            stageLeft.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaStageLeft);
            stageLeft.Height = Playfield.Height;

            Canvas.SetTop(stageLeft, 0);
            Canvas.SetLeft(stageLeft, -singleButtonWidth - 2);
            Playfield.Children.Add(stageLeft);

            Image stageRight = new Image();
            stageRight.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaStageRight);
            stageRight.Height = Playfield.Height;

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
            double buttonXlocation = 72.5;
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
            int lightingXlocation = -56;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                Image lightingOnClick = new Image();
                lightingOnClick.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaStageLight);
                lightingOnClick.Name = "lighting" + i;
                lightingOnClick.Width = singleButtonWidth;
                lightingOnClick.Height = Playfield.Height;
                lightingOnClick.Opacity = 0;

                Playfield.Children.Add(lightingOnClick);

                Canvas.SetTop(lightingOnClick, lightingXlocation);
                Canvas.SetLeft(lightingOnClick, singleButtonWidth * i);
            }

            Window.ApplicationWindowUI.Children.Add(Playfield);

            ActiveClicks = new bool[stringWidths.Length];
            for (int i = 0; i < ActiveClicks.Length; i++)
            {
                ActiveClicks[i] = false;
            }

            return true;
        }

        public static void Dispose()
        {
            Playfield.Dispose();
        }

        public static void UpdateGameplayLoop()
        {
            HitJudgementManager.HandleAliveHitJudgements();
            HitObjectManager.HandleVisibleHitObjects();
            ManiaClickManager.UpdatePlayfieldClicks();
            HandleCollapsedHitObjects();
        }

        // this is for seeking backwards and correctly showing objects
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
                ManiaClickManager.UpdatePlayfieldClicks();
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
            //ManiaLongNote.UpdateChildrenVisibility(f.Time);
        }

        public static void Resize()
        {
            double scale = (Window.ApplicationWindowUI.ActualHeight - Window.musicControlUI.ActualHeight) / Playfield.Height;
            Playfield.RenderTransform = new ScaleTransform(scale, scale);

            Canvas.SetTop(Playfield, 0);//                                  7 is magic number to center the playfield
            Canvas.SetLeft(Playfield, ((Window.ApplicationWindowUI.ActualWidth / 2) - ((Playfield.Width * scale) / 2)) + 7);
        }

        private static void CreateButton(SkinElement.SkinElements skinElementIdle, SkinElement.SkinElements skinElementActive, int width, double X, int i, Canvas maniaPlayfield)
        {
            Image idleButton = new Image();
            idleButton.Width = width;
            idleButton.Height = Playfield.Height;
            idleButton.Opacity = 0.5;
            idleButton.Source = SkinElement.GetElement(skinElementIdle);
            idleButton.Name = "Idle" + i;
        
            Image activeButton = new Image();
            activeButton.Width = width;
            activeButton.Height = Playfield.Height;
            activeButton.Source = SkinElement.GetElement(skinElementActive);
            activeButton.Opacity = 0;
            activeButton.Name = "Active" + i;

            Canvas.SetTop(idleButton, X);
            Canvas.SetLeft(idleButton, width * i);

            Canvas.SetTop(activeButton, X);
            Canvas.SetLeft(activeButton, width * i);
        
            maniaPlayfield.Children.Add(idleButton);
            maniaPlayfield.Children.Add(activeButton);
        }
    }
}
