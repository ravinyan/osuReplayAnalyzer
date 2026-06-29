using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.HitObjects.Taiko
{
    public class TaikoDrumRoll : HitObject
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public TaikoDrumRoll(TaikoDrumRollData noteData)
        {
            SpawnTime = noteData.SpawnTime;
            Judgement = new HitJudgement((HitObjectJudgement)noteData.Judgement.Judgement, noteData.Judgement.SpawnTime);
        }

        public static TaikoDrumRoll CreateDrumRoll(TaikoDrumRollData circleData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateDrumRollObject(circleData, index);
            }

            return CreateDrumRollPreload(circleData, index);
        }

        private static TaikoDrumRoll CreateDrumRollObject(TaikoDrumRollData drumRollData, int index)
        {
            TaikoDrumRoll drumRoll = new TaikoDrumRoll(drumRollData);

            Image drumRollImage = new Image();
            drumRollImage.Width = 50;
            // it will be somewhere here hopefully https://osu.ppy.sh/wiki/en/Skinning/osu%21taiko
            //drumRollImage.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoRollMiddle);

            drumRoll.Children.Add(drumRollImage);

            Canvas.SetLeft(drumRoll, Window.Width);
            Canvas.SetTop(drumRoll, 0);
            Canvas.SetZIndex(drumRoll, 1);

            drumRoll.Name = $"TaikoDrumRollObject{index}";

            return drumRoll;
        }

        private static TaikoDrumRoll CreateDrumRollPreload(TaikoDrumRollData drumRollData, int index)
        {
            TaikoDrumRoll drumRoll = new TaikoDrumRoll(drumRollData);

            Image drumRollImage = new Image();
            drumRoll.Children.Add(drumRollImage);

            Canvas.SetLeft(drumRoll, 0);
            Canvas.SetTop(drumRoll, 0);

            drumRoll.Name = $"TaikoDrumRollObject{index}";

            return drumRoll;
        }
    }
}
