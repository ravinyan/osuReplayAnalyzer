using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.HitObjects.Taiko
{
    public class TaikoHitCircle : HitObject
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public TaikoHitCircle(TaikoHitCircleData circleData)
        {
            SpawnTime = circleData.SpawnTime;
            IsBig = circleData.IsBig;
            IsDon = circleData.IsDon;
            Judgement = new HitJudgement((HitObjectJudgement)circleData.Judgement.Judgement, circleData.Judgement.SpawnTime);
        }

        public bool IsBig { get; private set; }

        /// <summary>
        /// true = Don (red), false = Kat (blue)
        /// </summary>
        public bool IsDon { get; private set; }

        public static TaikoHitCircle Create(TaikoHitCircleData circleData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateCircleObject(circleData, index);
            }

            return CreateCirclePreload(circleData, index);
        }

        private static TaikoHitCircle CreateCircleObject(TaikoHitCircleData circleData, int index)
        {
            TaikoHitCircle circle = new TaikoHitCircle(circleData);

            Image circleImage = new Image();
            circleImage.Width = circle.IsBig == false ? 50 : 75;
            circleImage.Source = GetColouredCircle(circle.IsDon, circle.IsBig);

            Image circleOverlay = new Image();
            circleOverlay.Width = circle.IsBig == false ? 50 : 75;
            if (circle.IsBig == false)
            {
                circleOverlay.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleOverlay);
            }
            else
            {
                circleOverlay.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleOverlayBig);
            }

            circle.Children.Add(circleImage);
            circle.Children.Add(circleOverlay);

            Canvas.SetLeft(circle, Window.Width);
            Canvas.SetTop(circle, circle.IsBig == false ? (50 / 2) : (25 / 2));
            Canvas.SetZIndex(circle, 1);

            circle.Name = $"TaikoCircleObject{index}";

            return circle;
        }

        private static TaikoHitCircle CreateCirclePreload(TaikoHitCircleData circleData, int index)
        {
            TaikoHitCircle circle = new TaikoHitCircle(circleData);

            Image circleImage = new Image();
            circle.Children.Add(circleImage);

            Canvas.SetLeft(circle, 0);
            Canvas.SetTop(circle, 0);

            circle.Name = $"TaikoCircleObject{index}";

            return circle;
        }

        private static BitmapSource GetColouredCircle(bool isDon, bool isBig)
        {
            if (isDon && isBig)
            {
                return SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleBigDon);
            }
            else if (isDon)
            {
                return SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleDon);
            }
            else if (!isDon && !isBig)
            {
                return SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleBigKat);
            }
            else if (!isDon)
            {
                return SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleKat);
            }
            else
            {
                throw new Exception("boo");
            }
        }
    }
}
