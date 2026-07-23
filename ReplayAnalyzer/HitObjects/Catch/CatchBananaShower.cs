using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows.Controls;

namespace ReplayAnalyzer.HitObjects.Catch
{
    public class CatchBananaShower : HitObject
    {
        public CatchBananaShower(CatchBananaShowerData bananaShowerData)
        {
            SpawnTime = bananaShowerData.SpawnTime;
            EndTime = bananaShowerData.EndTime;
            Judgement = new HitJudgement((HitObjectJudgement)bananaShowerData.Judgement.Judgement, bananaShowerData.Judgement.SpawnTime);
        }

        public double EndTime { get; set; }

        public static CatchBananaShower Create(CatchBananaShowerData bananaShowerData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateBananaShower(bananaShowerData, index);
            }

            return CreateBananaShowerPreload(bananaShowerData, index);
        }

        private static CatchBananaShower CreateBananaShower(CatchBananaShowerData bananaShowerData, int index)
        {
            CatchBananaShower fruit = new CatchBananaShower(bananaShowerData);

            Image fruitImage = new Image();
            fruitImage.Width = 10; // is it based on CS?

            fruit.Children.Add(fruitImage);

            Canvas.SetLeft(fruit, 123); // ???
            Canvas.SetTop(fruit, 0);
            Canvas.SetZIndex(fruit, -1);

            fruit.Name = $"CatchBananaShowerObject{index}";

            return fruit;
        }

        private static CatchBananaShower CreateBananaShowerPreload(CatchBananaShowerData bananaShowerData, int index)
        {
            CatchBananaShower fruit = new CatchBananaShower(bananaShowerData);

            Image fruitImage = new Image();
            fruit.Children.Add(fruitImage);

            Canvas.SetLeft(fruit, 100);
            Canvas.SetTop(fruit, 0);

            fruit.Name = $"CatchBananaShowerObject{index}";

            return fruit;
        }
    }
}
