using ReplayAnalyzer.HitObjects;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.GameplaySkin
{
    public static class SkinIniProperties
    {
        private static List<Color> ComboColours { get; set; } = null!;
        private static bool AreComboColoursSaved = false;

        private static string ManiaPlayfieldWidth = "";

        public static void ResetComboColours()
        {
            if (MainWindow.map.HitObjects != null)
            {
                ComboColours = null!;
                SkinElement.GetHitObjectsRGBValues();
                GetComboColours();
                HitObject.HitCircleBitmapColours = new List<BitmapSource>();
            }
        }

        public static void ResetManiaPlayfieldWidth()
        {
            ManiaPlayfieldWidth = "";
        }

        public static List<Color> GetComboColours()
        {
            if (AreComboColoursSaved == true && ComboColours != null)
            {
                return ComboColours;
            }

            List<Color> comboColours = new List<Color>();
            List<string> colourSection = ReadLinesAt("[Colours]");

            foreach (string s in colourSection)
            {
                if (s.Contains("Combo") && !s.Contains("//"))
                {
                    string newS = s.Trim();

                    string[] rgb = newS.Substring(8).Split(",");

                    comboColours.Add(Color.FromArgb(int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2])));
                }
            }

            ComboColours = comboColours;
            return comboColours;
        }

        public static string GetManiaPlayfieldWidth()
        {
            if (ManiaPlayfieldWidth != "")
            {
                return ManiaPlayfieldWidth;
            }

            // circle size is column count
            List<string> maniaInfo = ReadLinesAt($"Keys: {(int)MainWindow.map.Difficulty.CircleSize}");
            string result = "";
            for (int i = 0; i < maniaInfo.Count; i++)
            {
                if (maniaInfo[i].Contains("ColumnWidth"))
                {
                    result = maniaInfo[i].Split(":")[1].Trim();
                    break;
                }
            }

            ManiaPlayfieldWidth = result;
            return result;
        }

        public static int GetManiaDefaultPosition()
        {
            List<string> maniaInfo = ReadLinesAt($"Keys: {(int)MainWindow.map.Difficulty.CircleSize}");
            int result = 0;
            for (int i = 0; i < maniaInfo.Count; i++)
            {
                if (maniaInfo[i].Contains("ColumnWidth"))
                {
                    result = int.Parse(maniaInfo[i].Split(":")[1].Trim());
                    break;
                }
            }

            return result;
        }

        private static List<string> ReadLinesAt(string section)
        {
            // when app starts this path will not exist yet
            if (!File.Exists($"{SkinElement.SkinPath()}\\skin.ini"))
            {
                return new List<string>();
            }

            string[] properties = File.ReadAllLines($"{SkinElement.SkinPath()}\\skin.ini");
            List<string> elements = new List<string>();

            bool sectionFound = false;

            foreach (string s in properties)
            {
                if (s == section)
                {
                    sectionFound = true;
                } 

                if (s != section && sectionFound == true && s.StartsWith('[') && s.EndsWith(']'))
                {
                    sectionFound = false;
                }

                if (sectionFound)
                {
                    elements.Add(s);
                }
            }

            AreComboColoursSaved = true;
            return elements;
        }
    }
}
