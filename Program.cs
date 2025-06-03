using SevenZip;
using System.Text;
using what.Classes.Replay;

string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
string osuPath = "\\osu\\exports\\";
string osuReplay = "ravinyan playing Slax - Shimmer (conesaliiid) [Oh My Mistake] (2025-06-02_20-27).osr";
string fileName = @$"{appDataPath}{osuPath}{osuReplay}";

using (FileStream stream = File.Open(fileName, FileMode.Open))
{
    Replay replay = new Replay();

    using (FixedBinaryReader reader = new FixedBinaryReader(stream))
    {
        replay.GameMode = reader.ReadByte();
        replay.GameVersion = reader.ReadInt32();
        replay.BeatmapMD5Hash = reader.ReadString();
        replay.PlayerName = reader.ReadString();
        replay.ReplayMD5Hash = reader.ReadString();
        replay.Hit300 = reader.ReadInt16();
        replay.Hit100 = reader.ReadInt16();
        replay.Hit50 = reader.ReadInt16();
        replay.Gekis = reader.ReadInt16();
        replay.Katus = reader.ReadInt16();
        replay.MissCount = reader.ReadInt16();
        replay.TotalScore = reader.ReadInt32();
        replay.MaxComboAchieved = reader.ReadInt16();
        replay.IsFullCombo = reader.ReadBoolean();
        replay.ModsUsed = (Mods)reader.ReadInt32();

        replay.LifeBarGraph = reader.ReadString();

        var ticks = reader.ReadInt64();
        replay.TimeStamp = new DateTime(ticks).ToLocalTime();
        

        replay.ReplayDataLength = reader.ReadInt32();
        replay.CompressedReplayDataLength = reader.ReadBytes(replay.ReplayDataLength);

        // for LZMA decoding thing ?
        if (replay.ReplayDataLength > 0)
        {
            // long + 2 floats + int = 20 bytes chunks

        }
        
        // score id is useless value for me for now and it doesnt return correct values so... it will just sit here... menacingly
        // together with additional mod info
        //replay.ScoreId = reader.ReadInt64();
        //replay.AdditionalModInfo = reader.ReadDouble();
    }

    Console.WriteLine("mode -           " + replay.GameMode);
    Console.WriteLine("game version -   " + replay.GameVersion);
    Console.WriteLine("map MD5 -        " + replay.BeatmapMD5Hash);
    Console.WriteLine("player name -    " + replay.PlayerName);
    Console.WriteLine("replay MD5 -     " + replay.ReplayMD5Hash);
    Console.WriteLine("300 -            " + replay.Hit300);
    Console.WriteLine("100 -            " + replay.Hit100);
    Console.WriteLine("50 -             " + replay.Hit50);
    Console.WriteLine("gekis -          " + replay.Gekis);
    Console.WriteLine("katus -          " + replay.Katus);
    Console.WriteLine("miss -           " + replay.MissCount);
    Console.WriteLine("score -          " + replay.TotalScore);
    Console.WriteLine("max combo -      " + replay.MaxComboAchieved);
    Console.WriteLine("is full combo -  " + replay.IsFullCombo);
    Console.WriteLine("mods used -      " + replay.ModsUsed);
    Console.WriteLine("lifebar -        " + replay.LifeBarGraph);
    Console.WriteLine("timestamp -      " + replay.TimeStamp);
    Console.WriteLine("replay length -  " + replay.ReplayDataLength);
    Console.WriteLine("replay data -    " + replay.CompressedReplayDataLength);
    Console.WriteLine("score id -       " + replay.ScoreId);
    Console.WriteLine("additional mod - " + replay.AdditionalModInfo);
    Console.WriteLine("done");
    Console.WriteLine();
}


public class FixedBinaryReader : BinaryReader
{
    public FixedBinaryReader(Stream stream) : base(stream, Encoding.UTF8) { }

    // you stupid not working correctly as i want function who gave me big headache i personally want to kick you in the face if you had one
    public override string ReadString()
    {
        if (ReadByte() == 0)
        {
            return null!;
        }

        return base.ReadString();
    }

}