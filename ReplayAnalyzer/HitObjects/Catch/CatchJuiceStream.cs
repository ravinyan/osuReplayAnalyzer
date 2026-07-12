using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.SliderPathMath;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows.Controls;

namespace ReplayAnalyzer.HitObjects.Catch
{
    public class CatchJuiceStream : HitObject
    {
        public CatchJuiceStream(CatchJuiceStreamData juiceStreamData)
        {
            X = juiceStreamData.X;
            EndXPosition = juiceStreamData.EndXPosition;
            SpawnTime = juiceStreamData.SpawnTime;
            EndTime = juiceStreamData.EndTime;
            Drops = juiceStreamData.Drops;
            RepeatCount = juiceStreamData.RepeatCount;
            Path = juiceStreamData.Path;
            Judgement = new HitJudgement((HitObjectJudgement)juiceStreamData.Judgement.Judgement, juiceStreamData.Judgement.SpawnTime);
        }

        public int RepeatCount { get; set; }
        public double EndTime { get; set; }
        public int EndXPosition { get; set; }
        public SliderPath Path { get; set; }
        public List<SliderTick> Drops { get; set; } = new List<SliderTick>();

        public static CatchJuiceStream Create(CatchJuiceStreamData juiceStreamData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateJuiceStream(juiceStreamData, index);
            }

            return CreateJuiceStreamPreload(juiceStreamData, index);
        }

        // clean up after i figure out how to do this why this is so complicated for no reason LOL
        // i will never figure this out life is suffering and i refuse to just copy osu lazer code that would be boring
        // and made me sad that i couldnt solve this... life is pain dayo

        // I GIVE UP ON THIS until i finish literally everything else... there is small chance that it will work perfectly even it
        // it is not correct visually
        private static CatchJuiceStream CreateJuiceStream(CatchJuiceStreamData juiceStreamData, int index)
        {
            CatchJuiceStream juiceStream = new CatchJuiceStream(juiceStreamData);

            double diameter = MainWindow.OsuPlayfieldObjectDiameter;
            double scale = MainWindow.OsuPlayfieldObjectScale;

            // * 0.8 is coz of hyperdash outline taking 0.2 space
            Image head = CreateHead(diameter * 0.9);
            juiceStream.Children.Add(head);

            double spawnTime = juiceStream.EndTime - juiceStream.SpawnTime;
            double Ypos = CatchPlayfield.Playfield.Height * (spawnTime / CatchPlayfield.ScrollSpeed);
            int Xpos = juiceStream.RepeatCount % 2 == 1 ? juiceStream.EndXPosition : 0;

            double maxSliderHeight = Math.Abs(-Ypos - head.Width / 2);
            CreateSliderChildren(juiceStream, maxSliderHeight, diameter);

            Image tail = CreateTail(diameter * 0.9, Xpos, Ypos);
            juiceStream.Children.Add(tail);

            Canvas.SetLeft(juiceStream, (juiceStream.X * scale) - diameter / 2);
            Canvas.SetTop(juiceStream, 0);
            Canvas.SetZIndex(juiceStream, -1);

            juiceStream.Name = $"CatchJuiceStreamObject{index}";

            return juiceStream;
        }

        private static CatchJuiceStream CreateJuiceStreamPreload(CatchJuiceStreamData juiceStreamData, int index)
        {
            CatchJuiceStream juiceStream = new CatchJuiceStream(juiceStreamData);

            Image juiceStreamImage = new Image();
            juiceStream.Children.Add(juiceStreamImage);

            Canvas.SetLeft(juiceStream, 100);
            Canvas.SetTop(juiceStream, 0);

            juiceStream.Name = $"CatchJuiceStreamObject{index}";

            return juiceStream;
        }

        private static Image CreateHead(double diameter)
        {
            Image fruitHeadImage = new Image();
            fruitHeadImage.Name = "haed";
            fruitHeadImage.Width = diameter;
            fruitHeadImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);
            Canvas.SetLeft(fruitHeadImage, 0);
            Canvas.SetTop(fruitHeadImage, 0);

            return fruitHeadImage;
        }

        private static Image CreateTail(double diameter, double Xpos, double Ypos)
        {
            Image fruitTailImage = new Image();
            fruitTailImage.Name = "tael";
            fruitTailImage.Width = diameter;
            fruitTailImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);

            Canvas.SetLeft(fruitTailImage, Xpos);
            Canvas.SetTop(fruitTailImage, -Ypos);

            return fruitTailImage;
        }

        // why catch sliders cant be created normally? why droplets have small offset from center? why every single piece
        // needs different math to be positioned correctly? why in some sliders droplets are correctly placed but in others they are not?
        // why tail on some sliders is placed correctly but using same math it is not correct anymore on other sliders?
        // at this point i wonder if im masochist for even trying to understand this without just copy pasting lazer code
        // which wouldnt work anyway coz every single thing is calculated on other side of the universe instead of using one class
        // for creating whole object... im losing my mind or maybe i have already lost it
        // wait a second WHY POSITIONS OF HEAD AND TAIL IS SLIGHTLY DIFFERENT IN GAMEPLAY AND IN EDITOR??????
        // ^ all of this might not matter... oops
        private static void CreateSliderChildren(CatchJuiceStream juiceStream, double maxSliderHeight, double diameter)
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
                        Image droplet = new Image();
                        droplet.Name = "dwoplet";
                        droplet.Width = diameter * 0.4;
                        droplet.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitDrop);

                        double currProg = prevEvent.prog + (i / sinceLastTick2) * (currEvent.prog - prevEvent.prog);
                        Ypos = maxSliderHeight * (Math.Abs(juiceStream.SpawnTime - (currEvent.time - i)) / (juiceStream.EndTime - juiceStream.SpawnTime));
                        Xpos = juiceStream.Path.PositionAt(currProg).X;

                        Canvas.SetLeft(droplet, Xpos + (diameter / 2 - droplet.Width / 2));
                        Canvas.SetTop(droplet, -Ypos + (diameter / 2 - droplet.Width / 2));

                        juiceStream.Children.Add(droplet);
                    }
                }

                if ((juiceStream.Drops == null && juiceStream.SpawnTime + reverseArrowSpawn < juiceStream.EndTime)
                ||  (juiceStream.Drops != null && dropIndex < juiceStream.Drops.Count
                &&   juiceStream.SpawnTime + reverseArrowSpawn < juiceStream.Drops[dropIndex].Time))
                {
                    Image repeat = new Image();
                    repeat.Name = "repet";
                    repeat.Width = diameter * 0.9;
                    repeat.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);

                    Ypos = maxSliderHeight * (reverseArrowSpawn / (juiceStream.EndTime - juiceStream.SpawnTime));
                    Xpos = juiceStream.Path.PositionAt(currEvent.prog).X;
                    Canvas.SetLeft(repeat, Xpos);
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
                    Image drop = new Image();
                    drop.Name = "dwop";
                    drop.Width = diameter * 0.6;
                    drop.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitDrop);

                    Ypos = maxSliderHeight * (Math.Abs(juiceStream.SpawnTime - currEvent.time) / (juiceStream.EndTime - juiceStream.SpawnTime));
                    Xpos = juiceStream.Path.PositionAt(currEvent.prog).X;
                    Canvas.SetLeft(drop, Xpos + (diameter / 2 - drop.Width / 2));
                    Canvas.SetTop(drop, -Ypos + (diameter / 2 - drop.Width / 2));

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
    }
}
