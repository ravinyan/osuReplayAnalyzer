using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.SettingsMenu;
using System.Windows;
using System.Windows.Input;

namespace ReplayAnalyzer.KeyboardShortcuts
{
    public class ShortcutManager
    {
        private static readonly MainWindow Window = (MainWindow)Application.Current.MainWindow;

        public static void Initialize()
        {
            Window.KeyDown += ShortcutPicker;
        }

        private static void ShortcutPicker(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    SettingsButton.OpenClose();
                    break;
                case Key.Space:
                    PlayPauseControls.PlayPauseButton(null!, null!);
                    break;
                case Key.OemComma:
                    SongSliderControls.SeekByFrame(e.Key);
                    break;
                case Key.OemPeriod:
                    SongSliderControls.SeekByFrame(e.Key);
                    break;
                case Key.Left:
                    FindClosestMiss(e.Key);
                    break;
                case Key.Right:
                    FindClosestMiss(e.Key);
                    break;
                case Key.Up:
                    RateChangerControls.ChangeRateShortcut(e.Key);
                    break;
                case Key.Down:
                    RateChangerControls.ChangeRateShortcut(e.Key);
                    break;
                default:
                    break;
            }
        }

        // other functions fit for their respective classes, this one tho doesnt fit anywhere so will just leave it here
        private static void FindClosestMiss(Key key)
        {
            if (GamePlayClock.IsPaused() == false)
            {
                GamePlayClock.Pause();
                MusicPlayer.MusicPlayer.Pause();
                Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
            }
            
            int direction = 0;
            if (key == Key.Left)
            {
                direction = -727;
                FindMiss(direction);
            }
            else if (key == Key.Right)
            {
                direction = 727;
                FindMiss(direction);
            }
        }

        private static void FindMiss(int direction)
        {
            long time = (long)GamePlayClock.TimeElapsed;

            // long long maaaaaaaaaaaan
            HitObject? banana = direction > 0
                ? OsuBeatmap.HitObjectDictByIndex.FirstOrDefault(ho => time < ho.Value.SpawnTime && ho.Value.Judgement == HitJudgementManager.HitObjectJudgement.Miss).Value ?? null
                : OsuBeatmap.HitObjectDictByIndex.LastOrDefault(ho => time > ho.Value.SpawnTime && ho.Value.Judgement == HitJudgementManager.HitObjectJudgement.Miss).Value ?? null;

            if (banana != null)
            {
                HitObjectManager.ClearAliveObjects();

                GamePlayClock.Seek(banana.SpawnTime);
                Window.songSlider.Value = banana.SpawnTime;
                HitObjectSpawner.CatchUpToAliveHitObjects(banana.SpawnTime);

                // LastOrDefault updates cursor position correctly even tho it is performance hit especially on long maps... need to improve one day
                ReplayFrame f = MainWindow.replay.Frames.LastOrDefault(f => f.Time <= banana.SpawnTime) ?? MainWindow.replay.Frames.First();
                CursorManager.UpdateCursorPositionAfterSeek(f);

                HitMarkerManager.UpdateHitMarkerAfterSeek(direction, banana.SpawnTime);

                // clear all sliders since visuals break for some reason... thank god its fast and i dont need to optimize lol
                foreach (HitObject hitObject in OsuBeatmap.HitObjectDictByIndex.Values)
                {
                    if (hitObject is Slider)
                    {
                        Slider.ResetToDefault(hitObject);
                    }
                }

                HitObjectAnimations.Seek(HitObjectManager.GetAliveHitObjects());
            }
        }
    }
}
