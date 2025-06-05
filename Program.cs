
using what;
using what.Decoders;

while (true)
{
    //string fileName = FileWatchers.LazerReplayFileWatcher();

    Thread.Sleep(100);

    //ReplayDecoder.GetReplayData(fileName);

    BeatmapDecoder.GetBeatmapData(@$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\osu\exports\Slax - Shimmer (conesaliiid) [Oh My Mistake].osu");
}

