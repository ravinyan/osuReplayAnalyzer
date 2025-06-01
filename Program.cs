using System.Text;

string fileName = @"C:\Users\patry\AppData\Roaming\osu\exports\ravinyan playing Okabe Keiichi - The Tower (Dear.[NieR Automata] Feryquitous Remix) (Axarian) [Popola] (2025-05-30_22-12).osr";
byte gameMode;
int version;
string beatmapHash;
string playerName;
string replayHash;
ushort numberOf300;
ushort numberOf100;
ushort numberOf50;
ushort gekis;
ushort katus;
ushort numberOfMisses;
int totalScore;
ushort maxCombo;
bool isFullCombo;
int mods;
string lifeBar;
long timeStamp;
int replayLength;
byte[] compressedReplayData;
long scoreId;
double moreModInfo;

using (FileStream stream = File.Open(fileName, FileMode.Open))
{
    using (FixedBinaryReader reader = new FixedBinaryReader(stream))
    {
        gameMode = reader.ReadByte();
        version = reader.ReadInt32();
        beatmapHash = reader.ReadString(); 
        playerName = reader.ReadString();
        replayHash = reader.ReadString();
        numberOf300 = reader.ReadUInt16();
        numberOf100 = reader.ReadUInt16();
        numberOf50 = reader.ReadUInt16();
        gekis = reader.ReadUInt16();
        katus = reader.ReadUInt16();
        numberOfMisses = reader.ReadUInt16();
        totalScore = reader.ReadInt32();
        maxCombo = reader.ReadUInt16();
        isFullCombo = reader.ReadBoolean();
        mods = reader.ReadInt32();
        lifeBar = reader.ReadString();
        timeStamp = reader.ReadInt64();
        replayLength = reader.ReadInt32();
        compressedReplayData = reader.ReadBytes(replayLength);
        scoreId = reader.ReadInt64();
        moreModInfo = reader.ReadDouble();
    }

    Console.WriteLine(gameMode);
    Console.WriteLine(version);
    Console.WriteLine(beatmapHash);
    Console.WriteLine(playerName);
    Console.WriteLine(numberOf300);
    Console.WriteLine(numberOf100);
    Console.WriteLine(numberOf50);
    Console.WriteLine(gekis);
    Console.WriteLine(katus);
    Console.WriteLine(numberOfMisses);
    Console.WriteLine(totalScore);
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