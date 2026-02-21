using System.IO;

namespace ReplayAnalyzer.GameplaySkin
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
            return $"{AppContext.BaseDirectory}\\Skin\\Komori - PeguLian II (PwV)";
        }

        public static string SkinPath(string fileName)
        {
            return $"{AppContext.BaseDirectory}\\Skin\\Komori - PeguLian II (PwV)\\{fileName}";
        }

        public static string Cursor()
        {
            return $"{SkinPath()}\\cursor.png";
        }

        public static string ApproachCircle()
        {
            if (File.Exists($"{SkinPath()}\\approachcircle@2x.png"))
            {
                return $"{SkinPath()}\\approachcircle@2x.png";
            }

            return $"{SkinPath()}\\approachcircle.png";
        }

        // this uses enough ram already so i dont want to use higher res circles...
        // it makes no difference anyway in looks
        public static string HitCircle()
        {
            if (File.Exists($"{SkinPath()}\\hitcircle@2x.png"))
            {
                return $"{SkinPath()}\\hitcircle@2x.png";
            }

            return $"{SkinPath()}\\hitcircle.png";
        }

        public static string HitCircleOverlay()
        {
            if (File.Exists($"{SkinPath()}\\hitcircleoverlay@2x.png"))
            {
                return $"{SkinPath()}\\hitcircleoverlay@2x.png";
            }

            return $"{SkinPath()}\\hitcircleoverlay.png";
        }

        public static string ComboNumber(int number)
        {
            if (File.Exists($"{SkinPath()}\\combo-{number}@2x.png"))
            {
                return $"{SkinPath()}\\combo-{number}@2x.png";
            }

            return $"{SkinPath()}\\combo-{number}.png";
        }

        public static string ComboNumber(string number)
        {
            if (File.Exists($"{SkinPath()}\\combo-{number}@2x.png"))
            {
                return $"{SkinPath()}\\combo-{number}@2x.png";
            }

            return $"{SkinPath()}\\combo-{number}.png";
        }

        public static string ComboNumber(char number)
        {
            if (File.Exists($"{SkinPath()}\\combo-{number}@2x.png"))
            {
                return $"{SkinPath()}\\combo-{number}@2x.png";
            }

            return $"{SkinPath()}\\combo-{number}.png";
        }

        public static string ReverseArrow()
        {
            if (File.Exists($"{SkinPath()}\\reversearrow@2x.png"))
            {
                return $"{SkinPath()}\\reversearrow@2x.png";
            }

            return $"{SkinPath()}\\reversearrow.png";
        }

        public static string SliderBall()
        {
            if (File.Exists($"{SkinPath()}\\sliderb0@2x.png"))
            {
                return $"{SkinPath()}\\sliderb0@2x.png";
            }

            return $"{SkinPath()}\\sliderb0.png";
        }

        public static string SliderBallCircle()
        {
            if (File.Exists($"{SkinPath()}\\sliderfollowcircle@2x.png"))
            {
                return $"{SkinPath()}\\sliderfollowcircle@2x.png";
            }

            return $"{SkinPath()}\\sliderfollowcircle.png";
        }

        public static string Hit300()
        {
            if (File.Exists(SkinPath("hit300-0.png")) || File.Exists(SkinPath("hit300-0@2x.png")))
            {
                // animation?
                if (File.Exists($"{SkinPath()}\\hit300-0@2x.png"))
                {
                    return $"{SkinPath()}\\hit300-0@2x.png";
                }

                return SkinPath("hit300-0.png");
            }
            else
            {
                if (File.Exists($"{SkinPath()}\\hit300@2x.png"))
                {
                    return $"{SkinPath()}\\hit300@2x.png";
                }

                return $"{SkinPath()}\\hit300.png";
            }
        }

        public static string Hit100()
        {
            if (File.Exists(SkinPath("hit100-0.png")) || File.Exists(SkinPath("hit100-0@2x.png")))
            {
                // animation?
                if (File.Exists($"{SkinPath()}\\hit100-0@2x.png"))
                {
                    return $"{SkinPath()}\\hit100-0@2x.png";
                }

                return SkinPath("hit100-0.png");
            }
            else
            {
                if (File.Exists($"{SkinPath()}\\hit100@2x.png"))
                {
                    return $"{SkinPath()}\\hit100@2x.png";
                }

                return $"{SkinPath()}\\hit100.png";
            }
        }

        public static string Hit50()
        {
            if (File.Exists(SkinPath("hit50-0.png")) || File.Exists(SkinPath("hit50-0@2x.png")))
            {
                // animation?
                if (File.Exists($"{SkinPath()}\\hit50-0@2x.png"))
                {
                    return $"{SkinPath()}\\hit50-0@2x.png";
                }

                return SkinPath("hit50-0.png");
            }
            else
            {
                if (File.Exists($"{SkinPath()}\\hit50@2x.png"))
                {
                    return $"{SkinPath()}\\hit50@2x.png";
                }

                return $"{SkinPath()}\\hit50.png";
            }
        }

        public static string HitMiss()
        {
            if (File.Exists(SkinPath("hit0-0.png")) || File.Exists(SkinPath("hit0-0@2x.png")))
            {
                // animation?
                if (File.Exists($"{SkinPath()}\\hit0-0@2x.png"))
                {
                    return $"{SkinPath()}\\hit0-0@2x.png";
                }

                return SkinPath("hit0-0.png");
            }
            else
            {
                if (File.Exists($"{SkinPath()}\\hit0@2x.png"))
                {
                    return $"{SkinPath()}\\hit0@2x.png";
                }

                return $"{SkinPath()}\\hit0.png";
            }
        }

        public static string SliderEndMiss()
        {
            if (File.Exists($"{SkinPath()}\\sliderendmiss@2x.png"))
            {
                return $"{SkinPath()}\\sliderendmiss@2x.png";
            }

            return $"{SkinPath()}\\sliderendmiss.png";
        }

        public static string SliderTickMiss()
        {
            if (File.Exists($"{SkinPath()}\\slidertickmiss@2x.png"))
            {
                return $"{SkinPath()}\\slidertickmiss@2x.png";
            }

            return $"{SkinPath()}\\slidertickmiss.png";
        }

        public static string SliderTick()
        {
            if (File.Exists($"{SkinPath()}\\sliderscorepoint@2x.png"))
            {
                return $"{SkinPath()}\\sliderscorepoint@2x.png";
            }

            return $"{SkinPath()}\\sliderscorepoint.png";
        }

        // 99.99% i wont implement it but just in case i feel like doing it
        // spinner-rpm.png | spinner-clear.png | spinner-spin.png | spinner-metre.png and some more https://osu.ppy.sh/wiki/en/Skinning/osu%21
        public static string SpinnerApproachCircle()
        {
            if (File.Exists($"{SkinPath()}\\spinner-approachcircle@2x.png"))
            {
                return $"{SkinPath()}\\spinner-approachcircle@2x.png";
            }

            return $"{SkinPath()}\\spinner-approachcircle.png";
        }

        public static string SpinnerBackground()
        {
            if (File.Exists($"{SkinPath()}\\spinner-background@2x.png"))
            {
                return $"{SkinPath()}\\spinner-background@2x.png";
            }

            return $"{SkinPath()}\\spinner-background.png";
        }

        public static string SpinnerCircle()
        {
            if (File.Exists($"{SkinPath()}\\spinner-circle@2x.png"))
            {
                return $"{SkinPath()}\\spinner-circle@2x.png";
            }

            return $"{SkinPath()}\\spinner-circle.png";
        }
    }
}
