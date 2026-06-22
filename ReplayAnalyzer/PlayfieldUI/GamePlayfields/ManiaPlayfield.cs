using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.HitObjects.Mania;
using ReplayAnalyzer.OsuMaths;
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

        public static void UpdateGameplayLoop()
        {
            OsuPlayfield.UpdateGameplayLoop();
        }

        public static void Dispose()
        {
            Playfield.Dispose();
        }

        public static void Create()
        {
            if (Window.ApplicationWindowUI.Children.Contains(Playfield))
            {
                Playfield.Dispose();
                Playfield = new Movable(Movable.Movables.ManiaPlayfieldPosition, false);
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
            width = ColumnWidth * stringWidths.Length;
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
        }

        public static void Resize()
        {
            // brain empty
            double scale2 = (Window.ApplicationWindowUI.ActualHeight) / 512;
            Playfield.RenderTransform = new ScaleTransform(scale2, scale2);

            Canvas.SetTop(Playfield, 0);//                                  7 is magic number to center the playfield
            Canvas.SetLeft(Playfield, ((Window.playfieldGrid.ActualWidth / 2) - ((Playfield.Width * scale2) / 2)) + 7);
        }

        public static void UpdateClickUI()
        {
            if (MainWindow.CurrentFrame == null)
            {
                return;
            }
            //MainWindow.replay.FramesDict[CursorManager.CursorPositionIndex];
            ReplayFrame frame = MainWindow.CurrentFrame;//MainWindow.replay.FramesDict[MainWindow.frameIndex - 1];
            int startIndex = 3;
            int k1Value = (int)Clicks.ManiaK1;
            int columnCount = (int)MainWindow.map.Difficulty.CircleSize;

            // manipulating active skin elements and lighting skin elements
            // active elements are startIndex + 2 * column
            // lighting is (startIndex + (2 * columnCount)) + i - 1 lighting elements are added as last in playfield
            List<HitObject> notes = HitObjectManager.GetAliveHitObjects();
            for (int i = 0; i < columnCount; i++)
            {
                int column = i;
                if (frame.Clicks.Contains((Clicks)i + k1Value))
                {
                    Playfield.Children[startIndex + 2 * column].Opacity = 1;
                    Playfield.Children[(startIndex + (2 * columnCount)) + i - 1].Opacity = 1;

                    for (int j = 0; j < notes.Count; j++)
                    {
                        if (notes[j] is ManiaNote)
                        {
                            ManiaNote n = (ManiaNote)notes[j];

                            if (n.ColumnIndex == i && n.IsHit == true)
                            {
                                GetHitJudgment(n, frame.Time, ColumnWidth * i, 100);
                                n.IsHit = true;
                                break;
                            }
                        }
                        else
                        {
                            ManiaLongNote ln = (ManiaLongNote)notes[j];
                            if (ln.ColumnIndex == i && ln.IsHeld == false)
                            {
                                GetHitJudgment(ln, frame.Time, ColumnWidth * i, 100);
                                ln.IsHeld = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Playfield.Children[startIndex + 2 * column].Opacity = 0;
                    Playfield.Children[(startIndex + (2 * columnCount)) + i - 1].Opacity = 0;
                }
            }
        }

        private static OsuMath math = new OsuMath();
        private static void GetHitJudgment(HitObject hitObject, long hitTime, float X, float Y)
        {
            if (hitObject.Visibility == Visibility.Collapsed)
            {
                return;
            }

            double H320 = math.GetJudgement320HitWindow();
            double H300 = math.GetJudgement300HitWindow();
            double H200 = math.GetJudgement200HitWindow();
            double H100 = math.GetJudgement100HitWindow();
            double H50 = math.GetJudgement50HitWindow();

            double diff = Math.Abs(hitObject.SpawnTime - hitTime);
            HitObjectData hitObjectData = HitObjectManager.TransformHitObjectToDataObject(hitObject);
            if (hitObjectData.Judgement.Judgement == (int)HitObjectJudgement.Perfect || (diff <= H320 && diff >= -H320))
            {
                URBar.ShowHit(HitObjectJudgement.Perfect, hitObject.SpawnTime - hitTime);
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Perfect);
            }
            else if (hitObjectData.Judgement.Judgement == (int)HitObjectJudgement.Great || (diff <= H300 && diff >= -H300))
            {
                URBar.ShowHit(HitObjectJudgement.Great, hitObject.SpawnTime - hitTime);
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Great);
            }
            else if (hitObjectData.Judgement.Judgement == (int)HitObjectJudgement.Good || (diff <= H200 && diff >= -H200))
            {
                URBar.ShowHit(HitObjectJudgement.Good, hitObject.SpawnTime - hitTime);
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Good);
            }
            else if (hitObjectData.Judgement.Judgement == (int)HitObjectJudgement.Ok || (diff <= H100 && diff >= -H100))
            {
                URBar.ShowHit(HitObjectJudgement.Ok, hitObjectData.SpawnTime - hitTime);
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Ok);
            }
            else if (hitObjectData.Judgement.Judgement == (int)HitObjectJudgement.Meh || (diff <= H50 && diff >= -H50))
            {
                URBar.ShowHit(HitObjectJudgement.Meh, hitObject.SpawnTime - hitTime);
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Meh);
            }
            else if ((diff <= H50 && diff >= -H50))
            {
                HitJudgementManager.ApplyJudgement(hitObject, new Vector2(X, Y), hitTime, HitObjectJudgement.Miss);
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
