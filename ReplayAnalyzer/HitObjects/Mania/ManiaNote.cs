using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.HitObjects.Mania
{
    public class ManiaNote : HitObject
    {
        public ManiaNote(ManiaNoteData noteData)
        {
            ColumnIndex = noteData.ColumnIndex;
            SpawnTime = noteData.SpawnTime;
            //Judgement = new HitJudgement((HitObjectJudgement)noteData.Judgement.Judgement, noteData.Judgement.SpawnTime);
        }

        public int ColumnIndex { get; set; } = 0;

        public static ManiaNote CreateManiaNote(ManiaNoteData noteData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateNote(noteData, index);
            }

            return CreateNotePreload(noteData);
        }

        private static ManiaNote CreateNote(ManiaNoteData noteData, int index)
        {
            Stopwatch w = new Stopwatch();
            w.Start();
            string stringWidth = SkinIniProperties.GetManiaPlayfieldWidth();
            string[] stringWidths = stringWidth.Split(",");

            int width = 0;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                width += int.Parse(stringWidths[i]);
            }

            w.Stop();
            Console.WriteLine(w.ElapsedTicks);
            ManiaNote note = new ManiaNote(noteData);

            Image noteImage = new Image();
            noteImage.Width = width / stringWidths.Length;
            noteImage.Source = GetNoteImage(stringWidths.Length, note.ColumnIndex);
            
            note.Children.Add(noteImage);

            Canvas.SetLeft(note, width / stringWidths.Length * note.ColumnIndex);
            Canvas.SetTop(note, 0);
            Canvas.SetZIndex(note, -1);
            note.Name = $"ManiaNoteObject{index}";

            return note;
        }

        private static ManiaNote CreateNotePreload(ManiaNoteData noteData)
        {
            ManiaNote note = new ManiaNote(noteData);

            string stringWidth = SkinIniProperties.GetManiaPlayfieldWidth();
            string[] stringWidths = stringWidth.Split(",");

            int width = 0;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                width += int.Parse(stringWidths[i]);
            }

            Image noteImage = new Image();
            note.Children.Add(noteImage);

            Canvas.SetLeft(note, width / stringWidths.Length * note.ColumnIndex);
            Canvas.SetTop(note, 0);

            return note;
        }

        private static BitmapSource GetNoteImage(int columnCount, int columnIndex)
        {
            if (columnCount % 2 == 1) // for odd length key count: odds = white, even = pink, middle = yellow
            {
                if (columnIndex == columnCount / 2)
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaNote3);
                }
                else if (columnIndex % 2 == 1)
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaNote2);
                }
                else
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaNote1);
                }
            }
            else // for even length key count: split into 2 halves, lower one odd = white, even = pink and higher one is exact opposite
            {
                if (columnIndex < columnCount / 2)
                {
                    if (columnIndex % 2 == 1)
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaNote2);
                    }
                    else
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaNote1);
                    }
                }
                else
                {
                    if (columnIndex % 2 == 1)
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaNote1);
                    }
                    else
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaNote2);
                    }
                }
            }
        }
    }
}
