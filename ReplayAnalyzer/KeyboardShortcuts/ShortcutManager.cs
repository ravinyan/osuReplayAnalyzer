using OsuFileParsers.Classes.Replay;
using ReplayAnalyzer.Animations;
using ReplayAnalyzer.Beatmaps;
using ReplayAnalyzer.GameClock;
using ReplayAnalyzer.MusicPlayer.Controls;
using ReplayAnalyzer.Objects;
using ReplayAnalyzer.PlayfieldGameplay;
using ReplayAnalyzer.SettingsMenu;
using System.Collections.Frozen;
using System.Configuration;
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

        public static void ShortcutPicker(object sender, KeyEventArgs e)
        {
            string optionName = "";
            foreach (KeyValueConfigurationElement keyValue in SettingsOptions.config.AppSettings.Settings)
            {
                if (keyValue.Value == e.Key.ToString())
                {
                    optionName = keyValue.Key;
                    break;
                }
            }

            switch (optionName)
            {
                case "Open Menu":
                    SettingsButton.OpenClose();
                    break;
                case "Pause":
                    PlayPauseControls.PlayPauseButton(null!, null!);
                    break;
                case "Jump 1 Frame to Left":
                    SongSliderControls.SeekByFrame(-727);
                    break;
                case "Jump 1 Frame to Right":
                    SongSliderControls.SeekByFrame(727);
                    break;
                case "Jump to closest past miss":
                    FindClosestMiss(-727);
                    break;
                case "Jump to closest future miss":
                    FindClosestMiss(727);
                    break;
                case "Rate Change -0.25x":
                    RateChangerControls.ChangeRateShortcut(-727);
                    break;
                case "Rate Change +0.25x":
                    RateChangerControls.ChangeRateShortcut(727);
                    break;
                default:
                    break;
            }
        }

        // other functions fit for their respective classes, this one tho doesnt fit anywhere so will just leave it here
        private static void FindClosestMiss(int direction)
        {
            if (GamePlayClock.IsPaused() == false)
            {
                GamePlayClock.Pause();
                MusicPlayer.MusicPlayer.Pause();
                Window.playerButton.Style = Window.Resources["PlayButton"] as Style;
            }
            
            FindMiss(direction);
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
                //HitObjectSpawner.CatchUpToAliveHitObjects(banana.SpawnTime);

                // LastOrDefault updates cursor position correctly even tho it is performance hit especially on long maps... need to improve one day
                ReplayFrame f = MainWindow.replay.FramesDict.LastOrDefault(f => f.Value.Time <= banana.SpawnTime).Value ?? MainWindow.replay.FramesDict[0];
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
