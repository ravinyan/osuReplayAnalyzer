using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReplayAnalyzer.HitObjects
{
    public class ManiaNote : HitObject
    {
        public ManiaNote(ManiaNoteData noteData)
        {
            ColumnIndex = noteData.ColumnIndex;
            SpawnTime = noteData.SpawnTime;
            //Judgement = new HitJudgement((HitObjectJudgement)noteData.Judgement.Judgement, noteData.Judgement.SpawnTime);
        }

        public static int ColumnIndex { get; set; } = 0;

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
            string stringWidth = SkinIniProperties.GetManiaPlayfieldWidth();
            string[] stringWidths = stringWidth.Split(",");

            int width = 0;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                width += int.Parse(stringWidths[i]);
            }
            ManiaNote note = new ManiaNote(noteData);
            note.Width = 10;
            note.Height = 20;

            Image noteImage = new Image();
            noteImage.Width = width / stringWidths.Length;
            noteImage.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaNote1);

            note.Children.Add(noteImage);

            Canvas.SetLeft(note, (width / stringWidths.Length) * ColumnIndex);
            Canvas.SetTop(note, 0);

            note.Name = $"ManiaNoteObject{index}";

            return note;
        }

        private static ManiaNote CreateNotePreload(ManiaNoteData noteData)
        {
            string stringWidth = SkinIniProperties.GetManiaPlayfieldWidth();
            string[] stringWidths = stringWidth.Split(",");

            int width = 0;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                width += int.Parse(stringWidths[i]);
            }
            ManiaNote note = new ManiaNote(noteData);
            note.Width = 10;
            note.Height = 20;

            Image noteImage = new Image();
            note.Children.Add(noteImage);

            Canvas.SetLeft(note, 50);
            Canvas.SetTop(note, 0);

            return note;
        }
    }
}
