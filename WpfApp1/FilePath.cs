namespace WpfApp1
{
    public static class FilePath
    {
        // will test what will be better coz i have no clue and google doesnt help
        // AppDomain.CurrentDomain.BaseDirectory
        // or
        // AppContext.BaseDirectory
        public static string GetBeatmapAudioPath()
        {
            return $@"{AppContext.BaseDirectory}\osu\Audio\audio.mp3";
        }

        public static string GetBeatmapBackgroundPath()
        {
            return $@"{AppContext.BaseDirectory}\osu\Background\bg.jpg";
        }

        public static string GetBeatmapHitsoundPath()
        {
            return null;
        }

        public static string GetSkinPath()
        {
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuFileParser\\WpfApp1\\Skins\\Komori - PeguLian II (PwV)";
        }
    }
}
