using System.Globalization;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using what.Classes.Replay;

namespace what.Decoders
{
    public class ReplayDecoder
    {
        public static Replay GetReplayData(string fileName)
        {
            Replay replay = new Replay();

            using (FileStream stream = File.Open(fileName, FileMode.Open))
            {
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

                    if (replay.ReplayDataLength > 0)
                    {
                        byte[] replayDataBytes = reader.ReadBytes(replay.ReplayDataLength);
                        byte[] decompressedBytes = LZMADecoder.Decompress(replayDataBytes);
                        string replayDataString = Encoding.UTF8.GetString(decompressedBytes);

                        foreach (string s in replayDataString.Split(','))
                        {
                            if (s != "")
                            {
                                ReplayFrame frame = new ReplayFrame();

                                string[] data = s.Split('|');

                                frame.TimeBetweenActions = long.Parse(data[0]);
                                frame.X = float.Parse(data[1], CultureInfo.InvariantCulture.NumberFormat);
                                frame.Y = float.Parse(data[2], CultureInfo.InvariantCulture.NumberFormat);
                                frame.Click = (Clicks)int.Parse(data[3]);

                                replay.Frames!.Add(frame);
                            }
                        }
                    }
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
                Console.WriteLine();
            }

            return replay;
        }
    }
}
