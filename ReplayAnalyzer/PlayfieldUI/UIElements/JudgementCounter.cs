using OsuFileParsers.Classes.Beatmap.osu.BeatmapClasses;
using OsuFileParsers.Classes.Beatmap.osu.Objects;
using ReplayAnalyzer.HitObjects;
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

        public static void UpdateAfterSeek(long time)
        {
            List<HitObjectData> hitObjects = MainWindow.map.HitObjects;

            Reset();
            for (int i = 0; i < hitObjects.Count; i++)
            {
                HitObjectData hitObject = hitObjects[i];
                if (hitObject.SpawnTime > time)
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
