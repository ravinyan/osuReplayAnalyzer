using Realms;
using ReplayParsers.Classes.Beatmap.osu;
using ReplayParsers.Classes.Beatmap.osu.BeatmapClasses;
using ReplayParsers.Classes.Beatmap.osu.Objects;
using ReplayParsers.Classes.Beatmap.osu.OsuDB;
using ReplayParsers.Classes.Replay;
using ReplayParsers.FileWatchers;
using ReplayParsers.SliderPathMath;
using System.Drawing;
using System.Globalization;
using System.Numerics;

namespace ReplayParsers.Decoders
{
    public class BeatmapDecoder
    {
        private static Beatmap osuBeatmap = new Beatmap();

        public static Beatmap GetOsuLazerBeatmap(string beatmapMD5Hash)
        {
            List<(string, string)> mapFileList = new List<(string, string)>();

            string realmFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\client.realm";
            RealmConfiguration config = new RealmConfiguration(realmFilePath) { SchemaVersion = 51 };
            using (Realm realm = Realm.GetInstance(config))
            {
                IQueryable<Classes.Beatmap.osuLazer.Beatmap> realmData = realm.All<Classes.Beatmap.osuLazer.Beatmap>();
                Classes.Beatmap.osuLazer.Beatmap beatmap = realmData.FirstOrDefault(x => x.MD5Hash == beatmapMD5Hash)!;

                foreach (Classes.Beatmap.osuLazer.RealmNamedFileUsage file in beatmap.BeatmapSet!.Files)
                {
                    mapFileList.Add((file.File!.Hash!, file.Filename!));
                }

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
        public static Beatmap GetOsuBeatmap()
        {
            string replayFilePath = FileWatcher.OsuReplayFileWatcher();
            Replay replay = ReplayDecoder.GetReplayData(replayFilePath);
            
            OsuDB osuDB = OsuDBDecoder.GetOsuDBData();
            OsuDBBeatmap beatmap = osuDB.DBBeatmaps!.FirstOrDefault(x => x.BeatmapMD5Hash == replay.BeatmapMD5Hash)!;
            
            string beatmapFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\osu!\\Songs\\{beatmap.BeatmapFolderName}\\{beatmap.BeatmapFileName}";
            Beatmap osuBeatmap = GetBeatmap(beatmapFilePath);

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

            if (file.Length == 1)
            {
                file[0].Delete();

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
            
            if (file.Length == 1)
            {
                file[0].Delete();

                File.Copy($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                         ,$"{AppContext.BaseDirectory}\\osu\\Audio\\{audio}");
            }
            else
            {
                File.Copy($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\osu\\files\\{hash[0]}\\{hash.Substring(0, 2)}\\{hash}"
                         ,$"{AppContext.BaseDirectory}\\osu\\Audio\\audio");
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
                    File.Copy($"{songFolder!.FullName}\\{file.Name}"
                             ,$"{AppContext.BaseDirectory}\\osu\\Audio\\{file.Name}");
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
        
        private static List<HitObject> GetHitObjectsData(List<string> data)
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
                    circle.SpawnPosition = new Vector2(X, Y);
                    circle.SpawnTime = time;
                    circle.Type = type;
                    circle.HitSound = hitSound;
                    circle.HitSample = line[5];

                    hitObjectList.Add(circle);   
                }
                else if (type.HasFlag(ObjectType.Slider))
                {
                    Slider slider = new Slider();

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
                        Vector2 pos = ReadPoint(curves[i], slider.SpawnPosition);
                        controlPoints[i] = pos;
                    }
                    var convertedPoints = ConvertControlPoints(controlPoints, curveType).ToList();
                    slider.ControlPoints = MergeControlPointsLists(convertedPoints);

                    for (int i = 1; i < curves.Length; i++)
                    {
                        string[] c = curves[i].Split(":");
                        slider.CurvePoints!.Add(new Vector2(float.Parse(c[0]), float.Parse(c[1])));
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

                    slider.EndTime = GetSliderEndTime(slider);

                    slider.SliderTicks = GetSliderTicks(slider);

                    hitObjectList.Add(slider);
                }
                else if (type.HasFlag(ObjectType.Spinner))
                {
                    Spinner spinner = new Spinner();

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

            return hitObjectList;
        }

        private static int TimingPointIndex = 0;
        private static double BeatLength = 0;
        // this is one big mess... pain
        private static TimingPoint GetTimingPointAt(int time)
        {
            if (TimingPointIndex >= osuBeatmap.TimingPoints!.Count)
            {
                return osuBeatmap.TimingPoints[TimingPointIndex - 1];
            }

            // if bpm point is at the beginning and next timing point is not on first slider
            if (osuBeatmap.TimingPoints[TimingPointIndex].Time < time
            &&  osuBeatmap.TimingPoints[TimingPointIndex + 1].Time >= time
            &&  osuBeatmap.TimingPoints[TimingPointIndex].BeatLength > 0)
            {
                BeatLength = (double)osuBeatmap.TimingPoints[TimingPointIndex].BeatLength;
            }

            if (osuBeatmap.TimingPoints[TimingPointIndex].BeatLength > 0
            && (osuBeatmap.TimingPoints[TimingPointIndex].Time == time
            || osuBeatmap.TimingPoints[TimingPointIndex].Time == time + 1))
            {
                BeatLength = (double)osuBeatmap.TimingPoints[TimingPointIndex].BeatLength;

                // situation where there is BPM and then Slider Velocity at the same time point
                if (osuBeatmap.TimingPoints[TimingPointIndex + 1].Time == time)
                {
                    TimingPointIndex++;
                    return osuBeatmap.TimingPoints[TimingPointIndex++];
                }

                return osuBeatmap.TimingPoints[TimingPointIndex++];
            }

            if (osuBeatmap.TimingPoints[TimingPointIndex].Time > time)
            {
                return osuBeatmap.TimingPoints[TimingPointIndex - 1];
            }

            if (TimingPointIndex < osuBeatmap.TimingPoints.Count 
            &&  osuBeatmap.TimingPoints[TimingPointIndex].Time <= time)
            {
                while (TimingPointIndex + 1 < osuBeatmap.TimingPoints.Count
                &&  osuBeatmap.TimingPoints[TimingPointIndex].BeatLength == osuBeatmap.TimingPoints[TimingPointIndex + 1].BeatLength)
                {
                    TimingPointIndex++;
                }

                while (TimingPointIndex > 0 && osuBeatmap.TimingPoints[TimingPointIndex].Time < time)
                {
                    TimingPointIndex++;
                }

                // situation where there is BPM and then Slider Velocity at the same time point
                if (TimingPointIndex + 1 < osuBeatmap.TimingPoints.Count
                &&  osuBeatmap.TimingPoints[TimingPointIndex].Time == time
                &&  (osuBeatmap.TimingPoints[TimingPointIndex + 1].Time == time
                ||  osuBeatmap.TimingPoints[TimingPointIndex + 1].Time == time + 1)
                &&  osuBeatmap.TimingPoints[TimingPointIndex].BeatLength > 0)
                {
                    BeatLength = (double)osuBeatmap.TimingPoints[TimingPointIndex].BeatLength;

                    // skipping these points
                    TimingPointIndex++;
                    return osuBeatmap.TimingPoints[TimingPointIndex++];
                }

                // for this one timing point in sound chimera that is 1ms before circle spawn...
                if (osuBeatmap.TimingPoints[TimingPointIndex].Time > time)
                {
                    return osuBeatmap.TimingPoints[TimingPointIndex - 1];
                }

                return osuBeatmap.TimingPoints[TimingPointIndex++];
            }

            return osuBeatmap.TimingPoints[TimingPointIndex];
        }

        private static double GetSliderEndTime(Slider slider)
        {
            TimingPoint point = GetTimingPointAt(slider.SpawnTime);

            double sliderVelocityMultiplayer = point.BeatLength < 0 ? 100.0 / (double)-point.BeatLength : 1;

            double sliderVelocityAsBeatLength = -100 / sliderVelocityMultiplayer;
            double bpmMultiplier = sliderVelocityAsBeatLength < 0 ? Math.Clamp((float)-sliderVelocityAsBeatLength, 10, 1000) / 100.0 : 1;

            double SM = (double)osuBeatmap.Difficulty!.SliderMultiplier;
            double velocity = 100 * SM / (BeatLength * bpmMultiplier);

            return slider.EndTime = slider.SpawnTime + (slider.RepeatCount) * slider.Path.Distance / velocity;
        }

        private static SliderTick[] GetSliderTicks(Slider slider)
        {
            TimingPoint point = GetTimingPointAt(slider.SpawnTime);

            double sliderVelocityMultiplayer = point.BeatLength < 0 ? 100.0 / (double)-point.BeatLength : 1;

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

            Vector2 pos = new Vector2((int)float.Parse(split[0]), (int)float.Parse(split[1]));
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
