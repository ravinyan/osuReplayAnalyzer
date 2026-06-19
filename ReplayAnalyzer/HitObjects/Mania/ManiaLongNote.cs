using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System.Transactions;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ReplayAnalyzer.HitObjects.Mania
{
    public class ManiaLongNote : HitObject
    {
        public ManiaLongNote(ManiaLongNoteData noteData)
        {
            ColumnIndex = noteData.ColumnIndex;
            SpawnTime = noteData.SpawnTime;
            EndTime = noteData.EndTime;
            //Judgement = new HitJudgement((HitObjectJudgement)noteData.Judgement.Judgement, noteData.Judgement.SpawnTime);
        }

        public int ColumnIndex { get; set; } = 0;
        public int EndTime { get; set; } = 0;

        public static ManiaLongNote CreateManiaNote(ManiaLongNoteData noteData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateNote(noteData, index);
            }

            return CreateNotePreload(noteData);
        }

        private static ManiaLongNote CreateNote(ManiaLongNoteData noteData, int index)
        {
            string stringWidth = SkinIniProperties.GetManiaPlayfieldWidth();
            string[] stringWidths = stringWidth.Split(",");

            int width = 0;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                width += int.Parse(stringWidths[i]);
            }
            ManiaLongNote note = new ManiaLongNote(noteData);
            

            Image noteHead = new Image();
            noteHead.Width = width / stringWidths.Length;
            noteHead.Source = GetNoteHeadImage(stringWidths.Length, note.ColumnIndex);
            noteHead.Name = "head";
            SetTop(noteHead, 0);
            SetZIndex(noteHead, 10);

            Image noteBody = new Image();
            noteBody.Source = GetNoteBodyImage(stringWidths.Length, note.ColumnIndex);

            // why this took me so long why im bad at math? why
            double sp = HitObjectAnimations.ScrollSpeed;
            double h = ManiaPlayfield.Playfield.ActualHeight - 80;
            int timeBodyIsOnScreen = note.EndTime - note.SpawnTime;
            double bodyLength = h * (timeBodyIsOnScreen / sp);

            noteBody.Height = bodyLength;
            noteBody.Width = width / stringWidths.Length;
            noteBody.Stretch = System.Windows.Media.Stretch.Fill;
            
            noteBody.Name = "body";
            SetTop(noteBody, -noteBody.Height);
            SetZIndex(noteBody, -1);
            
            Image noteTail = new Image();
            noteTail.Width = width / stringWidths.Length;
            noteTail.Source = SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteHead3);// GetNoteHeadImage(stringWidths.Length, note.ColumnIndex);
            noteTail.Name = "tail";
            SetTop(noteTail, -noteBody.Height);
            SetZIndex(noteTail, 1);
            
            note.Children.Add(noteHead);
            note.Children.Add(noteBody);
            note.Children.Add(noteTail);
            SetTop(note, 0);
            SetLeft(note, width / stringWidths.Length * note.ColumnIndex);
            SetZIndex(note, -1);
            note.Name = $"ManiaLongNoteObject{index}";

            return note;
        }

        private static ManiaLongNote CreateNotePreload(ManiaLongNoteData noteData)
        {
            ManiaLongNote note = new ManiaLongNote(noteData);

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

        private static BitmapSource GetNoteHeadImage(int columnCount, int columnIndex)
        {
            if (columnCount % 2 == 1) // for odd length key count: odds = white, even = pink, middle = yellow
            {
                if (columnIndex == columnCount / 2)
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteHead3);
                }
                else if (columnIndex % 2 == 1)
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteHead2);
                }
                else
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteHead1);
                }
            }
            else // for even length key count: split into 2 halves, lower one odd = white, even = pink and higher one is exact opposite
            {
                if (columnIndex < columnCount / 2)
                {
                    if (columnIndex % 2 == 1)
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteHead2);
                    }
                    else
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteHead1);
                    }
                }
                else
                {
                    if (columnIndex % 2 == 1)
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteHead1);
                    }
                    else
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteHead2);
                    }
                }
            }
        }

        private static BitmapSource GetNoteBodyImage(int columnCount, int columnIndex)
        {
            if (columnCount % 2 == 1) // for odd length key count: odds = white, even = pink, middle = yellow
            {
                if (columnIndex == columnCount / 2)
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteBody3);
                }
                else if (columnIndex % 2 == 1)
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteBody2);
                }
                else
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteBody1);
                }
            }
            else // for even length key count: split into 2 halves, lower one odd = white, even = pink and higher one is exact opposite
            {
                if (columnIndex < columnCount / 2)
                {
                    if (columnIndex % 2 == 1)
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteBody2);
                    }
                    else
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteBody1);
                    }
                }
                else
                {
                    if (columnIndex % 2 == 1)
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteBody1);
                    }
                    else
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteBody2);
                    }
                }
            }
        }

        private static BitmapSource GetNoteTailImage(int columnCount, int columnIndex)
        {
            if (columnCount % 2 == 1) // for odd length key count: odds = white, even = pink, middle = yellow
            {
                if (columnIndex == columnCount / 2)
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteTail3);
                }
                else if (columnIndex % 2 == 1)
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteTail2);
                }
                else
                {
                    return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteTail1);
                }
            }
            else // for even length key count: split into 2 halves, lower one odd = white, even = pink and higher one is exact opposite
            {
                if (columnIndex < columnCount / 2)
                {
                    if (columnIndex % 2 == 1)
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteTail2);
                    }
                    else
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteTail1);
                    }
                }
                else
                {
                    if (columnIndex % 2 == 1)
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteTail1);
                    }
                    else
                    {
                        return SkinElement.GetElement(SkinElement.SkinElements.ManiaLongNoteTail2);
                    }
                }
            }
        }
    }
}
