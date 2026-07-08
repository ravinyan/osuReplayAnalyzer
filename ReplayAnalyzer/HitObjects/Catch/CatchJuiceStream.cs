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
        private static CatchJuiceStream CreateJuiceStream(CatchJuiceStreamData juiceStreamData, int index)
        {
            CatchJuiceStream juiceStream = new CatchJuiceStream(juiceStreamData);

            double diameter = MainWindow.OsuPlayfieldObjectDiameter;
            double scale = MainWindow.OsuPlayfieldObjectScale;

            // * 0.8 is coz of hyperdash outline taking 0.2 space
            Image head = CreateHead(diameter * 0.8);
            juiceStream.Children.Add(head);

            double spawnTime = juiceStream.EndTime - juiceStream.SpawnTime;
            double Ypos = CatchPlayfield.Playfield.Height * (spawnTime / ManiaPlayfield.ScrollSpeed);
            Image tail = CreateTail(diameter * 0.8, juiceStream.EndXPosition, Ypos);
            juiceStream.Children.Add(tail);

            CreateSliderChildren(juiceStream, tail, diameter);
            
            if (juiceStream.Drops != null && juiceStream.Drops.Count > 0)
            {
                double absoluteTickProgressBase = juiceStream.EndTime - juiceStream.SpawnTime / juiceStream.Drops.Count;
                double absoluteTickProgress = absoluteTickProgressBase;
                for (int i = 0; i < juiceStream.Drops.Count; i++)
                {
                    SliderTick drop = juiceStream.Drops[i];
                    drop.PositionAt = absoluteTickProgress;

                }
            }

            // fruit > droplet > drop > droplet > fruit > droplet > drop > droplet > fruit

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

        private static void CreateSliderChildren(CatchJuiceStream juiceStream, Image tail, double diameter)
        {
            // good code taken from osu lazer and bad code is mine... should be obvious to know which is which?
            double reverseDuration = juiceStream.EndTime - juiceStream.SpawnTime;
            double totalReverseDuration = juiceStream.RepeatCount * ((juiceStream.EndTime - juiceStream.SpawnTime) / juiceStream.RepeatCount);

            double finalSpanStartTime = juiceStream.SpawnTime + (juiceStream.RepeatCount - 1) * reverseDuration;

            double lastTickTime = Math.Max(juiceStream.SpawnTime + totalReverseDuration / 2, (finalSpanStartTime + reverseDuration) - 36);
            double lastTickProgress = (lastTickTime - finalSpanStartTime) / reverseDuration;
            if (juiceStream.RepeatCount % 2 == 0)
            {
                lastTickProgress = 1 - lastTickProgress;
            }

            // i have no clue what im doing
            int dropIndex = 0;
            double reverseArrowIndex = juiceStream.RepeatCount;
            //double reverseArrowProgBase = 1 / ((double)juiceStream.RepeatCount);
            double aaa = juiceStream.EndTime - juiceStream.SpawnTime;
            double reverseArrowSpawnBase = aaa / (juiceStream.RepeatCount - 1);
            double reverseArrowSpawn = reverseArrowSpawnBase;
            (int time, double prog) prevEvent = (juiceStream.SpawnTime, 0);
            (int time, double prog) currEvent = (0, 0);
            while (true)
            {
                if (juiceStream.Drops != null && dropIndex < juiceStream.Drops.Count)
                {
                    currEvent.time = (int)juiceStream.Drops[dropIndex].Time;
                    currEvent.prog = juiceStream.Drops[dropIndex].PositionAt;
                }
                else
                {
                    currEvent.time = (int)lastTickTime;
                    currEvent.prog = lastTickProgress;
                }

                int sinceLastTick2 = currEvent.time - prevEvent.time;
                if (sinceLastTick2 > 80)
                {
                    int timeBetweenTiny = sinceLastTick2;
                    while (timeBetweenTiny > 100)
                    {
                        timeBetweenTiny = timeBetweenTiny / 2;
                    }

                    for (int i = timeBetweenTiny; i < sinceLastTick2; i += timeBetweenTiny)
                    {
                        Image droplet = new Image();
                        droplet.Width = diameter * 0.3;
                        droplet.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitDrop);

                        double spawnTime = juiceStream.EndTime - (i + prevEvent.time);

                        double currProg = prevEvent.prog + (i / (double)sinceLastTick2) * (currEvent.prog - prevEvent.prog);
                        double Ypos = Math.Abs(Canvas.GetTop(tail) - tail.Width / 2) * currProg;

                        double middla = diameter / 2 - droplet.Width / 2;
                        double Xpos = juiceStream.Path.PositionAt(currProg).X;

                        Canvas.SetLeft(droplet, Xpos + middla);
                        Canvas.SetTop(droplet, -Ypos + middla);

                        juiceStream.Children.Add(droplet);
                    }
                }

                if (juiceStream.RepeatCount > 1 && dropIndex < juiceStream.Drops.Count
                &&  juiceStream.SpawnTime + reverseArrowSpawn < juiceStream.Drops[dropIndex].Time)
                {
                    Image repeat = new Image();
                    repeat.Width = diameter * 0.8;
                    repeat.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);

                    double Ypos = Math.Abs(Canvas.GetTop(tail) - tail.Width / 2) * currEvent.prog;

                    double middla = diameter / 2 - repeat.Width / 2;
                    double Xpos = juiceStream.Path.PositionAt(currEvent.prog).X;

                    Canvas.SetLeft(repeat, Xpos + middla);
                    Canvas.SetTop(repeat, -Ypos + middla);

                    reverseArrowSpawn += reverseArrowSpawnBase;
                    double fullDuration = juiceStream.EndTime - juiceStream.SpawnTime;

                    prevEvent = ((int)reverseArrowSpawn, 1);
                    continue;
                }

                if (juiceStream.Drops != null && dropIndex < juiceStream.Drops.Count)
                {
                    Image drop = new Image();
                    drop.Name = "dwop";
                    drop.Width = diameter * 0.6;
                    drop.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitDrop);

                    double Ypos = Math.Abs(Canvas.GetTop(tail) - tail.Width / 2) * currEvent.prog;

                    double middla = diameter / 2 - drop.Width / 2;
                    double Xpos = juiceStream.Path.PositionAt(currEvent.prog).X;

                    Canvas.SetLeft(drop, Xpos + middla);
                    Canvas.SetTop(drop, -Ypos + middla);
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
