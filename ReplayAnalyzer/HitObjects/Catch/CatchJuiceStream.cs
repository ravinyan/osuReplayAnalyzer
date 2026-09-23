using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.SliderPathMath;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.HitObjects.Catch
{
    public class CatchJuiceStream : HitObject
    {
        public CatchJuiceStream(CatchJuiceStreamData juiceStreamData)
        {
            ObjectIndex = juiceStreamData.ObjectIndex;
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

            double scale = MainWindow.OsuPlayfieldObjectScale;

            JuiceStreamFruit head = CreateHead(juiceStream, juiceStream.SpawnTime, index);

            Image headOverlay = new Image();
            headOverlay.Width = CatchPlayfield.FruitDiameter;
  
            juiceStream.Children.Add(head);

            double spawnTime = juiceStream.EndTime - juiceStream.SpawnTime;
            double Ypos = CatchPlayfield.Playfield.Height * (spawnTime / CatchPlayfield.ScrollSpeed);
            double Xpos = juiceStream.RepeatCount % 2 == 1 ? juiceStream.X + juiceStream.EndXPosition : juiceStream.X;

            double maxSliderHeight = Math.Abs(-Ypos - head.Width / 2);
            CreateSliderChildren(juiceStream, maxSliderHeight, juiceStreamData.Droplets, index);
            if (juiceStreamData.Droplets.Count == 0 && juiceStream.Droplets.Count > 0)
            {
                juiceStreamData.Droplets = juiceStream.Droplets.Cast<object>().ToList();
            }

            JuiceStreamFruit tail = CreateTail(juiceStream, Xpos, Ypos, juiceStream.EndTime, index);
            juiceStream.Children.Add(tail);

            Canvas.SetTop(juiceStream, -999);

            juiceStream.Name = $"CatchJuiceStreamObject{juiceStream.ObjectIndex}";

            return juiceStream;
        }

        private static JuiceStreamFruit CreateHead(CatchJuiceStream js, double spawnTime, int index)
        {
            JuiceStreamFruit fruitHeadImage = new JuiceStreamFruit(CatchFruit.GetFruitSkinElement(index), CatchFruit.GetFruitOverlaySkinElement(index)
                                                                 ,(int)spawnTime, 0, js.X, CatchPlayfield.FruitDiameter);
            fruitHeadImage.Name = "haed";

            Canvas.SetLeft(fruitHeadImage, js.X * MainWindow.OsuPlayfieldObjectScale - (fruitHeadImage.Width / 2));
            Canvas.SetTop(fruitHeadImage, 0);

            return fruitHeadImage;
        }

        private static JuiceStreamFruit CreateTail(CatchJuiceStream js, double Xpos, double Ypos, double spawnTime, int index)
        {
            JuiceStreamFruit fruitTailImage = new JuiceStreamFruit(CatchFruit.GetFruitSkinElement(index), CatchFruit.GetFruitOverlaySkinElement(index)
                                                                  ,spawnTime, -Ypos, Xpos, CatchPlayfield.FruitDiameter);
            fruitTailImage.Name = "tael";

            Canvas.SetLeft(fruitTailImage, Xpos * MainWindow.OsuPlayfieldObjectScale - (fruitTailImage.Width / 2));
            Canvas.SetTop(fruitTailImage, -Ypos);

            return fruitTailImage;
        }

        private static void CreateSliderChildren(CatchJuiceStream juiceStream, double maxSliderHeight, List<object> savedDroplets, int index)
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

                if (useSavedDroplets == true && dropletsSaved == false)
                {
                    for (int i = savedDroplets.Count - 1; i >= 0; i--)
                    {
                        JuiceStreamFruit? droplet = savedDroplets[i] as JuiceStreamFruit;
                        if (droplet.Visibility == Visibility.Collapsed)
                        {
                            droplet.Visibility = Visibility.Visible;
                        }
                    
                        // need to detach it from parent otherwise app buhFlipExplode
                        CatchJuiceStream? parent = droplet.Parent as CatchJuiceStream;
                        if (parent != null)
                        {
                            parent.Children.Remove(droplet);
                        }

                        double Y = CatchPlayfield.Playfield.Height * ((droplet.SpawnTime - juiceStream.SpawnTime) / CatchPlayfield.ScrollSpeed);
                        droplet.Width = CatchPlayfield.DropletDiameter;
                        Canvas.SetLeft(droplet, droplet.XPos * MainWindow.OsuPlayfieldObjectScale - (droplet.Width / 2));
                        Canvas.SetTop(droplet, -Y + ((CatchPlayfield.FruitDiameter / 2) - (droplet.Width / 2)));

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

                        for (double i = timeBetweenTiny; i < sinceLastTick2; i += timeBetweenTiny)
                        {
                            int spawnTime = 0;
                            if (juiceStream.Y < juiceStream.EndYPosition)
                            {
                                Ypos = (maxSliderHeight * (Math.Abs(juiceStream.SpawnTime - (currEvent.time - i)) / (juiceStream.EndTime - juiceStream.SpawnTime)));
                                spawnTime = (int)(currEvent.time - i);
                            }
                            else
                            {
                                Ypos = (maxSliderHeight * (Math.Abs(juiceStream.SpawnTime - (prevEvent.time + i)) / (juiceStream.EndTime - juiceStream.SpawnTime)));
                                spawnTime = (int)(prevEvent.time + i);
                            }

                            double currProg = prevEvent.prog + (i / sinceLastTick2) * (currEvent.prog - prevEvent.prog);
                            float pos = (float)juiceStream.X + juiceStream.Path.PositionAt(currProg).X;
                            float offset = Math.Clamp(CatchRNG.Next(-20, 20), -pos, 512 - pos);
                            Xpos = pos + offset;

                            JuiceStreamFruit droplet = new JuiceStreamFruit(CatchFruit.GetFruitSkinElement(-1), CatchFruit.GetFruitOverlaySkinElement(-1)
                                                                           ,spawnTime, -Ypos, Xpos, CatchPlayfield.DropletDiameter);
                            droplet.Name = "dwoplet";

                            juiceStream.Children.Add(droplet);
                            savedDroplets.Add(droplet);
                        }
                    }
                }
                   
                if ((juiceStream.Drops == null && juiceStream.SpawnTime + reverseArrowSpawn < juiceStream.EndTime)
                ||  (juiceStream.Drops != null && dropIndex < juiceStream.Drops.Count
                &&   juiceStream.SpawnTime + reverseArrowSpawn < juiceStream.Drops[dropIndex].Time))
                {
                    Ypos = CatchPlayfield.Playfield.Height * (reverseArrowSpawn / CatchPlayfield.ScrollSpeed);
                    Xpos = juiceStream.X + juiceStream.Path.PositionAt(currEvent.prog).X;

                    JuiceStreamFruit repeat = new JuiceStreamFruit(CatchFruit.GetFruitSkinElement(index), CatchFruit.GetFruitOverlaySkinElement(index)
                                                                 ,(int)(juiceStream.SpawnTime + reverseArrowSpawn), -Ypos, Xpos, CatchPlayfield.FruitDiameter);
                    repeat.Name = "repet";

                    Canvas.SetLeft(repeat, Xpos * MainWindow.OsuPlayfieldObjectScale - (repeat.Width / 2));
                    Canvas.SetTop(repeat, -Ypos);

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

                    Ypos = CatchPlayfield.Playfield.Height * ((currEvent.time - juiceStream.SpawnTime) / CatchPlayfield.ScrollSpeed);
                    Xpos = juiceStream.X + juiceStream.Path.PositionAt(currEvent.prog).X;

                    JuiceStreamFruit drop = new JuiceStreamFruit(CatchFruit.GetFruitSkinElement(-1), CatchFruit.GetFruitOverlaySkinElement(-1)
                                                                ,currEvent.time, -Ypos, Xpos, CatchPlayfield.DropDiameter);
                    drop.Name = "dwop";

                    Canvas.SetLeft(drop, Xpos * MainWindow.OsuPlayfieldObjectScale - (drop.Width / 2));
                    Canvas.SetTop(drop, -Ypos + ((CatchPlayfield.FruitDiameter / 2) - (drop.Width / 2)));

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
        public class JuiceStreamFruit : Canvas
        {
            public double XPos = 0;
            public double YPos = 0;
            public double SpawnTime = 0;
            public int XOffset = 0;
            public bool IsMissed = false;

            public Image SkinElement { get; private set; } = new Image();
            public Image SkinElementOverlay { get; private set; } = new Image();

            public JuiceStreamFruit(BitmapSource skin, BitmapSource skinOverlay, double spawnTime, double Ypos, double Xpos, double diameter)
            {
                SkinElement.Width = diameter;
                SkinElement.Source = skin;
                SkinElementOverlay.Width = diameter;
                SkinElementOverlay.Source = skinOverlay;
                this.Children.Add(SkinElement);
                this.Children.Add(SkinElementOverlay);

                SpawnTime = spawnTime;
                XPos = Xpos;
                YPos = Ypos;
                Width = diameter;
            }
        }
    }
}
