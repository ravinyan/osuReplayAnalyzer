using ReplayAnalyzer.PlayfieldGameplay;
using System.IO;

namespace ReplayAnalyzer.GameplaySkin
{
    public static class SkinElement
    {
        private static string CurrentSkinFolderPath = "";
        private static string DefaultSkinFolderPath = $"{AppContext.BaseDirectory}\\Skins\\Komori - PeguLian II (PwV)";

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
        }
        
        public static string SkinPath()
        {
            //if (CurrentSkinPath == "")
            //{
            //    CurrentSkinPath = DefaultSkinpath;
            //}
            return CurrentSkinFolderPath;
            return $"{AppContext.BaseDirectory}\\Skins\\Komori - PeguLian II (PwV)";
        }

        public static string SkinPath(string fileName)
        {
            //return $"{CurrentSkinPath}\\{fileName}";
            return $"{AppContext.BaseDirectory}\\Skins\\Komori - PeguLian II (PwV)\\{fileName}";
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
