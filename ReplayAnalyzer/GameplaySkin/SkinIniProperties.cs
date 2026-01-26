using System.Drawing;
using System.IO;

namespace ReplayAnalyzer.GameplaySkin
{
    public static class SkinIniProperties
    {
        private static List<Color> ComboColours { get; set; } = null!;

        public static List<Color> GetComboColours()
        {
            if (ComboColours != null)
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

        private static List<string> ReadLinesAt(string section)
        {
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

            return elements;
        }
    }
}
