using System.IO;

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
            return Directory.GetFiles($"{AppContext.BaseDirectory}\\osu\\Audio").Single();
        }

        public static string GetBeatmapBackgroundPath()
        {
            return Directory.GetFiles($"{AppContext.BaseDirectory}\\osu\\Background").Single();
        }

        public static string[] GetBeatmapHitsoundPath()
        {
            return Directory.GetFiles($"{AppContext.BaseDirectory}\\osu\\Hitsounds");
        }

        public static string GetSkinPath()
        {
            return $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\source\\repos\\OsuFileParser\\WpfApp1\\Skins\\Komori - PeguLian II (PwV)";
        }
    }
}
