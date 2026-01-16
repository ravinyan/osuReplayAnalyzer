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
                            replay.StableMods = (Mods)reader.ReadInt32();

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

                            long onlineid = reader.ReadInt64();

                            // this is lazer data so if end of stream is reached this will just poof
                            try
                            {
                                int osuLazerDataLength = reader.ReadInt32();
                                byte[] osuLazerDataBytes = reader.ReadBytes(osuLazerDataLength);
                                byte[] decompressedLazerBytes = LZMADecoder.Decompress(osuLazerDataBytes);
                                string lazerDataString = Encoding.UTF8.GetString(decompressedLazerBytes);

                                replay.LazerMods = GetLazerMods(lazerDataString);
                                replay.IsLazer = true;
                            }
                            catch
                            {
                                success = true;
                            }
                        }
                    }

                    success = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }

            return replay;
        }

        private static Dictionary<int, ReplayFrame> GetReplayFrames(string replayDataString, int delay)
        {
            Dictionary<int, ReplayFrame> frameDict = new Dictionary<int, ReplayFrame>();

            long totalTime = 0;
            int i = 0;

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

        private static List<LazerMod> GetLazerMods(string data)
        {
            List<string> parsedData = ParseModsDataFromString(data);

            List<LazerMod> mods = new List<LazerMod>();

            // after everything is filtered and only acronyms and settings remain then create lazer mods for them
            // i = 2 to skip indexes that are "mods" and "acronym"
            for (int i = 2; i < parsedData.Count; i++)
            {
                LazerMod mod = new LazerMod();

                mod.Acronym = parsedData[i++];

                if (i == parsedData.Count || parsedData[i] != "settings")
                {
                    mods.Add(mod);
                    continue;
                }

                i++; // skip "settings"
                while (i < parsedData.Count && parsedData[i] != "acronym")
                {
                    mod.Settings.Add(parsedData[i++], parsedData[i++]);
                }

                mods.Add(mod);
            }

            return mods;
        }

        private static List<string> ParseModsDataFromString(string data)
        {
            string modsData = "";
            char prevChar = '.';
            bool modsFound = false;
            foreach (char c in data)
            {
                if (modsFound == true || (prevChar == '"' && c == 'm'))
                {
                    modsFound = true;

                    // filter out json
                    if (c == '"' || c == '[' || c == ']' || c == '{' || c == '}' || c == '\r' || c == '\n')
                    {
                        if (c == ']')
                        {
                            // mods section ended
                            break;
                        }

                        continue;
                    }

                    modsData = @$"{modsData}{c}";
                }

                prevChar = c;
            }

            string[] splitDataString = modsData.Split(":");
            List<string> parsedDataList = new List<string>();
            foreach (string s in splitDataString)
            {
                if (s.Contains(','))
                {
                    string[] split = s.Split(',');
                    foreach (string s2 in split)
                    {
                        parsedDataList.Add(s2.Trim());
                    }

                    continue;
                }

                parsedDataList.Add(s.Trim());
            }

            return parsedDataList;
        }
    }
}