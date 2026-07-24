using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.SliderPathMath;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.HitObjects.Catch
{
    public class CatchJuiceStream : HitObject
    {
        public CatchJuiceStream(CatchJuiceStreamData juiceStreamData)
        {
            X = juiceStreamData.X;
            Y = juiceStreamData.Y;
            EndXPosition = juiceStreamData.EndXPosition;
            EndYPosition = juiceStreamData.EndYPosition;
            SpawnTime = juiceStreamData.SpawnTime;
            EndTime = juiceStreamData.EndTime;
            Drops = juiceStreamData.Drops;
            RepeatCount = juiceStreamData.RepeatCount;
            Path = juiceStreamData.Path;
            Judgement = new HitJudgement((HitObjectJudgement)juiceStreamData.Judgement.Judgement, juiceStreamData.Judgement.SpawnTime);
        }
        public int EndXPosition { get; set; }
        public int EndYPosition { get; set; }
        public double EndTime { get; set; }
        public int RepeatCount { get; set; }
        public SliderPath Path { get; set; }
        public List<JuiceStreamFruit> Droplets { get; set; } = new List<JuiceStreamFruit>();
        public List<SliderTick> Drops { get; set; } = new List<SliderTick>();

        public static CatchJuiceStream Create(CatchJuiceStreamData juiceStreamData, int index)
        {
            // this hit object wont return special preload object since it would be basically the same thing as this
            // and it preloaded crazy exgon map faster than i blinked anyway
            return CreateJuiceStream(juiceStreamData, index);
        }

        private static CatchJuiceStream CreateJuiceStream(CatchJuiceStreamData juiceStreamData, int index)
        {
            CatchJuiceStream juiceStream = new CatchJuiceStream(juiceStreamData);

            double diameter = MainWindow.OsuPlayfieldObjectDiameter;
            double scale = MainWindow.OsuPlayfieldObjectScale;

            // * 0.8 is coz of hyperdash outline taking 0.2 space
            JuiceStreamFruit head = CreateHead(juiceStream, diameter * 0.9, juiceStream.SpawnTime);
            juiceStream.Children.Add(head);

            double spawnTime = juiceStream.EndTime - juiceStream.SpawnTime;
            double Ypos = CatchPlayfield.Playfield.Height * (spawnTime / CatchPlayfield.ScrollSpeed);
            double Xpos = juiceStream.RepeatCount % 2 == 1 ? juiceStream.X + juiceStream.EndXPosition : juiceStream.X;

            double maxSliderHeight = Math.Abs(-Ypos - head.Width / 2);
            CreateSliderChildren(juiceStream, maxSliderHeight, diameter, juiceStreamData.Droplets);
            if (juiceStreamData.Droplets.Count == 0 && juiceStream.Droplets.Count > 0)
            {
                juiceStreamData.Droplets = juiceStream.Droplets.Cast<object>().ToList();
            }

            JuiceStreamFruit tail = CreateTail(juiceStream, diameter * 0.9, Xpos, Ypos, juiceStream.EndTime);
            juiceStream.Children.Add(tail);

            juiceStream.Name = $"CatchJuiceStreamObject{index}";

            return juiceStream;
        }

        private static JuiceStreamFruit CreateHead(CatchJuiceStream js, double diameter, double spawnTime)
        {
            JuiceStreamFruit fruitHeadImage = new JuiceStreamFruit(SkinElement.SkinElements.CatchFruitApple, (int)spawnTime, 0, js.X, diameter);
            fruitHeadImage.Name = "haed";

            Canvas.SetLeft(fruitHeadImage, js.X * MainWindow.OsuPlayfieldObjectScale - (diameter / 2));
            Canvas.SetTop(fruitHeadImage, 0);

            return fruitHeadImage;
        }

        private static JuiceStreamFruit CreateTail(CatchJuiceStream js, double diameter, double Xpos, double Ypos, double spawnTime)
        {
            JuiceStreamFruit fruitTailImage = new JuiceStreamFruit(SkinElement.SkinElements.CatchFruitApple, spawnTime, -Ypos, Xpos, diameter);
            fruitTailImage.Name = "tael";

            Canvas.SetLeft(fruitTailImage, Xpos * MainWindow.OsuPlayfieldObjectScale - (diameter / 2));
            Canvas.SetTop(fruitTailImage, -Ypos);

            return fruitTailImage;
        }

        private static void CreateSliderChildren(CatchJuiceStream juiceStream, double maxSliderHeight, double diameter, List<object> savedDroplets)
        {
            // good code taken from osu lazer and bad code is mine... should be obvious to know which is which?
            double reverseDuration = (juiceStream.EndTime - juiceStream.SpawnTime) / juiceStream.RepeatCount;
            double totalReverseDuration = juiceStream.RepeatCount * ((juiceStream.EndTime - juiceStream.SpawnTime) / juiceStream.RepeatCount);

            double finalSpanStartTime = juiceStream.SpawnTime + (juiceStream.RepeatCount - 1) * reverseDuration;

            double lastTickTime = Math.Max(juiceStream.SpawnTime + totalReverseDuration / 2, (finalSpanStartTime + reverseDuration) - 36);
            double lastTickProgress = (lastTickTime - finalSpanStartTime) / reverseDuration;
            if (juiceStream.RepeatCount % 2 == 0)
            {
                lastTickProgress = 1 - lastTickProgress;
            }

            if (juiceStream.SpawnTime == 90887)
            {

            }

            if (juiceStream.SpawnTime == 89828)
            {

            }


            if (juiceStream.SpawnTime ==90181)
             {

            }

            if (juiceStream.SpawnTime == 90534)
            {

            }

            // i have no clue what im doing < update: this but ^2
            bool useSavedDroplets = savedDroplets.Count > 0;
            bool dropletsSaved = false;
            double Xpos = 0;
            double Ypos = 0;
            int dropIndex = 0;
            bool isGoingToTail = true;
            double reverseArrowSpawn = reverseDuration;
            (int time, double prog) prevEvent = (juiceStream.SpawnTime, 0);
            (int time, double prog) currEvent = (0, 0);
            while (true)
            {
                if ((juiceStream.Drops == null && juiceStream.SpawnTime + reverseArrowSpawn < juiceStream.EndTime)
                ||  (juiceStream.Drops != null && dropIndex < juiceStream.Drops.Count 
                &&   juiceStream.SpawnTime + reverseArrowSpawn < juiceStream.Drops[dropIndex].Time))
                {
                    currEvent.time = juiceStream.SpawnTime + (int)reverseArrowSpawn;
                    currEvent.prog = isGoingToTail == true ? 1 : 0;
                }
                else if (juiceStream.Drops != null && dropIndex < juiceStream.Drops.Count)
                {
                    currEvent.time = (int)juiceStream.Drops[dropIndex].Time;
                    currEvent.prog = isGoingToTail == true 
                                   ? juiceStream.Drops[dropIndex].PositionAt
                                   : 1 - juiceStream.Drops[dropIndex].PositionAt;
                }
                else if (juiceStream.Drops == null || dropIndex >= juiceStream.Drops.Count)
                {
                    currEvent.time = (int)lastTickTime;
                    currEvent.prog = lastTickProgress;
                }

                // something here doesnt work with Y positioning on specific sliders pain
                if (useSavedDroplets == true && dropletsSaved == false)
                {
                    for (int i = 0; i < savedDroplets.Count; i++)
                    {
                        JuiceStreamFruit? droplet = savedDroplets[i] as JuiceStreamFruit;
                        if (droplet.Visibility == Visibility.Collapsed)
                        {   // when its catched/missed it becomes collapsed, and since this element is reused this needs to be reset
                            // when everything works i will change it coz saving it like i do is just waste of ram
                            droplet.Visibility = Visibility.Visible;
                        }
                    
                        // need to detach it from parent otherwise app buhFlipExplode
                        CatchJuiceStream? parent = droplet.Parent as CatchJuiceStream;
                        if (parent != null)
                        {
                            parent.Children.Remove(droplet);
                        }

                        Canvas.SetLeft(droplet, droplet.XPos * MainWindow.OsuPlayfieldObjectScale);
                        Canvas.SetTop(droplet, droplet.YPos);

                        if (i == savedDroplets.Count - 1)
                        {
                            Canvas.SetLeft(droplet, droplet.XPos * MainWindow.OsuPlayfieldObjectScale);
                            Canvas.SetTop(droplet, droplet.YPos + (droplet.Width / 2));
                        }
                        

                        // either im stupid or catch game mode is stupid...
                        //if (juiceStream.Y < juiceStream.EndYPosition)
                        //{
                        //    //if (i == 0)
                        //    //{
                        //    //    Canvas.SetLeft(droplet, (droplet.XPos * MainWindow.OsuPlayfieldObjectScale) - droplet.Width / 2);
                        //    //    Canvas.SetTop(droplet, droplet.YPos);
                        //    //}
                        //    //else
                        //    {
                        //        Canvas.SetLeft(droplet, (droplet.XPos * MainWindow.OsuPlayfieldObjectScale) - droplet.Width / 2);
                        //        Canvas.SetTop(droplet, droplet.YPos - ((diameter  / 2)));
                        //    }
                        //        
                        //}
                        //else
                        //{
                        //    //if (i == 0)  
                        //    //{
                        //    //    Canvas.SetLeft(droplet, (droplet.XPos * MainWindow.OsuPlayfieldObjectScale) - droplet.Width / 2);
                        //    //    Canvas.SetTop(droplet, (droplet.YPos + (diameter / 2)) + droplet.Width / 2);
                        //    //}
                        //    //else
                        //    {
                        //        Canvas.SetLeft(droplet, (droplet.XPos * MainWindow.OsuPlayfieldObjectScale) - droplet.Width / 2);
                        //        Canvas.SetTop(droplet, droplet.YPos + ((diameter / 2)));
                        //    }
                        //}

                        juiceStream.Children.Add(droplet);
                    }
                    
                    dropletsSaved = true;
                }
                else if (dropletsSaved == false)
                {
                    double sinceLastTick2 = currEvent.time - prevEvent.time;
                    if (sinceLastTick2 > 80)
                    {
                        double timeBetweenTiny = sinceLastTick2;
                        while (timeBetweenTiny > 100)
                        {
                            timeBetweenTiny = timeBetweenTiny / 2;
                        }

                        // on this one der wald part the droplets are put in reverse Y order which annoys the shit out of me
                        // dont understand why or how this even works
                        for (double i = timeBetweenTiny; i < sinceLastTick2; i += timeBetweenTiny)
                        {
                            double currProg = prevEvent.prog + (i / sinceLastTick2) * (currEvent.prog - prevEvent.prog);

                            var a = juiceStream.X;
                            var b = juiceStream.EndXPosition;
                            if (juiceStream.SpawnTime == 57988)
                            {

                            }
                            var aaa = Math.Abs(juiceStream.EndXPosition - juiceStream.X);
                            int spawnTime = 0;
                            // something with this end position? < yes
                            // HOW THE HELL DO YOU MAKE THIS VISUALLY CORRECT THIS IS SO ANNOYING
                            if (juiceStream.Y < juiceStream.EndYPosition)
                            {
                                Ypos = (maxSliderHeight * (Math.Abs(juiceStream.SpawnTime - (currEvent.time - i)) / (juiceStream.EndTime - juiceStream.SpawnTime))) - diameter / 2;
                                //Ypos = maxSliderHeight * (juiceStream.SpawnTime - (currEvent.time - i) / (juiceStream.EndTime - juiceStream.SpawnTime));
                                spawnTime = (int)(currEvent.time - i);
                            }
                            else
                            {
                                Ypos = (maxSliderHeight * (Math.Abs(juiceStream.SpawnTime - (prevEvent.time + i)) / (juiceStream.EndTime - juiceStream.SpawnTime))) - diameter / 2;
                                //Ypos = maxSliderHeight * (juiceStream.SpawnTime - (prevEvent.time + i) / (juiceStream.EndTime - juiceStream.SpawnTime));
                                spawnTime = (int)(prevEvent.time + i);
                            }

                            //Ypos = (maxSliderHeight * currProg) - ((diameter / 2) - (diameter * 0.4 / 2));
                            //Ypos = (maxSliderHeight * ((juiceStream.EndTime - spawnTime) / (juiceStream.EndTime - juiceStream.SpawnTime))) - diameter * 1.8 / 2;
                            //Ypos = (maxSliderHeight * (Math.Abs(juiceStream.SpawnTime - (currEvent.time - i)) / (juiceStream.EndTime - juiceStream.SpawnTime))) - diameter / 2;
                            //Ypos = (maxSliderHeight * currProg) - diameter / 2;
                            //spawnTime = (int)(currEvent.time - i);

                            float pos = (float)juiceStream.X + juiceStream.Path.PositionAt(currProg).X;
                            float offset = Math.Clamp(CatchRNG.Next(-20, 20), -pos, 512 - pos);
                            Xpos = pos + offset;

                            JuiceStreamFruit droplet = new JuiceStreamFruit(SkinElement.SkinElements.CatchFruitDrop, spawnTime, -Ypos, Xpos, diameter * 0.4);
                            droplet.Name = "dwoplet";

                            Canvas.SetLeft(droplet, Xpos);
                            Canvas.SetTop(droplet, -Ypos);

                            juiceStream.Children.Add(droplet);
                            savedDroplets.Add(droplet);
                        }
                    }
                }
                   
                if ((juiceStream.Drops == null && juiceStream.SpawnTime + reverseArrowSpawn < juiceStream.EndTime)
                ||  (juiceStream.Drops != null && dropIndex < juiceStream.Drops.Count
                &&   juiceStream.SpawnTime + reverseArrowSpawn < juiceStream.Drops[dropIndex].Time))
                {
                    Ypos = maxSliderHeight * (reverseArrowSpawn / (juiceStream.EndTime - juiceStream.SpawnTime));
                    Xpos = juiceStream.X + juiceStream.Path.PositionAt(currEvent.prog).X;

                    JuiceStreamFruit repeat = new JuiceStreamFruit(SkinElement.SkinElements.CatchFruitApple, (int)(juiceStream.SpawnTime + reverseArrowSpawn), -Ypos, Xpos, diameter * 0.9);
                    repeat.Name = "repet";

                    Canvas.SetLeft(repeat, Xpos * MainWindow.OsuPlayfieldObjectScale - (repeat.Width / 2));
                    Canvas.SetTop(repeat, -Ypos + (diameter / 2));

                    juiceStream.Children.Add(repeat);

                    double prog = isGoingToTail == true ? 1 : 0;
                    prevEvent = (juiceStream.SpawnTime + (int)reverseArrowSpawn, prog);

                    isGoingToTail = !isGoingToTail;
                    reverseArrowSpawn += reverseDuration;
                    continue;
                }

                if (juiceStream.Drops != null && dropIndex < juiceStream.Drops.Count)
                {
                    CatchRNG.Next(); // from lazer code "osu!stable retrieved a random droplet rotation"

                    Ypos = maxSliderHeight * (Math.Abs(juiceStream.SpawnTime - currEvent.time) / (juiceStream.EndTime - juiceStream.SpawnTime));
                    Xpos = juiceStream.X + juiceStream.Path.PositionAt(currEvent.prog).X;

                    JuiceStreamFruit drop = new JuiceStreamFruit(SkinElement.SkinElements.CatchFruitDrop, currEvent.time, -Ypos, Xpos, diameter * 0.6);
                    drop.Name = "dwop";

                    Canvas.SetLeft(drop, Xpos * MainWindow.OsuPlayfieldObjectScale - (drop.Width / 2));
                    Canvas.SetTop(drop, -Ypos + (diameter / 2));

                    juiceStream.Children.Add(drop);

                    dropIndex++;
                    prevEvent = currEvent;
                }
                else
                {
                    break;
                }
            }
        }

        public static JuiceStreamFruit Tail(Canvas juiceStream) => juiceStream.Children[juiceStream.Children.Count - 1] as JuiceStreamFruit;

        // custom class for mainly spawn time for correct hit judgements
        public class JuiceStreamFruit : Image
        {
            public double XPos = 0;
            public double YPos = 0;
            public double SpawnTime = 0;
            public int XOffset = 0;
            public bool IsMissed = false;

            public JuiceStreamFruit(SkinElement.SkinElements element, double spawnTime, double Ypos, double Xpos, double diameter)
            {
                Source = SkinElement.GetElement(element);
                SpawnTime = spawnTime;
                XPos = Xpos;
                YPos = Ypos;
                Width = diameter;
            }
        }
    }
}
