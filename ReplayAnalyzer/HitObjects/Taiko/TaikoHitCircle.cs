using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;

namespace ReplayAnalyzer.HitObjects.Taiko
{
    public class TaikoHitCircle : HitObject
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public TaikoHitCircle(TaikoHitCircleData noteData)
        {
            SpawnTime = noteData.SpawnTime;
            IsBig = noteData.IsBig;
            IsDon = noteData.IsDon;
            Judgement = new HitJudgement((HitObjectJudgement)noteData.Judgement.Judgement, noteData.Judgement.SpawnTime);
        }

        public bool IsBig { get; private set; }

        /// <summary>
        /// true = Don (red), false = Kat (blue)
        /// </summary>
        public bool IsDon { get; private set; }

        public static TaikoHitCircle CreateCircle(TaikoHitCircleData circleData, int index)
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

            // this will need to be coloured per wiki
            // Tinted red for "Don"(235, 69, 44)
            // Tinted blue for "Katsu"(68, 141, 171)

            Image circleImage = new Image();
            circleImage.Width = 50;
            circleImage.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircle);

            Image circleOverlay = new Image();
            circleOverlay.Width = 50;
            circleOverlay.Source = SkinElement.GetElement(SkinElement.SkinElements.TaikoHitCircleOverlay);

            circle.Children.Add(circleImage);

            Canvas.SetLeft(circle, Window.Width);
            Canvas.SetTop(circle, circleImage.Width / 2);
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
    }
}
