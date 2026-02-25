using System.IO;

namespace ReplayAnalyzer
{
    public static class FilePath
    {
        public static string GetBeatmapAudioPath()
        {
            return Directory.GetFiles($"{AppContext.BaseDirectory}\\osu\\Audio").First();
        }

        public static string GetBeatmapBackgroundPath()
        {
            return Directory.GetFiles($"{AppContext.BaseDirectory}\\osu\\Background").First();
        }

        public static string[] GetBeatmapHitsoundPath()
        {
            return Directory.GetFiles($"{AppContext.BaseDirectory}\\osu\\Hitsounds");
        }
    }
}
