using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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

        public bool IsMissed { get; set; } = false;

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
            fruit.Width = MainWindow.OsuPlayfieldObjectDiameter * 0.9;

            Image fruitImage = new Image();
            // 0.8 is fruit and remaining 0.2 is hyperdash outline
            fruitImage.Width = fruit.Width;
            fruitImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);
            fruit.Children.Add(fruitImage);

            Ellipse hitbox = new Ellipse();
            hitbox.Width = fruit.Width / 10;
            hitbox.Height = fruit.Width / 10;
            hitbox.Stroke = Brushes.Black;
            hitbox.StrokeThickness = 1;
            hitbox.Fill = Brushes.White;
            Canvas.SetLeft(hitbox, fruitImage.Width / 2 - hitbox.Width / 2);
            Canvas.SetTop(hitbox, fruitImage.Width / 2 - hitbox.Width / 2);

            fruit.Children.Add(hitbox);

            Canvas.SetLeft(fruit, (fruit.X * MainWindow.OsuPlayfieldObjectScale) - fruitImage.Width / 2);
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
