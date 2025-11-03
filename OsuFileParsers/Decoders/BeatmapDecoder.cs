using NAudio.Vorbis;
using NAudio.Wave;
using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using OsuFileParsers.Classes.Beatmap.osu.OsuDB;
using OsuFileParsers.SliderPathMath;
using Realms;
using ReplayParsers.Classes.Beatmap.osuLazer;
using System.Drawing;
using System.Globalization;
using System.Numerics;
using LazerBeatmap = ReplayParsers.Classes.Beatmap.osuLazer.Beatmap;
using Beatmap = OsuFileParsers.Classes.Beatmap.osu.Beatmap;
using File = System.IO.File;

namespace OsuFileParsers.Decoders
{
    public class BeatmapDecoder
    {
        private static Beatmap? osuBeatmap;

        public static Beatmap GetOsuLazerBeatmap(string beatmapMD5Hash)
        {
            List<(string, string)> mapFileList = new List<(string, string)>();
            
            string realmFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\client.realm";
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
                osuBeatmap = GetBeatmap($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{beatmap.Hash![0]}\\{beatmap.Hash.Substring(0, 2)}\\{beatmap.Hash}");
            }

            GetOsuLazerBeatmapBackground(osuBeatmap, mapFileList);
            GetOsuLazerBeatmapAudio(osuBeatmap, mapFileList);
            GetOsuLazerBeatmapHitsounds(osuBeatmap, mapFileList);
        
            return osuBeatmap;
        }

        /// <summary>
        /// Gets full osu! beatmap data.
        /// </summary>
        /// <returns></returns>
        public static Beatmap GetOsuBeatmap(string beatmapMD5Hash)
        {
            OsuDB osuDB = OsuDBDecoder.GetOsuDBData();
            OsuDBBeatmap beatmap = osuDB.DBBeatmaps!.FirstOrDefault(x => x.BeatmapMD5Hash == beatmapMD5Hash)!;
            
            string beatmapFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!\\Songs\\{beatmap.BeatmapFolderName}\\{beatmap.BeatmapFileName}";
            osuBeatmap = new Beatmap();
            osuBeatmap = GetBeatmap(beatmapFilePath);

            GetOsuBeatmapFiles(osuBeatmap);

            //DisplayData(osuBeatmap);

            return osuBeatmap;
        }

        private static Beatmap GetBeatmap(string beatmapFilePath)
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

        private static void GetOsuLazerBeatmapBackground(Beatmap beatmap, List<(string, string)> mapFileList)
        {
            if (!Directory.Exists($"{AppContext.BaseDirectory}\\osu\\Background"))
            {
                Directory.CreateDirectory($"{AppContext.BaseDirectory}\\osu\\Background");
            }

            string[] bgEvents = beatmap.Events!.Backgrounds!.Split(",");
            (string hash, string bg) = mapFileList.FirstOrDefault(x => x.Item2 == bgEvents[2]);

            DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}\\osu\\Background");
            FileInfo[] file = dir.GetFiles();

            if (file.Length >= 1)
            {
                foreach(var f in file)
                {
                    try
                    {
                        f.Delete();
                    }
                    catch { } // you are annoyingt
                }

                File.Copy($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                         ,$"{AppContext.BaseDirectory}\\osu\\Background\\{bg}");
            }
            else
            {
                File.Copy($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                         ,$"{AppContext.BaseDirectory}\\osu\\Background\\{bg}");
            }
        }

        private static void GetOsuLazerBeatmapAudio(Beatmap beatmap, List<(string, string)> mapFileList)
        {
            if (!Directory.Exists($"{AppContext.BaseDirectory}\\osu\\Audio"))
            {
                Directory.CreateDirectory($"{AppContext.BaseDirectory}\\osu\\Audio");
            }

            (string hash, string audio) = mapFileList.FirstOrDefault(x => x.Item2 == beatmap.General!.AudioFileName);

            DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}\\osu\\Audio");
            FileInfo[] file = dir.GetFiles();

            // god i hate .ogg files... i really really hate them... did i wrote that i hate them? coz i hate them
            if (audio.Contains(".ogg"))
            {
                if (file.Length >= 1)
                {
                    foreach (var f in file)
                    {
                        try
                        {
                            f.Delete();
                        }
                        catch { } // you are annoying
                    }
                }

                try
                {
                    using (FileStream stream = File.Open($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}", FileMode.Open, FileAccess.Read))
                    {
                        using (VorbisWaveReader reader = new VorbisWaveReader(stream))
                        {
                            MediaFoundationEncoder.EncodeToMp3(reader, $"{AppContext.BaseDirectory}\\osu\\Audio\\{audio.Split('.')[0]}.mp3");
                        }
                    }
                }
                catch
                {
                    throw new ArgumentException("File in use cant access");
                }
                
            }
            else
            {
                if (file.Length >= 1)
                {
                    foreach (var f in file)
                    {
                        try
                        {
                            f.Delete();
                        }
                        catch { } // you are annoying
                    }

                    File.Copy($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                             , $"{AppContext.BaseDirectory}\\osu\\Audio\\{audio}");
                }
                else
                {
                    File.Copy($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                             , $"{AppContext.BaseDirectory}\\osu\\Audio\\{audio}");
                }
            }
        }

        private static void GetOsuLazerBeatmapHitsounds(Beatmap beatmap, List<(string, string)> mapFileList)
        {
            if (!Directory.Exists($"{AppContext.BaseDirectory}\\osu\\Hitsounds"))
            {
                Directory.CreateDirectory($"{AppContext.BaseDirectory}\\osu\\Hitsounds");
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}\\osu\\Hitsounds");
                foreach (FileInfo file in dir.GetFiles())
                {
                    file.Delete();
                }
            }
            
            for (int i = 0; i < mapFileList.Count; i++)
            {
                (string hash, string audio) = mapFileList[i];

                if (audio.Contains(".wav"))
                {
                    File.Copy($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Hitsounds\\{audio}");
                }
            }
        }

        private static void GetOsuBeatmapFiles(Beatmap beatmap)
        {
            if (!Directory.Exists($"{AppContext.BaseDirectory}\\osu\\Background"))
            {
                Directory.CreateDirectory($"{AppContext.BaseDirectory}\\osu\\Background");
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}\\osu\\Background");
                foreach (FileInfo file in dir.GetFiles())
                {
                    file.Delete();
                }
            }

            if (!Directory.Exists($"{AppContext.BaseDirectory}\\osu\\Audio"))
            {
                Directory.CreateDirectory($"{AppContext.BaseDirectory}\\osu\\Audio");
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}\\osu\\Audio");
                foreach (FileInfo file in dir.GetFiles())
                {
                    file.Delete();
                }
            }

            if (!Directory.Exists($"{AppContext.BaseDirectory}\\osu\\Hitsounds"))
            {
                Directory.CreateDirectory($"{AppContext.BaseDirectory}\\osu\\Hitsounds");
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo($"{AppContext.BaseDirectory}\\osu\\Hitsounds");
                foreach (FileInfo file in dir.GetFiles())
                {
                    file.Delete();
                }
            }

            DirectoryInfo songsFolder = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!\\Songs");
            DirectoryInfo? songFolder = songsFolder.GetDirectories().FirstOrDefault(x => x.Name.Contains($"{beatmap.Metadata!.BeatmapSetId}"));

            foreach (FileInfo file in songFolder!.GetFiles())
            {
                string[] bg = beatmap.Events!.Backgrounds!.Split(",");
                if (file.Name == bg[2])
                {
                    File.Copy($"{songFolder!.FullName}\\{file.Name}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Background\\{file.Name}");
                }

                if (file.Name == beatmap.General!.AudioFileName)
                {
                    if (beatmap.General.AudioFileName.Contains(".ogg"))
                    {
                        using (FileStream stream = File.Open($"{songFolder!.FullName}\\{file.Name}", FileMode.Open, FileAccess.Read))
                        {
                            using (VorbisWaveReader reader = new VorbisWaveReader(stream))
                            {
                                MediaFoundationEncoder.EncodeToMp3(reader, $"{AppContext.BaseDirectory}\\osu\\Audio\\{file.Name.Split('.')[0]}.mp3");
                            }
                        }
                    }
                    else
                    {
                        File.Copy($"{songFolder!.FullName}\\{file.Name}"
                             , $"{AppContext.BaseDirectory}\\osu\\Audio\\{file.Name}");
                    }
                }

                if (file.Name.Contains(".wav"))
                {
                    File.Copy($"{songFolder!.FullName}\\{file.Name}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Hitsounds\\{file.Name}");
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
            List<HitObjectData> hitObjectList = new List<HitObjectData>();

            foreach (string property in data)
            {
                string[] line = property.Split(",");

                int X = (int)float.Parse(line[0], CultureInfo.InvariantCulture.NumberFormat);
                int Y = (int)float.Parse(line[1], CultureInfo.InvariantCulture.NumberFormat);
                int time = (int)float.Parse(line[2], CultureInfo.InvariantCulture.NumberFormat);
                ObjectType type = (ObjectType)int.Parse(line[3]);
                HitSound hitSound = (HitSound)int.Parse(line[4]);

                if (type.HasFlag(ObjectType.HitCircle))
                {
                    CircleData circle = new CircleData();

                    circle.X = X;
                    circle.Y = Y;
                    circle.SpawnPosition = new Vector2(X, Y);
                    circle.SpawnTime = time;
                    circle.Type = type;
                    circle.HitSound = hitSound;
                    circle.HitSample = line[5];

                    hitObjectList.Add(circle);   
                }
                else if (type.HasFlag(ObjectType.Slider))
                {
                    SliderData slider = new SliderData();

                    slider.X = X;
                    slider.Y = Y;
                    slider.SpawnPosition = new Vector2(X, Y);
                    slider.SpawnTime = time;
                    slider.Type = type;
                    slider.HitSound = hitSound;

                    string[] curves = line[5].Split("|");
                    CurveType curveType = GetCurveType(curves[0]);
                    Vector2[] controlPoints = new Vector2[curves.Length];
                    for (int i = 1; i < curves.Length; i++)
                    {
                        if (curves[i].Length == 1)
                        {
                            continue;
                        }

                        Vector2 pos = ReadPoint(curves[i], slider.SpawnPosition);
                        controlPoints[i] = pos;
                    }

                    var convertedPoints = ConvertControlPoints(controlPoints, curveType).ToList();
                    slider.ControlPoints = MergeControlPointsLists(convertedPoints);

                    for (int i = 1; i < curves.Length; i++)
                    {
                        if (curves[i].Length == 1)
                        {
                            continue;
                        }

                        string[] c = curves[i].Split(":");
                        slider.CurvePoints!.Add(new Vector2(float.Parse(c[0], CultureInfo.InvariantCulture.NumberFormat)
                                                           ,float.Parse(c[1], CultureInfo.InvariantCulture.NumberFormat)));
                    }
                    
                    slider.RepeatCount = int.Parse(line[6]);
                    slider.Length = decimal.Parse(line[7], CultureInfo.InvariantCulture.NumberFormat);

                    if (line.Length > 8)
                    {
                        slider.EdgeSounds = line[8];
                        slider.EdgeSets = line[9];
                        slider.HitSample = line[10];
                    }

                    slider.Path = new SliderPath(slider);
                    slider.EndPosition = slider.SpawnPosition + slider.Path.PositionAt(1);

                    var endTime = GetSliderEndTime(slider);
                    // (endTime - (endTime - slider.SpawnTime) / 1.5);

                    slider.EndTime = endTime;

                    slider.SliderTicks = GetSliderTicks(slider);

                    hitObjectList.Add(slider);
                }
                else if (type.HasFlag(ObjectType.Spinner))
                {
                    SpinnerData spinner = new SpinnerData();

                    spinner.X = X;
                    spinner.Y = Y;
                    spinner.SpawnPosition = new Vector2(X, Y);
                    spinner.SpawnTime = time;
                    spinner.Type = type;
                    spinner.HitSound = hitSound;
                    spinner.EndTime = int.Parse(line[5]);
                    spinner.HitSample = line[6];

                    hitObjectList.Add(spinner);
                }
            }

            BeatLength = 0;
            return hitObjectList;
        }

        private static double BeatLength = 0;
        private static TimingPoint GetTimingPointAt(int time)
        {
            TimingPoint point = BinarySearch(osuBeatmap!.TimingPoints!, time);

            // scenario where 2 timing points have the same Time and one has
            // positive BeatLength (bpm) and one negative (slider velocity)
            // this makes it so BeatLength is always set correctly (hopefully this time everithing works...)

            // there might be case like that in the middle of the map tho... need to test that somehow oops
            // deity mode time 216648 index 1029 and time 212614 index 1017 for both bpm change before and after
            int currentIndex = osuBeatmap.TimingPoints.IndexOf(point);
            if (currentIndex > 0 && osuBeatmap.TimingPoints[currentIndex - 1].Time == point.Time && osuBeatmap.TimingPoints[currentIndex - 1].BeatLength > 0)
            {
                BeatLength = osuBeatmap.TimingPoints[currentIndex - 1].BeatLength;
            }
            else if (osuBeatmap.TimingPoints[currentIndex + 1].Time == point.Time && osuBeatmap.TimingPoints[currentIndex + 1].BeatLength > 0)
            {
                BeatLength = osuBeatmap.TimingPoints[currentIndex + 1].BeatLength;
            }
            else if (point.BeatLength > 0)
            {
                BeatLength = point.BeatLength;
            }
            
            if (BeatLength == 0 && osuBeatmap.TimingPoints.Count > 0)
            {
                BeatLength = osuBeatmap.TimingPoints.First(b => b.BeatLength > 0).BeatLength;
            }

            return point == null ? TimingPoint.DEFAULT : point;
        }

        // taken from osu lazer code but done a bit different hopefully it works
        private static TimingPoint BinarySearch(List<TimingPoint> timingPoints, int time)
        {
            int n = timingPoints.Count;

            if (n == 0 || time < timingPoints[0].Time)
            {
                return null!;
            }

            int l = 0;
            int r = n - 1;

            // there are sometimes for some ANNOYING reason rare velocity changes 1ms before actual slider spawn
            // this finds always index with time higher than current one, then giving index - 1, hopefully removing this 1ms thing
            while (l < r)
            {
                int mid = l + ((r - l) >> 1);

                if (time >= timingPoints[mid].Time)
                {
                    l = mid + 1;
                }
                else if (time < timingPoints[mid].Time)
                {
                    r = mid;
                }
            }

            return timingPoints[l - 1];
        }

        private static double GetSliderEndTime(SliderData slider)
        {
            TimingPoint point = GetTimingPointAt(slider.SpawnTime);

            double sliderVelocityMultiplayer = point.BeatLength < 0 ? 100.0 / -point.BeatLength : 1;

            double sliderVelocityAsBeatLength = -100 / sliderVelocityMultiplayer;
            double bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 1000) / 100.0 : 1;

            double SM = (double)osuBeatmap.Difficulty!.SliderMultiplier;
            double velocity = 100 * SM / (BeatLength * bpmMultiplier);

            return slider.EndTime = slider.SpawnTime + slider.RepeatCount * slider.Path.Distance / velocity;
        }

        private static SliderTick[] GetSliderTicks(SliderData slider)
        {
            TimingPoint point = GetTimingPointAt(slider.SpawnTime);

            double sliderVelocityMultiplayer = point.BeatLength < 0 ? 100.0 / -point.BeatLength : 1;

            double sliderVelocityAsBeatLength = -100 / sliderVelocityMultiplayer;
            double bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 1000) / 100.0 : 1;

            double SM = (double)osuBeatmap.Difficulty!.SliderMultiplier;
            double velocity = 100 * SM / (BeatLength * bpmMultiplier);

            // math for tickDistance from osu lazer code
            double scoringDistance = velocity * BeatLength;

            double minimalDistanceFromEnd = velocity * 10;
            double tickDistance = (scoringDistance / (double)osuBeatmap.Difficulty.SliderTickRate * 1) + minimalDistanceFromEnd;
            //                                                                        change this?  ^

            double sliderDistance = slider.Path.Distance;

            int numberOfTicks = (int)(sliderDistance / tickDistance);

            if (numberOfTicks > 0)
            {
                SliderTick[] ticks = new SliderTick[numberOfTicks];
                double posIndex = 0;
                double tickIndex = 0;
                for (int i = 0; i < numberOfTicks; i++)
                {
                    SliderTick sliderTick = new SliderTick();

                    // im just stupid over 3 days
                    // pos is correct > tick AND pos is correct but in different places > tick is correct
                    // > no pos is correct > no tick is ACTUALLy correct > ???????????????
                    posIndex += tickDistance / sliderDistance;
                    tickIndex += (tickDistance - minimalDistanceFromEnd) / sliderDistance;

                    Vector2 posAt = slider.Path.PositionAt(tickIndex);

                    sliderTick.Position = posAt;
                    sliderTick.PositionAt = tickIndex;
                    sliderTick.Time = slider.SpawnTime + tickIndex * (slider.EndTime - slider.SpawnTime);

                    ticks[i] = sliderTick;
                }

                return ticks;
            }

            return null!;
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
