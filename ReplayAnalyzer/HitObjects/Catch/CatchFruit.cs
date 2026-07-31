using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplayMods.Mods;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers.Catch;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
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

        public bool IsMissed { get; set; } = false;

        public static CatchFruit Create(CatchFruitData fruitData, int index, ref float lastPosition, ref double lastSpawnTime)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateFruit(fruitData, index);
            }

            // last position and last spawn time are HR values only used when HR is enabled
            // it is only used in preload coz it will permanently change positions of Data objects so when replay is loaded it wont need to
            // do all these HR calculations over and over again
            return CreateFruitPreload(fruitData, index, ref lastPosition, ref lastSpawnTime);
        }

        private static CatchFruit CreateFruit(CatchFruitData fruitData, int index)
        {
            CatchFruit fruit = new CatchFruit(fruitData);
            fruit.Width = CatchPlayfield.FruitDiameter;

            Image fruitImage = new Image();
            fruitImage.Width = fruit.Width;
            fruitImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);
            fruit.Children.Add(fruitImage);

            Canvas.SetLeft(fruit, (fruit.X * MainWindow.OsuPlayfieldObjectScale) - fruitImage.Width / 2);
            Canvas.SetTop(fruit, -999);
            Canvas.SetZIndex(fruit, -1);

            fruit.Name = $"CatchFruitObject{index}";

            return fruit;
        }

        private static CatchFruit CreateFruitPreload(CatchFruitData fruitData, int index, ref float lastPosition, ref double lastSpawnTime)
        {
            if (HardRockMod.IsHardRockEnabled == true)
            {
                ApplyHardRockOffest(fruitData, ref lastPosition, ref lastSpawnTime);
            }

            CatchFruit fruit = new CatchFruit(fruitData);

            Image fruitImage = new Image();
            fruit.Children.Add(fruitImage);

            Canvas.SetLeft(fruit, fruit.X);
            Canvas.SetTop(fruit, 0);

            fruit.Name = $"CatchFruitObject{index}";

            return fruit;
        }

        // copied from lazer (obviously like how anyone could guess how this was made anyway)
        private static void ApplyHardRockOffest(CatchFruitData fruit, ref float lastPosition, ref double lastStartTime)
        {
            float offsetPosition = (float)fruit.X;
            double startTime = fruit.SpawnTime;

            if (lastPosition == null ||
                // some objects can get assigned position zero, making stable incorrectly go inside this if branch on the next object. to maintain behaviour and compatibility, do the same here.
                // reference: https://github.com/peppy/osu-stable-reference/blob/3ea48705eb67172c430371dcfc8a16a002ed0d3d/osu!/GameplayElements/HitObjects/Fruits/HitFactoryFruits.cs#L45-L50
                // nottodo: should be revisited and corrected later probably.
                lastPosition == 0)
            {
                lastPosition = offsetPosition;
                lastStartTime = startTime;

                return;
            }

            float positionDiff = offsetPosition - lastPosition;

            // nottodo: BUG!! Stable calculated time deltas as ints, which affects randomisation. This should be changed to a double.
            // ^ should be changed but isnt... so i wont touch this coz i feel like everything will explode if i do
            int timeDiff = (int)(startTime - lastStartTime);

            if (timeDiff > 1000)
            {
                lastPosition = offsetPosition;
                lastStartTime = startTime;
                return;
            }

            if (positionDiff == 0)
            {
                ApplyRandomOffset(ref offsetPosition, timeDiff / 4d);
                fruit.X = fruit.X + (offsetPosition - fruit.X);
                return;
            }

            if (Math.Abs(positionDiff) < timeDiff / 3)
            {
                ApplyOffset(ref offsetPosition, positionDiff);
            }

            fruit.X = fruit.X + (offsetPosition - fruit.X);

            lastPosition = offsetPosition;
            lastStartTime = startTime;
        }

        private static void ApplyRandomOffset(ref float position, double maxOffset)
        {
            bool right = CatchRNG.NextBool();
            float rand = Math.Min(20, (float)CatchRNG.Next(0, Math.Max(0, maxOffset)));
            if (right)
            {
                // Clamp to the right bound
                if (position + rand <= 512) // 512 is const playfield width
                {
                    position += rand;
                }
                else
                {
                    position -= rand;
                }
            }
            else
            {
                // Clamp to the left bound
                if (position - rand >= 0)
                {
                    position -= rand;
                }
                else
                {
                    position += rand;
                }
            }
        }

        private static void ApplyOffset(ref float position, float amount)
        {
            if (amount > 0)
            {
                // Clamp to the right bound
                if (position + amount < 512) // 512 is const playfield width
                {
                    position += amount;
                }
            }
            else
            {
                // Clamp to the left bound
                if (position + amount > 0)
                {
                    position += amount;
                }
            }
        }
    }
}
