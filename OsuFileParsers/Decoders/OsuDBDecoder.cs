using OsuFileParsers.Classes.Beatmap.osu.OsuDB;
using OsuFileParsers.Classes.Replay;

namespace OsuFileParsers.Decoders
{
    public class OsuDBDecoder
    {
        public static OsuDB GetOsuDBData()
        {
            string dbPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!\\osu!.db";

            OsuDB osuDB = new OsuDB();
            List<OsuDBBeatmap> beatmapList = new List<OsuDBBeatmap>();

            using (FileStream stream = File.Open(dbPath, FileMode.Open))
            {
                using (FixedBinaryReader reader = new FixedBinaryReader(stream))
                {
                    osuDB.Version = reader.ReadInt32();
                    osuDB.FolderCount = reader.ReadInt32();
                    osuDB.AccountUnlocked = reader.ReadBoolean();

                    long ticks = reader.ReadInt64();
                    osuDB.WhenUnlocked = new DateTime(ticks).ToLocalTime();

                    osuDB.PlayerName = reader.ReadString();
                    osuDB.NumberOfBeatmaps = reader.ReadInt32();

                    for (int i = 0; i < osuDB.NumberOfBeatmaps; i++)
                    {
                        OsuDBBeatmap beatmap = new OsuDBBeatmap();

                        if (osuDB.Version < 20191106)
                        {
                            beatmap.BeatmapByteSize = reader.ReadInt32();
                        }

                        beatmap.Artist = reader.ReadString();
                        beatmap.ArtistUnicode = reader.ReadString();
                        beatmap.SongTitle = reader.ReadString();
                        beatmap.SongTitleUnicode = reader.ReadString();
                        beatmap.Creator = reader.ReadString();
                        beatmap.Difficulty = reader.ReadString();
                        beatmap.AudioFileName = reader.ReadString();
                        beatmap.BeatmapMD5Hash = reader.ReadString();
                        beatmap.BeatmapFileName = reader.ReadString();
                        beatmap.RankedStatus = (RankedStatus)reader.ReadByte();
                        beatmap.NumberOfCircles = reader.ReadInt16();
                        beatmap.NumberOfSliders = reader.ReadInt16();
                        beatmap.NumberOfSpinners = reader.ReadInt16();
                        beatmap.LastModificationTimeInTicks = reader.ReadInt64();
                        beatmap.ApproachRate = reader.ReadSingle();
                        beatmap.CircleSize = reader.ReadSingle();
                        beatmap.HpDrain = reader.ReadSingle();
                        beatmap.OverallDifficulty = reader.ReadSingle();
                        beatmap.SliderVelocity = reader.ReadDouble();

                        if (osuDB.Version >= 20140609)
                        {
                            beatmap.OsuSTDModSR   = GetStarRatings(reader, osuDB.Version);
                            beatmap.OsuTaikoModSR = GetStarRatings(reader, osuDB.Version);
                            beatmap.OsuCatchModSR = GetStarRatings(reader, osuDB.Version);
                            beatmap.OsuManiaModSR = GetStarRatings(reader, osuDB.Version); 
                        }

                        beatmap.DrainTime = reader.ReadInt32();
                        beatmap.TotalTime = reader.ReadInt32();
                        beatmap.AudioPreviewStartTime = reader.ReadInt32();

                        int timingPointsCount = reader.ReadInt32();
                        for (int j = 0; j < timingPointsCount; j++)
                        {
                            OsuDBTimingPoint timingPoint = new OsuDBTimingPoint();

                            timingPoint.BPM = reader.ReadDouble();
                            timingPoint.Offset = reader.ReadDouble();
                            timingPoint.IsNotInherited = reader.ReadBoolean();

                            beatmap.TimingPoints!.Add(timingPoint);
                        }

                        beatmap.DifficultyID = reader.ReadInt32();
                        beatmap.BeatmapID = reader.ReadInt32();
                        beatmap.ThreadID = reader.ReadInt32();
                        beatmap.OsuSTDGrade = reader.ReadByte();
                        beatmap.OsuTaikoGrade = reader.ReadByte();
                        beatmap.OsuCatchGrade = reader.ReadByte();
                        beatmap.OsuManiaGrade = reader.ReadByte();
                        beatmap.LocalBeatmapOffset = reader.ReadInt16();
                        beatmap.StackLeniency = reader.ReadSingle();
                        beatmap.GameMode = (GameMode)reader.ReadByte();
                        beatmap.SongSource = reader.ReadString();
                        beatmap.SongTags = reader.ReadString();
                        beatmap.OnlineOffset = reader.ReadInt16();
                        beatmap.SongTitleFont = reader.ReadString();
                        beatmap.IsBeatmapUnplayed = reader.ReadBoolean();
                        beatmap.LastTimePlayed = reader.ReadInt64();
                        beatmap.IsBeatmapOSZ2 = reader.ReadBoolean();
                        beatmap.BeatmapFolderName = reader.ReadString();
                        beatmap.LastTimeCheckedAgainsOsuRepository = reader.ReadInt64();
                        beatmap.IgnoreBeatmapSound = reader.ReadBoolean();
                        beatmap.IgnoreBeatmapSkin = reader.ReadBoolean();
                        beatmap.DisableStoryboard = reader.ReadBoolean();
                        beatmap.DisableVideo = reader.ReadBoolean();
                        beatmap.VisualOverride = reader.ReadBoolean();

                        if (osuDB.Version < 20140609)
                        {
                            beatmap.Unknown = reader.ReadInt16();
                        }
                        
                        beatmap.LastModificationTime = reader.ReadInt32();
                        beatmap.OsuManiaScrollSpeed = reader.ReadByte();

                        beatmapList.Add(beatmap);
                    }

                    osuDB.UserPermission = (UserPermission)reader.ReadInt32();
                }

                osuDB.DBBeatmaps = beatmapList;
            }

            return osuDB;
        }

        private static List<(Mods, double)> GetStarRatings(FixedBinaryReader reader, int version)
        {
            List<(Mods, double)> list = new List<(Mods, double)>();

            int numberOfInt_FloatPairs = reader.ReadInt32();
            for (int j = 0; j < numberOfInt_FloatPairs; j++)
            {
                // int byte
                byte b1 = reader.ReadByte();
                Mods mods = (Mods)reader.ReadInt32();

                // float/double byte
                byte b2 = reader.ReadByte();

                double starRating = 0;
                if (version > 20250107)
                {
                    starRating = reader.ReadSingle();
                }
                else
                {
                    starRating = reader.ReadDouble();
                }

                list.Add((mods, starRating));
            }

            return list;
        }
    }
}
