using ReplayParsers.Decoders;

while (true)
{
    BeatmapDecoder.GetOsuBeatmap();

    //BeatmapDecoder.GetOsuLazerBeatmap(@$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\osu\client.realm", fileName);

    Thread.Sleep(1000); 
}

