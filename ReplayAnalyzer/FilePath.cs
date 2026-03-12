using System.IO;

namespace ReplayAnalyzer
{
    public static class FilePath
    {
        public static string GetBeatmapFilePath()
        {
            return Directory.GetFiles($"{AppContext.BaseDirectory}osu\\Beatmap").First();
        }

        public static string GetReplayPath()
        {
            DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}osu\\Replay");
            FileInfo file = dir.GetFiles().First();
            return file.FullName;
        }

        public static string GetReplayName()
        {
            DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}osu\\Replay");
            FileInfo file = dir.GetFiles().First();
            return file.Name;
        }

        public static string GetBeatmapAudioPath()
        {
            return Directory.GetFiles($"{AppContext.BaseDirectory}osu\\Audio").First();
        }

        public static string GetBeatmapBackgroundPath()
        {
            return Directory.GetFiles($"{AppContext.BaseDirectory}osu\\Background").First();
        }

        public static string[] GetBeatmapHitsoundPath()
        {
            return Directory.GetFiles($"{AppContext.BaseDirectory}osu\\Hitsounds");
        }
    }
}
