using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.SliderPathMath;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using ReplayAnalyzer.PlayfieldUI.UIElements;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

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

            Image fruitHeadImage = new Image();
            fruitHeadImage.Name = "haed";
            fruitHeadImage.Width = MainWindow.OsuPlayfieldObjectDiameter; // based on CS
            fruitHeadImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);
            Canvas.SetLeft(fruitHeadImage, -fruitHeadImage.Width / 2);
            Canvas.SetTop(fruitHeadImage, -fruitHeadImage.Width / 2);
            juiceStream.Children.Add(fruitHeadImage);

            double h = CatchPlayfield.Playfield.Height;
            double spawnTime = 0;
            double Ypos = 0;

            spawnTime = juiceStream.EndTime - juiceStream.SpawnTime;
            Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);

            Image fruitTailImage = new Image();
            fruitTailImage.Name = "tael";
            fruitTailImage.Width = MainWindow.OsuPlayfieldObjectDiameter;
            fruitTailImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);

            Canvas.SetLeft(fruitTailImage, juiceStream.EndXPosition - fruitTailImage.Width / 2);
            Canvas.SetTop(fruitTailImage, -Ypos - fruitTailImage.Width / 2);
            juiceStream.Children.Add(fruitTailImage);

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
            int brainDamage = 0;
            double prevPosition = 0;
            var aaaaa = juiceStream.Drops.Count != 0 ? (int)juiceStream.Drops[brainDamage].Time : lastTickTime;
            int sinceLastTick2 = (int)aaaaa - (int)juiceStream.SpawnTime;
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
                    droplet.Width = MainWindow.OsuPlayfieldObjectDiameter * 0.3;
                    droplet.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitDrop);
                    spawnTime = juiceStream.EndTime - (i + juiceStream.SpawnTime);
                    Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);

                    //double offsetPos = 1 - ((juiceStream.EndTime - (i + juiceStream.SpawnTime)) / (juiceStream.EndTime - juiceStream.SpawnTime));
                    var fsdifi = juiceStream.Drops.Count > 0 ? juiceStream.Drops[brainDamage].PositionAt : lastTickProgress;
                    double offsetPos =((i / (double)sinceLastTick2) * (fsdifi - prevPosition));
                    float a = juiceStream.Path.PositionAt(offsetPos).X;
                    Canvas.SetLeft(droplet, a - droplet.Width / 2);
                    Canvas.SetTop(droplet, -Ypos);

                    juiceStream.Children.Add(droplet);
                }
            }

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
                    System.Numerics.Vector2 a = juiceStream.Path.PositionAt(juiceStream.Drops[i].PositionAt);

                    Canvas.SetLeft(dropImage, (a.X - dropImage.Width / 2));
                    Canvas.SetTop(dropImage, ((-Ypos * 1.2) + dropImage.Width / 2) - 5);
                    juiceStream.Children.Add(dropImage);
                }
            }

            Canvas.SetLeft(juiceStream, juiceStream.X);
            Canvas.SetTop(juiceStream, 0);
            Canvas.SetZIndex(juiceStream, -1);


            Path p = new Path();
            p.StrokeThickness = 2;
            p.Stroke = Brushes.Red;

            LineGeometry myLineGeometry = new LineGeometry();
            myLineGeometry.StartPoint = new Point(Canvas.GetLeft(fruitHeadImage) + 35, Canvas.GetTop(fruitHeadImage) + 35);
            myLineGeometry.EndPoint = new Point(Canvas.GetLeft(fruitTailImage) + 35, Canvas.GetTop(fruitTailImage) + 35);
            myLineGeometry.Freeze();

            p.Data = myLineGeometry;
            juiceStream.Children.Add(p);

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
