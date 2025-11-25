using System.IO;

namespace ReplayAnalyzer.Skins
{
    public static class SkinElement
    {
        // https://osu.ppy.sh/wiki/en/Skinning/osu%21
        // https://osu.ppy.sh/wiki/en/Skinning/Interface
        public static void UpdateSkinPath()
        {
            // dont know if i will do it depends
        }

        public static string SkinPath()
        {
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuReplayAnalyzer\\ReplayAnalyzer\\Skins\\Komori - PeguLian II (PwV)";
        }

        public static string SkinPath(string fileName)
        {
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuReplayAnalyzer\\ReplayAnalyzer\\Skins\\Komori - PeguLian II (PwV)\\{fileName}";
        }

        public static string Cursor()
        {
            return $"{SkinPath()}\\cursor.png";
        }

        public static string ApproachCircle()
        {
            return $"{SkinPath()}\\approachcircle.png";
        }

        // this uses enough ram already so i dont want to use higher res circles...
        // it makes no difference anyway in looks
        public static string HitCircle()
        {
            if (!File.Exists($"{SkinPath()}\\hitcircle.png"))
            {
                return $"{SkinPath()}\\hitcircle@2x.png";
            }

            return $"{SkinPath()}\\hitcircle.png";
        }

        public static string HitCircleOverlay()
        {
            if (!File.Exists($"{SkinPath()}\\hitcircleoverlay.png"))
            {
                return $"{SkinPath()}\\hitcircleoverlay@2x.png";
            }

            return $"{SkinPath()}\\hitcircleoverlay.png";
        }

        public static string ComboNumber(int number)
        {
            return $"{SkinPath()}\\combo-{number}.png";
        }

        public static string ComboNumber(string number)
        {
            return $"{SkinPath()}\\combo-{number}.png";
        }

        public static string ComboNumber(char number)
        {
            return $"{SkinPath()}\\combo-{number}.png";
        }

        public static string ReverseArrow()
        {
            return $"{SkinPath()}\\reversearrow.png";
        }

        public static string SliderBall()
        {
            return $"{SkinPath()}\\sliderb0.png";
        }

        public static string SliderBallCircle()
        {
            return $"{SkinPath()}\\sliderfollowcircle.png";
        }

        public static string Hit300()
        {
            if (File.Exists(SkinPath("hit300-0.png")))
            {
                // animation?
                return SkinPath("hit300-0.png");
            }
            else
            {
                return $"{SkinPath()}\\hit300.png";
            }
        }

        public static string Hit100()
        {
            if (File.Exists(SkinPath("hit100-0.png")))
            {
                // animation?
                return SkinPath("hit100-0.png");
            }
            else
            {
                return $"{SkinPath()}\\hit100.png";
            }
        }

        public static string Hit50()
        {
            if (File.Exists(SkinPath("hit50-0.png")))
            {
                // animation?
                return SkinPath("hit50-0.png");
            }
            else
            {
                return $"{SkinPath()}\\hit50.png";
            }
        }

        public static string HitMiss()
        {
            if (File.Exists(SkinPath("hit0-0.png")))
            {
                // animation?
                return SkinPath("hit0-0.png");
            }
            else
            {
                return $"{SkinPath()}\\hit0.png";
            }
        }

        public static string SliderEndMiss()
        {
            return $"{SkinPath()}\\sliderendmiss.png";
        }

        public static string SliderTickMiss()
        {
            return $"{SkinPath()}\\slidertickmiss.png";
        }

        public static string SliderTick()
        {
            return $"{SkinPath()}\\sliderscorepoint.png";
        }

        // 99.99% i wont implement it but just in case i feel like doing it
        // spinner-rpm.png | spinner-clear.png | spinner-spin.png | spinner-metre.png and some more https://osu.ppy.sh/wiki/en/Skinning/osu%21
        public static string SpinnerApproachCircle()
        {
            return $"{SkinPath()}\\spinner-approachcircle.png";
        }

        public static string SpinnerBackground()
        {
            return $"{SkinPath()}\\spinner-background.png";
        }

        public static string SpinnerCircle()
        {
            return $"{SkinPath()}\\spinner-circle.png";
        }
    }
}
