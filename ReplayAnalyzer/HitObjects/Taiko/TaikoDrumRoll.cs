using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.HitObjects.Taiko
{
    public class TaikoDrumRoll : HitObject
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public TaikoDrumRoll(TaikoDrumRollData drumRollData)
        {
            SpawnTime = drumRollData.SpawnTime;
            IsBig = drumRollData.IsBig;
            Length = drumRollData.Length;
            Judgement = new HitJudgement((HitObjectJudgement)drumRollData.Judgement.Judgement, drumRollData.Judgement.SpawnTime);
        }

        public bool IsBig { get; set; }
        public double Length { get; set; }

        public static TaikoDrumRoll CreateDrumRoll(TaikoDrumRollData drumRollData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateDrumRollObject(drumRollData, index);
            }

            return CreateDrumRollPreload(drumRollData, index);
        }

        private static TaikoDrumRoll CreateDrumRollObject(TaikoDrumRollData drumRollData, int index)
        {
            TaikoDrumRoll drumRoll = new TaikoDrumRoll(drumRollData);

            // it will be somewhere here hopefully https://osu.ppy.sh/wiki/en/Skinning/osu%21taiko
            Image drumRollHead = new Image();
            drumRollHead.Width = drumRoll.IsBig == false ? 50 : 75;
            drumRollHead.Source = GetHeadSkinElement(drumRoll.IsBig);
            Canvas.SetLeft(drumRollHead, 0);
            Canvas.SetTop(drumRollHead, drumRoll.IsBig == false ? (50 / 2) : (25 / 2));
            Canvas.SetZIndex(drumRollHead, 1);

            Image drumRollHeadOverlay = new Image();
            drumRollHeadOverlay.Width = drumRoll.IsBig == false ? 50 : 75;
            drumRollHeadOverlay.Source = GetHeadOverlaySkinElement(drumRoll.IsBig);
            Canvas.SetLeft(drumRollHeadOverlay, 0);
            Canvas.SetTop(drumRollHeadOverlay, drumRoll.IsBig == false ? (50 / 2) : (25 / 2));
            Canvas.SetZIndex(drumRollHeadOverlay, 2);

            Image drumRollMiddle = new Image();
            drumRollMiddle.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoRollMiddle);
            drumRollMiddle.Width = drumRoll.Length * 2;
            drumRollMiddle.Height = drumRoll.IsBig == false ? 50 : 75;
            drumRollMiddle.Stretch = Stretch.Fill;
            Canvas.SetLeft(drumRollMiddle, drumRollHead.Width / 2);
            Canvas.SetTop(drumRollMiddle, drumRoll.IsBig == false ? (50 / 2) : (25 / 2));
            Canvas.SetZIndex(drumRollMiddle, 0);

            Image drumRollTail = new Image();
            drumRollTail.Width = drumRoll.IsBig == false ? 25 : 37.5;
            drumRollTail.Height = drumRollHead.Height;
            drumRollTail.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoRollEnd);
            Canvas.SetLeft(drumRollTail, (drumRollHead.Width / 2) + (drumRoll.Length * 2));
            Canvas.SetTop(drumRollTail, drumRoll.IsBig == false ? 25 : 12);

            drumRoll.Children.Add(drumRollHead);
            drumRoll.Children.Add(drumRollHeadOverlay);
            drumRoll.Children.Add(drumRollMiddle);
            drumRoll.Children.Add(drumRollTail);

            Canvas.SetLeft(drumRoll, Window.Width);
            Canvas.SetTop(drumRoll, 0);
            Canvas.SetZIndex(drumRoll, 1);

            RenderOptions.SetEdgeMode(drumRoll, EdgeMode.Aliased);

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

        private static BitmapSource GetHeadSkinElement(bool isBig)
        {
            if (isBig == false)
            {
                return SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleDrumRoll);
            }
            else
            {
                return SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleBigDrumRoll);
            }
        }

        private static BitmapSource GetHeadOverlaySkinElement(bool isBig)
        {
            if (isBig == false)
            {
                return SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleOverlay);
            }
            else
            {
                return SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleOverlayBig);
            }
        }
    }
}
