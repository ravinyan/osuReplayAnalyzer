using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.OsuMaths;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Numerics;
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
        
        // number in ms
        public static double ScrollSpeed { get; set; } = 700;

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
                MessageBox.Show("This skin doesnt contain osu!mania skin.ini properties for \"ColumnWidth\"", "Incorrect skin selected");
                return false;
            }

            string[] stringWidths = stringWidth.Split(",");

            //string[] testStringWidths = new string[9];
            //
            //for (int i = 0; i < testStringWidths.Length; i++)
            //{
            //    testStringWidths[i] = "52";
            //}
            //stringWidths = testStringWidths;

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

            ActiveClicks = new (Clicks, bool)[stringWidths.Length];
            for (int i = 0; i < ActiveClicks.Length; i++)
            {
                ActiveClicks[i] = (Clicks.ManiaK1 + i, false);
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
        }

        public static void PreloadReplay()
        {
            for (int i = 0; i < MainWindow.replay.FramesDict.Count; i++)
            {
                long time = MainWindow.replay.FramesDict[i].Time;
                GamePlayClock.Seek(time);

                HitObjectSpawner.UpdateHitObjects();
                HitObjectManager.HandleVisibleHitObjects();
                UpdateClickPreload(MainWindow.replay.FramesDict[i]);
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

        public static void UpdateClickPreload(ReplayFrame frame)
        {
            int startIndex = 3;
            int k1Value = (int)Clicks.ManiaK1;
            int columnCount = (int)MainWindow.map.Difficulty.CircleSize;

            List<HitObject> notes = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < columnCount; i++)
            {
                int column = i;
                if (frame.Clicks.Contains((Clicks)column + k1Value))
                {
                    for (int j = 0; j < notes.Count; j++)
                    {
                        if (notes[j] is ManiaNote)
                        {
                            ManiaNote n = (ManiaNote)notes[j];

                            if (n.ColumnIndex == column && ActiveClicks[column].active == false)
                            {
                                ManiaHitDetection.GetHitJudgment(n, frame.Time, ColumnWidth * column, 100);
                                ActiveClicks[column].active = true;
                                break;
                            }
                        }
                        else
                        {
                            // HOW TO DO THIS IM TOO STUPID
                            ManiaLongNote ln = (ManiaLongNote)notes[j];
                            if (ln.ColumnIndex == column && ActiveClicks[column].active == false)
                            {
                                ln.HoldStarted = true;
                                ManiaHitDetection.GetHitJudgment(ln, frame.Time, ColumnWidth * column, 100);
                                ActiveClicks[column].active = true;
                                break;
                            }
                        }
                    }
                }
                else
                {

                    for (int j = 0; j < notes.Count; j++)
                    {
                        if (notes[j] is ManiaLongNote)
                        {
                            ManiaLongNote ln = (ManiaLongNote)notes[j];
                            if (ln.ColumnIndex == column && ActiveClicks[column].active == true && ln.HoldStarted == true)
                            {
                                ManiaHitDetection.GetHitJudgment(ln, frame.Time, ColumnWidth * column, 100, true);
                                ActiveClicks[column].active = false;
                                break;
                            }
                        }
                    }

                    ActiveClicks[column].active = false;
                }
            }
        }

        public static void SeekGameplay(double direction, ReplayFrame f)
        {

        }

        public static void Resize()
        {
            // brain empty
            double scale2 = Window.ApplicationWindowUI.ActualHeight / 512;
            Playfield.RenderTransform = new ScaleTransform(scale2, scale2);

            Canvas.SetTop(Playfield, 0);//                                  7 is magic number to center the playfield
            Canvas.SetLeft(Playfield, ((Window.playfieldGrid.ActualWidth / 2) - ((Playfield.Width * scale2) / 2)) + 7);
        }

        private static (Clicks click, bool active)[] ActiveClicks;
        public static void UpdateClickUI(bool isSeekingForward = false)
        {
            ReplayFrame frame = MainWindow.CurrentFrame;
            int startIndex = 3;
            int k1Value = (int)Clicks.ManiaK1;
            int columnCount = (int)MainWindow.map.Difficulty.CircleSize;
            HitObjectManager.GetAliveHitObjects().Sort((x, y) => x.SpawnTime.CompareTo(y.SpawnTime));
            // manipulating active skin elements and lighting skin elements
            // active elements are startIndex + 2 * column
            // lighting is (startIndex + (2 * columnCount)) + i - 1 lighting elements are added as last in playfield
            List<HitObject> notes = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < columnCount; i++)
            {
                int column = i;
                if (frame.Clicks.Contains((Clicks)column + k1Value))
                {
                    Playfield.Children[startIndex + 2 * column].Opacity = 1;
                    Playfield.Children[(startIndex + (2 * columnCount)) + column - 1].Opacity = 1;

                    if (GamePlayClock.IsPaused() == false || isSeekingForward == true)
                    {
                        for (int j = 0; j < notes.Count; j++)
                        {
                            if (notes[j] is ManiaNote)
                            {
                                ManiaNote n = (ManiaNote)notes[j];

                                if (n.ColumnIndex == column && ActiveClicks[column].active == false)
                                {
                                    ManiaHitDetection.GetHitJudgment(n, (long)GamePlayClock.TimeElapsed, ColumnWidth * column, 100);
                                    ActiveClicks[column].active = true;
                                    break;
                                }
                            }
                            else
                            {
                                ManiaLongNote ln = (ManiaLongNote)notes[j];
                                if (ln.ColumnIndex == column && ActiveClicks[column].active == false)
                                {
                                    ln.HoldStarted = true;
                                    ManiaHitDetection.GetHitJudgment(ln, (long)GamePlayClock.TimeElapsed, ColumnWidth * column, 100);
                                    ActiveClicks[column].active = true;
                                    break;
                                }
                            }
                        }
                    }   
                }
                else
                {
                    Playfield.Children[startIndex + 2 * column].Opacity = 0;
                    Playfield.Children[(startIndex + (2 * columnCount)) + column - 1].Opacity = 0;

                    if (GamePlayClock.IsPaused() == false || isSeekingForward == true)
                    {
                        for (int j = 0; j < notes.Count; j++)
                        {
                            if (notes[j] is ManiaLongNote)
                            {
                                ManiaLongNote ln = (ManiaLongNote)notes[j];
                                if (ln.ColumnIndex == column && ActiveClicks[column].active == true && ln.HoldStarted == true)
                                {
                                    ManiaHitDetection.GetHitJudgment(ln, (long)GamePlayClock.TimeElapsed, ColumnWidth * column, 100, true);
                                    ActiveClicks[column].active = false;
                                    break;
                                }
                            }
                        }
                    }

                    ActiveClicks[column].active = false;
                }
            }
        }

        private static void CreateButton(SkinElement.SkinElements skinElementIdle, SkinElement.SkinElements skinElementActive, int width, double X, int i, Canvas maniaPlayfield)
        {
            Image idleButton = new Image();
            idleButton.Width = width;
            idleButton.Height = Playfield.Height;
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
