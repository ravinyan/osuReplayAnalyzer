using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows.Controls;

namespace ReplayAnalyzer.HitObjects.Catch
{
    public class CatchFruit : HitObject
    {
        public CatchFruit(CatchFruitData fruitData)
        {
            X = fruitData.X;
            SpawnTime = fruitData.SpawnTime;
            Judgement = new HitJudgement((HitObjectJudgement)fruitData.Judgement.Judgement, fruitData.Judgement.SpawnTime);
        }

        public static CatchFruit Create(CatchFruitData fruitData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateFruit(fruitData, index);
            }

            return CreateFruitPreload(fruitData, index);
        }

        private static CatchFruit CreateFruit(CatchFruitData fruitData, int index)
        {
            CatchFruit fruit = new CatchFruit(fruitData);

            Image fruitImage = new Image();
            fruitImage.Width = 50;// based on CS
            fruitImage.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaNote3);

            fruit.Children.Add(fruitImage);

            Canvas.SetLeft(fruit, fruit.X);
            Canvas.SetTop(fruit, 0);
            Canvas.SetZIndex(fruit, -1);

            fruit.Name = $"CatchFruitObject{index}";

            return fruit;
        }

        private static CatchFruit CreateFruitPreload(CatchFruitData fruitData, int index)
        {
            CatchFruit fruit = new CatchFruit(fruitData);

            Image fruitImage = new Image();
            fruit.Children.Add(fruitImage);

            Canvas.SetLeft(fruit, fruit.X);
            Canvas.SetTop(fruit, 0);

            fruit.Name = $"CatchFruitObject{index}";

            return fruit;
        }
    }
}
