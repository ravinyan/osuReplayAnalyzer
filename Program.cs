
using what;
using what.Decoders;

while (true)
{

    //string fileName = FileWatchers.LazerReplayFileWatcher();
    
    Thread.Sleep(100);

    //ReplayDecoder.GetReplayData(fileName);
    //Slax - Shimmer (conesaliiid) [Oh My Mistake].osu
    BeatmapDecoder.GetOsuLazerRealmData(@$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\osu\client.realm");
}

