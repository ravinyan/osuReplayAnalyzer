using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayAnalyzer.HitObjects;
using ReplayAnalyzer.PlayfieldGameplay;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.GameplaySkin
{
    public static class SkinElement
    {
        private static string CurrentSkinFolderPath = "";
        private static string DefaultSkinFolderPath = $"{AppContext.BaseDirectory}\\Skins\\Komori - PeguLian II (PwV)";

        // well... i dont see better way to do this
        // this is special thing for hit circle, rest is just cached BitmapSource
        private static bool IsSaved = false;
        private static BitmapSource HitCircleSource = null!;
        private static WriteableBitmap[] HitCirclesColoured = new WriteableBitmap[0];

        private static BitmapSource   Cursor                = null!;
        private static BitmapSource[] ComboNumbers          = new BitmapImage[0]; // from 0 to 9
        private static BitmapSource   HitCircleOverlay      = null!;
        private static BitmapSource   ApproachCircle        = null!;
        private static BitmapSource   ReverseArrow          = null!;
        private static BitmapSource   SliderBall            = null!;
        private static BitmapSource   SliderBallCircle      = null!;
        private static BitmapSource   SliderTick            = null!;
        private static BitmapSource   SpinnerApproachCircle = null!;
        private static BitmapSource   SpinnerBackground     = null!;
        private static BitmapSource   SpinnerCircle         = null!;
        private static BitmapSource   Hit300                = null!;
        private static BitmapSource   Hit100                = null!;
        private static BitmapSource   Hit50                 = null!;
        private static BitmapSource   Hit0                  = null!;
        private static BitmapSource   SliderEndMiss         = null!;
        private static BitmapSource   SliderTickMiss        = null!;

        // https://osu.ppy.sh/wiki/en/Skinning/osu%21
        // https://osu.ppy.sh/wiki/en/Skinning/Interface
        public static void UpdateSkinPath(string path)
        {
            if (Directory.Exists(path))
            {
                CurrentSkinFolderPath = path;
            }
            else
            {
                CurrentSkinFolderPath = DefaultSkinFolderPath;
            }

            SkinIniProperties.ResetComboColours();

            Cursor = null!; // reset cached cursor before applying new skin
            CursorSkin.InitializeCursor();

            // reset everything to save new skin elements
            IsSaved               = false;
            HitCircleSource       = null!;
            HitCirclesColoured    = new WriteableBitmap[SkinIniProperties.GetComboColours().Count + 1];
            ComboNumbers          = new BitmapImage[0];
            HitCircleOverlay      = null!;
            ApproachCircle        = null!;
            ReverseArrow          = null!;
            SliderBall            = null!;
            SliderBallCircle      = null!;
            SliderTick            = null!;
            SpinnerApproachCircle = null!;
            SpinnerBackground     = null!;
            SpinnerCircle         = null!;
            Hit300                = null!;
            Hit100                = null!;
            Hit50                 = null!;
            Hit0                  = null!;
            SliderEndMiss         = null!;
            SliderTickMiss        = null!;
        }
        
        public static string SkinPath()
        {
            return CurrentSkinFolderPath;
        }

        // 99.99% i wont implement it but just in case i feel like doing it
        // spinner-rpm.png | spinner-clear.png | spinner-spin.png | spinner-metre.png and some more https://osu.ppy.sh/wiki/en/Skinning/osu%21
        /// <summary>
        /// 
        /// </summary>
        /// <param name="skinElement"></param>
        /// <param name="index">this is used to put index for ComboNumber index and HitCircle colour index</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static BitmapSource GetElement(SkinElements skinElement, string index = "0")
        {
            switch (skinElement)
            {
                case SkinElements.Cursor:
                    if (Cursor == null)
                    {
                        Cursor = new BitmapImage(new Uri(SkinElementPath("cursor")));
                    }
                    return Cursor;
                case SkinElements.ApproachCircle:
                    if (ApproachCircle == null)
                    {
                        ApproachCircle = new BitmapImage(new Uri(SkinElementPath("approachcircle")));
                    }
                    return ApproachCircle;
                case SkinElements.HitCircle:
                    if (HitCircleSource == null)
                    {
                        HitCircleSource = new BitmapImage(new Uri(SkinElementPath("hitcircle")));
                    }
                    return HitCircleSource;
                case SkinElements.HitCircleOverlay:
                    if (HitCircleOverlay == null)
                    {
                        HitCircleOverlay = new BitmapImage(new Uri(SkinElementPath("hitcircleoverlay")));
                    }
                    return HitCircleOverlay;
                case SkinElements.ComboNumber:
                    if (ComboNumbers.Length == 0)
                    {
                        ComboNumbers = new BitmapSource[10];
                        for (int i = 0; i < 10; i++) // combo numbers are from 0 to 9
                        {
                            ComboNumbers![i] = new BitmapImage(new Uri(SkinElementPath($"default-{i}")));
                        }
                    }
                    return ComboNumbers![int.Parse(index)];
                case SkinElements.ReverseArrow:
                    if (ReverseArrow == null)
                    {
                        ReverseArrow = new BitmapImage(new Uri(SkinElementPath("reversearrow")));
                    }
                    return ReverseArrow;
                case SkinElements.SliderBall:
                    if (SliderBall == null)
                    {
                        SliderBall = new BitmapImage(new Uri(SkinElementPath("sliderb0"))); // this one can be animated but will leave it like this unless its a problem
                    }
                    return SliderBall;
                case SkinElements.SliderBallCircle:
                    if (SliderBallCircle == null)
                    {
                        SliderBallCircle = new BitmapImage(new Uri(SkinElementPath("sliderfollowcircle")));
                    }
                    return SliderBallCircle;
                case SkinElements.Hit300:
                    if (Hit300 == null)
                    {
                        Hit300 = new BitmapImage(new Uri(JudgementPath("hit300")));
                    }
                    return Hit300;
                case SkinElements.Hit100:
                    if (Hit100 == null)
                    {
                        Hit100 = new BitmapImage(new Uri(JudgementPath("hit100")));
                    }
                    return Hit100;
                case SkinElements.Hit50:
                    if (Hit50 == null)
                    {
                        Hit50 = new BitmapImage(new Uri(JudgementPath("hit50")));
                    }
                    return Hit50;
                case SkinElements.Hit0:
                    if (Hit0 == null)
                    {
                        Hit0 = new BitmapImage(new Uri(JudgementPath("hit0")));
                    }
                    return Hit0;
                case SkinElements.SliderEndMiss:
                    if (SliderEndMiss == null)
                    {
                        SliderEndMiss = new BitmapImage(new Uri(SkinElementPath("sliderendmiss")));
                    }
                    return SliderEndMiss;
                case SkinElements.SliderTickMiss:
                    if (SliderTickMiss == null)
                    {
                        SliderTickMiss = new BitmapImage(new Uri(SkinElementPath("slidertickmiss")));
                    }
                    return SliderTickMiss;
                case SkinElements.SliderTick:
                    if (SliderTick == null)
                    {
                        SliderTick = new BitmapImage(new Uri(SkinElementPath("sliderscorepoint")));
                    }
                    return SliderTick;
                case SkinElements.SpinnerApproachCircle:
                    if (SpinnerApproachCircle == null)
                    {
                        SpinnerApproachCircle = new BitmapImage(new Uri(SkinElementPath("spinner-approachcircle")));
                    }
                    return SpinnerApproachCircle;
                case SkinElements.SpinnerBackground:
                    if (SpinnerBackground == null)
                    {
                        SpinnerBackground = new BitmapImage(new Uri(SkinElementPath("spinner-background")));
                    }
                    return SpinnerBackground;
                case SkinElements.SpinnerCircle:
                    if (SpinnerCircle == null)
                    {
                        SpinnerCircle = new BitmapImage(new Uri(SkinElementPath("spinner-circle")));
                    }
                    return SpinnerCircle;
                default:
                    throw new Exception("Skin element does not exist");
            }
        }

        // old colouring functions https://github.com/ravinyan/osuReplayAnalyzer/blob/9d73d6f2580b8e5402dab6e3ae35e8090d997c7a/ReplayAnalyzer/HitObjects/HitObject.cs
        unsafe public static WriteableBitmap GetColouredHitCircle(int comboColourIndex)
        {
            // fun fact: base implementation i took from internet took 3.5ms on average, mine takes 15-20 ticks... smh noobs

            // skip white colour coz if skin element is already coloured (like in -Nekoha Shizuku -(Suminoze) skin)
            // then it will just become white or just be some ugly abomination... there might be more cases like this
            // but i cant care enough to download 500 skins and check if there are more cases like that...
            List<Color> colours = SkinIniProperties.GetComboColours();
            if (colours.Count == 0 || colours[comboColourIndex] == Color.FromArgb(255, 255, 255))
            {
                if (IsSaved == false)
                {
                    IsSaved = true;
                    HitCirclesColoured = new WriteableBitmap[2]; // 1 is base circle, 2 is notelock effect colour

                    if (HitCircleSource == null)
                    {
                        HitCircleSource = GetElement(SkinElements.HitCircle);
                    }
                    
                    HitCirclesColoured[0] = new WriteableBitmap(HitCircleSource);
                    RecolourHitCircle(1, colours);
                }
                
                return HitCirclesColoured[0];
            }

            if (IsSaved == true)
            {
                return HitCirclesColoured[comboColourIndex];
            }

            IsSaved = true;
            HitCircleSource = GetElement(SkinElements.HitCircle);
            // + 1 here coz we reserving last index as notelock colour effect
            HitCirclesColoured = new WriteableBitmap[SkinIniProperties.GetComboColours().Count + 1];
            for (int i = 0; i < colours.Count + 1; i++)
            {
                RecolourHitCircle(i, colours);
            }

            return HitCirclesColoured[comboColourIndex];
        }

        public static string GetSkinElementPath(SkinElements skinElement, string index = "0")
        {
            switch (skinElement)
            {
                case SkinElements.Cursor:
                    return SkinElementPath("cursor");
                case SkinElements.ApproachCircle:
                    return SkinElementPath("approachcircle");
                case SkinElements.HitCircle:
                    return SkinElementPath("hitcircle");
                case SkinElements.HitCircleOverlay:
                    return SkinElementPath("hitcircleoverlay");
                case SkinElements.ComboNumber:
                    return SkinElementPath($"default-{index}");
                case SkinElements.ReverseArrow:
                    return SkinElementPath("reversearrow");
                case SkinElements.SliderBall:
                    return SkinElementPath("sliderb0");
                case SkinElements.SliderBallCircle:
                    return SkinElementPath("sliderfollowcircle");
                case SkinElements.Hit300:
                    return JudgementPath("hit300");
                case SkinElements.Hit100:
                    return JudgementPath("hit100");
                case SkinElements.Hit50:
                    return JudgementPath("hit50");
                case SkinElements.Hit0:
                    return JudgementPath("hit0");
                case SkinElements.SliderEndMiss:
                    return SkinElementPath("sliderendmiss");
                case SkinElements.SliderTickMiss:
                    return SkinElementPath("slidertickmiss");
                case SkinElements.SliderTick:
                    return SkinElementPath("sliderscorepoint");
                case SkinElements.SpinnerApproachCircle:
                    return SkinElementPath("spinner-approachcircle");
                case SkinElements.SpinnerBackground:
                    return SkinElementPath("spinner-background");
                case SkinElements.SpinnerCircle:
                    return SkinElementPath("spinner-circle");
                default:
                    throw new Exception("Skin element does not exist");
            }
        }

        public static void ApplyNotelockEffect(HitObject hitObject)
        {
            if (hitObject is HitCircle hc)
            {
                HitCircle.Circle(hc).Source = HitCirclesColoured.Last();
            }
            else if (hitObject is Slider s)
            {
                Slider.HeadHitCircle(s).Source = HitCirclesColoured.Last();
            }
        }

        public static void ApplyComboColoursFromSkin()
        {
            List<Color> colours = SkinIniProperties.GetComboColours();
            if (colours.Count == 0)
            {
                return;
            }

            int index = 0;
            foreach (HitObjectData hitObjectData in MainWindow.map.HitObjects!)
            {
                if (hitObjectData.ComboNumber == 1)
                {
                    index++;
                    if (index >= colours.Count - 1)
                    {
                        index = 0;
                    }
                }

                hitObjectData.RGBValue = colours[index];
            }
        }

        unsafe private static void RecolourHitCircle(int i, List<Color> colours)
        {
            HitCirclesColoured[i] = new WriteableBitmap(HitCircleSource);

            // i guess this is memory buffer of the WHOLE image
            IntPtr pBackBuffer = HitCirclesColoured[i].BackBuffer;
            // pointers are basically arrays so it creates array of all colour data in packs of 4 (BGRA format)
            byte* pBuff = (byte*)pBackBuffer.ToPointer();

            int backBufferStride = HitCirclesColoured[i].BackBufferStride;

            int pixelX;
            int pixelY;
            int pixelIndex;

            byte a;
            byte b;
            byte g;
            byte r;

            for (int x = 0; x < HitCirclesColoured[i].PixelHeight; x++)
            {
                for (int y = 0; y < HitCirclesColoured[i].PixelWidth; y++)
                {
                    pixelX = 4 * x;                 // 4 * x (y * buff) is the boundle of BGRA on this specific X/Y pixel
                    pixelY = y * backBufferStride;  // back buffer stride is memory size of single column of the image
                    pixelIndex = pixelX + pixelY;

                    a = pBuff[pixelIndex + 3];      // < to colours we add up to +3 to get B(0) G(1) R(2) A(3) values
                    if (a == 0)
                    {
                        continue;                   // we skip invisible pixels to speed up the recolouring
                    }

                    // get current colours of hit object
                    b = pBuff[pixelIndex];
                    g = pBuff[pixelIndex + 1];
                    r = pBuff[pixelIndex + 2];

                    // apply new colours to hit object based on skin colours and based on how strong the white colour is (thanks google AI for once you werent useless)
                    // based on this formula: White - (White - Colour) (all pixels are white before they are changed)
                    // i found multiple different formulas for this but they use multiplication and division which is slow
                    // also after i found my formula i couldnt find anything close to that so... it would suck if i refreshed my browser lol
                    // another formula just in case: (byte)(colour.R + (255 - colour.R) * (b / 255)) i think it does same thing just slower

                    // THIS COLOURING IS LIGHTER THAN WHAT OSU HAS
                    // i like it this way but if needed just uncomment * 0.85 and change it to darken the colours (lower = darker)
                    if (i < colours.Count)
                    {
                        pBuff[pixelIndex + 0] = (byte)((b - (b - colours[i].B))); //* 0.85);
                        pBuff[pixelIndex + 1] = (byte)((g - (g - colours[i].G))); //* 0.85);
                        pBuff[pixelIndex + 2] = (byte)((r - (r - colours[i].R))); //* 0.85);
                    }
                    else if (i == colours.Count)
                    {// notelock colour
                        pBuff[pixelIndex + 0] = 0;
                        pBuff[pixelIndex + 1] = 0;
                        pBuff[pixelIndex + 2] = 255;
                    }
                }
            }
        }

        private static string SkinElementPath(string skinElement)
        {
            // base example hitcircle
            // priority hitcircle@2x > no hd
            string fullPath = $"{SkinPath()}\\{skinElement}";
            if (File.Exists($"{fullPath}@2x.png"))
            {
                return $"{fullPath}@2x.png";
            }
            else if (File.Exists($"{fullPath}.png"))
            {
                return $"{fullPath}.png";
            }
            else
            {
                if (File.Exists($"{DefaultSkinFolderPath}\\{skinElement}@2x.png"))
                {
                    return $"{DefaultSkinFolderPath}\\{skinElement}@2x.png";
                }
                else
                {
                    return $"{DefaultSkinFolderPath}\\{skinElement}.png";
                }        
            }
        }

        // special function for judgements coz it can have animated skin elements (but no animations)
        private static string JudgementPath(string skinElement)
        {
            // base example  hit300
            // priority -0@2x > the non hd > hit300@2x > non
            string fullPath = $"{SkinPath()}\\{skinElement}";
            if (File.Exists($"{fullPath}-0@2x.png"))
            {
                return $"{fullPath}-0@2x.png";
            }
            else if (File.Exists($"{fullPath}-0.png"))
            {
                return $"{fullPath}-0.png";
            }
            else if (File.Exists($"{fullPath}@2x.png"))
            {
                return $"{fullPath}@2x.png";
            }
            else if (File.Exists($"{fullPath}.png"))
            {
                return $"{fullPath}.png";
            }
            else
            {
                return $"{DefaultSkinFolderPath}\\{skinElement}-0@2x.png";
            }
        }

        public enum SkinElements
        {
            Cursor = 0,
            ApproachCircle = 1,
            HitCircle = 2,
            HitCircleOverlay = 3,
            ComboNumber = 4,
            ReverseArrow = 5,
            SliderBall = 6,
            SliderBallCircle = 7,
            Hit300 = 8,
            Hit100 = 9,
            Hit50 = 10,
            Hit0 = 11,
            SliderEndMiss = 12,
            SliderTickMiss = 13,
            SliderTick = 14,
            SpinnerApproachCircle = 15,
            SpinnerBackground = 16,
            SpinnerCircle = 17,
        }
    }
}
