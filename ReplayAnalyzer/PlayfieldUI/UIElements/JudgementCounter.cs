using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.PlayfieldGameplay.ObjectManagers;
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

        public static void Increment(HitObjectJudgement judgement)
        {
            TextBlock counter;
            switch (judgement)
            {
                case HitObjectJudgement.Perfect:
                case HitObjectJudgement.Great:
                    counter = (TextBlock)JudgementCounterPanel.Children[0];
                    counter.Text = $"{++Hit300Count}";
                    break;
                case HitObjectJudgement.Good:
                case HitObjectJudgement.Ok:
                    counter = (TextBlock)JudgementCounterPanel.Children[1];
                    counter.Text = $"{++Hit100Count}";
                    break;
                case HitObjectJudgement.Meh:
                    counter = (TextBlock)JudgementCounterPanel.Children[2];
                    counter.Text = $"{++Hit50Count}";
                    break;
                case HitObjectJudgement.Miss:
                //case Judgement.SliderTickMiss:
                    counter = (TextBlock)JudgementCounterPanel.Children[3];
                    counter.Text = $"{++MissCount}";
                    break;
                default:
                    break;
            }
        }

        private static void Decrement(HitObjectJudgement judgement)
        {
            TextBlock counter;
            switch (judgement)
            {
                case HitObjectJudgement.Great:
                    counter = (TextBlock)JudgementCounterPanel.Children[0];
                    counter.Text = $"{--Hit300Count}";
                    break;
                case HitObjectJudgement.Ok:
                    counter = (TextBlock)JudgementCounterPanel.Children[1];
                    counter.Text = $"{--Hit100Count}";
                    break;
                case HitObjectJudgement.Meh:
                    counter = (TextBlock)JudgementCounterPanel.Children[2];
                    counter.Text = $"{--Hit50Count}";
                    break;
                case HitObjectJudgement.Miss:
                //case Judgement.SliderTickMiss:
                    counter = (TextBlock)JudgementCounterPanel.Children[3];
                    counter.Text = $"{--MissCount}";
                    break;
                default:
                    break;
            }
        }

        // should be now correct... if it wont be i will just remove this idk < it wasnt correct
        // + i liked my incorrect implementation better where numbers just increment and nothing else
        private static void UpdateAfterSeek(double directiom, long time)
        {
            List<HitObjectData> hitObjects = MainWindow.map.HitObjects.Where(h => h is not OsuSpinnerData).ToList();

            // from testing this works correctly and is way more optimized (even tho it doesnt matter before it was <2ms)
            int ii = Hit300Count + Hit100Count + Hit50Count + MissCount - 1;
            if (directiom < 0)
            {
                if (ii < 0)
                {
                    return;
                }
                else if (ii >= hitObjects.Count)
                {
                    ii = hitObjects.Count - 1;
                }

                for (int i = ii; i >= 0; i--)
                {
                    HitObjectData hitObject = hitObjects[i];
                    if (hitObject.Judgement.SpawnTime <= time)
                    {
                        break;
                    }

                    Decrement((HitObjectJudgement)hitObject.Judgement.Judgement);
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

                    Increment((HitObjectJudgement)hitObject.Judgement.Judgement);
                }
            }

            //// i dont want to delete this before commit JUST in case...
            //var a = Hit300Count;
            //var b = Hit100Count;
            //var c = Hit50Count;
            //var d = MissCount;
            //
            //// this 100% works (i think)
            //Reset();
            //for (int i = 0; i < hitObjects.Count; i++)
            //{
            //    HitObjectData hitObject = hitObjects[i];
            //    if (hitObject.Judgement.SpawnTime > time)
            //    {
            //        break;
            //    }
            //    
            //    Increment((HitObjectJudgement)hitObject.Judgement.Judgement);
            //    
            //    // for slider tick misses
            //    if (hitObject is SliderData)
            //    {
            //        SliderData sliderData = (SliderData)hitObject;
            //        Increment((HitObjectJudgement)sliderData.SliderEndJudgement.Judgement);
            //    }
            //}
            //
            //Console.WriteLine("correct - idk if correct");
            //if (Hit300Count != a || Hit100Count != b || Hit50Count != c || MissCount != d) 
            //{ Console.ForegroundColor = ConsoleColor.Red; }
            //else { Console.ForegroundColor = ConsoleColor.White; }
            //Console.WriteLine($"{Hit300Count} - {a}");
            //Console.WriteLine($"{Hit100Count}    - {b}");
            //Console.WriteLine($"{Hit50Count}    - {c}");
            //Console.WriteLine($"{MissCount}    - {d}");
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
    }
}
