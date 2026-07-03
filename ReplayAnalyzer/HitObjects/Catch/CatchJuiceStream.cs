using OsuFileParsers.Classes.Beatmap.osu.Objects;
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
            Judgement = new HitJudgement((HitObjectJudgement)juiceStreamData.Judgement.Judgement, juiceStreamData.Judgement.SpawnTime);
        }

        public int EndTime { get; set; }
        public int EndXPosition { get; set; }
        public List<SliderTick> Drops { get; set; }

        public static CatchJuiceStream Create(CatchJuiceStreamData juiceStreamData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateJuiceStream(juiceStreamData, index);
            }

            return CreateJuiceStreamPreload(juiceStreamData, index);
        }

        private static CatchJuiceStream CreateJuiceStream(CatchJuiceStreamData juiceStreamData, int index)
        {
            CatchJuiceStream juiceStream = new CatchJuiceStream(juiceStreamData);

            Image fruitHeadImage = new Image();
            fruitHeadImage.Width = 50; // based on CS
            fruitHeadImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);
            Canvas.SetLeft(fruitHeadImage, juiceStream.X);
            Canvas.SetTop(fruitHeadImage, 0);
            juiceStream.Children.Add(fruitHeadImage);

            double h = CatchPlayfield.Playfield.Height - 80;
            double spawnTime = 0;
            double Ypos = 0;

            if (juiceStream.Drops != null)
            {
                for (int i = 0; i < juiceStream.Drops.Count; i++)
                {
                    Image dropImage = new Image();
                    dropImage.Width = 40;
                    dropImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitDrop);
                    Canvas.SetLeft(dropImage, juiceStream.Drops[i].Position.X);

                    spawnTime = juiceStream.EndTime - juiceStream.Drops[i].Time;
                    Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);
                    Canvas.SetTop(dropImage, Ypos);

                    juiceStream.Children.Add(dropImage);
                }
            }


            //Image dropletImage = new Image();
            //dropletImage.Width = 10;
            //Canvas.SetLeft();
            //Canvas.SetTop();

            spawnTime = juiceStream.EndTime - juiceStream.SpawnTime;
            Ypos = h * (spawnTime / ManiaPlayfield.ScrollSpeed);

            Image fruitTailImage = new Image();
            fruitTailImage.Width = 50; // based on CS
            fruitTailImage.Source = SkinElement.GetElement(SkinElement.SkinElements.CatchFruitApple);
            Canvas.SetLeft(fruitTailImage, juiceStream.EndXPosition);
            Canvas.SetTop(fruitTailImage, Ypos);
            juiceStream.Children.Add(fruitTailImage);

            Canvas.SetLeft(juiceStream, 0);
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
