using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.AnalyzerTools;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Colora = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;

#nullable disable

namespace ReplayAnalyzer.HitObjects
{
    public class HitCircle : HitObject
    {
        public HitCircle(CircleData circleData)
        {
            X = circleData.X;
            Y = circleData.Y;
            BaseSpawnPosition = circleData.BaseSpawnPosition;
            SpawnTime = circleData.SpawnTime;
            Judgement = new HitJudgement((HitObjectJudgement)circleData.Judgement.Judgement, circleData.Judgement.SpawnTime);
        }

        public static HitCircle CreateCircle(CircleData circleData, double diameter, int currentComboNumber, int index, int comboColourIndex)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateCircleObject(circleData, diameter, currentComboNumber, index, comboColourIndex);
            }

            return CreateCirclePreload(circleData, diameter, index);
        }

        private static HitCircle CreateCircleObject(CircleData circleData, double diameter, int currentComboNumber, int index, int comboColourIndex)
        {
            HitCircle hitObject = new HitCircle(circleData);
            hitObject.Width = diameter;
            hitObject.Height = diameter;

            Image hitCircle = ApplyComboColourToHitCircle(new Bitmap(SkinElement.Get(SkinElement.SkinElements.HitCircle)), comboColourIndex, diameter);

            // the source bitmap, no matter if B/W or anything else
            BitmapSource source = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.HitCircle)));

            //int[] pixels = new int[source.PixelWidth * source.PixelHeight];
            //WriteableBitmap killme = new WriteableBitmap(source);
            //int k = 0;
            //for (int i = 0; i < killme.PixelHeight; i++)
            //{
            //    for (int j = 0; j < killme.PixelWidth; j++)
            //    {
            //        Console.WriteLine(GetPixelColor(killme, i, j));
            //
            //    }
            //}

            //killme.WritePixels((0, 0, killme.PixelWidth, killme.PixelHeight), PixelData, stride, 0)

            BitmapSource borderBitmap = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.HitCircleOverlay)));
            if (borderBitmap.Width != 256)
            {
                borderBitmap = new CroppedBitmap(borderBitmap, new Int32Rect(8, 8, 256, 256));
            }
            Image hitCircleBorder = new Image()
            {
                Width = diameter,
                Height = diameter,
                Source = borderBitmap,
            };
            
            Grid comboNumber = AddComboNumber(currentComboNumber, diameter);

            BitmapSource approachCircleBitmap = new BitmapImage(new Uri(SkinElement.Get(SkinElement.SkinElements.ApproachCircle)));
            if (approachCircleBitmap.Width != 256)
            {
                //approachCircleBitmap = new CroppedBitmap(approachCircleBitmap, new Int32Rect(8, 8, 256, 256));
            }
            Image approachCircle = new Image()
            {
                Height = (diameter * 4),
                Width = (diameter * 4),
                Source = approachCircleBitmap,
            };

            hitObject.Children.Add(hitCircle);
            hitObject.Children.Add(hitCircleBorder);
            hitObject.Children.Add(comboNumber);
            hitObject.Children.Add(approachCircle);

            Canvas.SetLeft(hitObject, (hitObject.X - diameter / 2));
            Canvas.SetTop(hitObject, (hitObject.Y - diameter / 2));

            // circles 1 2 3 were rendered so 3 was on top...
            // (0 - index) gives negative value so that 1 will be rendered on top
            // basically correct zindexing like it should be for every object
            SetZIndex(hitObject, 0 - index);

            hitObject.Name = $"CircleHitObject{index}";

            hitObject.Visibility = Visibility.Collapsed;

            //HitObjectAnimations.ApplyHitCircleAnimations(hitObject);

            return hitObject;
        }

        private static unsafe (int R, int G, int B, int A) GetPixelColor(WriteableBitmap canvasBitmap, int x, int y)
        {
            var pix = new Colora();
            byte[] ColorData = { 0, 0, 0, 0 }; // NOTE, results comes in BGRA order! 
            IntPtr pBackBuffer = canvasBitmap.BackBuffer;
            byte* pBuff = (byte*)pBackBuffer.ToPointer();
            var b = pBuff[4 * x + (y * canvasBitmap.BackBufferStride)];
            var g = pBuff[4 * x + (y * canvasBitmap.BackBufferStride) + 1];
            var r = pBuff[4 * x + (y * canvasBitmap.BackBufferStride) + 2];
            var a = pBuff[4 * x + (y * canvasBitmap.BackBufferStride) + 3];

            return (r, g, b, a);
        }

        private static HitCircle CreateCirclePreload(CircleData circleData, double diameter, int index)
        {
            HitCircle hitObject = new HitCircle(circleData);
            hitObject.Width = diameter;
            hitObject.Height = diameter;

            Image hitCircle = new Image();
            Canvas hitCircleBorder2 = new Canvas();
            Canvas comboNumber = new Canvas();
            Image approachCircle = new Image();

            hitObject.Children.Add(hitCircle);
            hitObject.Children.Add(hitCircleBorder2);
            hitObject.Children.Add(comboNumber);
            hitObject.Children.Add(approachCircle);

            Canvas.SetLeft(hitObject, hitObject.X - diameter / 2);
            Canvas.SetTop(hitObject, hitObject.Y - diameter / 2);

            hitObject.Name = $"CircleHitObject{index}";

            return hitObject;
        }

        public static Image Circle(HitCircle c)
        {
            return c.Children[0] as Image;
        }

        public static Image ApproachCircle(HitCircle c)
        {
            return c.Children[3] as Image;
        }
    }
}
