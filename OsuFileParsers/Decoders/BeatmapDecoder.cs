using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Beatmap.osu.OsuDB;
using OsuFileParsers.SliderPathMath;
using Realms;
using ReplayParsers.Classes.Beatmap.osuLazer;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using Beatmap = OsuFileParsers.Classes.Beatmap.osu.Beatmap;
using File = System.IO.File;
using LazerBeatmap = ReplayParsers.Classes.Beatmap.osuLazer.Beatmap;

namespace OsuFileParsers.Decoders
{
    public class BeatmapDecoder
    {
        private static Beatmap osuBeatmap = new Beatmap();

        public static void Clear()
        {
            osuBeatmap = new Beatmap();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);
            GC.GetTotalMemory(true);
        }

        public static Beatmap GetOsuLazerBeatmap(string beatmapMD5Hash, string path)
        {
            List<(string, string)> mapFileList = new List<(string, string)>();
            
            string realmFilePath = $"{path}\\client.realm";
            RealmConfiguration config = new RealmConfiguration(realmFilePath) { SchemaVersion = 51 };
            using (Realm realm = Realm.GetInstance(config))
            {
                IQueryable<LazerBeatmap> realmData = realm.All<LazerBeatmap>();
                LazerBeatmap beatmap = realmData.FirstOrDefault(x => x.MD5Hash == beatmapMD5Hash)!;

                foreach (RealmNamedFileUsage file in beatmap.BeatmapSet!.Files)
                {
                    mapFileList.Add((file.File!.Hash!, file.Filename!));
                }

                osuBeatmap = new Beatmap();

                string beatmapDataPath = $"{path}\\files\\{beatmap.Hash![0]}\\{beatmap.Hash.Substring(0, 2)}\\{beatmap.Hash}";
                osuBeatmap = GetBeatmapData(beatmapDataPath);

                PrepareAnalyzerBeatmapFolders();
                GetOsuLazerBeatmapFile(mapFileList, beatmapDataPath, beatmap.Hash);
                GetOsuLazerBeatmapBackground(mapFileList, path);
                GetOsuLazerBeatmapAudio(mapFileList, path);
                GetOsuLazerBeatmapHitsounds(mapFileList, path);
            }

            Stacking.Stacking.ApplyStacking(osuBeatmap);
        
            return osuBeatmap;
        }

        public static Beatmap GetOsuStableBeatmap(string beatmapMD5Hash, string osuFolderPath, string externalSongsFolderPath)
        {
            OsuDB osuDB = OsuDBDecoder.GetOsuDBData(osuFolderPath);
            OsuDBBeatmap DBbeatmap = osuDB.DBBeatmaps!.FirstOrDefault(x => x.BeatmapMD5Hash == beatmapMD5Hash)!;

            string songsFolderPath = externalSongsFolderPath != "" ? externalSongsFolderPath : osuFolderPath;
            string beatmapFilePath = $"{songsFolderPath}\\Songs\\{DBbeatmap.BeatmapFolderName}\\{DBbeatmap.BeatmapFileName}";

            // SOMETIMES DBbeatmap.BeatmapFolderName gives additional \\Songs string if songs folder is not in osu folder and sometimes it doesnt... whatever
            bool containsSongs = false;
            foreach (string part in beatmapFilePath.Split("\\"))
            {
                if (containsSongs == true && part == "Songs")
                {
                    beatmapFilePath = beatmapFilePath.Replace("\\Songs\\Songs", "\\Songs");
                    break;
                }

                if (part == "Songs")
                {
                    containsSongs = true;
                }
            }
                
            osuBeatmap = new Beatmap();
            osuBeatmap = GetBeatmapData(beatmapFilePath);

            PrepareAnalyzerBeatmapFolders();
            GetOsuStableBeatmapFiles(songsFolderPath, DBbeatmap.BeatmapFileName!);

            Stacking.Stacking.ApplyStacking(osuBeatmap);

            return osuBeatmap;
        }

        public static Beatmap GetPreviouslyLoadedReplay(string beatmapFilePath)
        {
            osuBeatmap = new Beatmap();
            osuBeatmap = GetBeatmapData(beatmapFilePath);

            Stacking.Stacking.ApplyStacking(osuBeatmap);

            return osuBeatmap;
        }

        private static Beatmap GetBeatmapData(string beatmapFilePath)
        {
            string[] beatmapProperties = File.ReadAllLines(beatmapFilePath);

            string currentSection = "";
            List<string> sectionProperties = new List<string>();

            osuBeatmap.FileVersion = int.Parse(beatmapProperties[0].Substring(17));
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
                                    osuBeatmap.General = GetGeneralData(sectionProperties);
                                    break;
                                case "[Editor]":
                                    osuBeatmap.Editor = GetEditorData(sectionProperties);
                                    break;
                                case "[Metadata]":
                                    osuBeatmap.Metadata = GetMetadataData(sectionProperties);
                                    break;
                                case "[Difficulty]":
                                    osuBeatmap.Difficulty = GetDifficultyData(sectionProperties);
                                    break;
                                case "[Events]":
                                    osuBeatmap.Events = GetEventsData(sectionProperties);
                                    break;
                                case "[TimingPoints]":
                                    osuBeatmap.TimingPoints = GetTimingPointsData(sectionProperties);
                                    break;
                                case "[Colours]":
                                    osuBeatmap.Colours = GetColoursData(sectionProperties);
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

            osuBeatmap.HitObjects = GetHitObjectsData(sectionProperties);
            sectionProperties.Clear();
            
            return osuBeatmap;
        }

        private static void PrepareAnalyzerBeatmapFolders()
        {
            string[] folders = ["Audio", "Background", "Beatmap", "Hitsounds"];
            for (int i = 0; i < folders.Length; i++)
            {
                if (Directory.Exists($"{AppContext.BaseDirectory}\\osu\\{folders[i]}") == false)
                {
                    Directory.CreateDirectory($"{AppContext.BaseDirectory}\\osu\\{folders[i]}");
                }
                else
                {
                    DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}\\osu\\{folders[i]}");
                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        // sometimes deleting lazer files crashed app due to file in access error... idk if it still will happen
                        // but i will leave this just in case since this doesnt break anything if file wont be deleted
                        try
                        {
                            file.Delete();
                        } catch { }
                    }
                }
            }
        }

        private static void GetOsuLazerBeatmapFile(List<(string, string)> mapFileList, string path, string beatmapHash)
        {
            string beatmapFileName = mapFileList.FirstOrDefault(x => x.Item1 == beatmapHash).Item2;
            File.Copy($"{path}", $"{AppContext.BaseDirectory}\\osu\\Beatmap\\{beatmapFileName}");
        }

        private static void GetOsuLazerBeatmapBackground(List<(string, string)> mapFileList, string path)
        {
            string[] bgEvents = osuBeatmap.Events!.Backgrounds!.Split(",");
            (string hash, string bg) = mapFileList.FirstOrDefault(x => x.Item2 == bgEvents[2]);

            if (hash != null)
            {
                File.Copy($"{path}\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                         ,$"{AppContext.BaseDirectory}\\osu\\Background\\{bg}");
            }
        }

        private static void GetOsuLazerBeatmapAudio(List<(string, string)> mapFileList, string path)
        {
            (string hash, string audio) = mapFileList.FirstOrDefault(x => x.Item2 == osuBeatmap.General!.AudioFileName);

            File.Copy($"{path}\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                     ,$"{AppContext.BaseDirectory}\\osu\\Audio\\{audio}");

            /* if my mew audio implementation doesnt work on something or if specific format wont work i can still use this
            // god i hate .ogg files... i really really hate them... did i wrote that i hate them? coz i hate them
            // new thing: convert EVERYTHING to mp3... i dont care just do it and observe if there will be any issues
            if (true == false)//audio.Contains(".mp3") == false)
            {
                bool fileCreated = false;
                while (fileCreated == false)
                {
                    File.Copy($"{path}\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Audio\\temp{audio}");

                    try
                    {
                        using (FileStream stream = File.Open($"{AppContext.BaseDirectory}\\osu\\Audio\\temp{audio}", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (VorbisWaveReader reader = new VorbisWaveReader(stream))
                            {
                                MediaFoundationEncoder.EncodeToMp3(reader, $"{AppContext.BaseDirectory}\\osu\\Audio\\{audio.Split('.')[0]}.mp3");
                            }
                        }
                        File.Delete($"{AppContext.BaseDirectory}\\osu\\Audio\\temp{audio}");

                        fileCreated = true;
                    }
                    catch// idk if i want this here and in osu!stable function coz it will never retry (like i want it to) and instead crash the app
                    {
                        //throw new ArgumentException("File in use cant access");
                    }
                } 
            }
            */
        }

        private static void GetOsuLazerBeatmapHitsounds(List<(string, string)> mapFileList, string path)
        {
            for (int i = 0; i < mapFileList.Count; i++)
            {
                (string hash, string audio) = mapFileList[i];
                if (audio.Contains(".wav"))
                {
                    File.Copy($"{path}\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Hitsounds\\{audio}");
                }
            }
        }

        private static void GetOsuStableBeatmapFiles(string path, string beatmapFileName)
        {
            DirectoryInfo songsFolder = new DirectoryInfo($"{path}\\Songs");
            DirectoryInfo? beatmapFolder = songsFolder.GetDirectories().FirstOrDefault(x => x.Name.Contains($"{osuBeatmap.Metadata!.BeatmapSetId}"));

            foreach (FileInfo file in beatmapFolder!.GetFiles())
            {
                if (file.Name == beatmapFileName)
                {
                    File.Copy($"{beatmapFolder!.FullName}\\{file.Name}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Beatmap\\{file.Name}");

                    continue;
                }

                string[] bg = osuBeatmap.Events!.Backgrounds!.Split(",");
                if (file.Name == bg[2])
                {
                    File.Copy($"{beatmapFolder!.FullName}\\{file.Name}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Background\\{file.Name}");

                    continue;
                }

                if (file.Name == osuBeatmap.General!.AudioFileName)
                {
                    File.Copy($"{beatmapFolder!.FullName}\\{file.Name}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Audio\\{file.Name}");

                    continue;
                }

                if (file.Name.Contains(".wav"))
                {
                    File.Copy($"{beatmapFolder!.FullName}\\{file.Name}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Hitsounds\\{file.Name}");

                    continue;
                }
            }
        }

        private static General GetGeneralData(List<string> data)
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

            return general;
        }
        
        private static Editor GetEditorData(List<string> data)
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

            return editor;
        }
        
        private static Metadata GetMetadataData(List<string> data)
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

            return metadata;
        }
        
        private static Difficulty GetDifficultyData(List<string> data)
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

            return difficulty;
        }
        
        private static Events GetEventsData(List<string> data)
        {
            Events events = new Events();

            foreach (string property in data)
            {
                switch (property[0])
                {
                    case '0':
                        events.Backgrounds = property.Replace("\"", "");
                        break;
                    case '1' or 'V':
                        events.Videos = property;
                        break;
                    case '2' or 'B':
                        events.Breaks!.Add(property);
                        break;
                }
            }

            return events;
        }
        
        private static List<TimingPoint> GetTimingPointsData(List<string> data)
        {
            List<TimingPoint> timingList = new List<TimingPoint>();

            foreach (string property in data)
            {
                string[] line = property.Split(",");

                TimingPoint timingPoint = new TimingPoint();

                timingPoint.Time = decimal.Parse(line[0], CultureInfo.InvariantCulture.NumberFormat);
                timingPoint.BeatLength = double.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                timingPoint.Meter = int.Parse(line[2]);
                timingPoint.SampleIndex = int.Parse(line[3]);
                timingPoint.SampleSet = int.Parse(line[4]);
                timingPoint.Volume = int.Parse(line[5]);
                timingPoint.Uninherited = line[6] == "1" ? true : false;
                timingPoint.Effects = (Effects)int.Parse(line[7]);

                timingList.Add(timingPoint);
            }

            return timingList;
        }
        
        private static Colours GetColoursData(List<string> data)
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

            return colours;
        }
        
        private static List<HitObjectData> GetHitObjectsData(List<string> data)
        {
            // Game mode (0 = osu!, 1 = osu!taiko, 2 = osu!catch, 3 = osu!mania)
            List<HitObjectData> hitObjectList = new List<HitObjectData>();
            if (osuBeatmap.General.Mode == 0)
            {
                int comboNumber = 0;
                foreach (string property in data)
                {
                    string[] line = property.Split(",");

                    int X = (int)float.Parse(line[0], CultureInfo.InvariantCulture.NumberFormat);
                    int Y = (int)float.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                    int time = (int)float.Parse(line[2], CultureInfo.InvariantCulture.NumberFormat);
                    ObjectType type = (ObjectType)int.Parse(line[3]);
                    HitSound hitSound = (HitSound)int.Parse(line[4]);

                    if (type.HasFlag(ObjectType.StartNewCombo))
                    {
                        comboNumber = 0;
                    }
                    comboNumber++;

                    if (type.HasFlag(ObjectType.HitCircle))
                    {
                        OsuCircleData circle = new OsuCircleData();

                        circle.BaseX = X;
                        circle.BaseY = Y;
                        circle.BaseSpawnPosition = new Vector2(X, Y);
                        circle.SpawnTime = time;
                        circle.Type = type;
                        circle.HitSound = hitSound;

                        if (line.Length > 5)
                        {
                            circle.HitSample = line[5];
                        }

                        circle.ComboNumber = comboNumber;

                        hitObjectList.Add(circle);
                    }
                    else if (type.HasFlag(ObjectType.Slider))
                    {
                        OsuSliderData slider = new OsuSliderData();

                        slider.BaseX = X;
                        slider.BaseY = Y;
                        slider.BaseSpawnPosition = new Vector2(X, Y);
                        slider.SpawnTime = time;
                        slider.Type = type;
                        slider.HitSound = hitSound;
                        slider.ComboNumber = comboNumber;

                        string[] curves = line[5].Split("|");
                        CurveType curveType = GetCurveType(curves[0]);
                        Vector2[] controlPoints = new Vector2[curves.Length];
                        for (int i = 1; i < curves.Length; i++)
                        {
                            if (curves[i].Length == 1)
                            {
                                continue;
                            }

                            Vector2 pos = ReadPoint(curves[i], slider.BaseSpawnPosition);
                            controlPoints[i] = pos;
                        }

                        List<ArraySegment<PathControlPoint>> convertedPoints = ConvertControlPoints(controlPoints, curveType).ToList();
                        slider.ControlPoints = MergeControlPointsLists(convertedPoints);

                        for (int i = 1; i < curves.Length; i++)
                        {
                            if (curves[i].Length == 1)
                            {
                                continue;
                            }

                            string[] c = curves[i].Split(":");
                            slider.CurvePoints!.Add(new Vector2(float.Parse(c[0], CultureInfo.InvariantCulture.NumberFormat)
                                                               , float.Parse(c[1], CultureInfo.InvariantCulture.NumberFormat)));
                        }

                        slider.RepeatCount = int.Parse(line[6]);
                        slider.Length = decimal.Parse(line[7], CultureInfo.InvariantCulture.NumberFormat);

                        // ok i found out by extreme accident that these values can just not exist
                        // i might just not know better way (other than ternary operation but this feels faster?)
                        // why this doesnt have set default value myan
                        if (line.Length > 10)
                        {
                            slider.HitSample = line[10];
                        }
                        if (line.Length > 9)
                        {
                            slider.EdgeSets = line[9];
                        }
                        if (line.Length > 8)
                        {
                            slider.EdgeSounds = line[8];
                        }

                        slider.Path = new SliderPath(slider);
                        slider.EndPosition = slider.BaseSpawnPosition + slider.Path.PositionAt(1);
                        slider.EndTime = GetSliderEndTime(slider);
                        slider.SliderTicks = GetSliderTicks(slider);

                        hitObjectList.Add(slider);
                    }
                    else if (type.HasFlag(ObjectType.Spinner))
                    {
                        OsuSpinnerData spinner = new OsuSpinnerData();

                        spinner.BaseX = X;
                        spinner.BaseY = Y;
                        spinner.BaseSpawnPosition = new Vector2(X, Y);
                        spinner.SpawnTime = time;
                        spinner.Type = type;
                        spinner.HitSound = hitSound;
                        spinner.EndTime = int.Parse(line[5]);

                        if (line.Length > 6)
                        {
                            spinner.HitSample = line[6];
                        }

                        hitObjectList.Add(spinner);
                    }
                }

                BeatLength = 0;
            }
            else if (osuBeatmap.General.Mode == 1)
            {
                foreach (string property in data)
                {
                    string[] line = property.Split(",");

                    int time = (int)float.Parse(line[2], CultureInfo.InvariantCulture.NumberFormat);
                    ObjectType type = (ObjectType)int.Parse(line[3]);
                    HitSound hitSound = (HitSound)int.Parse(line[4]);

                    if (type.HasFlag(ObjectType.HitCircle))
                    {
                        TaikoHitCircleData circle = new TaikoHitCircleData();
                        circle.SpawnTime = time;
                        circle.IsBig = hitSound.HasFlag(HitSound.Finish);
                        circle.IsDon = !(hitSound.HasFlag(HitSound.Whistle) || hitSound.HasFlag(HitSound.Clap));

                        hitObjectList.Add(circle);
                    }
                    else if (type.HasFlag(ObjectType.Slider))
                    {
                        TaikoDrumRollData drumRoll = new TaikoDrumRollData();
                        drumRoll.SpawnTime = time;
                        drumRoll.IsBig = hitSound.HasFlag(HitSound.Finish);
                        drumRoll.Length = double.Parse(line[7], CultureInfo.InvariantCulture);

                        // this is all needed for accurate drum roll end time apparently since drum rolls are made from sliders
                        OsuSliderData d = new OsuSliderData();
                        string[] curves = line[5].Split("|");
                        CurveType curveType = GetCurveType(curves[0]);
                        Vector2[] controlPoints = new Vector2[curves.Length];
                        for (int i = 1; i < curves.Length; i++)
                        {
                            if (curves[i].Length == 1)
                            {
                                continue;
                            }

                            Vector2 pos = ReadPoint(curves[i], d.BaseSpawnPosition);
                            controlPoints[i] = pos;
                        }

                        List<ArraySegment<PathControlPoint>> convertedPoints = ConvertControlPoints(controlPoints, curveType).ToList();
                        d.ControlPoints = MergeControlPointsLists(convertedPoints);

                        for (int i = 1; i < curves.Length; i++)
                        {
                            if (curves[i].Length == 1)
                            {
                                continue;
                            }

                            string[] c = curves[i].Split(":");
                            d.CurvePoints!.Add(new Vector2(float.Parse(c[0], CultureInfo.InvariantCulture.NumberFormat)
                                                               , float.Parse(c[1], CultureInfo.InvariantCulture.NumberFormat)));
                        }

                        d.RepeatCount = int.Parse(line[6]);
                        d.Length = decimal.Parse(line[7], CultureInfo.InvariantCulture);
                        d.Path = new SliderPath(d);
                        d.SpawnTime = drumRoll.SpawnTime;
                        drumRoll.EndTime = (int)GetDrumRollEndTime(d);

                        hitObjectList.Add(drumRoll);
                    }
                    else if (type.HasFlag(ObjectType.Spinner))
                    {
                        TaikoSpinnerData spinner = new TaikoSpinnerData();
                        spinner.SpawnTime = time;

                        hitObjectList.Add(spinner);
                    }
                }   
            }
            else if (osuBeatmap.General.Mode == 2)
            {
                foreach (string property in data)
                {
                    string[] line = property.Split(",");

                    int X = (int)float.Parse(line[0], CultureInfo.InvariantCulture.NumberFormat);
                    int time = (int)float.Parse(line[2], CultureInfo.InvariantCulture.NumberFormat);
                    ObjectType type = (ObjectType)int.Parse(line[3]);
                    HitSound hitSound = (HitSound)int.Parse(line[4]);

                    if (type.HasFlag(ObjectType.HitCircle))
                    {
                        CatchFruitData fruit = new CatchFruitData();
                        fruit.SpawnTime = time;
                        fruit.X = X;

                        hitObjectList.Add(fruit);
                    }
                    else if (type.HasFlag(ObjectType.Slider))
                    {
                        CatchJuiceStreamData slider = new CatchJuiceStreamData();
                        slider.SpawnTime = time;
                        slider.X = X;
                        
                        // probably Xstart and Xend will be needed?

                        // this is all needed for accurate end time and probably something else...
                        OsuSliderData d = new OsuSliderData();
                        string[] curves = line[5].Split("|");
                        CurveType curveType = GetCurveType(curves[0]);
                        Vector2[] controlPoints = new Vector2[curves.Length];
                        int Y = (int)float.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                        for (int i = 1; i < curves.Length; i++)
                        {
                            if (curves[i].Length == 1)
                            {
                                continue;
                            }

                            Vector2 pos = ReadPoint(curves[i], new Vector2(X, Y));
                            controlPoints[i] = pos;
                        }

                        List<ArraySegment<PathControlPoint>> convertedPoints = ConvertControlPoints(controlPoints, curveType).ToList();
                        d.ControlPoints = MergeControlPointsLists(convertedPoints);

                        for (int i = 1; i < curves.Length; i++)
                        {
                            if (curves[i].Length == 1)
                            {
                                continue;
                            }

                            string[] c = curves[i].Split(":");
                            d.CurvePoints!.Add(new Vector2(float.Parse(c[0], CultureInfo.InvariantCulture.NumberFormat)
                                                          ,float.Parse(c[1], CultureInfo.InvariantCulture.NumberFormat)));
                        }

                        d.RepeatCount = int.Parse(line[6]);
                        d.Length = decimal.Parse(line[7], CultureInfo.InvariantCulture);
                        d.Path = new SliderPath(d);
                        d.SpawnTime = slider.SpawnTime;
                        double endTime = GetDrumRollEndTime(d);
                        d.EndTime = endTime;
                        slider.EndTime = endTime;
                        slider.Drops = GetSliderTicks(d);
                        slider.EndXPosition = (int)d.Path.PositionAt(1).X;
                        slider.RepeatCount = int.Parse(line[6]);
                        slider.Path = d.Path;

                        hitObjectList.Add(slider);
                    }
                    else if (type.HasFlag(ObjectType.Spinner))
                    {
                        CatchBananaShowerData spinner = new CatchBananaShowerData();
                        spinner.SpawnTime = time;
                        spinner.EndTime = int.Parse(line[5]);

                        hitObjectList.Add(spinner);
                    }
                }
            }
            else if (osuBeatmap.General.Mode == 3)
            {
                foreach (string property in data)
                {
                    string[] line = property.Split(",");

                    int X = (int)float.Parse(line[0], CultureInfo.InvariantCulture.NumberFormat);
                    int time = (int)float.Parse(line[2], CultureInfo.InvariantCulture.NumberFormat);
                    ObjectType type = (ObjectType)int.Parse(line[3]);
                    HitSound hitSound = (HitSound)int.Parse(line[4]);

                    // circle size is number of columns https://osu.ppy.sh/wiki/en/Client/Beatmap_editor/Song_setup
                    int columnCount = (int)osuBeatmap.Difficulty.CircleSize;
                    // math https://osu.ppy.sh/wiki/en/Client/File_formats/osu_%28file_format%29
                    int columnIndex = (int)Math.Clamp(Math.Floor(X * columnCount / 512.0), 0, columnCount - 1);

                    // i dont know if HasFlag() is needed here since there objects dont have combo properties but will use it just in case
                    if (type.HasFlag(ObjectType.HoldNote))
                    {
                        ManiaLongNoteData holdNote = new ManiaLongNoteData();
                        holdNote.SpawnTime = time;
                        holdNote.Type = type;
                        holdNote.ColumnIndex = columnIndex;
                        holdNote.EndTime = int.Parse(line[5].Split(":")[0]);

                        hitObjectList.Add(holdNote);
                    }
                    else if (type.HasFlag(ObjectType.HitCircle))
                    {
                        ManiaNoteData note = new ManiaNoteData();
                        note.SpawnTime = time;
                        note.Type = type;
                        note.ColumnIndex = columnIndex;

                        hitObjectList.Add(note);
                    }
                }
            }

            return hitObjectList;
        }

        private static double BeatLength = 0;
        private static TimingPoint PreviousPoint = null!;
        private static TimingPoint GetTimingPointAt(int time)
        {
            if (PreviousPoint == null)
            {
                PreviousPoint = osuBeatmap!.TimingPoints![0];
            }

            TimingPoint point = BinarySearch(osuBeatmap!.TimingPoints!, time);

            // time value when gettng TimingPoint is from sliders (needed to update their velocity) and this causes sometimes
            // timing points to be missed and this loop checks current index and previous index and checks if
            // TimingPoint with BPM change wasnt missed and if it was missed then change BeatLength (BPM) (scars of calamity has that)
            int indx1 = osuBeatmap!.TimingPoints!.IndexOf(point);
            int indx2 = osuBeatmap!.TimingPoints!.IndexOf(PreviousPoint!);
            if (indx1 - indx2 > 1 && indx2 != -1)
            {
                for (int i = indx1 - indx2; i > 0; i--)
                {
                    TimingPoint p = osuBeatmap.TimingPoints[indx1 - i];

                    if (p.BeatLength > 0)
                    {
                        BeatLength = p.BeatLength;
                    }
                }
            }

            PreviousPoint = point;

            // scenario where 2 timing points have the same Time and one has
            // positive BeatLength (bpm) and one negative (slider velocity)
            // this makes it so BeatLength is always set correctly (hopefully this time everithing works...)

            // there might be case like that in the middle of the map tho... need to test that somehow oops
            // deity mode time 216648 index 1029 and time 212614 index 1017 for both bpm change before and after
            int currentIndex = osuBeatmap.TimingPoints.IndexOf(point);
            if (currentIndex > 0
            && osuBeatmap.TimingPoints[currentIndex - 1].Time == point.Time
            && osuBeatmap.TimingPoints[currentIndex - 1].BeatLength > 0)
            {
                BeatLength = osuBeatmap.TimingPoints[currentIndex - 1].BeatLength;
            }
            else if (currentIndex > osuBeatmap.TimingPoints.Count + 1
            && osuBeatmap.TimingPoints[currentIndex + 1].Time == point.Time
            && osuBeatmap.TimingPoints[currentIndex + 1].BeatLength > 0)
            {
                BeatLength = osuBeatmap.TimingPoints[currentIndex + 1].BeatLength;
            }
            else if (point != null && point.BeatLength > 0)
            {
                BeatLength = point.BeatLength;
            }

            // need to somehow update bpm that is changed somewhere on timeline pain

            if (BeatLength == 0 && osuBeatmap.TimingPoints.Count > 0)
            {
                BeatLength = osuBeatmap.TimingPoints.First(b => b.BeatLength > 0).BeatLength;
            }

            return point == null ? TimingPoint.DEFAULT : point;
        }

        // taken from osu lazer code but done a bit different hopefully it works
        public static TimingPoint BinarySearch(List<TimingPoint> timingPoints, int time)
        {
            int n = timingPoints.Count;

            // || time < timingPoints[0].Time < this caused bug for first timing point if it was lower so uhh hopefully this works
            if (n == 0)
            {
                return null!;
            }

            int l = 0;
            int r = n;

            // there are sometimes for some ANNOYING reason rare velocity changes 1ms before actual slider spawn
            // this finds always index with time higher than current one, then giving index - 1, hopefully removing this 1ms thing
            while (l < r)
            {
                int mid = l + ((r - l) >> 1);

                if (time >= (int)timingPoints[mid].Time)
                {
                    l = mid + 1;
                }
                else if (time < (int)timingPoints[mid].Time)
                {
                    r = mid;
                }
            }

            return l - 1 == -1 ? timingPoints[0] : timingPoints[l - 1];
        }

        private static double GetSliderEndTime(OsuSliderData slider)
        {
            TimingPoint point = GetTimingPointAt(slider.SpawnTime);

            double sliderVelocityMultiplayer = point.BeatLength < 0 ? 100.0 / -point.BeatLength : 1;

            double sliderVelocityAsBeatLength = -100 / sliderVelocityMultiplayer;
            double bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 1000) / 100.0 : 1;

            double SM = (double)osuBeatmap.Difficulty!.SliderMultiplier;
            double velocity = 100 * SM / (BeatLength * bpmMultiplier);

            return slider.SpawnTime + slider.RepeatCount * slider.Path.Distance / velocity;
        }

        private static double GetDrumRollEndTime(OsuSliderData slider)
        {
            TimingPoint point = GetTimingPointAt(slider.SpawnTime);

            double sliderVelocityMultiplayer = point.BeatLength < 0 ? 100.0 / -point.BeatLength : 1;

            double sliderVelocityAsBeatLength = -100 / sliderVelocityMultiplayer;
            double bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 1000) / 100.0 : 1;

            double SM = (double)osuBeatmap.Difficulty!.SliderMultiplier;
            double velocity = 100 * SM / (BeatLength * bpmMultiplier);

            return slider.SpawnTime + slider.RepeatCount * (slider.Path.Distance) / velocity;
        }

        private static List<SliderTick> GetSliderTicks(OsuSliderData slider)
        {
            TimingPoint point = GetTimingPointAt(slider.SpawnTime);

            double sliderVelocityMultiplayer = point.BeatLength < 0 ? 100.0 / -point.BeatLength : 1;

            double sliderVelocityAsBeatLength = -100 / sliderVelocityMultiplayer;
            double bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 1000) / 100.0 : 1;

            double SM = (double)osuBeatmap.Difficulty!.SliderMultiplier;
            double velocity = 100 * SM / (BeatLength * bpmMultiplier);

            // math for tickDistance and velocity from osu lazer code
            double scoringDistance = velocity * BeatLength;

            double minimalDistanceFromEnd = velocity * 10;
            double tickDistance = (scoringDistance / (double)osuBeatmap.Difficulty.SliderTickRate * 1);
            //                                                                        change this?  ^

            double sliderDistance = slider.Path.Distance;
            int numberOfTicks = (int)(sliderDistance / tickDistance);
            if (numberOfTicks == 0)
            {
                return null!;
            }

            List<SliderTick> ticks = new List<SliderTick>();
            double sliderBaseDuration = (slider.EndTime - slider.SpawnTime) / slider.RepeatCount;
            for (int i = 0; i < slider.RepeatCount; i++)
            {
                List<SliderTick> tempTicks = new List<SliderTick>();

                double repeatArrowStartTime = slider.SpawnTime + (i * sliderBaseDuration);
                bool reversed = i % 2 == 1;

                for (double j = tickDistance; j < sliderDistance; j += tickDistance)
                {
                    if (j + minimalDistanceFromEnd >= sliderDistance)
                    {// tick cant be too close to tail
                        break;
                    }

                    Vector2 positionInSlider = slider.Path.PositionAt((j - minimalDistanceFromEnd) / sliderDistance);
                    double tickProgress = reversed == false
                                          ? (j - minimalDistanceFromEnd) / sliderDistance
                                          : 1 - (j - minimalDistanceFromEnd) / sliderDistance;
                    double time = repeatArrowStartTime + (tickProgress * sliderBaseDuration);

                    tempTicks.Add(new SliderTick(positionInSlider, tickProgress, time));
                }

                if (reversed == true)
                {
                    tempTicks.Reverse();
                }

                for (int j = 0; j < tempTicks.Count; j++)
                {
                    ticks.Add(tempTicks[j]);
                }
            }

            return ticks;
        }

        private static IEnumerable<ArraySegment<PathControlPoint>> ConvertControlPoints(Vector2[] points, CurveType type)
        {
            PathControlPoint[] vertices = new PathControlPoint[points.Length];

            for (int i = 0; i < points.Length; i++)
            {
                vertices[i] = new PathControlPoint(points[i]);
            }

            if (type == CurveType.PerfectCircle)
            {
                if (vertices.Length != 3)
                {
                    type = CurveType.Bezier;
                }
                else if (IsLinear(points[0], points[1], points[2]))
                {
                    type = CurveType.Linear;
                }
            }

            vertices[0].Type = type;

            int startIndex = 0;
            int endIndex = 0;

            while (++endIndex < vertices.Length)
            {
                if (vertices[endIndex].Position != vertices[endIndex - 1].Position)
                    continue;

                if (type == CurveType.Catmull && endIndex > 1)
                    continue;

                if (endIndex == vertices.Length - 1)
                    continue;

                vertices[endIndex - 1].Type = type;
                yield return new ArraySegment<PathControlPoint>(vertices, startIndex, endIndex - startIndex);

                startIndex = endIndex + 1;
            }

            if (endIndex > startIndex)
            {
                yield return new ArraySegment<PathControlPoint>(vertices, startIndex, endIndex - startIndex);
            }

            static bool IsLinear(Vector2 p0, Vector2 p1, Vector2 p2)
            {
                return Precision.AlmostEquals(0, (p1.Y - p0.Y) * (p2.X - p0.X) - (p1.X - p0.X) * (p2.Y - p0.Y));
            }
        }

        private static PathControlPoint[] MergeControlPointsLists(List<ArraySegment<PathControlPoint>> controlPointList)
        {
            int totalCount = 0;

            foreach (var arr in controlPointList)
                totalCount += arr.Count;

            var mergedArray = new PathControlPoint[totalCount];
            int copyIndex = 0;

            foreach (var arr in controlPointList)
            {
                arr.AsSpan().CopyTo(mergedArray.AsSpan(copyIndex));
                copyIndex += arr.Count;
            }

            return mergedArray;
        }

        private static Vector2 ReadPoint(string value, Vector2 startPos)
        {
            string[] split = value.Split(':');

            Vector2 pos = new Vector2((int)float.Parse(split[0], CultureInfo.InvariantCulture.NumberFormat)
                                     ,(int)float.Parse(split[1], CultureInfo.InvariantCulture.NumberFormat));
            pos -= startPos;
            return pos;
        }
        
        private static CurveType GetCurveType(string type)
        {
            switch(type)
            {
                default:
                case "B":
                    return CurveType.Bezier;
                case "C":
                    return CurveType.Catmull;
                case "L":
                    return CurveType.Linear;
                case "P":
                    return CurveType.PerfectCircle;
            }
        }
    }
}
