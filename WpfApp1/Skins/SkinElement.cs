namespace WpfApp1.Skins
{
    public static class SkinElement
    {
        public static void UpdateSkinPath()
        {
            // dont know if i will do it depends
        }

        private static string SkinPath()
        {
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuFileParser\\WpfApp1\\Skins\\Komori - PeguLian II (PwV)";
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
            if (!System.IO.File.Exists($"{SkinPath()}\\hitcircle.png"))
            {
                return $"{SkinPath()}\\hitcircle@2x.png";
            }

            return $"{SkinPath()}\\hitcircle.png";
        }

        public static string HitCircleOverlay()
        {
            if (!System.IO.File.Exists($"{SkinPath()}\\hitcircleoverlay.png"))
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
    }
}
