using Realms;
using System.Drawing;
using System.Globalization;
using System.Runtime.CompilerServices;
using what.Classes.Beatmap.osu;
using what.Classes.Beatmap.osu.BeatmapClasses;
using what.Classes.Beatmap.osu.Objects;
using what.Classes.Replay;

namespace what.Decoders
{
    public class BeatmapDecoder
    {
        // gets full beatmap data from osu lazer replay
        public static Beatmap GetOsuLazerBeatmap(string realmFilePath, string replayFilePath)
        {
            RealmConfiguration config = new RealmConfiguration(realmFilePath) { SchemaVersion = 48 };

            string beatmapFilePath = "";
            Replay replay = ReplayDecoder.GetReplayData(replayFilePath);
            List<Classes.Beatmap.osuLazer.Beatmap> beatmaps;

            using (Realm realm = Realm.GetInstance(config))
            {
                IQueryable<Classes.Beatmap.osuLazer.Beatmap> realmData = realm.All<Classes.Beatmap.osuLazer.Beatmap>();
                beatmaps = realmData.ToList();
                Classes.Beatmap.osuLazer.Beatmap beatmap = beatmaps.Find(h => h.MD5Hash == replay.BeatmapMD5Hash)!;

                beatmapFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{beatmap.Hash![0]}\\{beatmap.Hash.Substring(0, 2)}\\{beatmap.Hash}";
            }

            string[] beatmapProperties = File.ReadAllLines(beatmapFilePath);

            string currentSection = "";
            List<string> sectionProperties = new List<string>();
            Beatmap osuBeatmap = new Beatmap();

            foreach (string property in beatmapProperties)
            {
                if (!string.IsNullOrWhiteSpace(property) && !property.StartsWith("//"))
                {
                    if (property.StartsWith('[') && property.EndsWith(']'))
                    {
                        if (currentSection != "")
                        {
                            switch (currentSection)
                            {
                                case "[General]":
                                    GetGeneralData(sectionProperties, osuBeatmap);
                                    break;
                                case "[Editor]":
                                    GetEditorData(sectionProperties, osuBeatmap);
                                    break;
                                case "[Metadata]":
                                    GetMetadataData(sectionProperties, osuBeatmap);
                                    break;
                                case "[Difficulty]":
                                    GetDifficultyData(sectionProperties, osuBeatmap);
                                    break;
                                case "[Events]":
                                    GetEventsData(sectionProperties, osuBeatmap);
                                    break;
                                case "[TimingPoints]":
                                    GetTimingPointsData(sectionProperties, osuBeatmap);
                                    break;
                                case "[Colours]":
                                    GetColoursData(sectionProperties, osuBeatmap);
                                    break;
                            }
                            
                            sectionProperties.Clear();
                        }

                        currentSection = property.Substring(0, property.Length);
                    }
                    else if (currentSection != "")
                    {
                        sectionProperties.Add(property);
                    }
                }
            }

            GetHitObjectsData(sectionProperties, osuBeatmap);
            sectionProperties.Clear();

            DisplayData(osuBeatmap);

            return osuBeatmap;
        }

        // get full beatmap data from osu replay
        public static Beatmap GetOsuBeatmap(string replayFileName)
        {
            string[] beatmapProperties = File.ReadAllLines(replayFileName);

            string currentSection = "";
            List<string> sectionProperties = new List<string>();
            Beatmap osuBeatmap = new Beatmap();

            foreach (string property in beatmapProperties)
            {
                if (!string.IsNullOrWhiteSpace(property) && !property.StartsWith("//"))
                {
                    if (property.StartsWith('[') && property.EndsWith(']'))
                    {
                        if (currentSection != "")
                        {
                            foreach (string section in sectionProperties)
                            {
                                switch (currentSection)
                                {
                                    case "[General]":
                                        GetGeneralData(sectionProperties, osuBeatmap);
                                        break;
                                    case "[Editor]":
                                        GetEditorData(sectionProperties, osuBeatmap);
                                        break;
                                    case "[Metadata]":
                                        GetMetadataData(sectionProperties, osuBeatmap);
                                        break;
                                    case "[Difficulty]":
                                        GetDifficultyData(sectionProperties, osuBeatmap);
                                        break;
                                    case "[Events]":
                                        GetEventsData(sectionProperties, osuBeatmap);
                                        break;
                                    case "[TimingPoints]":
                                        GetTimingPointsData(sectionProperties, osuBeatmap);
                                        break;
                                    case "[Colours]":
                                        GetColoursData(sectionProperties, osuBeatmap);
                                        break;
                                    case "[HitObjects]":
                                        GetHitObjectsData(sectionProperties, osuBeatmap);
                                        break;
                                }
                            }
                        }

                        currentSection = property.Substring(0, property.Length);
                    }
                    else if (currentSection != "")
                    {
                        sectionProperties.Add(property);
                    }
                }
            }

            DisplayData(osuBeatmap);

            return osuBeatmap;
        }
        
        private static void GetGeneralData(List<string> data, Beatmap beatmap)
        {
            General general = new General();

            foreach (string property in data)
            {
                string[] line = property.Split(':');
                line[1] = line[1].Trim();

                switch (line[0])
                {
                    case "AudioFilename":
                        general.AudioFileName = line[1];
                        break;
                    case "AudioLeadIn":
                        general.AudioLeadIn = int.Parse(line[1]);
                        break;
                    case "AudioHash":
                        general.AudioHash = line[1];
                        break;
                    case "PreviewTime":
                        general.PreviewTime = int.Parse(line[1]);
                        break;
                    case "CountDown":
                        general.Countdown = int.Parse(line[1]);
                        break;
                    case "SampleSet":
                        general.SampleSet = line[1];
                        break;
                    case "StackLeniency":
                        general.StackLeniency = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "Mode":
                        general.Mode = int.Parse(line[1]);
                        break;
                    case "LetterboxInBreaks":
                        general.LetterboxInBreaks = line[1] == "1" ? true : false;
                        break;
                    case "StoryFireInFront":
                        general.StoryFireInFront = line[1] == "1" ? true : false;
                        break;
                    case "UseSkinSprites":
                        general.UseSkinSprites = line[1] == "1" ? true : false;
                        break;
                    case "AlwaysShowPlayfield":
                        general.AlwaysShowPlayfield = line[1] == "1" ? true : false;
                        break;
                    case "OverlayPosition":
                        general.OverlayPosition = line[1];
                        break;
                    case "SkinPreference":
                        general.SkinPreference = line[1];
                        break;
                    case "EpilepsyWarning":
                        general.EpilepsyWarning = line[1] == "1" ? true : false;
                        break;
                    case "CountdownOffset":
                        general.CountdownOffset = int.Parse(line[1]);
                        break;
                    case "SpecialStyle":
                        general.SpecialStyle = line[1] == "1" ? true : false;
                        break;
                    case "WidescreenStoryboard":
                        general.WidescreenStoryboard = line[1] == "1" ? true : false;
                        break;
                    case "SamplesMatchPlaybackRate":
                        general.SamplesMatchPlaybackRate = line[1] == "1" ? true : false;
                        break;
                }
            }

            beatmap.General = general;
        }
        
        private static void GetEditorData(List<string> data, Beatmap beatmap)
        {
            Editor editor = new Editor();

            foreach (string property in data)
            {
                string[] line = property.Split(':');
                line[1] = line[1].Trim();

                switch (line[0])
                {
                    case "Bookmarks":
                        editor.Bookmarks = line[1];
                        break;
                    case "DistanceSpacing":
                        editor.DistanceSpacing = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "BeatDivisor":
                        editor.BeatDivisor = int.Parse(line[1]);
                        break;
                    case "GridSize":
                        editor.GridSize = int.Parse(line[1]);
                        break;
                    case "TimelineZoom":
                        editor.TimelineZoom = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                }
            }

            beatmap.Editor =  editor;
        }
        
        private static void GetMetadataData(List<string> data, Beatmap beatmap)
        {
            Metadata metadata = new Metadata();

            foreach (string property in data)
            {
                string[] line = property.Split(':');

                switch (line[0])
                {
                    case "Title":
                        metadata.Title = line[1].Trim();
                        break;
                    case "TitleUnicode":
                        metadata.TitleUnicode = line[1].Trim();
                        break;
                    case "Artist":
                        metadata.Artist = line[1].Trim();
                        break;
                    case "ArtistUnicode":
                        metadata.ArtistUnicode = line[1].Trim();
                        break;
                    case "Creator":
                        metadata.Creator = line[1].Trim();
                        break;
                    case "Version":
                        metadata.Version = line[1].Trim();
                        break;
                    case "Source":
                        metadata.Source = line[1].Trim();
                        break;
                    case "Tags":
                        metadata.Tags = line[1];
                        break;
                    case "BeatmapID":
                        metadata.BeatmapId = int.Parse(line[1]);
                        break;
                    case "BeatmapSetID":
                        metadata.BeatmapSetId = int.Parse(line[1]);
                        break;
                }
            }

            beatmap.Metadata = metadata;
        }
        
        private static void GetDifficultyData(List<string> data, Beatmap beatmap)
        {
            Difficulty difficulty = new Difficulty();

            foreach (string property in data)
            {
                string[] line = property.Split(':');

                switch (line[0])
                {
                    case "HPDrainRate":
                        difficulty.HPDrainRate = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "CircleSize":
                        difficulty.CircleSize = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "OverallDifficulty":
                        difficulty.OverallDifficulty = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "ApproachRate":
                        difficulty.ApproachRate = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "SliderMultiplier":
                        difficulty.SliderMultiplier = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "SliderTickRate":
                        difficulty.SliderTickRate = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        break;
                }
            }

            beatmap.Difficulty = difficulty;
        }
        
        private static void GetEventsData(List<string> data, Beatmap beatmap)
        {
            Events events = new Events();

            foreach (string property in data)
            {
                switch (property[0])
                {
                    case '0':
                        events.Backgrounds = property;
                        break;
                    case '1' or 'V':
                        events.Videos = property;
                        break;
                    case '2' or 'B':
                        events.Breaks!.Add(property);
                        break;
                }
            }

            beatmap.Events = events;
        }
        
        private static void GetTimingPointsData(List<string> data, Beatmap beatmap)
        {
            List<TimingPoints> timingList = new List<TimingPoints>();
            TimingPoints timingPoint = new TimingPoints();

            foreach (string property in data)
            {
                string[] line = property.Split(",");

                timingPoint.Time = int.Parse(line[0]);
                timingPoint.BeatLength = decimal.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                timingPoint.Meter = int.Parse(line[2]);
                timingPoint.SampleIndex = int.Parse(line[3]);
                timingPoint.SampleSet = int.Parse(line[4]);
                timingPoint.Volume = int.Parse(line[5]);
                timingPoint.Uninherited = line[6] == "1" ? true : false;
                timingPoint.Effects = (Effects)int.Parse(line[7]);

                timingList.Add(timingPoint);
            }

            beatmap.TimingPoints = timingList;
        }
        
        private static void GetColoursData(List<string> data, Beatmap beatmap)
        {
            Colours colours = new Colours();

            foreach (string property in data)
            {
                string[] line = property.Split(':');
                string[] rgb = line[1].Split(",");

                byte red = byte.Parse(rgb[0]);
                byte green = byte.Parse(rgb[1]);
                byte blue = byte.Parse(rgb[2]);

                if (line[0].Trim() == "SliderTrackOverride")
                {
                    colours.SliderTrackOverride = Color.FromArgb(255, red, green, blue);
                }
                else if(line[0].Trim() == "SliderBorder")
                {
                    colours.SliderBorder = Color.FromArgb(255, red, green, blue);
                }
                else
                {
                    colours.ComboColour!.Add(Color.FromArgb(255, red, green, blue));
                }
            }

            beatmap.Colours = colours;
        }
        
        private static void GetHitObjectsData(List<string> data, Beatmap beatmap)
        {
            List<HitObject> hitObjectList = new List<HitObject>();

            foreach (string property in data)
            {
                string[] line = property.Split(",");

                int X = int.Parse(line[0]);
                int Y = int.Parse(line[1]);
                int time = int.Parse(line[2]);
                ObjectType type = (ObjectType)int.Parse(line[3]);
                HitSound hitSound = (HitSound)int.Parse(line[4]);

                if (type.HasFlag(ObjectType.HitCircle))
                {
                    Circle circle = new Circle();

                    circle.X = X;
                    circle.Y = Y;
                    circle.Time = time;
                    circle.Type = type;
                    circle.HitSound = hitSound;

                    // do class for it
                    circle.HitSample = line[5];

                    hitObjectList.Add(circle);
                }
                else if (type.HasFlag(ObjectType.Slider))
                {
                    Slider slider = new Slider();

                    slider.X = X;
                    slider.Y = Y;
                    slider.Time = time;
                    slider.Type = type;
                    slider.HitSound = hitSound;

                    string[] curves = line[5].Split("|");

                    switch (curves[0])
                    {
                        case "B":
                            slider.CurveType = CurveType.Bezier;
                            break;
                        case "C":
                            slider.CurveType = CurveType.Centripetal;
                            break;
                        case "L":
                            slider.CurveType = CurveType.Linear;
                            break;
                        case "P":
                            slider.CurveType = CurveType.PerfectCirle;
                            break;
                    }

                    slider.CurvePoints = curves[1];
                    slider.Slides = int.Parse(line[6]);
                    slider.Length = decimal.Parse(line[7], CultureInfo.InvariantCulture.NumberFormat);

                    // when slider repeats
                    if (slider.Slides > 1)
                    {
                        slider.EdgeSounds = line[8];
                        slider.EdgeSets = line[9];
                        slider.HitSample = line[10];
                    }

                    hitObjectList.Add(slider);
                }
                else if (type.HasFlag(ObjectType.Spinner))
                {
                    Spinner spinner = new Spinner();

                    spinner.X = X;
                    spinner.Y = Y;
                    spinner.Time = time;
                    spinner.Type = type;
                    spinner.HitSound = hitSound;

                    spinner.EndTime = int.Parse(line[5]);
                    spinner.HitSample = line[6];

                    hitObjectList.Add(spinner);
                }
            }

            beatmap.HitObjects = hitObjectList;
        }

        private static void DisplayData(Beatmap osuBeatmap)
        {
            Console.WriteLine("GENERAL");
            Console.WriteLine("Audio File Name             - " + osuBeatmap.General!.AudioFileName);
            Console.WriteLine("Audio Lead In               - " + osuBeatmap.General.AudioLeadIn);
            Console.WriteLine("Audio Hash                  - " + osuBeatmap.General.AudioHash);
            Console.WriteLine("Preview Time                - " + osuBeatmap.General.PreviewTime);
            Console.WriteLine("Countdown                   - " + osuBeatmap.General.Countdown);
            Console.WriteLine("Sample set                  - " + osuBeatmap.General.SampleSet);
            Console.WriteLine("Stack Leniency              - " + osuBeatmap.General.StackLeniency);
            Console.WriteLine("Mode                        - " + osuBeatmap.General.Mode);
            Console.WriteLine("Letterbox in breaks         - " + osuBeatmap.General.LetterboxInBreaks);
            Console.WriteLine("Story Fire In Front         - " + osuBeatmap.General.StoryFireInFront);
            Console.WriteLine("Use skin sprites            - " + osuBeatmap.General.UseSkinSprites);
            Console.WriteLine("Always show playfield       - " + osuBeatmap.General.AlwaysShowPlayfield);
            Console.WriteLine("Overlay position            - " + osuBeatmap.General.OverlayPosition);
            Console.WriteLine("Sking preference            - " + osuBeatmap.General.SkinPreference);
            Console.WriteLine("Epilepsy warning            - " + osuBeatmap.General.EpilepsyWarning);
            Console.WriteLine("Countdown offset            - " + osuBeatmap.General.CountdownOffset);
            Console.WriteLine("Special style               - " + osuBeatmap.General.SpecialStyle);
            Console.WriteLine("Widescreen storyboard       - " + osuBeatmap.General.WidescreenStoryboard);
            Console.WriteLine("Samples match playback rate - " + osuBeatmap.General.SamplesMatchPlaybackRate);
            Console.WriteLine("EDITOR------------------------------------------------");
            Console.WriteLine("Bookmarks                   - " + osuBeatmap.Editor!.Bookmarks);
            Console.WriteLine("Distance spacing            - " + osuBeatmap.Editor.DistanceSpacing);
            Console.WriteLine("Beat divisor                - " + osuBeatmap.Editor.BeatDivisor);
            Console.WriteLine("Grid size                   - " + osuBeatmap.Editor.GridSize);
            Console.WriteLine("Timeline zoom               - " + osuBeatmap.Editor.TimelineZoom);
            Console.WriteLine("METADATA------------------------------------------------");
            Console.WriteLine("Title                       - " + osuBeatmap.Metadata!.Title);
            Console.WriteLine("TitleUnicode                - " + osuBeatmap.Metadata.TitleUnicode);
            Console.WriteLine("Artist                      - " + osuBeatmap.Metadata.Artist);
            Console.WriteLine("ArtistUnicode               - " + osuBeatmap.Metadata.ArtistUnicode);
            Console.WriteLine("Creator                     - " + osuBeatmap.Metadata.Creator);
            Console.WriteLine("Version                     - " + osuBeatmap.Metadata.Version);
            Console.WriteLine("Source                      - " + osuBeatmap.Metadata.Source);
            Console.WriteLine("Tags                        - " + osuBeatmap.Metadata.Tags);
            Console.WriteLine("BeatmapID                   - " + osuBeatmap.Metadata.BeatmapId);
            Console.WriteLine("BeatmapSetID                - " + osuBeatmap.Metadata.BeatmapSetId);
            Console.WriteLine("DIFFICULTY------------------------------------------------");
            Console.WriteLine("HP DR                       - " + osuBeatmap.Difficulty!.HPDrainRate);
            Console.WriteLine("CS                          - " + osuBeatmap.Difficulty.CircleSize);
            Console.WriteLine("OD                          - " + osuBeatmap.Difficulty.OverallDifficulty);
            Console.WriteLine("AR                          - " + osuBeatmap.Difficulty.ApproachRate);
            Console.WriteLine("Slider multiplayer          - " + osuBeatmap.Difficulty.SliderMultiplier);
            Console.WriteLine("Slidetr tickrate            - " + osuBeatmap.Difficulty.SliderTickRate);
            Console.WriteLine("EVENTS------------------------------------------------");
            Console.WriteLine("Background                  - " + osuBeatmap.Events!.Backgrounds);
            Console.WriteLine("Videos                      - " + osuBeatmap.Events.Videos);
            Console.WriteLine("Breaks                      - " + osuBeatmap.Events.Breaks);

            Console.WriteLine("TIMING POINTS------------------------------------------------");
            
            Console.WriteLine("COLOURS------------------------------------------------");
            Console.WriteLine("Combo colours               - " + osuBeatmap.Colours!.ComboColour);
            Console.WriteLine("Slider track override       - " + osuBeatmap.Colours.SliderTrackOverride);
            Console.WriteLine("Slider border               - " + osuBeatmap.Colours.SliderBorder);

            Console.WriteLine("HIT OBJECTS------------------------------------------------");
        }
    }
}
