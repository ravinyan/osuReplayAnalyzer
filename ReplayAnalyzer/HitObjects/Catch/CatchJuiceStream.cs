using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.SliderPathMath;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Threading;
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
        public List<SliderTick> Drops { get; set; }

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
            juiceStream.Width = 1;
            juiceStream.Height = 1;

            Image fruitHeadImage = new Image();
            fruitHeadImage.Name = "haed";
            fruitHeadImage.Width = MainWindow.OsuPlayfieldObjectDiameter; // based on CS
            fruitHeadImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);
            Canvas.SetLeft(fruitHeadImage, -fruitHeadImage.Width / 2);
            Canvas.SetTop(fruitHeadImage, 0);
            juiceStream.Children.Add(fruitHeadImage);

            double h = CatchPlayfield.Playfield.Height;
            double spawnTime = 0;
            double Ypos = 0;

            // taken from osu lazer
            double reverseDuration = juiceStream.EndTime - juiceStream.SpawnTime;
            double totalReverseDuration = juiceStream.RepeatCount * ((juiceStream.EndTime - juiceStream.SpawnTime) / juiceStream.RepeatCount);

            double finalSpanStartTime = juiceStream.SpawnTime + (juiceStream.RepeatCount - 1) * reverseDuration;

            double lastTickTime = Math.Max(juiceStream.SpawnTime + totalReverseDuration / 2, (finalSpanStartTime + reverseDuration) - 36);
            double lastTickProgress = (lastTickTime - finalSpanStartTime) / reverseDuration;

            if (juiceStream.RepeatCount % 2 == 0)
            {
                lastTickProgress = 1 - lastTickProgress;
            }
            int sinceLastTick = (int)lastTickTime - (int)juiceStream.SpawnTime;
            if (sinceLastTick > 80)
            {
                int timeBetweenTiny = sinceLastTick;
                while (timeBetweenTiny > 100)
                {
                    timeBetweenTiny = timeBetweenTiny / 2;
                }

                for (int i = timeBetweenTiny; i < sinceLastTick; i += timeBetweenTiny)
                {
                    Image droplet = new Image();
                    droplet.Width = MainWindow.OsuPlayfieldObjectDiameter * 0.5;
                    droplet.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitDrop);

                    spawnTime = i + juiceStream.SpawnTime;
                    Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);

                    Canvas.SetLeft(droplet, juiceStream.Path.PositionAt((juiceStream.EndTime - i) / spawnTime).X);
                    Canvas.SetTop(droplet, -Ypos);

                    juiceStream.Children.Add(droplet);
                }
            }


            spawnTime = juiceStream.EndTime - juiceStream.SpawnTime;
            Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);

            Image fruitTailImage = new Image();
            fruitTailImage.Name = "tael";
            fruitTailImage.Width = MainWindow.OsuPlayfieldObjectDiameter;
            fruitTailImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);
            //double tailPos = 0;
            //if (juiceStream.X > juiceStream.EndXPosition)
            //{
            //    tailPos = juiceStream.EndXPosition - fruitTailImage.Width / 2;
            //}
            //else
            //{
            //    tailPos = -(juiceStream.X - juiceStream.EndXPosition) + fruitTailImage.Width / 2;
            //}
            Canvas.SetLeft(fruitTailImage, juiceStream.EndXPosition - fruitTailImage.Width / 2);
            Canvas.SetTop(fruitTailImage, -Ypos);
            juiceStream.Children.Add(fruitTailImage);

            if (juiceStream.Drops != null)
            {
                for (int i = 0; i < juiceStream.Drops.Count; i++)
                {
                    Image dropImage = new Image();
                    dropImage.Name = "dwop";
                    dropImage.Width = MainWindow.OsuPlayfieldObjectDiameter * 0.8;
                    dropImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitDrop);

                    spawnTime = juiceStream.EndTime - juiceStream.Drops[i].Time;
                    Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);
                    double offsetPos = ((juiceStream.EndTime - juiceStream.Drops[i].Time) / (juiceStream.EndTime - juiceStream.SpawnTime));
                    System.Numerics.Vector2 a = juiceStream.Path.PositionAt(offsetPos);
                    Canvas.SetLeft(dropImage, a.X - dropImage.Width / 2);
                    Canvas.SetTop(dropImage, a.Y + -dropImage.Width);
                    juiceStream.Children.Add(dropImage);

                    //if (tailPos > 0)
                    //{
                    //    Canvas.SetLeft(dropImage, juiceStream.Drops[i].Position.X - dropImage.Width / 2);
                    //}
                    //else
                    //{
                    //    double offsetPos = (juiceStream.X + tailPos) *  (1 - ((juiceStream.EndTime - juiceStream.Drops[i].Time) / (juiceStream.EndTime - juiceStream.SpawnTime)));
                    //    Canvas.SetLeft(dropImage, -(offsetPos + juiceStream.Drops[i].Position.X) - dropImage.Width / 2);
                    //    Canvas.SetTop(dropImage, -Ypos);
                    //}
                }
            }

            Canvas.SetLeft(juiceStream, juiceStream.X);
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
    }
}
