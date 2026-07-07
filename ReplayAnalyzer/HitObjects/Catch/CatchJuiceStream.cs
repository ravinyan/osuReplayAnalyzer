using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.SliderPathMath;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System;
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
        private static CatchJuiceStream CreateJuiceStream(CatchJuiceStreamData juiceStreamData, int index)
        {
            CatchJuiceStream juiceStream = new CatchJuiceStream(juiceStreamData);

            double diameter = MainWindow.OsuPlayfieldObjectDiameter;
            double scale = MainWindow.OsuPlayfieldObjectScale;

            double h = CatchPlayfield.Playfield.Height;
            double spawnTime = 0;
            double Ypos = 0;

            spawnTime = juiceStream.EndTime - juiceStream.SpawnTime;
            Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);

            juiceStream.Children.Add(CreateHead(diameter));
            juiceStream.Children.Add(CreateTail(diameter, juiceStream.EndXPosition, Ypos));

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
            int brainDamage = 0;
            (int time, double prog) prevEvent = (juiceStream.SpawnTime, 0);
            (int time, double prog) currEvent = (0, 0);
            while (true)
            {
                if (juiceStream.Drops != null && brainDamage < juiceStream.Drops.Count)
                {
                    currEvent.time = (int)juiceStream.Drops[brainDamage].Time;
                    currEvent.prog = juiceStream.Drops[brainDamage].PositionAt;
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
                        spawnTime = juiceStream.EndTime - (i + prevEvent.time);
                        Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);

                        double Xpos = juiceStream.Path.PositionAt(prevEvent.prog + (i / (double)sinceLastTick2) * (currEvent.prog - prevEvent.prog)).X;
                        Canvas.SetLeft(droplet, Xpos - diameter / 2);
                        Canvas.SetTop(droplet, -Ypos + diameter / 2);

                        juiceStream.Children.Add(droplet);
                    }
                }

                if (juiceStream.Drops != null && brainDamage < juiceStream.Drops.Count)
                {
                    Image dropImage = new Image();
                    dropImage.Name = "dwop";
                    dropImage.Width = diameter * 0.7;
                    dropImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitDrop);

                    spawnTime = juiceStream.EndTime - juiceStream.Drops[brainDamage].Time;
                    Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);
                    double X = juiceStream.Path.PositionAt(juiceStream.Drops[brainDamage].PositionAt).X;

                    // make this tick position correct then droplets and then its done... BUT OF COURSE IT CANT BE EASY
                    // LIKE USING  X * scale - diameter / 2 FORUMLA WHICH WORKED EVERYWHERE AAAAA pain
                    Canvas.SetLeft(dropImage, 0);
                    Canvas.SetTop(dropImage, -Ypos * scale);
                    juiceStream.Children.Add(dropImage);

                    brainDamage++;
                    prevEvent = currEvent;
                }
                else
                {
                    break;
                }
            }
            

            Canvas.SetLeft(juiceStream, (juiceStream.X * scale) - diameter / 2);
            Canvas.SetTop(juiceStream, 0);
            Canvas.SetZIndex(juiceStream, -1);


            //Path p = new Path();
            //p.StrokeThickness = 2;
            //p.Stroke = Brushes.Red;
            //
            //LineGeometry myLineGeometry = new LineGeometry();
            //myLineGeometry.StartPoint = new Point(Canvas.GetLeft(fruitHeadImage) + 35, Canvas.GetTop(fruitHeadImage) + 35);
            //myLineGeometry.EndPoint = new Point(Canvas.GetLeft(fruitTailImage) + 35, Canvas.GetTop(fruitTailImage) + 35);
            //myLineGeometry.Freeze();
            //
            //p.Data = myLineGeometry;
            //juiceStream.Children.Add(p);

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

        private static void CreateDropsAndDroplets()
        {

        }
    }
}
