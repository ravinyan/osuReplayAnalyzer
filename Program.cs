
using what;
using what.Decoders;

while (true)
{
    string fileName = FileWatchers.LazerReplayFileWatcher();

    BeatmapDecoder.GetOsuLazerBeatmap(@$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\osu\client.realm", fileName);

    Thread.Sleep(100);
}

