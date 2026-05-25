using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using System.CodeDom;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReplayAnalyzer.PlayfieldUI.UIElements
{
    public class JudgementCounter
    {
        private static StackPanel JudgementCounterPanel = new StackPanel();
        
        private static int Hit300Count = 0;
        private static int Hit100Count = 0;
        private static int Hit50Count = 0;
        private static int MissCount = 0;

        public static void Reset()
        {
            Hit300Count = 0;
            Hit100Count = 0;
            Hit50Count = 0;
            MissCount = 0;

            for (int i = 0; i < JudgementCounterPanel.Children.Count; i++)
            {
                TextBlock counter = (TextBlock)JudgementCounterPanel.Children[i];
                counter.Text = "0";
            }
        }

        public static StackPanel Create()
        {
            ApplyPropertiesToJudgementCounter();

            Brush[] brushes = { Brushes.Blue, Brushes.Green, Brushes.Orange, Brushes.Red };
            for (int i = 0; i < brushes.Length; i++)
            {
                JudgementCounterPanel.Children.Add(CreateJudgementCounter(brushes[i]));
            }

            return JudgementCounterPanel;
        }

        public static void Increment(Judgement judgement)
        {
            TextBlock counter;
            switch (judgement)
            {
                case Judgement.Max:
                    counter = (TextBlock)JudgementCounterPanel.Children[0];
                    counter.Text = $"{++Hit300Count}";
                    break;
                case Judgement.Ok:
                    counter = (TextBlock)JudgementCounterPanel.Children[1];
                    counter.Text = $"{++Hit100Count}";
                    break;
                case Judgement.Meh:
                    counter = (TextBlock)JudgementCounterPanel.Children[2];
                    counter.Text = $"{++Hit50Count}";
                    break;
                case Judgement.Miss:
                case Judgement.SliderTickMiss:
                    counter = (TextBlock)JudgementCounterPanel.Children[3];
                    counter.Text = $"{++MissCount}";
                    break;
                default:
                    break;
            }
        }

        public static void Decrement(Judgement judgement)
        {
            TextBlock counter;
            switch (judgement)
            {
                case Judgement.Max:
                    counter = (TextBlock)JudgementCounterPanel.Children[0];
                    counter.Text = $"{--Hit300Count}";
                    break;
                case Judgement.Ok:
                    counter = (TextBlock)JudgementCounterPanel.Children[1];
                    counter.Text = $"{--Hit100Count}";
                    break;
                case Judgement.Meh:
                    counter = (TextBlock)JudgementCounterPanel.Children[2];
                    counter.Text = $"{--Hit50Count}";
                    break;
                case Judgement.Miss:
                case Judgement.SliderTickMiss:
                    counter = (TextBlock)JudgementCounterPanel.Children[3];
                    counter.Text = $"{--MissCount}";
                    break;
                default:
                    break;
            }
        }

        // on 7k object map at the end of the map this takes 1.4ms average... there are ways to do it better but idk how
        // spinners might be the issue... 6323 6315 x300 and 8 x100
        public static void UpdateAfterSeek(double directiom, long time)
        {
            List<HitObjectData> hitObjects = MainWindow.map.HitObjects.Where(h => h is not SpinnerData).ToList();

            // from testing this works correctly and is way more optimized (even tho it doesnt matter before it was <2ms)
            int ii = Hit300Count + Hit100Count + Hit50Count + MissCount - 1;
            if (directiom < 0)
            {
                if (ii < 0)
                {
                    return;
                }
                if (ii >= hitObjects.Count)
                {
                    ii = hitObjects.Count - 1;
                }

                for (int i = ii; i >= 0; i--)
                {
                    HitObjectData hitObject = hitObjects[i];
                    if (hitObject.Judgement.SpawnTime > time)
                    {
                        Decrement((Judgement)hitObject.Judgement.Judgement);
            
                        // for slider tick misses
                        if (hitObject is SliderData)
                        {
                            SliderData sliderData = (SliderData)hitObject;
                            Decrement((Judgement)sliderData.SliderEndJudgement.Judgement);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (ii < 0)
                {
                    ii = 0;
                }
                else
                {
                    ii++;
                }

                for (int i = ii; i < hitObjects.Count; i++)
                {
                    HitObjectData hitObject = hitObjects[i];
                    if (hitObject.Judgement.SpawnTime > time)
                    {
                        break;
                    }

                    Increment((Judgement)hitObject.Judgement.Judgement);

                    // for slider tick misses
                    if (hitObject is SliderData)
                    {
                        SliderData sliderData = (SliderData)hitObject;
                        Increment((Judgement)sliderData.SliderEndJudgement.Judgement);
                    }
                }
            }

            // i dont want to delete this before commit JUST in case...
            var a = Hit300Count;
            var b = Hit100Count;
            var c = Hit50Count;
            var d = MissCount;
            
            // this 100% works (i think)
            Reset();
            for (int i = 0; i < hitObjects.Count; i++)
            {
                HitObjectData hitObject = hitObjects[i];
                if (hitObject.Judgement.SpawnTime > time)
                {
                    break;
                }
                
                Increment((Judgement)hitObject.Judgement.Judgement);
                
                // for slider tick misses
                if (hitObject is SliderData)
                {
                    SliderData sliderData = (SliderData)hitObject;
                    Increment((Judgement)sliderData.SliderEndJudgement.Judgement);
                }
            }
            
            Console.WriteLine("correct - idk if correct");
            if (Hit300Count != a || Hit100Count != b || Hit50Count != c || MissCount != d) 
            { Console.ForegroundColor = ConsoleColor.Red; }
            else { Console.ForegroundColor = ConsoleColor.White; }
            Console.WriteLine($"{Hit300Count} - {a}");
            Console.WriteLine($"{Hit100Count}    - {b}");
            Console.WriteLine($"{Hit50Count}    - {c}");
            Console.WriteLine($"{MissCount}    - {d}");
        }

        private static void ApplyPropertiesToJudgementCounter()
        {
            JudgementCounterPanel.Name = "JudgementPanel";
            JudgementCounterPanel.Height = 15;
            JudgementCounterPanel.Orientation = Orientation.Horizontal;
            JudgementCounterPanel.HorizontalAlignment = HorizontalAlignment.Right;
            JudgementCounterPanel.VerticalAlignment = VerticalAlignment.Top;
            JudgementCounterPanel.Margin = new Thickness(0, 0, 5, 0);
        }

        private static TextBlock CreateJudgementCounter(Brush colour)
        {
            TextBlock counter = new TextBlock();
            counter.Background = Brushes.Transparent;
            counter.Foreground = colour;
            counter.Text = "0";

            return counter;
        }

        public enum Judgement
        {
            Max = 300,
            Ok = 100,
            Meh = 50,
            Miss = 0,
            SliderTickMiss = -1,
        }
    }
}
