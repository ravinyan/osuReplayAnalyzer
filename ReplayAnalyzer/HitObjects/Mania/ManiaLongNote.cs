using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.GameplaySkin;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
using ReplayAnalyzer.PlayfieldUI.GamePlayfields;
using System;
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
            WasHoldBroken = false;
            Judgement = new HitJudgement((HitObjectJudgement)noteData.Judgement.Judgement, noteData.Judgement.SpawnTime);
        }

        public int ColumnIndex { get; set; } = 0;
        public int EndTime { get; set; } = 0;
        public bool HoldStarted { get; set; } = false;
        public bool WasHoldBroken { get; set; } = false;

        public static ManiaLongNote CreateManiaNote(ManiaLongNoteData noteData, int index)
        {
            if (MainWindow.IsReplayPreloading == false)
            {
                return CreateNote(noteData, index);
            }

            return CreateNotePreload(noteData, index);
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
            noteHead.Width = ManiaPlayfield.ColumnWidth;
            noteHead.Source = GetNoteHeadImage(stringWidths.Length, note.ColumnIndex);
            noteHead.Name = "head";
            Canvas.SetTop(noteHead, 0);
            Canvas.SetZIndex(noteHead, 10);

            Image noteBody = new Image();
            noteBody.Source = GetNoteBodyImage(stringWidths.Length, note.ColumnIndex);

            // why this took me so long why im bad at math? why
            double h = ManiaPlayfield.Playfield.ActualHeight - 80;
            int timeBodyIsOnScreen = note.EndTime - note.SpawnTime;
            double bodyLength = h * (timeBodyIsOnScreen / HitObjectAnimations.ScrollSpeed);

            noteBody.Height = bodyLength;
            noteBody.Width = ManiaPlayfield.ColumnWidth;
            noteBody.Stretch = System.Windows.Media.Stretch.Fill;
            noteBody.Name = "body";
            Canvas.SetTop(noteBody, -noteBody.Height);
            Canvas.SetZIndex(noteBody, -1);
            
            Image noteTail = new Image();
            noteTail.Width = ManiaPlayfield.ColumnWidth;
            noteTail.Source = GetNoteTailImage(stringWidths.Length, note.ColumnIndex);
            noteTail.Name = "tail";
            Canvas.SetTop(noteTail, -noteBody.Height);
            Canvas.SetZIndex(noteTail, 1);
            
            note.Children.Add(noteHead);
            note.Children.Add(noteBody);
            note.Children.Add(noteTail);

            Canvas.SetTop(note, 0);
            Canvas.SetLeft(note, ManiaPlayfield.ColumnWidth * note.ColumnIndex);
            Canvas.SetZIndex(note, -1);

            note.Name = $"ManiaLongNoteObject{index}";

            return note;
        }

        private static ManiaLongNote CreateNotePreload(ManiaLongNoteData noteData, int index)
        {
            string stringWidth = SkinIniProperties.GetManiaPlayfieldWidth();
            string[] stringWidths = stringWidth.Split(",");

            int width = 0;
            for (int i = 0; i < stringWidths.Length; i++)
            {
                width += int.Parse(stringWidths[i]);
            }

            ManiaLongNote note = new ManiaLongNote(noteData);

            // there is no reason to do anything else even tho its long note
            // the math should be based on spawn and end time to get all judgements
            Image noteHead = new Image();
            note.Children.Add(noteHead);

            Canvas.SetLeft(note, width / stringWidths.Length * note.ColumnIndex);
            Canvas.SetTop(note, 0);

            note.Name = $"ManiaLongNoteObject{index}";

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
