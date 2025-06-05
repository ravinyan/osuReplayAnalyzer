using what.Classes.Beatmap;
using what.Classes.Beatmap.BeatmapClasses;

namespace what.Decoders
{
    public class BeatmapDecoder
    {
        // scuffed implementation now fix after scuffed done
        public static Beatmap GetBeatmapData(string fileName)
        {
            Beatmap beatmap = new Beatmap();

            using (Stream stream = new FileStream(fileName, FileMode.Open))
            {
                using (FixedBinaryReader reader = new FixedBinaryReader(stream))
                {
                    General general = GetGeneralData(reader);
                    Editor editor = GetEditorData(reader);
                    Metadata metadata = GetMetadataData(reader);
                    Difficulty difficulty = GetDifficultyData(reader);
                    Events events = GetEventsData(reader);
                    //List<TimingPoints> timingPoints = GetTimingPointsData(reader);
                    //Colours colours = GetColoursData(reader);
                    //List<HitObjects> hitObjects = GetHitObjectsData(reader);

                    beatmap.General = general;
                    beatmap.Editor = editor;
                    beatmap.Metadata = metadata;
                    beatmap.Difficulty = difficulty;
                    beatmap.Events = events;
                    // its not finished yet
                    //beatmap.TimingPoints = timingPoints;
                    //beatmap.Colours = colours;
                    //beatmap.HitObjects = hitObjects;
                }
            }

            return beatmap;
        }

        private static General GetGeneralData(FixedBinaryReader reader)
        {
            General general = new General();

            general.AudioFileName = reader.ReadString();
            general.AudioLeadIn = reader.ReadInt32();
            general.AudioHash = reader.ReadString();
            general.PreviewTime = reader.ReadInt32();
            general.Countdown = reader.ReadInt32();
            general.SampleSet = reader.ReadString();
            general.StackLeniency = reader.ReadDecimal();
            general.Mode = reader.ReadInt32();
            general.LetterboxInBreaks = reader.ReadBoolean();
            general.StoryFireInFront = reader.ReadBoolean();
            general.UseSkinSprites = reader.ReadBoolean();
            general.AlwaysShowPlayfield = reader.ReadBoolean();
            general.OverlayPosition = reader.ReadString();
            general.SkinPreference = reader.ReadString();
            general.EpilepsyWarning = reader.ReadBoolean();
            general.CountdownOffset = reader.ReadInt32();
            general.SpecialStyle = reader.ReadBoolean();
            general.WidescreenStoryboard = reader.ReadBoolean();
            general.SamplesMatchPlaybackRate = reader.ReadBoolean();

            Console.WriteLine("Audio File Name             - " + general.AudioFileName);
            Console.WriteLine("Audio Lead In               - " + general.AudioLeadIn);
            Console.WriteLine("Audio Hash                  - " + general.AudioHash);
            Console.WriteLine("Preview Time                - " + general.PreviewTime);
            Console.WriteLine("Countdown                   - " + general.Countdown);
            Console.WriteLine("Sample set                  - " + general.SampleSet);
            Console.WriteLine("Stack Leniency              - " + general.StackLeniency);
            Console.WriteLine("Mode                        - " + general.Mode);
            Console.WriteLine("Letterbox in breaks         - " + general.LetterboxInBreaks);
            Console.WriteLine("Story Fire In Front         - " + general.StoryFireInFront);
            Console.WriteLine("Use skin sprites            - " + general.UseSkinSprites);
            Console.WriteLine("Always show playfield       - " + general.AlwaysShowPlayfield);
            Console.WriteLine("Overlay position            - " + general.OverlayPosition);
            Console.WriteLine("Sking preference            - " + general.SkinPreference);
            Console.WriteLine("Epilepsy warning            - " + general.EpilepsyWarning);
            Console.WriteLine("Countdown offset            - " + general.CountdownOffset);
            Console.WriteLine("Special style               - " + general.SpecialStyle);
            Console.WriteLine("Widescreen storyboard       - " + general.WidescreenStoryboard);
            Console.WriteLine("Samples match playback rate - " + general.SamplesMatchPlaybackRate);

            return general;
        }

        private static Editor GetEditorData(FixedBinaryReader reader)
        {
            Editor editor = new Editor();

            editor.Bookmarks = reader.ReadString();
            editor.DistanceSpacing = reader.ReadDecimal();
            editor.BeatDivisor = reader.ReadInt32();
            editor.GridSize = reader.ReadInt32();
            editor.TimelineZoom = reader.ReadDecimal();

            Console.WriteLine("Bookmarks        - " + editor.Bookmarks);
            Console.WriteLine("Distance spacing - " + editor.DistanceSpacing);
            Console.WriteLine("Beat divisor     - " + editor.BeatDivisor);
            Console.WriteLine("Grid size        - " + editor.GridSize);

            return editor;
        }

        private static Metadata GetMetadataData(FixedBinaryReader reader)
        {
            Metadata metadata = new Metadata();

            metadata.Title = reader.ReadString();
            metadata.TitleUnicode = reader.ReadString();
            metadata.Artist = reader.ReadString();
            metadata.ArtistUnicode = reader.ReadString();
            metadata.Creator = reader.ReadString();
            metadata.Version = reader.ReadString();
            metadata.Source = reader.ReadString();
            metadata.Tags = reader.ReadString();
            metadata.BeatmapId = reader.ReadInt32();
            metadata.BeatmapSetId = reader.ReadInt32();

            Console.WriteLine("Title         - " + metadata.Title);
            Console.WriteLine("TitleUnicode  - " + metadata.TitleUnicode);
            Console.WriteLine("Artist        - " + metadata.Artist);
            Console.WriteLine("ArtistUnicode - " + metadata.ArtistUnicode);
            Console.WriteLine("Creator       - " + metadata.Creator);
            Console.WriteLine("Version       - " + metadata.Version);
            Console.WriteLine("Source        - " + metadata.Source);
            Console.WriteLine("Tags          - " + metadata.Tags);
            Console.WriteLine("BeatmapID     - " + metadata.BeatmapId);
            Console.WriteLine("BeatmapSetID  - " + metadata.BeatmapSetId);

            return metadata;
        }

        private static Difficulty GetDifficultyData(FixedBinaryReader reader)
        {
            Difficulty difficulty = new Difficulty();

            difficulty.HPDrainRate = reader.ReadDecimal();
            difficulty.CircleSize = reader.ReadDecimal();
            difficulty.OverallDifficulty = reader.ReadDecimal();
            difficulty.ApproachRate = reader.ReadDecimal();
            difficulty.SliderMultiplier = reader.ReadDecimal();
            difficulty.SliderTickRate = reader.ReadDecimal();

            Console.WriteLine("HP DR              - " + difficulty.HPDrainRate);
            Console.WriteLine("CS                 - " + difficulty.CircleSize);
            Console.WriteLine("OD                 - " + difficulty.OverallDifficulty);
            Console.WriteLine("AR                 - " + difficulty.ApproachRate);
            Console.WriteLine("Slider multiplayer - " + difficulty.SliderMultiplier);
            Console.WriteLine("Slidetr tickrate   - " + difficulty.SliderTickRate);

            return difficulty;
        }

        private static Events GetEventsData(FixedBinaryReader reader)
        {
            Events events = new Events();

            events.Backgrounds = reader.ReadString();
            events.Videos = reader.ReadString();
            events.Breaks = reader.ReadString();

            return events;
        }

        private static List<TimingPoints> GetTimingPointsData(FixedBinaryReader reader)
        {
            TimingPoints timingPoints = new TimingPoints();



            return null;
        }

        private static Colours GetColoursData(FixedBinaryReader reader)
        {
            Colours colours = new Colours();



            return colours;
        }

        private static List<HitObjects> GetHitObjectsData(FixedBinaryReader reader)
        {
            HitObjects hitObjects = new HitObjects();



            return null;
        }
    }
}
