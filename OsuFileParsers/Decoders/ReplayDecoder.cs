using OsuFileParsers.Classes.Replay;
using System.Globalization;
using System.Text;

namespace OsuFileParsers.Decoders
{
    public class ReplayDecoder
    {
        public static Replay GetReplayData(string fileName, int delay)
        {
            Replay replay = new Replay();

            bool success = false;
            while (success == false)
            {
                try
                {
                    using (FileStream stream = File.Open(fileName, FileMode.Open))
                    {
                        using (FixedBinaryReader reader = new FixedBinaryReader(stream))
                        {
                            replay.GameMode = (GameMode)reader.ReadByte();
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

                            long ticks = reader.ReadInt64();
                            replay.TimeStamp = new DateTime(ticks).ToLocalTime();

                            replay.ReplayDataLength = reader.ReadInt32();

                            if (replay.ReplayDataLength > 0)
                            {
                                byte[] replayDataBytes = reader.ReadBytes(replay.ReplayDataLength);
                                byte[] decompressedBytes = LZMADecoder.Decompress(replayDataBytes);
                                string replayDataString = Encoding.UTF8.GetString(decompressedBytes);

                                replay.FramesDict = GetReplayFrames(replayDataString, delay);
                            }
                        }
                    }

                    success = true;
                }
                catch
                {
                    Console.WriteLine("file not found");
                }
            }

            return replay;
        }

        private static Dictionary<int, ReplayFrame> GetReplayFrames(string replayDataString, int delay)
        {
            Dictionary<int, ReplayFrame> frameDict = new Dictionary<int, ReplayFrame>();

            long totalTime = 0;
            int i = 1;

            ReplayFrame fakeFrame = new ReplayFrame();
            fakeFrame.Time = delay;
            frameDict.Add(0, fakeFrame);
            foreach (string s in replayDataString.Split(','))
            {
                if (s != "")
                {
                    ReplayFrame frame = new ReplayFrame();

                    string[] data = s.Split('|');
 
                    // not needed and breaks seeking stuff
                    if (long.Parse(data[0]) == -12345)
                    {
                        break;
                    }

                    totalTime += long.Parse(data[0]);
                    frame.Time = totalTime + delay;
                    frame.X = float.Parse(data[1], CultureInfo.InvariantCulture.NumberFormat);
                    frame.Y = float.Parse(data[2], CultureInfo.InvariantCulture.NumberFormat);
                    frame.Click = (Clicks)int.Parse(data[3]);

                    frameDict.Add(i, frame);
                    i++;
                }
            }
            
            return frameDict;
        }
    }
}