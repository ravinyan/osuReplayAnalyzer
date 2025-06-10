using ReplayParsers;
using ReplayParsers.Decoders;

while (true)
{
    string fileName = FileWatchers.LazerReplayFileWatcher();

    BeatmapDecoder.GetOsuLazerBeatmap(@$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\osu\client.realm", fileName);

    Thread.Sleep(1000);
}

