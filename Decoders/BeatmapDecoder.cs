using Realms;
using what.Classes.Beatmap;
using what.Classes.Beatmap.osu.BeatmapClasses;

namespace what.Decoders
{
    public class BeatmapDecoder
    {
        // this gives all beatmaps user has in osu lazer client.realm... thought it would be impossible but did i did it somehow
        // osu!lazer client.realm data
        public static List<Beatmap> GetOsuLazerRealmData(string fileName)
        {
            List<Beatmap> osuLazerRealmBeatmasData;
            RealmConfiguration config = new RealmConfiguration(fileName)
            {
                SchemaVersion = 48
            };

            using (Realm realm = Realm.GetInstance(config))
            {
                IQueryable<Beatmap> data = realm.All<Beatmap>();
                osuLazerRealmBeatmasData = data.ToList();
            }

            return osuLazerRealmBeatmasData;
        }

        // osu! beatmap data... might never do it but if osu!lazer will be impossible then will do this... or one day both
        public static Beatmap GetOsuBeatmapData(string fileName)
        {
            Beatmap beatmap = new Beatmap();
        
            string[] a = System.IO.File.ReadAllLines(fileName);
        
            return beatmap;
        }
        
        private static General GetGeneralData(FixedBinaryReader reader)
        {
            General general = new General();
        
            
        
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
        
            
        
            Console.WriteLine("Bookmarks        - " + editor.Bookmarks);
            Console.WriteLine("Distance spacing - " + editor.DistanceSpacing);
            Console.WriteLine("Beat divisor     - " + editor.BeatDivisor);
            Console.WriteLine("Grid size        - " + editor.GridSize);
        
            return editor;
        }
        
        private static Metadata GetMetadataData(FixedBinaryReader reader)
        {
            Metadata metadata = new Metadata();
        
            
        
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
