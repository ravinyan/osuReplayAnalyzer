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

        private static BitmapSource Source = new BitmapImage();
        private static WriteableBitmap[] ModyfiableBitmaps = new WriteableBitmap[0];
        private static bool IsSaved = false;

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
            CursorSkin.ApplySkin();

            Source = new BitmapImage(new Uri(Get(SkinElements.HitCircle)));
            ModyfiableBitmaps = new WriteableBitmap[SkinIniProperties.GetComboColours().Count];
            IsSaved = false;
        }
        
        public static string SkinPath()
        {
            return CurrentSkinFolderPath;
        }

        // 99.99% i wont implement it but just in case i feel like doing it
        // spinner-rpm.png | spinner-clear.png | spinner-spin.png | spinner-metre.png and some more https://osu.ppy.sh/wiki/en/Skinning/osu%21
        public static string Get(SkinElements skinElement, string comboNumber = "")
        {
            switch (skinElement)
            {
                case SkinElements.Cursor:
                    return GetSkinElement(SkinPath(), "cursor");
                case SkinElements.ApproachCircle:
                    return GetSkinElement(SkinPath(), "approachcircle");
                case SkinElements.HitCircle:
                    return GetSkinElement(SkinPath(), "hitcircle");
                case SkinElements.HitCircleOverlay:
                    return GetSkinElement(SkinPath(), "hitcircleoverlay");
                case SkinElements.ComboNumber:
                    return GetSkinElement(SkinPath(),$"default-{comboNumber}");
                case SkinElements.ReverseArrow:
                    return GetSkinElement(SkinPath(), "reversearrow");
                case SkinElements.SliderBall:
                    return GetSkinElement(SkinPath(), "sliderb0"); // this one can be animated but will leave it like this unless its a problem
                case SkinElements.SliderBallCircle:
                    return GetSkinElement(SkinPath(), "sliderfollowcircle");
                case SkinElements.Hit300:
                    return GetJudgement(  SkinPath(), "hit300");
                case SkinElements.Hit100:
                    return GetJudgement(  SkinPath(), "hit100");
                case SkinElements.Hit50:  
                    return GetJudgement(  SkinPath(), "hit50");
                case SkinElements.Hit0:   
                    return GetJudgement(  SkinPath(), "hit0");
                case SkinElements.SliderEndMiss:
                    return GetSkinElement(SkinPath(), "sliderendmiss");
                case SkinElements.SliderTickMiss:
                    return GetSkinElement(SkinPath(), "slidertickmiss");
                case SkinElements.SliderTick:
                    return GetSkinElement(SkinPath(), "sliderscorepoint");
                case SkinElements.SpinnerApproachCircle:
                    return GetSkinElement(SkinPath(), "spinner-approachcircle");
                case SkinElements.SpinnerBackground:
                    return GetSkinElement(SkinPath(), "spinner-background");
                case SkinElements.SpinnerCircle:
                    return GetSkinElement(SkinPath(), "spinner-circle");
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
                    ModyfiableBitmaps = new WriteableBitmap[1];
                    ModyfiableBitmaps[0] = new WriteableBitmap(Source);
                }
                
                return ModyfiableBitmaps[0];
            }

            if (IsSaved == true)
            {
                return ModyfiableBitmaps[comboColourIndex];
            }

            IsSaved = true;
            Source = new BitmapImage(new Uri(Get(SkinElements.HitCircle)));
            ModyfiableBitmaps = new WriteableBitmap[SkinIniProperties.GetComboColours().Count];
            for (int i = 0; i < colours.Count; i++)
            {
                ModyfiableBitmaps[i] = new WriteableBitmap(Source);

                // i guess this is memory buffer of the WHOLE image
                IntPtr pBackBuffer = ModyfiableBitmaps[i].BackBuffer;
                // pointers are basically arrays so it creates array of all colour data in packs of 4 (BGRA format)
                byte* pBuff = (byte*)pBackBuffer.ToPointer();

                int backBufferStride = ModyfiableBitmaps[i].BackBufferStride;

                int pixelX;
                int pixelY;
                int pixelIndex;

                byte a;
                byte b;
                byte g;
                byte r;

                for (int x = 0; x < ModyfiableBitmaps[i].PixelHeight; x++)
                {
                    for (int y = 0; y < ModyfiableBitmaps[i].PixelWidth; y++)
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
                        pBuff[pixelIndex + 0] = (byte)((b - (b - colours[i].B))); //* 0.85);
                        pBuff[pixelIndex + 1] = (byte)((g - (g - colours[i].G))); //* 0.85);
                        pBuff[pixelIndex + 2] = (byte)((r - (r - colours[i].R))); //* 0.85);
                    }
                }
            }

            return ModyfiableBitmaps[comboColourIndex];
        }

        private static string GetSkinElement(string skinFolderPath, string skinElement)
        {
            // base example hitcircle
            // priority hitcircle@2x > no hd
            string fullPath = $"{skinFolderPath}\\{skinElement}";
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
        private static string GetJudgement(string skinFolderPath, string skinElement)
        {
            // base example  hit300
            // priority -0@2x > the non hd > hit300@2x > non
            string fullPath = $"{skinFolderPath}\\{skinElement}";
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
