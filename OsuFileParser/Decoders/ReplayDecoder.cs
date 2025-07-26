using ReplayParsers.Classes.Replay;
using System.Globalization;
using System.Text;

namespace ReplayParsers.Decoders
{
    public class ReplayDecoder
    {
        public static Replay GetReplayData(string fileName)
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

                                replay.Frames = GetReplayFrames(replayDataString);
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

        private static List<ReplayFrame> GetReplayFrames(string replayDataString)
        {
            List<ReplayFrame> replayFrames = new List<ReplayFrame>();

            long totalTime = 0;
            foreach (string s in replayDataString.Split(','))
            {
                if (s != "")
                {
                    ReplayFrame frame = new ReplayFrame();

                    string[] data = s.Split('|');

                    totalTime += long.Parse(data[0]);
                    frame.Time = totalTime;
                    frame.X = float.Parse(data[1], CultureInfo.InvariantCulture.NumberFormat);
                    frame.Y = float.Parse(data[2], CultureInfo.InvariantCulture.NumberFormat);
                    frame.Click = (Clicks)int.Parse(data[3]);

                    replayFrames.Add(frame);
                }
            }
            
            return replayFrames;
        }
    }
}